set AddinsDir=%programdata%\Autodesk\Revit\Addins
set BowerbirdDir=%AddinsDir%\2023\Ara3D.Bowerbird
set ScriptsDir=%localappdata%\Ara 3D\Bowerbird for Revit 2023\Scripts
xcopy /Y *2023.addin %AddinsDir%\2023
if not exist "%BowerbirdDir%" mkdir "%BowerbirdDir%"
xcopy %1 "%BowerbirdDir%" /h /i /c /k /e /r /y
xcopy ..\Ara3D.Bowerbird.WinForms.Net48\Samples\SampleCommands.cs "%ScriptsDir%" /y
xcopy refs.txt "%ScriptsDir%" /y
xcopy includes.txt "%ScriptsDir%" /y
