using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Autodesk.Revit.UI;
using MessagePack;
using MessagePack.Resolvers;

namespace Ara3D.Bowerbird.RevitSamples;

public class CommandExportParametersNaive : NamedCommand
{
    public override string Name => "Export parameters (Name/Value pairs)";

    public override void Execute(object arg)
    {
        ExportParameters(arg as UIApplication);
    }

    public static void ExportParameters(UIApplication app)
    {
        var uiDoc = app.ActiveUIDocument;
        var doc = uiDoc.Document;

        var timer = Stopwatch.StartNew();
        var np = new NaiveParameters();
        doc.ProcessElements(e =>
        {
            np.ElementParameters[e.Id.Value]
                = e.GetParameterMap();
        });

        var processingTime = timer.Elapsed;

        var s1 = new SerializationHelper("JSON without indenting", "parameters.json",
            fs =>
            {
                JsonSerializer.Serialize(fs, np, new JsonSerializerOptions() { WriteIndented = false });
            });

        var s2 = new SerializationHelper("JSON with indenting", "parameters.json",
            fs => { JsonSerializer.Serialize(fs, np, new JsonSerializerOptions() { WriteIndented = true }); });

        var s3 = new SerializationHelper("Message pack", "parameters.mp", fs =>
        {
            var options = MessagePackSerializerOptions.Standard
                .WithResolver(ContractlessStandardResolver.Instance);
            MessagePackSerializer.Serialize(fs, np, options);
        });

        var s4 = new SerializationHelper("Message pack with compression", "parameters.mpz", fs =>
        {
            var options = MessagePackSerializerOptions.Standard
                .WithResolver(ContractlessStandardResolver.Instance)
                .WithCompression(MessagePackCompression.Lz4Block);
            MessagePackSerializer.Serialize(fs, np, options);
        });

        OutputData(np, processingTime, s1, s2, s3, s4);
    }

    public static void OutputData(NaiveParameters np, TimeSpan processingTime, params SerializationHelper[] shs)
    {
        var objectCount = np.ElementParameters.Count;
        var parameterCount = np.ElementParameters.Sum(kv => kv.Value.Count);

        var text = $"Exported {objectCount} elements\r\n" +
                   $"with a total of {parameterCount} parameters\r\n" +
                   $"Processing took {processingTime.TotalSeconds:F} seconds\r\n";

        foreach (var sh in shs)
        {
            text += $"{sh.Method} took {sh.Elapsed.TotalSeconds:F} seconds and output {sh.FileSize} data\r\n";
        }

        MessageBox.Show(text);
    }
}