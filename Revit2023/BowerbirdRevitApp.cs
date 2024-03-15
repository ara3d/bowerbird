using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Bowerbird.WinForms.Net48;
using Ara3D.Services;
using Ara3D.Utils;
using Autodesk.Revit.UI;
using Application = Ara3D.Services.Application;

namespace Ara3D.Bowerbird.Revit
{
    public class BowerbirdRevitApp : IExternalApplication, IBowerbirdHost
    {
        public UIControlledApplication UicApp { get; private set; }
        public UIApplication UiApp { get; private set; }
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

            return Result.Succeeded;
        }

        public BowerbirdForm Window { get; private set; }

        public BowerbirdForm GetOrCreateWindow()
        {
            if (Window == null)
            {
                Window = new BowerbirdForm(this, BowerbirdOptions.CreateFromName("Bowerbird for Revit 2023"));
                Window.FormClosing += (sender, args) =>
                {
                    Window.Hide();
                    args.Cancel = true;
                };

                // TODO: run certain scripts automatically.
                
                // TODO: run this automatically? 
                /* 
                var sampleText = Resources.SampleRevitCommands;
                Window.BowerbirdService
                    .Options
                    .ScriptsFolder
                    .RelativeFile("SampleRevitCommands.cs")
                    .WriteAllText(sampleText);
                */
            }

            Window.Show();
            return Window;
        }

        public void FirstRun()
        {

        }

        public void ExecuteCommand(IBowerbirdCommand obj)
        {
            obj.Execute(Instance.UiApp);
        }

        public void Run(UIApplication application)
        {
            UiApp = application;
            GetOrCreateWindow();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
    }
}
