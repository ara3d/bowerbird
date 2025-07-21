using Autodesk.Revit.UI;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Ara3D.BimOpenSchema;
using Ara3D.BimOpenSchema.IO;
using Ara3D.Utils;
using FilePath = Autodesk.Revit.DB.FilePath;

namespace Ara3D.Bowerbird.RevitSamples;

public class CommandExportBIMOpenSchema : NamedCommand
{
    public override string Name => "Export BIM Open Schema";

    public override void Execute(object arg)
    {
        ExportData(arg as UIApplication);
    }

    public void ExportData(UIApplication app)
    {
        var uiDoc = app.ActiveUIDocument;
        var doc = uiDoc.Document;

        var timer = Stopwatch.StartNew();
        var builder = new RevitToOpenBimSchema(doc, true);

        var processingTime = timer.Elapsed;
        timer.Restart();

        var bimData = builder.bdb.Data;
        var dataSet = bimData.ToDataSet();
        var buildTime = timer.Elapsed;

        var fp = new DirectoryPath(Path.GetTempPath()).RelativeFile("bimdata.parquet.zip");
        Task.Run(() => dataSet.WriteParquetToZipAsync(fp)).GetAwaiter().GetResult();
        OutputData(bimData, processingTime, buildTime, fp);
    }

    public static void OutputData(BimData bd, TimeSpan processingTime, TimeSpan buildTime, Ara3D.Utils.FilePath fp)
    {
        var text = $"Processed {bd.Documents.Count} documents\r\n" +
                   $"{bd.Entities.Count} entities\r\n" +
                   $"{bd.Descriptors.Count} descriptors\r\n" +
                   $"{bd.IntegerParameters.Count} integer parameters\r\n" +
                   $"{bd.DoubleParameters.Count} double parameters\r\n" +
                   $"{bd.EntityParameters.Count} entity parameters\r\n" +
                   $"{bd.StringParameters.Count} string parameters\r\n" +
                   $"{bd.PointParameters.Count} point parameters\r\n" +
                   $"{bd.Points.Count} points\r\n" +
                   $"{bd.Strings.Count} strings\r\n" +
                   $"{bd.Relations.Count} relations\r\n" +
                   $"Processing took {processingTime.TotalSeconds:F} seconds\r\n" + 
                   $"Building took {buildTime.TotalSeconds:F} seconds\r\n" +
                   $"Output size was {fp.GetFileSizeAsString()}";

        MessageBox.Show(text);
    }
}