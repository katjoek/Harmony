# PowerShell script to build the Harmony installer
# This script publishes the application and then builds the NSIS installer

param(
    [string]$Configuration = "Release",
    [string]$ProjectPath = "..\src\Harmony.Web\Harmony.Web.csproj",
    [string]$PublishPath = "..\src\Harmony.Web\bin\Release\net9.0\win-x64\publish",
    [string]$NsiFile = "Harmony.nsi"
)

Write-Host "Building Harmony Installer..." -ForegroundColor Green
Write-Host ""

# Check if NSIS is installed
$nsisPath = Get-Command makensis -ErrorAction SilentlyContinue
if (-not $nsisPath) {
    Write-Host "ERROR: NSIS (makensis) is not found in PATH." -ForegroundColor Red
    Write-Host "Please install NSIS and ensure makensis.exe is in your PATH." -ForegroundColor Yellow
    Write-Host "Download NSIS from: https://nsis.sourceforge.io/Download" -ForegroundColor Yellow
    exit 1
}

Write-Host "Step 1: Publishing Harmony.Web application..." -ForegroundColor Cyan
dotnet publish $ProjectPath -c $Configuration -r win-x64 --self-contained true

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to publish the application." -ForegroundColor Red
    exit 1
}

Write-Host "Publish completed successfully." -ForegroundColor Green
Write-Host ""

# Verify publish output exists
if (-not (Test-Path $PublishPath)) {
    Write-Host "ERROR: Publish output not found at: $PublishPath" -ForegroundColor Red
    exit 1
}

# Extract version from Directory.Build.props
$buildPropsPath = "..\Directory.Build.props"
if (-not (Test-Path $buildPropsPath)) {
    Write-Host "ERROR: Directory.Build.props not found at: $buildPropsPath" -ForegroundColor Red
    exit 1
}

$buildProps = [xml](Get-Content $buildPropsPath)
# Find Version in any PropertyGroup
$version = $null
foreach ($propertyGroup in $buildProps.Project.PropertyGroup) {
    if ($propertyGroup.Version) {
        $version = $propertyGroup.Version
        break
    }
}

if (-not $version) {
    Write-Host "WARNING: Version not found in Directory.Build.props, using default '1.0.0'" -ForegroundColor Yellow
    $version = "1.0.0"
}

Write-Host "Detected version: $version" -ForegroundColor Cyan
Write-Host ""

Write-Host "Step 2: Building NSIS installer..." -ForegroundColor Cyan
makensis /DVERSION="$version" $NsiFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Failed to build the installer." -ForegroundColor Red
    exit 1
}

$installerName = "Harmony-Setup-$version.exe"
Write-Host ""
Write-Host "Installer built successfully!" -ForegroundColor Green
if (Test-Path $installerName) {
    Write-Host "Installer location: $(Resolve-Path $installerName)" -ForegroundColor Green
} else {
    Write-Host "WARNING: Expected installer file '$installerName' not found. Checking for Harmony-Setup.exe..." -ForegroundColor Yellow
    if (Test-Path "Harmony-Setup.exe") {
        Write-Host "Installer location: $(Resolve-Path 'Harmony-Setup.exe')" -ForegroundColor Green
    }
}

