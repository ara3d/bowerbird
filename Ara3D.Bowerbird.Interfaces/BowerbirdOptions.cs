using Ara3D.Utils;

namespace Ara3D.Bowerbird.Interfaces
{
    public class BowerbirdOptions
    {
        public string AppName { get; set; }
        public string OrgName { get; set; }
        public FilePath ConfigFile { get; set; }
        public DirectoryPath ScriptsFolder { get; set; }
        public DirectoryPath LibrariesFolder { get; set; }
        public string AppTitle => $"BETA - {AppName} v{GetType().GetAssemblyData().Version} by Ara 3D";

        public static BowerbirdOptions CreateFromName(string appName)
            => CreateFromName("Ara 3D", appName);

        public static BowerbirdOptions CreateFromName(string orgName, string appName)
        {
            var appData = SpecialFolders.LocalApplicationData;
            return new BowerbirdOptions()
            {
                OrgName = orgName,
                AppName = appName,
                ConfigFile = appData.RelativeFile(orgName, appName, "config.json"),
                ScriptsFolder = appData.RelativeFolder(orgName, appName, "Scripts"),
                LibrariesFolder = appData.RelativeFolder(orgName, appName, "Libraries"),
            };
        }
    }
}