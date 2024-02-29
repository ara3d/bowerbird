using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Bowerbird.Wpf.Net48.Lib;
using Ara3D.Services;
using Autodesk.Revit.UI;
using Application = Ara3D.Services.Application;

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

        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("Bowerbird.Revit2023") && !args.Name.Contains("resources"))
            {
                return typeof(BowerbirdRevitApp).Assembly;

                //var filename = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                //filename = Path.Combine(filename, "MaterialDesignTheme.Wpf.dll");
                /*
                if (File.Exists(filename))
                {
                    return Assembly.LoadFrom(filename);
                }
                */
            }
            return null;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            UicApp = application;

            Instance = this;
            Debug.WriteLine($"Current culture = {CultureInfo.CurrentCulture}");
            Debug.WriteLine($"Current UI culture = {CultureInfo.CurrentUICulture}");
            Debug.WriteLine($"Default current culture = {CultureInfo.DefaultThreadCurrentCulture}");
            Debug.WriteLine($"Default current UI culture = {CultureInfo.DefaultThreadCurrentUICulture}");

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
        public LoggingService Logging { get; }
        public BowerbirdService Bowerbird { get; }
        public BowerbirdOptions Options { get; }
        public BowerbirdMainWindow Window { get; private set; }

        public BowerbirdRevitApp()
        {
            App = new Application();
            Options = BowerbirdOptions.CreateFromName("Ara 3D", "Bowerbird for Revit");
            Logging = new LoggingService("Bowerbird", App);
            Bowerbird = new BowerbirdService(App, Logging, Options);
        }

        public void Run(UIApplication application)
        {
            Logging.Log("Running Bowerbird Revit application");

            Debug.WriteLine($"Current culture = {CultureInfo.CurrentCulture}");
            Debug.WriteLine($"Current UI culture = {CultureInfo.CurrentUICulture}");
            Debug.WriteLine($"Default current culture = {CultureInfo.DefaultThreadCurrentCulture}");
            Debug.WriteLine($"Default current UI culture = {CultureInfo.DefaultThreadCurrentUICulture}");

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            Window = Window ?? new BowerbirdMainWindow();

            Window.Show();
            Window.BowerbirdPanel.RegisterServices(Bowerbird, Logging);

            /*
            MyExportCommand.ExportView3D(application.ActiveUIDocument.Document,
                (Autodesk.Revit.DB.View3D)application.ActiveUIDocument.ActiveGraphicalView);            
            Service.Compile();
            */
        }
    }
}
