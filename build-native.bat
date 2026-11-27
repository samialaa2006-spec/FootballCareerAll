@echo off
rem Builds the C and C++ library for Windows (career.dll).
rem Needs gcc and g++ from MinGW-w64 (for example installed with MSYS2) on your PATH.
cd /d "%~dp0"

where g++ >nul 2>nul
if errorlevel 1 (
  echo g++ was not found. Install MinGW-w64 ^(see README^) or skip this step: the game works without it.
  exit /b 1
)

gcc -O2 -c native\curves.c -o native\curves.o || exit /b 1
g++ -O2 -shared -static-libgcc -static-libstdc++ -o career.dll native\market_value.cpp native\curves.o || exit /b 1
del native\curves.o
echo Built career.dll
