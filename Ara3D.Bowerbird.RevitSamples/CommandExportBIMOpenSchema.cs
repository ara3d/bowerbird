using Autodesk.Revit.UI;
using MessagePack.Resolvers;
using MessagePack;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using Autodesk.Revit.DB;
using BIMOpenSchema;
using Autodesk.Revit.DB.Architecture;

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
        var builder = new RevitBIMDataBuilder(doc, true);

        var processingTime = timer.Elapsed;
        timer.Restart();
        var bimData = builder.bdb.Build();

        var buildTime = timer.Elapsed;

        var s1 = new SerializationHelper("JSON with indenting", "bimdata.json",
            fs => { JsonSerializer.Serialize(fs, bimData, new JsonSerializerOptions() { WriteIndented = true }); });

        var s2 = new SerializationHelper("Message pack with compression", "bimdata.mpz", fs =>
        {
            var options = MessagePackSerializerOptions.Standard
                .WithResolver(ContractlessStandardResolver.Instance)
                .WithCompression(MessagePackCompression.Lz4Block);
            MessagePackSerializer.Serialize(fs, bimData, options);
        });

        OutputData(bimData, processingTime, buildTime, s1, s2);
    }

    public static void OutputData(BIMData bd, TimeSpan processingTime, TimeSpan buildTime, params SerializationHelper[] shs)
    {
        var text = $"Processed {bd.Documents.Count} documents\r\n" +
                   $"{bd.Entities.Count} entities\r\n" +
                   $"{bd.Descriptors.Count} descriptors\r\n" +
                   $"{bd.IntegerParameters.Count} integer parameters\r\n" +
                   $"{bd.DoubleParameters.Count} double parameters\r\n" +
                   $"{bd.EntityParameters.Count} entity parameters\r\n" +
                   $"{bd.StringParameters.Count} string parameters\r\n" +
                   $"{bd.Points.Count} points\r\n" +
                   $"{bd.Strings.Count} strings\r\n" +
                   $"Processing took {processingTime.TotalSeconds:F} seconds\r\n";

        foreach (var sh in shs)
        {
            text += $"{sh.Method} took {sh.Elapsed.TotalSeconds:F} seconds and output {sh.FileSize} data\r\n";
        }
        MessageBox.Show(text);
    }
}