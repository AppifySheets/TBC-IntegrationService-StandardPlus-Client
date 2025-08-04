using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

// GitHub Actions workflow for PR validation
// Runs on all pull requests to ensure code quality and version bump
// Checks compilation and verifies VERSION has been incremented
[GitHubActions(
    "pr-validation",
    GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.PullRequest],
    InvokedTargets = [nameof(CheckVersion), nameof(Compile)],
    EnableGitHubToken = true,
    PublishArtifacts = true,
    CacheKeyFiles = ["global.json", "**/*.csproj", "**/Directory.*.props"])]
// GitHub Actions workflow for releases
// Triggered when a new release is created, publishes NuGet packages
// Publishes to both NuGet.org and GitHub Packages
[GitHubActions(
    "release",
    GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.WorkflowDispatch],
    InvokedTargets = [nameof(Publish)],
    EnableGitHubToken = true,
    PublishArtifacts = true,
    ImportSecrets = [nameof(NuGetApiKey)],
    CacheKeyFiles = ["global.json", "**/*.csproj", "**/Directory.*.props"])]
class Build : NukeBuild
{
    // Entry point for the build
    // Default target is 'Test' to ensure all tests pass
    public static int Main() => Execute<Build>(x => x.Test);

    // Build configuration - Debug for local builds, Release for CI
    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    // NuGet API key for publishing packages (imported from GitHub secrets)
    [Parameter("NuGet API Key for publishing packages")]
    [Secret]
    readonly string NuGetApiKey;

    // GitHub token for publishing packages to GitHub Packages
    [Parameter("GitHub Token for publishing packages")]
    [Secret]
    readonly string GitHubToken;

    // Solution file reference
    [Solution(GenerateProjects = true)]
    readonly Solution Solution;

    // Git repository information for version detection
    [GitRepository]
    readonly GitRepository GitRepository;

    // Source directory for the build
    AbsolutePath SourceDirectory => RootDirectory / "AppifySheets.TBC.IntegrationService.Client";
    
    // Test project directory
    AbsolutePath TestsDirectory => RootDirectory / "AppifySheets.TBC.IntegrationService.Tests";
    
    // Output directory for build artifacts
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";
    
    // Output directory for test results
    AbsolutePath TestResultsDirectory => ArtifactsDirectory / "test-results";
    
    // Output directory for NuGet packages
    AbsolutePath PackagesDirectory => ArtifactsDirectory / "packages";

    // Version management - reads from VERSION file or uses Git tag
    string Version => GetVersion();

    string GetVersion()
    {
        // Priority 1: Git tag (for releases)
        if (GitHubActions.Instance?.EventName == "release")
        {
            var tag = GitHubActions.Instance?.Ref?.Replace("refs/tags/", "").TrimStart('v');
            if (!string.IsNullOrEmpty(tag))
            {
                Log.Information("Using version from Git tag: {Version}", tag);
                return tag;
            }
        }

        // Priority 2: VERSION file
        var versionFile = RootDirectory / "VERSION";
        if (File.Exists(versionFile))
        {
            var fileVersion = File.ReadAllText(versionFile).Trim();
            Log.Information("Using version from VERSION file: {Version}", fileVersion);
            return fileVersion;
        }

        // Fallback
        Log.Warning("No version source found, using default: 1.0.0");
        return "1.0.0";
    }

    // Clean build outputs and artifacts
    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            Log.Information("Cleaning build outputs and artifacts");
            
            // Clean solution build outputs
            DotNetClean(s => s
                .SetProject(Solution)
                .SetConfiguration(Configuration));
            
            // Clean artifacts directory
            ArtifactsDirectory.CreateOrCleanDirectory();
        });

    // Restore NuGet packages
    Target Restore => _ => _
        .Executes(() =>
        {
            Log.Information("Restoring NuGet packages");
            
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    // Compile the solution
    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            Log.Information("Building solution in {Configuration} configuration", Configuration);
            
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(Version)
                .SetFileVersion(Version)
                .SetInformationalVersion(Version)
                // Ensures deterministic builds for reproducibility
                .SetDeterministic(IsServerBuild)
                // Enables continuous integration build mode
                .SetContinuousIntegrationBuild(IsServerBuild)
                .EnableNoRestore());
        });

    // Run all unit tests
    // Tests MUST pass for the build to succeed
    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            Log.Information("Running unit tests");
            
            // Find all test projects
            var testProjects = Solution.AllProjects
                .Where(p => p.Name.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase));

            if (!testProjects.Any())
            {
                Log.Warning("No test projects found");
                return;
            }

            // Run tests for each test project
            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .SetNoBuild(true)
                .SetNoRestore(true)
                // Set results directory for test output
                .SetResultsDirectory(TestResultsDirectory)
                // Generate test results in TRX format for CI visibility
                .SetLoggers($"trx;LogFileName={TestsDirectory.Name}.trx")
                // Detailed output for test results
                .SetVerbosity(DotNetVerbosity.normal)
                .CombineWith(testProjects, (settings, project) => settings
                    .SetProjectFile(project)));

            Log.Information("All tests passed successfully!");
        });

    // Create NuGet packages
    Target Pack => _ => _
        .DependsOn(Compile)
        .Produces(PackagesDirectory / "*.nupkg")
        .Produces(PackagesDirectory / "*.snupkg")
        .Executes(() =>
        {
            Log.Information("Creating NuGet packages with version {Version}", Version);
            
            PackagesDirectory.CreateOrCleanDirectory();

            // Pack projects that are marked as packable
            var packableProjects = new[]
            {
                RootDirectory / "AppifySheets.TBC.IntegrationService.Client" / "AppifySheets.TBC.IntegrationService.Client.csproj",
                RootDirectory / "AppifySheets.Immutable.BankIntegrationTypes" / "AppifySheets.Immutable.BankIntegrationTypes.csproj"
            };

            DotNetPack(s => s
                .SetConfiguration(Configuration)
                .SetOutputDirectory(PackagesDirectory)
                .SetVersion(Version)
                // Include symbols for debugging
                .SetIncludeSymbols(true)
                .SetSymbolPackageFormat(DotNetSymbolPackageFormat.snupkg)
                // Include source for source link
                .SetIncludeSource(false)
                .SetNoBuild(true)
                .SetNoRestore(true)
                .CombineWith(packableProjects, (settings, project) => settings
                    .SetProject(project)));

            // List created packages
            var packages = PackagesDirectory.GlobFiles("*.nupkg", "*.snupkg");
            foreach (var package in packages)
            {
                Log.Information("Created package: {Package}", package.Name);
            }
        });

    // Publish NuGet packages to NuGet.org
    // Only runs on release builds with valid API key
    Target PublishToNuGet => _ => _
        .DependsOn(Pack)
        .Requires(() => !string.IsNullOrEmpty(NuGetApiKey))
        .Requires(() => IsServerBuild)
        .Executes(() =>
        {
            Log.Information("Publishing NuGet packages to NuGet.org");

            var packages = PackagesDirectory.GlobFiles("*.nupkg")
                .Where(x => !x.Name.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase));

            if (!packages.Any())
            {
                throw new Exception("No packages found to publish");
            }

            // Push each package to NuGet.org
            DotNetNuGetPush(s => s
                .SetSource("https://api.nuget.org/v3/index.json")
                .SetApiKey(NuGetApiKey)
                // Skip duplicate packages (in case of retry)
                .SetSkipDuplicate(true)
                .CombineWith(packages, (settings, package) => settings
                    .SetTargetPath(package)));

            Log.Information("Successfully published {Count} packages to NuGet.org", packages.Count());
        });

    // Publish NuGet packages to GitHub Packages
    // Publishes to GitHub Container Registry (ghcr.io)
    Target PublishToGitHub => _ => _
        .DependsOn(Pack)
        .Requires(() => IsServerBuild)
        .Executes(() =>
        {
            // GitHub token is provided automatically by GitHub Actions
            var token = GitHubToken ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            
            if (string.IsNullOrEmpty(token))
            {
                Log.Warning("GitHub token not available, skipping GitHub Packages publish");
                return;
            }

            Log.Information("Publishing NuGet packages to GitHub Packages");

            var packages = PackagesDirectory.GlobFiles("*.nupkg")
                .Where(x => !x.Name.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase));

            if (!packages.Any())
            {
                throw new Exception("No packages found to publish");
            }

            // GitHub Packages NuGet feed URL
            var githubSource = "https://nuget.pkg.github.com/AppifySheets/index.json";

            // Push each package to GitHub Packages
            DotNetNuGetPush(s => s
                .SetSource(githubSource)
                .SetApiKey(token)
                // Skip duplicate packages (in case of retry)
                .SetSkipDuplicate(true)
                .CombineWith(packages, (settings, package) => settings
                    .SetTargetPath(package)));

            Log.Information("Successfully published {Count} packages to GitHub Packages", packages.Count());
        });

    // Publish packages to all configured registries
    // Combines publishing to NuGet.org and GitHub Packages
    Target Publish => _ => _
        .DependsOn(Pack)
        .Triggers(PublishToNuGet, PublishToGitHub)
        .Executes(() =>
        {
            Log.Information("Publishing packages to all configured registries");
        });

    // Helper target to display version information
    Target ShowVersion => _ => _
        .Executes(() =>
        {
            Log.Information("Version: {Version}", Version);
            Log.Information("Configuration: {Configuration}", Configuration);
            Log.Information("IsLocalBuild: {IsLocalBuild}", IsLocalBuild);
            Log.Information("IsServerBuild: {IsServerBuild}", IsServerBuild);
        });

    // Check that version has been incremented compared to main branch
    // This ensures every PR includes a version bump
    Target CheckVersion => _ => _
        .Executes(() =>
        {
            // Only check version on PRs in CI environment
            if (!IsServerBuild || GitHubActions.Instance?.EventName != "pull_request")
            {
                Log.Information("Skipping version check - not a PR build");
                return;
            }

            Log.Information("Checking if VERSION has been incremented from main branch");
            
            // Get the current version from VERSION file
            var currentVersion = Version;
            Log.Information("Current version: {Version}", currentVersion);
            
            // Fetch main branch VERSION file content
            var mainVersionContent = string.Empty;
            try
            {
                // Fetch latest main branch
                ProcessTasks.StartProcess("git", "fetch origin main", logOutput: false)
                    .AssertZeroExitCode();
                
                // Get VERSION file content from main branch
                var result = ProcessTasks.StartProcess("git", "show origin/main:VERSION", logOutput: false);
                if (result.ExitCode == 0)
                {
                    mainVersionContent = string.Join("", result.Output).Trim();
                }
                else
                {
                    // VERSION file might not exist in main branch yet
                    Log.Warning("VERSION file not found in main branch - assuming this is the first version");
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Warning("Could not fetch main branch VERSION: {Message}", ex.Message);
                return;
            }
            
            Log.Information("Main branch version: {Version}", mainVersionContent);
            
            // Parse versions for comparison
            if (!TryParseVersion(currentVersion, out var current))
            {
                throw new Exception($"Invalid version format in VERSION file: {currentVersion}");
            }
            
            if (!TryParseVersion(mainVersionContent, out var main))
            {
                throw new Exception($"Invalid version format in main branch: {mainVersionContent}");
            }
            
            // Compare versions
            if (current <= main)
            {
                throw new Exception($"Version must be incremented! Current: {currentVersion}, Main: {mainVersionContent}");
            }
            
            Log.Information("✅ Version check passed: {Current} > {Main}", currentVersion, mainVersionContent);
        });
    
    bool TryParseVersion(string versionString, out Version version)
    {
        version = null;
        
        // Handle semantic versioning (strip pre-release and build metadata)
        var parts = versionString.Split('-', '+')[0].Split('.');
        
        if (parts.Length < 2 || parts.Length > 4)
            return false;
        
        // Parse major.minor[.build[.revision]]
        if (!int.TryParse(parts[0], out var major)) return false;
        if (!int.TryParse(parts[1], out var minor)) return false;
        
        var build = 0;
        var revision = 0;
        
        if (parts.Length > 2 && !int.TryParse(parts[2], out build)) return false;
        if (parts.Length > 3 && !int.TryParse(parts[3], out revision)) return false;
        
        version = new Version(major, minor, build, revision);
        return true;
    }
}