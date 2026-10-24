@echo off
setlocal EnableExtensions
rem Let Windows PowerShell initialize its own module paths. A pwsh -> cmd
rem parent otherwise leaves PowerShell 7 modules ahead of Desktop modules.
rem This is local to this launcher and does not change the caller's environment.
set "PSModulePath="
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0build.ps1" %*
exit /b %ERRORLEVEL%
