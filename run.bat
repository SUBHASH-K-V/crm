@echo off
set "PATH=C:\Program Files\dotnet;%PATH%"
taskkill /F /IM AcxiomCRM.exe 2>nul
ping 127.0.0.1 -n 2 >nul
cd /d "%~dp0AcxiomCRM"
echo Starting AcxiomCRM on http://localhost:5001 ...
dotnet run
