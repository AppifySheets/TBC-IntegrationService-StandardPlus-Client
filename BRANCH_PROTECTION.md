# Branch Protection Configuration

To ensure code quality and that all tests pass before merging pull requests, configure the following branch protection rules on GitHub for the `main` branch:

## Setting up Branch Protection

1. Go to your repository on GitHub: https://github.com/AppifySheets/TBC-IntegrationService-StandardPlus-Client
2. Navigate to **Settings** → **Branches**
3. Click **Add rule** under "Branch protection rules"
4. Enter `main` as the branch name pattern

## Required Settings

### ✅ Require pull request reviews before merging
- **Required approving reviews:** 1 (or more based on team size)
- **Dismiss stale pull request approvals when new commits are pushed:** ✓
- **Require review from CODEOWNERS:** (optional, if you have a CODEOWNERS file)

### ✅ Require status checks to pass before merging
- **Require branches to be up to date before merging:** ✓
- **Status checks that are required:**
  - `pr-validation / ubuntu-latest` (from our Nuke CI pipeline)
  - `pr-validation / windows-latest` (from our Nuke CI pipeline)

### ✅ Require conversation resolution before merging
This ensures all PR comments are addressed before merge.

### ✅ Include administrators
This ensures even repository admins must follow the same rules.

### ✅ Restrict who can push to matching branches (Optional)
Configure this if you want to limit direct pushes to main branch.

## Additional Recommended Settings

- **Require signed commits:** (optional but recommended for security)
- **Require linear history:** (optional, prevents merge commits)
- **Require deployments to succeed before merge:** (if you have deployment workflows)

## Workflow Summary

With these settings in place:

1. All changes must go through a pull request
2. The Nuke build pipeline (`pr-validation` workflow) automatically runs on every PR
3. All tests must pass (executed via `dotnet test` in the Nuke build)
4. At least one approval is required
5. The PR cannot be merged until all checks pass

## NuGet Package Publishing

When you create a new GitHub Release:
1. Tag the release with a version (e.g., `v1.1.0`)
2. The `release` workflow automatically triggers
3. Nuke builds and packs the libraries with the tag version
4. Packages are published to NuGet.org using the `NUGET_API_KEY` secret

## Required Repository Secrets

Make sure to configure the following secret in **Settings** → **Secrets and variables** → **Actions**:
- `NUGET_API_KEY`: Your NuGet.org API key for package publishing

*Collaboration by Claude*