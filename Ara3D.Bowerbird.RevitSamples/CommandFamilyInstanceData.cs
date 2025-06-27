using System.Linq;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandFamilyInstanceData : NamedCommand
    {
        public override string Name => "Family Instances";

        public override void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            var instances = doc.GetFamilyInstances();
            var grps = instances.GroupBy(fi => fi.Symbol.Category.Name).OrderBy(g => g.Key);
            var text = string.Join("\r\n", grps.Select(g => $"{g.Key} = {g.Count()}"));
            TextDisplayForm.DisplayText(text);
        }
    }
}