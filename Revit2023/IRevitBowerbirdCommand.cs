using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.Wpf
{
    public interface IRevitBowerbirdCommand
    {
        string Name { get; }
        void Execute(UIApplication application);
    }
}