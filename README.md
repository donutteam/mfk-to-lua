# MFK To Lua
A tool to convert MFK or CON files from The Simpsons: Hit and Run to Game.lua scripts.

## Prerequisites
- This tool requires `.NET 10.0 x64`. Download here: [https://dotnet.microsoft.com/en-us/download/dotnet/10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).
  - *You want either the `.NET Desktop Runtime` or `.NET Runtime`.*

## How to use
```
Usage: MFKToLua [options] <MFKPath>

Options:
  -?, --help                        Show this help message and exit
  -o, --output <path>               Set the output path
  -f, --force                       Force overwrite the output path if it exists
```

## Useful scripts
A selection of example scripts to run all MFK or CON files in a directory through the tool.

**Powershell:**
```powershell
$root = Read-Host "Enter root script path:"

if (-not (Test-Path $root)) {
	Write-Error "The specified path does not exist."
	exit 1
}

Get-ChildItem -Path $root -Recurse -Include *.mfk, *.con -File | ForEach-Object {
	$inputFile = $_.FullName
	$outputFile = [System.IO.Path]::ChangeExtension($inputFile, ".lua")

	Write-Host "Processing: $inputFile -> $outputFile"

	& ".\MFKToLua.exe" -o $outputFile $inputFile
}
```

**Batch:**
```bat
@echo off
setlocal enabledelayedexpansion

set /p root="Enter root script path: "

if not exist "%root%" (
	echo The specified path does not exist.
	exit /b 1
)

for /r "%root%" %%f in (*.mfk *.con) do (
	set "inputFile=%%f"
	set "outputFile=%%~dpnf.lua"

	echo Processing: !inputFile! -> !outputFile!

	".\MFKToLua.exe" -o "!outputFile!" "!inputFile!"
)

endlocal
pause
```

**Bash:**
```sh
#!/bin/bash

read -p "Enter root script path: " root

if [[ ! -d "$root" ]]; then
	echo "The specified path does not exist."
	exit 1
fi

find "$root" -type f \( -iname "*.mfk" -o -iname "*.con" \) | while read -r file; do
	output="${file%.*}.lua"
	echo "Processing: $file -> $output"
	./MFKToLua -o "$output" "$file"
done
```
