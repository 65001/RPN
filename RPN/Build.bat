@echo off

echo Starting build for Windows.
dotnet publish -c Release -r win10-x64 -v m
echo.

echo Starting build for Ubuntu.
dotnet publish -c Release -r ubuntu.16.10-x64 -v m
echo.

echo Starting build for OSX.
dotnet publish -c Release -r osx.10.11-x64 -v m 
@echo on