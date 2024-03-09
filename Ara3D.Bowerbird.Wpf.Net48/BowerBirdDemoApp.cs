using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Services;
using Ara3D.Utils;
using Ara3D.Logging;

namespace Ara3D.Bowerbird.Wpf.Net48
{
    public class BowerbirdDemoApp
    {
        public IApplication App { get; } = new Application();
        public IBowerbirdService Service { get; }
        public ILoggingService Logger { get; }
        
        public static readonly DirectoryPath SamplesSrcFolder 
            = PathUtil.GetCallerSourceFolder().RelativeFolder("Samples");

        public static readonly BowerbirdOptions Options = 
            BowerbirdOptions.CreateFromName("Ara 3D", "Bowerbird WPF Demo");

        public BowerbirdDemoApp()
        {
            Logger = new LoggingService("Compilation", App);
            Logger.Log($"Copying script files from {SamplesSrcFolder} to {Options.ScriptsFolder}");
            SamplesSrcFolder.CopyDirectory(Options.ScriptsFolder);
            Service = new BowerbirdService(App, Logger, Options);
        }
    }
}