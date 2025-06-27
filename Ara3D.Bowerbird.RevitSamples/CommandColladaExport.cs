using System;
using System.IO;
using Ara3D.Bowerbird.RevitSamples;
using Ara3D.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitExporter;

namespace Ara3D.Bowerbird.Revit.Samples
{
    public class CommandColladaExport : NamedCommand
    {
        public override string Name => "Export Collada";

        public override void Execute(object argument)
        {
            var uiapp = argument as UIApplication;
            if (uiapp == null)
                throw new Exception("Argument is not a UIApplication");

            var uidoc = uiapp.ActiveUIDocument;
            var doc = uidoc.Document;

            if (!(doc.ActiveView is View3D view3D))
                throw new Exception("You must be in 3D view to export.");


            var outputFilePath = doc.CurrentFileName().ChangeDirectoryAndExt(Config.OutputDir, ".dae");
            using (var textWriter = new StreamWriter(outputFilePath.OpenWrite()))
            {
                var context = new ColladaExportContext(doc, textWriter);
                var exporter = new CustomExporter(doc, context);
                exporter.IncludeGeometricObjects = true;
                exporter.ShouldStopOnError = false;
                exporter.Export(view3D);
            }

            outputFilePath.SelectFileInExplorer();
        }
    }
}
