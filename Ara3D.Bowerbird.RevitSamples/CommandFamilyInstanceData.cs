using System.Linq;
using Ara3D.Bowerbird.Interfaces;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandFamilyInstanceData : IBowerbirdCommand
    {
        public string Name => "Family Instances";

        public void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            var instances = doc.GetFamilyInstances();
            var grps = instances.GroupBy(fi => fi.Symbol.Category.Name).OrderBy(g => g.Key);
            var text = string.Join("\r\n", grps.Select(g => $"{g.Key} = {g.Count()}"));
            TextDisplayForm.DisplayText(text);
        }
    }
}