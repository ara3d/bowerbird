using System;
using Ara3D.Logging;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.Revit
{
    public class RevitContext : IExternalEventHandler
    {
        public Action<UIApplication> Action { get; private set; }
        private readonly ExternalEvent _event;
        public ILogger Logger { get; }
        public string Name { get; private set; }

        public RevitContext(ILogger logger)
        {
            Logger = logger;
            _event = ExternalEvent.Create(this);
        }

        public void Schedule(Action<UIApplication> action, string name = "untitled")
        {
            Name = name;
            Action = action;
            Logger.Log($"Scheduling an action: {Name}");
            _event.Raise();
        }

        public void Execute(UIApplication app)
        {
            try
            {
                Logger.Log($"Executing action: {Name}");
                Action?.Invoke(app);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error occureed {ex}");
            }
            finally
            {
                Action = null;
            }
        }

        public string GetName()
        {
            return "Revit context";
        }
    }
}