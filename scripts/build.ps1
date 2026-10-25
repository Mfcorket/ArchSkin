# ArchSkin Build Script
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$src = Join-Path $root "src"
$artifacts = Join-Path $root "artifacts"
$installer = Join-Path $root "installer"

Write-Host "=== ArchSkin Build ===" -ForegroundColor Cyan
Write-Host "Configuration: $Configuration"
Write-Host "Version: $Version"

# Clean artifacts
if (Test-Path $artifacts) { Remove-Item $artifacts -Recurse -Force }
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null

# Build solution
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
dotnet restore (Join-Path $src "ArchSkin.slnx")
if ($LASTEXITCODE -ne 0) { throw "Restore failed" }

dotnet build (Join-Path $src "ArchSkin.slnx") -c $Configuration
if ($LASTEXITCODE -ne 0) { throw "Build failed" }

# Publish Shell
Write-Host "`nPublishing ArchSkin.Shell..." -ForegroundColor Yellow
dotnet publish (Join-Path $src "ArchSkin.Shell\ArchSkin.Shell.csproj") `
    -c $Configuration -r win-x64 --self-contained `
    -o (Join-Path $src "ArchSkin.Shell\bin\$Configuration\net10.0-windows10.0.19041.0\win-x64\publish")
if ($LASTEXITCODE -ne 0) { throw "Shell publish failed" }

# Publish Settings
Write-Host "`nPublishing ArchSkin.Settings..." -ForegroundColor Yellow
dotnet publish (Join-Path $src "ArchSkin.Settings\ArchSkin.Settings.csproj") `
    -c $Configuration -r win-x64 --self-contained `
    -o (Join-Path $src "ArchSkin.Settings\bin\$Configuration\net10.0-windows10.0.19041.0\win-x64\publish")
if ($LASTEXITCODE -ne 0) { throw "Settings publish failed" }

# Publish Service
Write-Host "`nPublishing ArchSkin.Service..." -ForegroundColor Yellow
dotnet publish (Join-Path $src "ArchSkin.Service\ArchSkin.Service.csproj") `
    -c $Configuration -r win-x64 --self-contained `
    -o (Join-Path $src "ArchSkin.Service\bin\$Configuration\net10.0-windows\win-x64\publish")
if ($LASTEXITCODE -ne 0) { throw "Service publish failed" }

# Publish ThemeSync
Write-Host "`nPublishing ArchSkin.ThemeSync..." -ForegroundColor Yellow
dotnet publish (Join-Path $src "ArchSkin.ThemeSync\ArchSkin.ThemeSync.csproj") `
    -c $Configuration -r win-x64 --self-contained `
    -o (Join-Path $src "ArchSkin.ThemeSync\bin\$Configuration\net10.0-windows\win-x64\publish")
if ($LASTEXITCODE -ne 0) { throw "ThemeSync publish failed" }

# Run tests
Write-Host "`nRunning tests..." -ForegroundColor Yellow
dotnet test (Join-Path $src "ArchSkin.Tests\ArchSkin.Tests.csproj") -c $Configuration
if ($LASTEXITCODE -ne 0) { Write-Host "WARNING: Tests failed" -ForegroundColor Red }

# Build installer (if Inno Setup is available)
$isetup = Join-Path $installer "setup.iss"
if (Test-Path $isetup) {
    $iscc = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    if (Test-Path $iscc) {
        Write-Host "`nBuilding installer..." -ForegroundColor Yellow
        & $iscc "/dMyAppVersion=$Version" $isetup
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Installer built successfully." -ForegroundColor Green
        }
    } else {
        Write-Host "Inno Setup not found. Skipping installer build." -ForegroundColor Yellow
    }
}

Write-Host "`n=== Build Complete ===" -ForegroundColor Green
Write-Host "Artifacts: $artifacts"
