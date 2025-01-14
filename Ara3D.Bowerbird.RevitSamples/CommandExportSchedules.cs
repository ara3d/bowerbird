using Ara3D.Bowerbird.Interfaces;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandExportSchedules : IBowerbirdCommand
    {
        public string Name => "Export Schedules";

        public void Execute(object arg)
        {
            var uidoc = (arg as UIApplication)?.ActiveUIDocument;
            if (uidoc == null) return;
            var doc = uidoc.Document;
            doc.WriteSchedulesAsJson(Config.OutputDir);
            Ara3D.Utils.ProcessUtil.OpenFolderInExplorer(Config.OutputDir);
        }
    }
}