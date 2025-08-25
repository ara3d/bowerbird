using Ara3D.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandBackgroundExportBos : NamedCommand
    {
        public BosForm BosForm;

        public override void Execute(object arg)
        {
            BosForm = new BosForm();
            BosForm.Show();

            var uiapp = arg as UIApplication; 
            Doc = uiapp?.ActiveUIDocument?.Document;
            Dir.TryToCreateAndClearDirectory();
            Ids.Clear();
            Processor = new BackgroundProcessor<long>(Process, uiapp);
            var collector1 = new FilteredElementCollector(Doc).WhereElementIsNotElementType();
            var collector2 = new FilteredElementCollector(Doc).WhereElementIsNotElementType();
            var ids = collector1.ToElementIds().Concat(collector2.ToElementIds());
            StartPipe();
            Processor.OnHeartbeat += ProcessorOnOnHeartbeat;
            Processor.EnqueueWork(ids.Select(id => id.Value));
        }

        private void ProcessorOnOnHeartbeat(object sender, EventArgs e)
        {
            if (BosForm != null)
                BosForm.SetIdle(DateTime.Now.ToShortTimeString());
        }

        public static string PipeName = "bos.pipe";

        public void StartPipe()
        {
            Server = new NamedPipeServerStream(
                PipeName,
                PipeDirection.InOut,
                maxNumberOfServerInstances: 1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous);

            Server.WaitForConnection();
            Writer = new BinaryWriter(Server);
            Reader = new BinaryReader(Server);
            Writer.Write("hello");
            Writer.Flush();
        }

        public NamedPipeServerStream Server;
        public BinaryWriter Writer;
        public BinaryReader Reader;
        public List<long> Ids = new();

        public void Process(long id)
        {
            Writer.Write(id);

            if (BosForm != null)
                BosForm.SetId(id.ToString());
        }

        public Document Doc;

        public BackgroundProcessor<long> Processor;

        public DirectoryPath Dir => @"C:\Users\cdigg\OneDrive\Documents";

        public override string Name => "Background Export BOM";
    }
}
