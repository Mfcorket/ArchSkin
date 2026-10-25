# ArchSkin Test Script
param(
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    [switch]$Coverage
)

$ErrorActionPreference = "Stop"
$src = Join-Path (Split-Path -Parent $PSScriptRoot) "src"

if ($Coverage) {
    dotnet test (Join-Path $src "ArchSkin.Tests\ArchSkin.Tests.csproj") `
        -c $Configuration --collect:"XPlat Code Coverage"
} else {
    dotnet test (Join-Path $src "ArchSkin.Tests\ArchSkin.Tests.csproj") `
        -c $Configuration --verbosity normal
}
