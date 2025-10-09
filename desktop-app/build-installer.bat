@echo off
echo Building Who Wants to be a Shillionaire Installer...

REM Check if NSIS is installed
where makensis >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo NSIS not found. Please install NSIS from https://nsis.sourceforge.io/
    echo Or use the portable executable instead.
    pause
    exit /b 1
)

REM Build the installer
makensis installer.nsi

if %ERRORLEVEL% EQU 0 (
    echo.
    echo Installer created successfully!
    echo File: WhoWantsToBeAShillionaire-Installer.exe
    echo.
) else (
    echo.
    echo Failed to create installer.
    echo.
)

pause
