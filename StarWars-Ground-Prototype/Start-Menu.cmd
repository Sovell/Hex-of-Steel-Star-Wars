@echo off
cd /d "%~dp0Game"
start "" "Hex of Steel.exe" -prototype-menu -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "%~dp0Logs\menu-player.log"
