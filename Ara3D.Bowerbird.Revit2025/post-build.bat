
set AddinsDir=%programdata%\Autodesk\Revit\Addins\
set BowerbirdDir=%AddinsDir%\2025\Ara3D.Bowerbird\
set ScriptsDir=%localappdata%\Ara 3D\Bowerbird for Revit 2025\Scripts\


:: -------- 1) No argument?  Leave quietly --------------------
if "%~1"=="" (
    echo No argument supplied – nothing to do.
    goto :eof
)

:: -------- 2)  -clean  ---------------------------------------
if /I "%~1"=="-clean" goto :clean

:: -------- 3)  Normal install --------------------------------
xcopy /Y *2025.addin %AddinsDir%\2025
if not exist "%BowerbirdDir%" mkdir "%BowerbirdDir%"

xcopy %1 "%BowerbirdDir%" /h /i /c /k /e /r /y
xcopy ..\Ara3D.Bowerbird.Demo\Samples\SampleCommands.cs "%ScriptsDir%" /y
xcopy ..\Ara3D.Bowerbird.RevitSamples\*.cs "%ScriptsDir%" /y

echo Done.
goto :eof


:clean
echo Removing Bowerbird for Revit 2025 …

REM Delete manifest(s) we previously copied
if exist "%AddinsDir%2025" (
    del /Q "%AddinsDir%2025\*2025.addin" >nul 2>&1
)

REM Remove add-in folder and scripts folder (with contents)
if exist "%BowerbirdDir%" rd /S /Q "%BowerbirdDir%"
if exist "%ScriptsDir%"   rd /S /Q "%ScriptsDir%"

echo Clean-up complete.
goto :eof