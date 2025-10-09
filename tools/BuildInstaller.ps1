param(
    [string]$SolutionRoot = (Resolve-Path ".").Path,
    [string]$Desktop = [Environment]::GetFolderPath('Desktop'),
    [string]$Runtime = 'win-x64'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Run($cmd, $args){
    Write-Host "› $cmd $args" -ForegroundColor Cyan
    & $cmd $args
}

# Generate icons fresh every build — no placeholders allowed
Run pwsh "-NoProfile -ExecutionPolicy Bypass -File tools/GenerateIcon.ps1"

# Publish WPF app as self-contained single-file
Run dotnet "publish WhoWantsToBeAShillionaire/WhoWantsToBeAShillionaire.csproj -c Release -r $Runtime --self-contained true /p:PublishSingleFile=true /p:DebugType=none"

$publishDir = Join-Path $SolutionRoot "WhoWantsToBeAShillionaire/bin/Release/net8.0-windows/$Runtime/publish"
if(-not (Test-Path $publishDir)){ throw "Publish directory not found: $publishDir" }

# Zip payload for installer
$payloadDir = Join-Path $SolutionRoot "Installer/Payload"
if(-not (Test-Path $payloadDir)){ New-Item -ItemType Directory -Path $payloadDir | Out-Null }
$zipPath = Join-Path $payloadDir "app.zip"
if(Test-Path $zipPath){ Remove-Item $zipPath -Force }

Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($publishDir, $zipPath)

# Build Installer project
Run dotnet "build Installer/Installer.csproj -c Release"
Run dotnet "publish Installer/Installer.csproj -c Release -r $Runtime --self-contained true /p:PublishSingleFile=true /p:DebugType=none"

$installerExe = Join-Path $SolutionRoot "Installer/bin/Release/net8.0-windows/$Runtime/publish/Installer.exe"
if(-not (Test-Path $installerExe)){ throw "Installer exe not found: $installerExe" }

# Copy installer to Desktop
$dest = Join-Path $Desktop "Shillionaire-Setup.exe"
Copy-Item $installerExe $dest -Force
Write-Host "Installer ready: $dest" -ForegroundColor Green