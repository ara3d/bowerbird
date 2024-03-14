using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Bowerbird.WinForms.Net48;
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
            // TODO: upgrade this code for different Revit versions. 
            if (args.Name.Contains("Bowerbird.Revit2023") && !args.Name.Contains("resources"))
            {
                // NOTE: this is horrible, but we have to do it. The assembly can't be found otherwise?! 
                return typeof(BowerbirdRevitApp).Assembly;
            }
            return null;
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
        public LoggingService Logging { get; }
        public BowerbirdService Bowerbird { get; }
        public BowerbirdOptions Options { get; }
        public BowerbirdForm Window { get; private set; }

        public BowerbirdRevitApp()
        {
            //App = new Application();
            //Options = BowerbirdOptions.CreateFromName("Ara 3D", "Bowerbird for Revit");
            //Logging = new LoggingService("Bowerbird", App);
            //Bowerbird = new BowerbirdService(App, Logging, Options);
        }

        public void Run(UIApplication application)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Window = Window ?? new BowerbirdForm();
            Window.Show();
        }
    }
}
