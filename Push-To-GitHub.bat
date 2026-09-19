@echo off
cd /d "%~dp0"
echo ========================================================
echo   STARSHIP VIEWPORT // TRANSMITTING REPO TO GITHUB      
echo ========================================================
echo Remote: https://github.com/jiludkumar-therealone/starship-viewport.git
echo Branch: main
echo.
git push -u origin main
echo.
echo ========================================================
pause
