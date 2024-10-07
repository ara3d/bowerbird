using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;
using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Bowerbird.WinForms.Net48;
using Ara3D.Logging;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Plato.Geometry.Revit;
using Bitmap = System.Drawing.Bitmap;

namespace Ara3D.Bowerbird.Revit
{
    public class BowerbirdRevitApp : IExternalApplication, IBowerbirdHost
    {
        public static BowerbirdRevitApp Instance { get; private set; }

        public UIControlledApplication UicApp { get; private set; }
        public UIApplication UiApp { get; private set; }
        
        public BowerbirdOptions Options { get; private set; }
        public CommandExecutor CommandExecutor { get; set; }

        public Services.Application ServiceApp { get; private set; }
        public BowerbirdService BowerbirdService { get; private set; }

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
            CommandExecutor = new CommandExecutor();
            
            var rvtRibbonPanel = application.CreateRibbonPanel("Ara 3D");
            var pushButtonData = new PushButtonData("Bowerbird", "Bowerbird", 
                Assembly.GetExecutingAssembly().Location,
                typeof(BowerbirdExternalCommand).FullName);
            // https://www.revitapidocs.com/2020/544c0af7-6124-4f64-a25d-46e81ac5300f.htm
            if (!(rvtRibbonPanel.AddItem(pushButtonData) is PushButton runButton))
                return Result.Failed;
            runButton.LargeImage = BitmapToImageSource(Resources.Bowerbird_32x32);
            runButton.ToolTip = "Compile and Load C# Scripts";

            ServiceApp = new Services.Application();
            Options = BowerbirdOptions.CreateFromName("Bowerbird for Revit 2023");
            BowerbirdService = new BowerbirdService(this, ServiceApp, Logger.Debug, Options);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            application.ControlledApplication.ApplicationInitialized += ControlledApplicationOnApplicationInitialized;

            return Result.Succeeded;
        }

        void ControlledApplicationOnApplicationInitialized(object sender, ApplicationInitializedEventArgs e)
        {
            if (!(sender is Application app)) return;
            var uiApp = new UIApplication(app);

            try
            {
                BowerbirdService.Compile();
                // Run the auto-run commands 
                foreach (var cmd in BowerbirdService.Commands.Where(c => c.Name == "AutoRun"))
                {
                    cmd.Execute(uiApp);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Compilation and execution of auto-run failed: {ex}");
            }
        }
    

        public BowerbirdForm Window { get; private set; }

        public BowerbirdForm GetOrCreateWindow(IBowerbirdService service)
        {
            if (Window == null)
            {
                Window = new BowerbirdForm(service);
                Window.Text = Options.AppTitle;
                Window.FormClosing += (sender, args) =>
                {
                    Window.Hide();
                    args.Cancel = true;
                };
            }

            Window.Show();
            return Window;
        }

        public void ExecuteCommand(IBowerbirdCommand obj)
        {
            CommandExecutor.Raise(obj);
        }

        public void Run(UIApplication application)
        {
            if (UiApp == null)
            {
                UiApp = application;
            }
            GetOrCreateWindow(BowerbirdService);
        }
    }
}
