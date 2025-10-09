@echo off
echo Building Who Wants to Be a Shillionaire...

REM Check if .NET SDK is available
dotnet --version >nul 2>&1
if %errorlevel% == 0 (
    echo Using .NET CLI...
    dotnet restore
    dotnet build --configuration Release
    if %errorlevel% == 0 (
        echo Build successful! Run with: dotnet run
        goto :end
    )
)

REM Try to find MSBuild
set "MSBUILD_PATH="
for /f "delims=" %%i in ('where msbuild 2^>nul') do set "MSBUILD_PATH=%%i"

if not "%MSBUILD_PATH%"=="" (
    echo Using MSBuild...
    "%MSBUILD_PATH%" WhoWantsToBeAShillionaire.csproj /p:Configuration=Release
    if %errorlevel% == 0 (
        echo Build successful!
        goto :end
    )
)

REM Try Visual Studio Build Tools paths
set "VS_PATHS=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
set "VS_PATHS=%VS_PATHS%;C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
set "VS_PATHS=%VS_PATHS%;C:\Program Files\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS_PATHS=%VS_PATHS%;C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"

for %%p in ("%VS_PATHS:;=" "%") do (
    if exist %%p (
        echo Using Visual Studio MSBuild...
        %%p WhoWantsToBeAShillionaire.csproj /p:Configuration=Release
        if %errorlevel% == 0 (
            echo Build successful!
            goto :end
        )
    )
)

echo ERROR: Could not find .NET SDK, MSBuild, or Visual Studio Build Tools
echo Please install one of the following:
echo - .NET 6.0 SDK or later
echo - Visual Studio 2019/2022 with C# workload
echo - Visual Studio Build Tools
echo.
echo You can download .NET SDK from: https://dotnet.microsoft.com/download

:end
pause