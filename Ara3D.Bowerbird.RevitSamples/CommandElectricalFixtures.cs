using System.Linq;
using Ara3D.Bowerbird.Interfaces;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandElectricalFixtures : IBowerbirdCommand
    {
        public string Name => "Electrical Fixtures";

        public void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            var sockets = doc.GetSockets();
            var text = string.Join("\r\n", sockets.Select(s => ExtensionsRevit.GetRoomId(s).ToString()));
            TextDisplayForm.DisplayText(text);
        }
    }
}