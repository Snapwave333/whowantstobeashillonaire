# Installation Guide - Who Wants to Be a Shillionaire?

This guide will help you get the game running on your Windows system.

## Quick Start (Recommended)

### Option 1: Install .NET SDK (Easiest)
1. Download .NET 6.0 SDK from: https://dotnet.microsoft.com/download
2. Install the SDK (follow the installer prompts)
3. Open Command Prompt or PowerShell in the game folder
4. Run: `dotnet run`

### Option 2: Use Visual Studio Code (Free)
1. Download Visual Studio Code from: https://code.visualstudio.com/
2. Install the C# extension in VS Code
3. Open the game folder in VS Code
4. Press F5 to run the application

### Option 3: Use Visual Studio Community (Free, Full IDE)
1. Download Visual Studio Community from: https://visualstudio.microsoft.com/vs/community/
2. During installation, select ".NET desktop development" workload
3. Open the `.csproj` file in Visual Studio
4. Press F5 to run the application

## Alternative Methods

### If you have .NET Framework 4.8
The game can potentially run with just .NET Framework 4.8 (usually pre-installed on Windows 10/11):
1. Try running `compile.bat` (may require manual dependency installation)
2. This method is less reliable and not recommended

### Pre-built Executable
If available, you can download a pre-built executable that includes all dependencies.

## System Requirements

- **Operating System**: Windows 10 or later
- **Memory**: 512 MB RAM minimum
- **Storage**: 50 MB free space
- **Audio**: Sound card for audio effects (optional)

## Troubleshooting

### "dotnet command not found"
- Install .NET SDK from the link above
- Restart your command prompt/PowerShell after installation

### Build Errors
- Ensure you have internet connection (for NuGet package downloads)
- Try running `dotnet restore` first, then `dotnet build`

### Runtime Errors
- Make sure you have write permissions in the game folder (for database creation)
- Check that your antivirus isn't blocking the application

### Audio Issues
- Audio is optional - the game will work without sound
- Ensure your system has audio drivers installed

## Game Features

Once installed, you'll have access to:
- ✅ Complete offline gameplay
- ✅ Local SQLite database
- ✅ Sound effects
- ✅ Admin panel for question management
- ✅ All classic lifelines (50:50, Phone a Friend, Ask the Audience)
- ✅ Prize ladder with safe havens
- ✅ Professional Windows UI

## Getting Help

If you encounter issues:
1. Check this troubleshooting section
2. Ensure your system meets the requirements
3. Try the different installation options above
4. Make sure Windows is up to date

## Development

For developers who want to modify the game:
- The project uses C# with WPF
- SQLite for local database
- MVVM pattern for UI binding
- All source code is included and commented