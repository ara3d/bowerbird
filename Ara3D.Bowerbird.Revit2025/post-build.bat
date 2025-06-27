set AddinsDir=%programdata%\Autodesk\Revit\Addins\
set BowerbirdDir=%AddinsDir%\2025\Ara3D.Bowerbird\
set ScriptsDir=%localappdata%\Ara 3D\Bowerbird for Revit 2025\Scripts\
xcopy /Y *2025.addin %AddinsDir%\2025
if not exist "%BowerbirdDir%" mkdir "%BowerbirdDir%"
xcopy %1 "%BowerbirdDir%" /h /i /c /k /e /r /y
xcopy ..\Ara3D.Bowerbird.Demo\Samples\SampleCommands.cs "%ScriptsDir%" /y
xcopy ..\Ara3D.Bowerbird.RevitSamples\*.cs "%ScriptsDir%" /y
xcopy refs.txt "%ScriptsDir%" /y
