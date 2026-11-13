@echo off
setlocal EnableExtensions
cd /d "%~dp0"

chcp 65001 >nul
set "PS_EXE="
where pwsh.exe >nul 2>nul
if not errorlevel 1 set "PS_EXE=pwsh.exe"
if not defined PS_EXE set "PS_EXE=powershell.exe"

if not defined GSC_CONSOLE_HIDE_DONE (
    "%PS_EXE%" -WindowStyle Hidden -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\hide-console.ps1" >nul 2>nul
    if errorlevel 1 exit /b 90
)

if not exist "%~dp0scripts\dev-install-run.ps1" (
    echo [FAILED] Missing scripts\dev-install-run.ps1
    exit /b 2
)

findstr /c:"DEV-INSTALL-011" "%~dp0scripts\dev-install-run.ps1" >nul 2>nul
if errorlevel 1 (
    echo [FAILED] The installer script is stale or this is not the source checkout.
    echo Expected: %~dp0scripts\dev-install-run.ps1
    echo Please run this file from the current GameSaveCenter source directory.
    exit /b 3
)

echo ==============================================
echo   GameSaveCenter build, install and run
echo ==============================================
echo.

"%PS_EXE%" -WindowStyle Hidden -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\dev-install-run.ps1" -Configuration Release
set "EXIT_CODE=%ERRORLEVEL%"

echo.
if not "%EXIT_CODE%"=="0" (
    echo [FAILED] Build or installation did not complete. Exit code: %EXIT_CODE%
    if defined GSC_INSTALL_RUN_LOG_PATH (
        echo See the configured run log under artifacts.
    ) else (
        echo See artifacts\one-click-install.log for details.
    )
    exit /b %EXIT_CODE%
)

echo [OK] Build, verification, installation and Playnite startup completed.
if defined GSC_INSTALL_REPORT_PATH (
    echo See the configured install report under artifacts.
) else (
    echo See artifacts\last-dev-install.txt for the installed version.
)
echo.
exit /b 0
