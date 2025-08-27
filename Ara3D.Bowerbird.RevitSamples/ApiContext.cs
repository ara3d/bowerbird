using System;
using System.Runtime.InteropServices;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples;

/// <summary>
/// This allows you to execute arbitrary Revit code within a valid UI context. 
/// </summary>
public static class ApiContext
{
    [DllImport("USER32.DLL")]
    public static extern bool PostMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

    public class ExternalEventHandler : IExternalEventHandler
    {
        public readonly Action<UIApplication> Action;
        public readonly string Name;

        public ExternalEventHandler(Action<UIApplication> action, string name)
        {
            Action = action ?? throw new ArgumentNullException(nameof(action));
            Name = name;
        }

        public void Execute(UIApplication app)
        {
            try
            {
                Action(app);
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Error", ex.Message);
            }
        }

        public string GetName()
            => Name;
    }

    public static ExternalEvent CreateEvent(Action<UIApplication> action, string name)
    {
        var eeh = new ExternalEventHandler(action, name);
        return ExternalEvent.Create(eeh);
    }
}