@echo off
:Start
set /p name= >Enter the name of .exe file: 
echo.
С:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc -out:%name%.exe *.cs
echo.
goto Start