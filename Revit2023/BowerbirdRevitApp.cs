using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Ara3D.Bowerbird.Core;
using Ara3D.Services;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.Revit
{
    public class BowerbirdRevitApp : IExternalApplication
    {
        public UIControlledApplication UicApp { get; private set; }
        public static BowerbirdRevitApp Instance { get; private set; }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        public Result OnStartup(UIControlledApplication application)
        {
            UicApp = application;
            Instance = this;
            var rvtRibbonPanel = application.CreateRibbonPanel("Ara 3D");
            var pushButtonData = new PushButtonData("Bowerbird", "Bowerbird", 
                Assembly.GetExecutingAssembly().Location,
                typeof(BowerbirdExternalCommand).FullName);
            // https://www.revitapidocs.com/2020/544c0af7-6124-4f64-a25d-46e81ac5300f.htm
            if (!(rvtRibbonPanel.AddItem(pushButtonData) is PushButton runButton))
                return Result.Failed;
            runButton.LargeImage = BitmapToImageSource(Resources.Bowerbird_32x32);
            runButton.ToolTip = "Compile and Load C# Scripts";

            // TODO: run the scripts automatically.

            return Result.Succeeded;
        }

        public IApplication App { get; }
        public LoggingService Logger { get; }
        public BowerbirdService Service { get; }
        public BowerbirdOptions Options { get; }
        public BowerbirdWindow Window { get; private set; }

        public BowerbirdRevitApp()
        {
            App = new Application();
            Logger = new LoggingService("Bowerbird", App);
            Options = BowerbirdOptions.CreateFromName("Ara 3D", "Bowerbird for Revit");
            Service = new BowerbirdService(App, Logger, Options);
            Service.Compiler.RecompileEvent += CompilerOnRecompileEvent;
        }

        private void CompilerOnRecompileEvent(object sender, EventArgs e)
        {
            if (Window != null && Window.IsVisible)
            {
                Window.CommandListBox.Items.Clear();
                foreach (var t in Service.Compiler.ExportedTypes)
                {
                    Window.CommandListBox.Items.Add(t.Name);
                }
            }
        }

        public void Compile()
        {
            Service.Compiler.Compile();
        }

        public void OnLogEntry(LogEntry entry)
        {
            Debug.WriteLine($"Log entry: {entry.Text}");
            if (Window != null)
                Window.ConsoleListBox.Items.Add(entry.ToString());
        }

        public void Run(UIApplication application)
        {
            Logger.Log("Running command");

            /*
            MyExportCommand.ExportView3D(application.ActiveUIDocument.Document,
                (Autodesk.Revit.DB.View3D)application.ActiveUIDocument.ActiveGraphicalView);
            
            window.Show();
            Service.Compile();
            */
            Window = Window ?? new BowerbirdWindow(this);
            Window.Show();
        }
    }
}
