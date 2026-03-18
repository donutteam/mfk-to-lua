@echo off
rd publish /s /q
md publish
dotnet publish MFKToLua\MFKToLua.csproj -c Release -r win-x64 /p:SelfContained=false /p:PublishSingleFile=true -o publish\win-x64
dotnet publish MFKToLua\MFKToLua.csproj -c Release -r linux-x64 /p:SelfContained=false /p:PublishSingleFile=true -o publish\linux-x64
PAUSE