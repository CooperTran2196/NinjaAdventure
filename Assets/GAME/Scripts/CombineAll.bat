@echo off
REM Audio Folder
set outputFile=AllAudioScripts.txt
echo. > %outputFile%
for %%f in ("D:\Google Drive\Code\Final project\Testing\Assets\GAME\Scripts\Audio\*.cs") do (
    echo // ----- File: %%~nxf ----- >> %outputFile%
    type "%%f" >> %outputFile%
    echo. >> %outputFile%
)
echo Done! Check %outputFile% in the Scripts folder.

REM PlayerScripts Folder
set outputFile=AllEnemyScripts.txt
echo. > %outputFile%
for %%f in ("D:\Google Drive\Code\Final project\Testing\Assets\GAME\Scripts\Enemy\*.cs") do (
    echo // ----- File: %%~nxf ----- >> %outputFile%
    type "%%f" >> %outputFile%
    echo. >> %outputFile%
)
echo Done! Check %outputFile% in the Scripts folder.

REM PlayerScripts Folder
set outputFile=AllPlayerScripts.txt
echo. > %outputFile%
for %%f in ("D:\Google Drive\Code\Final project\Testing\Assets\GAME\Scripts\Player\*.cs") do (
    echo // ----- File: %%~nxf ----- >> %outputFile%
    type "%%f" >> %outputFile%
    echo. >> %outputFile%
)
echo Done! Check %outputFile% in the Scripts folder.

REM SkillTree Folder
set outputFile=AllSkillTreeScripts.txt
echo. > %outputFile%
for %%f in ("D:\Google Drive\Code\Final project\Testing\Assets\GAME\Scripts\SkillTree\*.cs") do (
    echo // ----- File: %%~nxf ----- >> %outputFile%
    type "%%f" >> %outputFile%
    echo. >> %outputFile%
)
echo Done! Check %outputFile% in the Scripts folder.

REM UI Folder
set outputFile=AllUIScripts.txt
echo. > %outputFile%
for %%f in ("D:\Google Drive\Code\Final project\Testing\Assets\GAME\Scripts\UI\*.cs") do (
    echo // ----- File: %%~nxf ----- >> %outputFile%
    type "%%f" >> %outputFile%
    echo. >> %outputFile%
)
echo Done! Check %outputFile% in the Scripts folder.

REM Weapon Folder
set outputFile=AllWeaponScripts.txt
echo. > %outputFile%
for %%f in ("D:\Google Drive\Code\Final project\Testing\Assets\GAME\Scripts\Weapon\*.cs") do (
    echo // ----- File: %%~nxf ----- >> %outputFile%
    type "%%f" >> %outputFile%
    echo. >> %outputFile%
)
echo Done! Check %outputFile% in the Scripts folder.
echo All scripts processed! Press any key to exit.
pause