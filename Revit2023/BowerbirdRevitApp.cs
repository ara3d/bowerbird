using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;
using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Bowerbird.WinForms.Net48;
using Ara3D.Logging;
using Ara3D.Utils;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using Bitmap = System.Drawing.Bitmap;

namespace Ara3D.Bowerbird.Revit
{
    public class BowerbirdRevitApp : IExternalApplication, IBowerbirdHost
    {
        public const string BOWERBIRD_AUTORUN_ONLOAD_ENV_VAR = "BOWERBIRD_AUTORUN_ONLOAD";

        public static BowerbirdRevitApp Instance { get; private set; }
        public RevitContext RevitContext { get; private set; }

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
            if (args.Name.Contains("Bowerbird.Revit") && !args.Name.Contains("resources"))
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

            var logger = new Logger(LogWriter.DebugWriter, "Bowerbird");
            CommandExecutor = new CommandExecutor(logger);
            RevitContext = new RevitContext(logger);

            application.ControlledApplication.DocumentOpened += App_DocumentOpened;

            // Store a reference to the UIApplication
            application.Idling += (sender, args) =>
            {
                if (UiApp == null)
                {
                    UiApp = sender as UIApplication;
                }
            };

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
            
            BowerbirdService = new BowerbirdService(this, ServiceApp, logger, Options);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            try
            {
                BowerbirdService.Compile();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Compilation failed: {ex}");
            }
            
            return Result.Succeeded;
        }

        private void App_DocumentOpened(object sender, DocumentOpenedEventArgs e)
        {
            var autoRunScript = Environment.GetEnvironmentVariable(BOWERBIRD_AUTORUN_ONLOAD_ENV_VAR);

            if (autoRunScript != null)
            {
                var fp = new FilePath(autoRunScript);
                if (fp.Exists())
                {
                    var command = BowerbirdService.CompileSingleCommand(fp);

                    if (UiApp == null)
                        UiApp = new UIApplication(e.Document.Application);

                    command.Execute(UiApp); 
                }
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

        public void Schedule(Action<UIApplication> action, string name = "")
        {
            RevitContext.Schedule(action, name);
        }
    }
}
