using Ara3D.Bowerbird.Core;
using Ara3D.Services;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Wpf.Net48
{
    public class BowerBirdDemoApp
    {
        public IApplication App { get; } = new Application();
        public BowerbirdService Service { get; }
        public LoggingService Logger { get; }
        
        public static readonly DirectoryPath SamplesSrcFolder 
            = PathUtil.GetCallerSourceFolder().RelativeFolder("Samples");

        public static readonly BowerbirdOptions Options = 
            BowerbirdOptions.CreateFromName("Ara 3D", "Bowerbird WPF Demo");

        public BowerBirdDemoApp()
        {
            Logger = new LoggingService("Compilation", App);
            Logger.Log($"Copying script files from {SamplesSrcFolder} to {Options.ScriptsFolder}");
            SamplesSrcFolder.CopyDirectory(Options.ScriptsFolder);
            Service = new BowerbirdService(App, Logger, Options);
        }
    }
}