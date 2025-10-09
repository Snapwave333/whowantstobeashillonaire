@echo off
echo Compiling Who Wants to Be a Shillionaire with .NET Framework...

REM Set up paths for .NET Framework
set "DOTNET_PATH=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319"
if not exist "%DOTNET_PATH%" (
    set "DOTNET_PATH=%WINDIR%\Microsoft.NET\Framework\v4.0.30319"
)

if not exist "%DOTNET_PATH%\csc.exe" (
    echo ERROR: .NET Framework compiler not found
    echo Please ensure .NET Framework 4.0 or later is installed
    pause
    exit /b 1
)

echo Using .NET Framework compiler at: %DOTNET_PATH%

REM Create output directory
if not exist "bin" mkdir bin
if not exist "bin\Release" mkdir bin\Release

REM Compile the application with AI integration
"%DOTNET_PATH%\csc.exe" ^
    /target:winexe ^
    /out:bin\Release\WhoWantsToBeAShillionaire.exe ^
    /reference:PresentationCore.dll ^
    /reference:PresentationFramework.dll ^
    /reference:WindowsBase.dll ^
    /reference:System.dll ^
    /reference:System.Core.dll ^
    /reference:System.Data.dll ^
    /reference:System.Data.SQLite.dll ^
    /reference:System.Windows.Forms.dll ^
    /reference:System.Xaml.dll ^
    /reference:Newtonsoft.Json.dll ^
    /reference:System.Net.Http.dll ^
    App.xaml.cs ^
    MainWindow.xaml.cs ^
    Models\Question.cs ^
    Models\AppSettings.cs ^
    Services\DatabaseService.cs ^
    Services\AudioService.cs ^
    Services\GameLogic.cs ^
    Services\SettingsService.cs ^
    Services\AIService.cs ^
    Views\SettingsWindow.xaml.cs ^
    ViewModels\MainViewModel.cs

if %errorlevel% == 0 (
    echo.
    echo Build successful! Executable created at: bin\Release\WhoWantsToBeAShillionaire.exe
    echo.
    echo AI Integration Features Added:
    echo - AI-powered question generation
    echo - Multiple AI provider support (OpenAI, Anthropic, Custom)
    echo - Secure API key storage
    echo - Advanced settings configuration
    echo - Connection testing functionality
    echo.
    echo Note: You may need to install the following NuGet packages manually:
    echo - System.Data.SQLite
    echo - Newtonsoft.Json
    echo - System.Text.Json
    echo - Microsoft.Extensions.Http
    echo.
    echo Or run the application from Visual Studio/Visual Studio Code
) else (
    echo.
    echo Build failed. This is expected as we need NuGet packages.
    echo Please use Visual Studio, Visual Studio Code, or install .NET SDK
)

pause