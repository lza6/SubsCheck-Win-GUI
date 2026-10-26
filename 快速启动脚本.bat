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

rem 优先探测 Visual Studio 自带 MSBuild。
rem 注意：PATH 上的 Framework MSBuild（C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe）
rem 自带 csc 只支持到 C#5，无法编译本项目使用的 C#7 元组与 $ 字符串插值，会报 CS1056。
rem 因此必须优先使用 VS MSBuild，仅当 VS 全部缺失时才回退到 PATH msbuild。
set "MSBUILD_PATH="

set "VS18_COMMUNITY=C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS18_PRO=C:\Program Files\Microsoft Visual Studio\18\Professional\MSBuild\Current\Bin\MSBuild.exe"
set "VS18_ENTERPRISE=C:\Program Files\Microsoft Visual Studio\18\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
set "VS18_BUILDTOOLS=C:\Program Files (x86)\Microsoft Visual Studio\18\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_COMMUNITY=C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_PRO=C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_ENTERPRISE=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_BUILDTOOLS=C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
set "VS2022_COMMUNITY_X86=C:\Program Files (x86)\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS2019_COMMUNITY=C:\Program Files\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
set "VS2019_BUILDTOOLS=C:\Program Files (x86)\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"

if exist "%VS18_COMMUNITY%" (
    set "MSBUILD_PATH=%VS18_COMMUNITY%"
    goto :build
)
if exist "%VS18_PRO%" (
    set "MSBUILD_PATH=%VS18_PRO%"
    goto :build
)
if exist "%VS18_ENTERPRISE%" (
    set "MSBUILD_PATH=%VS18_ENTERPRISE%"
    goto :build
)
if exist "%VS18_BUILDTOOLS%" (
    set "MSBUILD_PATH=%VS18_BUILDTOOLS%"
    goto :build
)
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

rem VS 全部缺失时，最后回退到 PATH 上的 msbuild。
rem 注意：Framework MSBuild（v4.0.30319）的 csc 只支持 C#5，无法编译本项目（C#7 语法），
rem 命中时显式报错提示安装 VS，而不是让编译进行到一半才失败。
where msbuild.exe >nul 2>&1
if %errorlevel% equ 0 (
    for /f "delims=" %%i in ('where msbuild.exe 2^>nul') do (
        echo %%i | findstr /i "v4.0.30319" >nul
        if errorlevel 1 (
            set "MSBUILD_PATH=%%i"
            goto :build
        )
    )
    echo Error: PATH 上的 msbuild 是 Framework 版（仅支持 C#5，无法编译本项目）。
    echo Please install Visual Studio 2022 or later.
    pause
    exit /b 1
)

echo Error: MSBuild not found. Please install Visual Studio 2022 or later.
pause
exit /b 1

:build
echo Success: Found MSBuild
echo.

echo Step 2/4: Restoring NuGet packages...

rem 使用脚本所在目录的绝对路径，避免 PATH/codepage 下裸文件名解析失败
set "NUGET_EXE=%~dp0nuget.exe"
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