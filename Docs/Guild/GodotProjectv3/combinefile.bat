@echo off
setlocal EnableExtensions EnableDelayedExpansion
rem Use UTF-8 so Godot scripts don’t get mangled
chcp 65001 >nul

rem ==== CONFIG ====
set "ROOT=D:\Google Drive\Code\Final project\Project Sample\GodotProjectv3"
rem Space-separated list of extensions to include (edit if needed)
set "EXTS=.gd .cs .gdshader .shader"
set "OUT=%ROOT%\godot_code_dump.txt"
rem ================

rem Fresh output
del "%OUT%" 2>nul

echo Collecting code from:
echo   %ROOT%
echo Writing to:
echo   %OUT%
echo.

for /r "%ROOT%" %%F in (*) do (
    for %%E in (%EXTS%) do (
        if /I "%%~xF"=="%%E" (
            >>"%OUT%" echo =======================================================================
            >>"%OUT%" echo File: %%F
            >>"%OUT%" echo =======================================================================
            type "%%F" >> "%OUT%"
            >>"%OUT%" echo.
            >>"%OUT%" echo.
        )
    )
)

echo Done!
echo Output: %OUT%
endlocal
