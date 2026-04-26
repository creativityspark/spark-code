param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Resolve-Path (Join-Path $scriptRoot '..\..\..\..')

$apiProject = Join-Path $repoRoot 'src\assemblies\SparkCode.API\SparkCode.API.csproj'
$registrationToolProject = Join-Path $repoRoot 'src\assemblies\SparkCode.APIRegistrationTool\SparkCode.APIRegistrationTool.csproj'
$apiProjectDir = Split-Path -Parent $apiProject
$expectedPackage = Join-Path $apiProjectDir ("bin\{0}\SparkCode.API.1.0.0.nupkg" -f $Configuration)

if (-not (Test-Path $apiProject)) {
    throw "SparkCode.API project not found at: $apiProject"
}

if (-not (Test-Path $registrationToolProject)) {
    throw "SparkCode.APIRegistrationTool project not found at: $registrationToolProject"
}

if ([string]::IsNullOrWhiteSpace($env:DATAVERSE_CONNECTION_STRING_SPARK_CODE)) {
    throw 'Environment variable DATAVERSE_CONNECTION_STRING_SPARK_CODE is not set. Deployment cannot continue.'
}

Write-Host "[1/5] Cleaning SparkCode.API outputs ($Configuration)"
$apiBinDir = Join-Path $apiProjectDir 'bin'
$apiObjDir = Join-Path $apiProjectDir 'obj'
if (Test-Path $apiBinDir) {
    Remove-Item -Path $apiBinDir -Recurse -Force
}
if (Test-Path $apiObjDir) {
    Remove-Item -Path $apiObjDir -Recurse -Force
}
& dotnet clean $apiProject -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet clean failed for SparkCode.API ($Configuration)."
}

Write-Host "[2/5] Rebuilding SparkCode.API ($Configuration)"
& dotnet build $apiProject -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for SparkCode.API ($Configuration)."
}

Write-Host "[3/5] Rebuilding SparkCode.APIRegistrationTool ($Configuration)"
& dotnet build $registrationToolProject -c $Configuration
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed for SparkCode.APIRegistrationTool ($Configuration)."
}

Write-Host "[4/5] Running SparkCode.APIRegistrationTool ($Configuration)"
$runOutput = & dotnet run --project $registrationToolProject -c $Configuration --no-build 2>&1
$runOutput | ForEach-Object { Write-Host $_ }
if ($LASTEXITCODE -ne 0) {
    throw "SparkCode.APIRegistrationTool returned non-zero exit code: $LASTEXITCODE"
}

Write-Host '[5/5] Verifying deployment output and package generation'
$outputText = ($runOutput | Out-String)
if ($outputText -match '(?i)\berror\b|\bexception\b|\bfailed\b') {
    throw 'Registration tool output contains an error indicator (error/exception/failed). Review logs above.'
}

$package = Get-Item -Path $expectedPackage -ErrorAction SilentlyContinue
if (-not $package) {
    $package = Get-ChildItem -Path (Join-Path $apiProjectDir ("bin\{0}" -f $Configuration)) -Filter 'SparkCode.API.*.nupkg' -Recurse -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
}

if (-not $package) {
    throw "No SparkCode.API nupkg found under bin\\$Configuration"
}

Write-Host "Deployment completed successfully."
Write-Host ("NuGet package: {0}" -f $package.FullName)
