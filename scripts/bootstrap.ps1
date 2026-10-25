# ArchSkin Bootstrap Script - First-time setup
$ErrorActionPreference = "Stop"

Write-Host "=== ArchSkin Development Setup ===" -ForegroundColor Cyan

# Check prerequisites
$dotnet = dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: .NET SDK not found. Install .NET 8+ SDK." -ForegroundColor Red
    exit 1
}
Write-Host ".NET SDK: $dotnet"

$git = git --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "WARNING: Git not found." -ForegroundColor Yellow
} else {
    Write-Host "Git: $git"
}

# Restore packages
Write-Host "`nRestoring packages..." -ForegroundColor Yellow
$src = Join-Path (Split-Path -Parent $PSScriptRoot) "src"
dotnet restore (Join-Path $src "ArchSkin.slnx")

# Build
Write-Host "`nBuilding solution..." -ForegroundColor Yellow
dotnet build (Join-Path $src "ArchSkin.slnx") -c Debug

Write-Host "`n=== Bootstrap Complete ===" -ForegroundColor Green
Write-Host "Open ArchSkin.slnx in Visual Studio or run 'dotnet run' to start."
