using System.Diagnostics;
using System.Threading;
using Ara3D.Logging;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandIdlingDemo : NamedCommand
    {
        public TextDisplayForm Form;
        public ILogger Logger;
        public int MSecElapsed;
        public const int WORK_ITEM_MSEC = 100;
        public const int WORK_TOTAL_MSEC = 1000;
        public override string Name => "Idling Demo";
        public Stopwatch Stopwatch = new Stopwatch();

        public void Log(string msg)
        {
            Logger.Log(msg);
        }

        public override void Execute(object arg)
        {
            Form = new TextDisplayForm("");
            var uiApp = arg as UIApplication; ;
            uiApp.Idling += Application_Idling;
            Logger = Form.CreateLogger();
            Form.Show();
            Stopwatch.Start();
        }

        private void Application_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {   
            var uiApp = sender as UIApplication;
            if (uiApp == null) return;

            // Simulate work 
            Thread.Sleep(WORK_ITEM_MSEC);
            MSecElapsed += WORK_ITEM_MSEC;
            if (MSecElapsed > WORK_TOTAL_MSEC)
            {
                var n1 = MSecElapsed / 1000f;
                Log($"{n1:##.00} seconds elapsed during idling");
                MSecElapsed = 0;
                var n2 = Stopwatch.ElapsedMilliseconds / 1000f;
                Log($"{n2:##.00} seconds elapsed in real-time");
                Stopwatch.Reset();
                Stopwatch.Start();
            }
            e.SetRaiseWithoutDelay();
        }
    }
}