@echo off
cd /d "%~dp0JoyShockMapper\JoyShockMapper_SDL2_x64"
taskkill /f /im JoyShockMapper.exe >nul 2>&1 && timeout /t 1 /nobreak >nul
echo GyroConfigs/JoyCon_Individual_Hybrid.txt> "OnStartup.txt"
start "" "JoyShockMapper.exe"
