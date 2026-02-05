@echo off
echo ========================================
echo   SubsCheck Win GUI - Quick Start
echo ========================================
echo.

set "PROJECT_FILE=subs-check.win.gui.csproj"
set "OUTPUT_DIR=bin\Debug"
set "EXE_NAME=subs-check.win.gui.exe"

if not exist "%PROJECT_FILE%" (
    echo Error: Project file not found
    pause
    exit /b 1
)

echo Step 1/3: Finding MSBuild...

where msbuild.exe >nul 2>&1
if %errorlevel% equ 0 (
    echo Success: MSBuild found in PATH
    goto :build
)

echo Checking Visual Studio installations...
set "VS2022_COMMUNITY=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_PRO=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_ENTERPRISE=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_BUILDTOOLS=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_COMMUNITY_X86=C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS2019_COMMUNITY=C:\Program Files\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS2019_BUILDTOOLS=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"

if exist "%VS2022_COMMUNITY%" (
    set "MSBUILD_PATH=%VS2022_COMMUNITY%"
    goto :build
)
if exist "%VS2022_PRO%" (
    set "MSBUILD_PATH=%VS2022_PRO%"
    goto :build
)
if exist "%VS2022_ENTERPRISE%" (
    set "MSBUILD_PATH=%VS2022_ENTERPRISE%"
    goto :build
)
if exist "%VS2022_BUILDTOOLS%" (
    set "MSBUILD_PATH=%VS2022_BUILDTOOLS%"
    goto :build
)
if exist "%VS2022_COMMUNITY_X86%" (
    set "MSBUILD_PATH=%VS2022_COMMUNITY_X86%"
    goto :build
)
if exist "%VS2019_COMMUNITY%" (
    set "MSBUILD_PATH=%VS2019_COMMUNITY%"
    goto :build
)
if exist "%VS2019_BUILDTOOLS%" (
    set "MSBUILD_PATH=%VS2019_BUILDTOOLS%"
    goto :build
)

echo Error: MSBuild not found. Please install Visual Studio or add MSBuild to PATH.
pause
exit /b 1

:build
echo Success: Found MSBuild
echo.

echo Step 2/4: Restoring NuGet packages...

set "NUGET_EXE=nuget.exe"
if not exist "%NUGET_EXE%" (
    echo Downloading NuGet.exe...
    powershell -Command "Invoke-WebRequest -Uri 'https://dist.nuget.org/win-x86-commandline/latest/nuget.exe' -OutFile '%NUGET_EXE%'"
    if errorlevel 1 (
        echo Error: Failed to download NuGet.exe
        pause
        exit /b 1
    )
)

"%NUGET_EXE%" restore "%PROJECT_FILE%" -PackagesDirectory packages

if errorlevel 1 (
    echo.
    echo Error: Package restore failed
    pause
    exit /b 1
)

echo Success: Packages restored
echo.

echo Step 3/4: Building project...
"%MSBUILD_PATH%" "%PROJECT_FILE%" /t:Build /p:Configuration=Debug /v:minimal /nologo

if errorlevel 1 (
    echo.
    echo Error: Build failed
    pause
    exit /b 1
)

echo Success: Build completed
echo.

if not exist "%OUTPUT_DIR%\%EXE_NAME%" (
    echo Error: Output file not found
    pause
    exit /b 1
)

echo Step 4/4: Starting application...
echo.
start "" "%OUTPUT_DIR%\%EXE_NAME%"

echo Done: Application started
pause