@echo off
setlocal EnableExtensions
set "PS_EXE="
where pwsh.exe >nul 2>nul
if not errorlevel 1 set "PS_EXE=pwsh.exe"
if not defined PS_EXE set "PS_EXE=powershell.exe"

"%PS_EXE%" -WindowStyle Hidden -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\hide-console.ps1" >nul 2>nul
if errorlevel 1 exit /b 90
set "GSC_CONSOLE_HIDE_DONE=1"
call "%~dp0GameSaveCenter-Run.cmd"
exit /b %ERRORLEVEL%
