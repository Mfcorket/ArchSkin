# ArchSkin Uninstall Script - Removes ArchSkin and restores settings
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$serviceExe = Join-Path $root "src\ArchSkin.Service\bin\Release\net10.0-windows\win-x64\publish\ArchSkin.Service.exe"

if (-not (Test-Path $serviceExe)) {
    Write-Host "ERROR: ArchSkin.Service.exe not found." -ForegroundColor Red
    exit 1
}

Write-Host "Uninstalling ArchSkin..." -ForegroundColor Cyan
& $serviceExe uninstall

if ($LASTEXITCODE -eq 0) {
    Write-Host "ArchSkin uninstalled successfully!" -ForegroundColor Green
    Write-Host "Desktop icons have been restored."
} else {
    Write-Host "Uninstallation failed." -ForegroundColor Red
    exit 1
}
