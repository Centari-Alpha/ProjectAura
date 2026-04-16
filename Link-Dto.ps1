# Script to link Aura.Core DTOs into the Unity Project
$ErrorActionPreference = "Stop"

$CoreDtoPath = Resolve-Path ".\Aura.Core\DTOs" -ErrorAction SilentlyContinue
$UnityPluginsPath = ".\Aura.Unity\Assets\Plugins"
$SymlinkPath = "$UnityPluginsPath\Aura.Shared"

if (-not $CoreDtoPath) {
    Write-Host "Error: Aura.Core\DTOs directory not found." -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $UnityPluginsPath)) {
    New-Item -ItemType Directory -Force -Path $UnityPluginsPath | Out-Null
}

if (Test-Path $SymlinkPath) {
    Write-Host "Symlink already exists. Removing the old one..." -ForegroundColor Yellow
    Remove-Item -Path $SymlinkPath -Force -Recurse
}

Write-Host "Creating symlink from $CoreDtoPath to $SymlinkPath" -ForegroundColor Cyan
New-Item -ItemType Junction -Path $SymlinkPath -Target $CoreDtoPath.Path | Out-Null

Write-Host "Successfully linked Aura.Core DTOs to Unity!" -ForegroundColor Green
