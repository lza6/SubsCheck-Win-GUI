@echo off
setlocal enabledelayedexpansion

title SubsCheck Win GUI - Quick Test Script

echo ========================================
echo   SubsCheck Win GUI - Quick Test
echo ========================================
echo.
echo This script helps test different proxy modes
echo.

:menu
echo Please select test mode:
echo.
echo [1] Test GitHub Proxy Mode (use GitHub proxy first)
echo [2] Test Local Proxy Mode (use local V2Ray/Clash proxy)
echo [3] Test Smart Fallback (GitHub proxy fails - use local)
echo [4] Clean and Rebuild
echo [5] Run Cleanup Tool
echo [6] Start Program
echo [7] Exit
echo.
set /p choice=Enter option (1-7):

if "%choice%"=="1" goto test_github_proxy
if "%choice%"=="2" goto test_local_proxy
if "%choice%"=="3" goto test_smart_fallback
if "%choice%"=="4" goto clean_rebuild
if "%choice%"=="5" goto run_clean_tool
if "%choice%"=="6" goto run_program
if "%choice%"=="7" goto end

echo.
echo Invalid option, please try again!
echo.
goto menu

:test_github_proxy
echo.
echo ========================================
echo   Configuring GitHub Proxy Mode
echo ========================================
echo.

if not exist "config" mkdir "config"

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
echo use-local-proxy: false
echo gui-auto: false
) > "config\config.yaml"

echo Configured as GitHub Proxy Mode
echo use-local-proxy: false (will use GitHub proxy first)
echo.
echo Please start the program to test
echo.
pause
goto menu

:test_local_proxy
echo.
echo ========================================
echo   Configuring Local Proxy Mode
echo ========================================
echo.

if not exist "config" mkdir "config"

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
echo use-local-proxy: true
echo gui-auto: false
) > "config\config.yaml"

echo Configured as Local Proxy Mode
echo use-local-proxy: true (will use local proxy directly)
echo.
echo Note: Please ensure your V2Ray/Clash proxy is running
echo.
pause
goto menu

:test_smart_fallback
echo.
echo ========================================
echo   Configuring Smart Fallback Mode
echo ========================================
echo.

if not exist "config" mkdir "config"

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
echo use-local-proxy: false
echo gui-auto: false
) > "config\config.yaml"

echo Configured as Smart Fallback Mode
echo use-local-proxy: false (will try GitHub proxy first, fallback to local if fails)
echo.
echo Test Flow:
echo 1. Program will test GitHub proxy availability
echo 2. If GitHub proxy is available, use it
echo 3. If GitHub proxy is unavailable, switch to local proxy
echo 4. If local proxy is also unavailable, show warning
echo.
echo Please start the program to test
echo.
pause
goto menu

:clean_rebuild
echo.
echo ========================================
echo   Cleaning and Rebuilding
echo ========================================
echo.

echo Cleaning build outputs...
if exist "bin\Debug" rd /s /q "bin\Debug" 2>nul
if exist "obj\Debug" rd /s /q "obj\Debug" 2>nul
if exist "build.log" del /f /q "build.log" 2>nul

echo Cleanup complete
echo.

echo Calling quick start script...
echo.
call "快速启动脚本.bat"

goto menu

:run_clean_tool
echo.
echo Starting cleanup tool...
echo.
call "清理环境.bat"
goto menu

:run_program
echo.
echo Starting program...
echo.
if exist "bin\Debug\subs-check.win.gui.exe" (
    start "" "bin\Debug\subs-check.win.gui.exe"
    echo Program started
) else (
    echo Executable not found, please compile first
    echo.
    echo Options:
    echo [1] Return to menu
    echo [2] Clean and rebuild
    set /p run_choice=Enter choice (1-2):

    if "!run_choice!"=="2" goto clean_rebuild
)
echo.
pause
goto menu

:end
echo.
echo Thank you for using!
exit /b 0