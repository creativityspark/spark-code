---
name: sparkcode-api-package-deploy
description: 'Build, package, and deploy SparkCode.API to Dataverse using SparkCode.APIRegistrationTool. Use when asked to deploy custom APIs, generate the nupkg, prepare environment before unit tests or integration tests, or run API registration in Debug/Release.'
argument-hint: 'configuration: Debug|Release (default Debug)'
user-invocable: true
---

# SparkCode API Package And Deploy

## When To Use
- Before running unit tests or integration tests in this repository.
- When SparkCode.API changes must be packaged and deployed to Dataverse.
- When you need to run deployment in Debug or Release mode.

## Preconditions
- Environment variable DATAVERSE_CONNECTION_STRING_SPARK_CODE is set for the current user or process.
- .NET SDK is installed and available in PATH.

## Required Procedure
Run the script below from the repository root with the requested configuration:

- [scripts/build-and-deploy.ps1](./scripts/build-and-deploy.ps1)

The procedure performs these required steps in order:
1. Clean SparkCode.API project outputs.
2. Rebuild SparkCode.API in Debug or Release.
3. Rebuild SparkCode.APIRegistrationTool in the same mode.
4. Run SparkCode.APIRegistrationTool.
5. Validate the run has no errors and report the generated package path.

## Commands
- Debug: pwsh -ExecutionPolicy Bypass -File ./.github/skills/sparkcode-api-package-deploy/scripts/build-and-deploy.ps1 -Configuration Debug
- Release: pwsh -ExecutionPolicy Bypass -File ./.github/skills/sparkcode-api-package-deploy/scripts/build-and-deploy.ps1 -Configuration Release

## Notes
- Use the same configuration for both projects so SparkCode.APIRegistrationTool resolves the correct SparkCode.API output folder.
- If deployment fails, stop and surface the exact error output instead of continuing to tests.
