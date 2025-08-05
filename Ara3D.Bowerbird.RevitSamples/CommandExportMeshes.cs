using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Ara3D.Utils;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples;

public class CommandExportMeshes : NamedCommand
{
    public override string Name => "Export meshes";

    public UIApplication app { get; private set; }

    public override void Execute(object arg)
    {
        app = (arg as UIApplication);

        if (app == null)
            throw new Exception($"Passed argument {arg} is either null or not a UI application");

        var uiDoc = app.ActiveUIDocument;
        var doc = uiDoc.Document;

        var timer = Stopwatch.StartNew();

        var bldr = new RevitModelBuilder();
        bldr.ProcessDocument(doc);

        var processingTime = timer.Elapsed;
        timer.Restart();

        var filePath = new FilePath(doc.PathName);
        var outputFilePath = filePath.ChangeDirectoryAndExt(SpecialFolders.MyDocuments, "meshes.json");
        var fs = File.Create(outputFilePath);
        var model = bldr.Build();
        var buildTime = timer.Elapsed;
        timer.Restart();
        var options = new JsonSerializerOptions { WriteIndented = true, IncludeFields = true };
        JsonSerializer.Serialize(fs, model, options);
        fs.Close();

        var serializationTime = timer.Elapsed;

        var docCount = bldr.DocumentMeshGroups.Count;
        var meshCount = bldr.DocumentMeshGroups.Values.Sum(CountMeshes);

        var totalSize = outputFilePath.GetFileSizeAsString();

        var text = "Just created a geometry representation using JSON\r\n" +
                   $"# meshes = {meshCount}\r\n" +
                   $"# elements = {bldr.ElementLookup.Count}\r\n" +
                   $"# solids = {bldr.SolidLookup.Count}\r\n" +
                   $"# geometries = {bldr.SymbolGeometryLookup.Count}\r\n" +
                   $"# errors = {bldr.Errors.Count}\r\n" + 
                   $"across {docCount + 1} documents\r\n" +
                   $"Processing took {processingTime.TotalSeconds:F} seconds\r\n" +
                   $"Building took {buildTime.TotalSeconds:F} seconds\r\n" +
                   $"Serialization took {serializationTime.TotalSeconds:F} seconds\r\n" +
                   $"Generated file size is {totalSize}\r\n" +
                   $"File saved as {outputFilePath}";

        outputFilePath.GetDirectory().OpenFolderInExplorer();
        MessageBox.Show(text);
    }

    public int CountMeshes(MeshGroup g)
        => g.Meshes.Count + g.Children.Sum(CountMeshes);
}