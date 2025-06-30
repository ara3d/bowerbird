using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using MessagePack;
using MessagePack.Resolvers;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class ProcessBReps : NamedCommand
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
            var dd = doc.ProcessDocument();

            /*
            var s = JsonConvert.SerializeObject(dd, Formatting.Indented);
            var outputFilePath = Path.Combine(Path.GetTempPath(), "brep.json");
            File.WriteAllText(outputFilePath, s)
            */

            /*
#pragma warning disable SYSLIB0011
            var bf = new BinaryFormatter();
#pragma warning restore SYSLIB0011
            using var ms = new MemoryStream();
            bf.Serialize(ms, dd);
            var outputFilePath = Path.Combine(Path.GetTempPath(), "brep.dat");
            var fs = File.Create(outputFilePath);
            ms.CopyTo(fs);
            fs.Close();
            */

            var options = MessagePackSerializerOptions.Standard
                .WithResolver(ContractlessStandardResolver.Instance);

            var outputFilePath = Path.Combine(Path.GetTempPath(), "brep.mp");
            var fs = File.Create(outputFilePath);
            MessagePackSerializer.Serialize(fs, dd, options);
            fs.Close();
        }
    }
}