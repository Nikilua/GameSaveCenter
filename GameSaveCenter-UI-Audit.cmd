@echo off
setlocal
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\capture-ui-audit.ps1" %*
if errorlevel 1 (
    echo UI audit failed.
    exit /b %errorlevel%
)
