set AddinsDir=%programdata%\Autodesk\Revit\Addins
xcopy /Y *2023.addin %AddinsDir%\2023
mkdir %AddinsDir%\2023\Ara3D.Bowerbird
xcopy /Y %1 %AddinsDir%\2023\Ara3D.Bowerbird