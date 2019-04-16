@echo off
echo Starting windows build.
dotnet publish -c Release -r win10-x64
echo Starting Ubuntu build.
dotnet publish -c Release -r ubuntu.16.10-x64
echo Starting OSX build.
dotnet publish -c Release -r osx.10.11-x64
@echo on