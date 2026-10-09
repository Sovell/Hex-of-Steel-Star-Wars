@echo off
cd /d "%~dp0Game"
start "" "Hex of Steel.exe" -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "%~dp0Logs\player.log"
