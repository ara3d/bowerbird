using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandExportSchedules : NamedCommand
    {
        public override string Name => "Export Schedules";

        public override void Execute(object arg)
        {
            var uidoc = (arg as UIApplication)?.ActiveUIDocument;
            if (uidoc == null) return;
            var doc = uidoc.Document;
            doc.WriteSchedulesAsJson(Config.OutputDir);
            Ara3D.Utils.ProcessUtil.OpenFolderInExplorer(Config.OutputDir);
        }
    }
}