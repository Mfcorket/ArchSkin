# ArchSkin Install Script - Installs ArchSkin and sets up autostart
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$serviceExe = Join-Path $root "src\ArchSkin.Service\bin\Release\net10.0-windows\win-x64\publish\ArchSkin.Service.exe"

if (-not (Test-Path $serviceExe)) {
    Write-Host "ERROR: ArchSkin.Service.exe not found. Run build.ps1 first." -ForegroundColor Red
    exit 1
}

Write-Host "Installing ArchSkin..." -ForegroundColor Cyan
& $serviceExe install

if ($LASTEXITCODE -eq 0) {
    Write-Host "ArchSkin installed successfully!" -ForegroundColor Green
    Write-Host "The shell will start automatically on next login."
} else {
    Write-Host "Installation failed." -ForegroundColor Red
    exit 1
}
