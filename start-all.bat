@echo off
rem Starts everything that is installed: PHP saves, Java news, then the C# game.
cd /d "%~dp0"

where php >nul 2>nul
if errorlevel 1 (
  echo PHP not found: saving is switched off.
) else (
  start "PHP saves" /min php -S 127.0.0.1:8082 -t php
  echo PHP save service on http://127.0.0.1:8082
)

where java >nul 2>nul
if errorlevel 1 (
  echo Java not found: news is switched off.
) else (
  start "Java news" /min java java\NewsServer.java
  echo Java news service on http://127.0.0.1:8081
)

if not exist career.dll (
  where g++ >nul 2>nul
  if not errorlevel 1 call build-native.bat
)

echo Starting the game on http://localhost:5000
echo Close the two minimized PHP and Java windows when you are done.
dotnet run
