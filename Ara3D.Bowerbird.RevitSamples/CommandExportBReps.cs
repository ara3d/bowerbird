using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using Ara3D.Collections;
using Ara3D.Utils;
using Autodesk.Revit.UI;
using FilePath = Autodesk.Revit.DB.FilePath;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandExportBReps : NamedCommand
    {
        public override string Name => "Export faces and boundaries";

        public UIApplication app { get; private set; }

        public override void Execute(object arg)
        {
            app = (arg as UIApplication);

            if (app == null)
                throw new Exception($"Passed argument {arg} is either null or not a UI application");

            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;

            var timer = Stopwatch.StartNew();
            var dd = doc.ProcessDocumentBrep();
            var processingTime = timer.Elapsed;
            timer.Restart();
            
            var filePath = new Utils.FilePath(doc.PathName);
            var outputFilePath = filePath.ChangeDirectoryAndExt(SpecialFolders.MyDocuments, "brep.json"); 
            var fs = File.Create(outputFilePath);
            var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
            JsonSerializer.Serialize(fs, dd, options);
            fs.Close();
            
            var serializationTime = timer.Elapsed;

            var docCount = dd.Documents.Count;
            var objectCount = dd.Documents.Sum(d => d.Geometry.GeometryObjects.Count);
            var elementCount = dd.Documents.Sum(d => d.Geometry.Elements.Count);

            var totalSize = PathUtil.GetFileSizeAsString(outputFilePath);

            var text = "Just created a BREP representation using JSON\r\n" + 
                       $"Containing {elementCount} elements with geometry\r\n" +
                       $"with a total of {objectCount} geometric objects\r\n" +
                       $"across {docCount+1} documents\r\n" + 
                       $"Processing took {processingTime.TotalSeconds:F} seconds\r\n" + 
                       $"Serialization took {serializationTime.TotalSeconds:F} seconds\r\n" + 
                       $"Generated file size is {totalSize}\r\n" + 
                       $"File saved as {outputFilePath}";

            outputFilePath.GetDirectory().OpenFolderInExplorer();
            MessageBox.Show(text);
        }
    }
}