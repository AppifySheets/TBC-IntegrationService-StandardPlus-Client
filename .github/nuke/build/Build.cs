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
// Runs on all pull requests to ensure code quality
// Currently only compiles due to integration test requirements
[GitHubActions(
    "pr-validation",
    GitHubActionsImage.UbuntuLatest,
    On = [GitHubActionsTrigger.PullRequest],
    InvokedTargets = [nameof(Compile)],
    EnableGitHubToken = true,
    PublishArtifacts = true,
    CacheKeyFiles = ["global.json", "**/*.csproj", "**/Directory.*.props"])]
// GitHub Actions workflow for releases
// Triggered when a new release is created, publishes NuGet packages
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
    Target Publish => _ => _
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

    // Helper target to display version information
    Target ShowVersion => _ => _
        .Executes(() =>
        {
            Log.Information("Version: {Version}", Version);
            Log.Information("Configuration: {Configuration}", Configuration);
            Log.Information("IsLocalBuild: {IsLocalBuild}", IsLocalBuild);
            Log.Information("IsServerBuild: {IsServerBuild}", IsServerBuild);
        });
}