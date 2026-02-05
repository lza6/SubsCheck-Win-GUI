@echo off
setlocal enabledelayedexpansion

title SubsCheck Win GUI - Environment Cleanup Tool

echo ========================================
echo   SubsCheck Win GUI - Cleanup Tool
echo ========================================
echo.

:menu
echo Please select an operation:
echo.
echo [1] Full Cleanup (build outputs, config, cache)
echo [2] Cleanup Build Outputs Only (bin and obj)
echo [3] Cleanup Config Files Only
echo [4] Cleanup Temporary Files Only
echo [5] Reset to Default Config
echo [6] Exit
echo.
set /p choice=Enter option (1-6):

if "%choice%"=="1" goto full_clean
if "%choice%"=="2" goto clean_build
if "%choice%"=="3" goto clean_config
if "%choice%"=="4" goto clean_temp
if "%choice%"=="5" goto reset_config
if "%choice%"=="6" goto end

echo.
echo Invalid option, please try again!
echo.
goto menu

:full_clean
echo.
echo ========================================
echo   Performing Full Cleanup...
echo ========================================
echo.

call :clean_build
call :clean_config
call :clean_temp

echo.
echo ========================================
echo   Full Cleanup Complete!
echo ========================================
echo.
pause
goto menu

:clean_build
echo.
echo [1/3] Cleaning build output directories...

if exist "bin\Debug" (
    echo Deleting bin\Debug directory...
    rd /s /q "bin\Debug" 2>nul
    echo bin\Debug deleted
)

if exist "bin\Release" (
    echo Deleting bin\Release directory...
    rd /s /q "bin\Release" 2>nul
    echo bin\Release deleted
)

if exist "obj\Debug" (
    echo Deleting obj\Debug directory...
    rd /s /q "obj\Debug" 2>nul
    echo obj\Debug deleted
)

if exist "obj\Release" (
    echo Deleting obj\Release directory...
    rd /s /q "obj\Release" 2>nul
    echo obj\Release deleted
)

if exist "build.log" (
    echo Deleting build.log...
    del /f /q "build.log" 2>nul
    echo build.log deleted
)

echo Build output cleanup complete
goto :eof

:clean_config
echo.
echo [2/3] Cleaning config files...

if exist "config\config.yaml" (
    echo Backing up config.yaml...
    copy "config\config.yaml" "config\config.yaml.backup" >nul 2>&1
    echo Deleting config.yaml...
    del /f /q "config\config.yaml" 2>nul
    echo config.yaml deleted (backup: config.yaml.backup)
)

if exist "config\more.yaml" (
    echo Deleting more.yaml...
    del /f /q "config\more.yaml" 2>nul
    echo more.yaml deleted
)

echo Config files cleanup complete
goto :eof

:clean_temp
echo.
echo [3/3] Cleaning temporary files...

if exist "bin\Debug\output" (
    echo Cleaning output directory...
    if exist "bin\Debug\output\all.yaml" del /f /q "bin\Debug\output\all.yaml" 2>nul
    if exist "bin\Debug\output\base64.txt" del /f /q "bin\Debug\output\base64.txt" 2>nul
    if exist "bin\Debug\output\mihomo.yaml" del /f /q "bin\Debug\output\mihomo.yaml" 2>nul
    if exist "bin\Debug\output\sub-store.log" del /f /q "bin\Debug\output\sub-store.log" 2>nul
    if exist "bin\Debug\output\root.json" del /f /q "bin\Debug\output\root.json" 2>nul
    echo output directory cleaned
)

if exist "*.zip" (
    echo Deleting temporary zip files...
    del /f /q "*.zip" 2>nul
    echo Temporary zip files deleted
)

echo Temporary files cleanup complete
goto :eof

:reset_config
echo.
echo ========================================
echo   Resetting to Default Config...
echo ========================================
echo.

call :clean_config

echo Creating default config directory...
if not exist "config" mkdir "config"

echo Creating default config file...
(
echo concurrent: 32
echo check-interval: 120
echo timeout: 5000
echo min-speed: 1024
echo download-timeout: 10
echo speed-test-url: https://cachefly.cachefly.net/100mb.test
echo save-method: local
echo listen-port: :8199
echo sub-store-port: :8299
echo githubproxy: Auto
echo github-proxy: ""
) > "config\config.yaml"

echo Default config created
echo.
echo ========================================
echo   Config Reset Complete!
echo ========================================
echo.
pause
goto menu

:end
echo.
echo Thank you for using!
exit /b 0