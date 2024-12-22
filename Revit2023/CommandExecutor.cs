using Ara3D.Bowerbird.Interfaces;
using Ara3D.Logging;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.Revit
{
    public class CommandExecutor : IExternalEventHandler
    {
        private IBowerbirdCommand _command;
        private readonly ExternalEvent _event;
        public ILogger Logger { get; }

        public CommandExecutor(ILogger logger)
        {
            Logger = logger;
            _event = ExternalEvent.Create(this);
        }

        public void Raise(IBowerbirdCommand command)
        {
            SetCommand(command);
            _event.Raise();
        }

        public void SetCommand(IBowerbirdCommand command)
            => _command = command;

        public void ResetCommand()
            => _command = null;

        public void Execute(UIApplication app)
        {
            try
            {
                _command?.Execute(app);
            }
            catch (System.Exception ex)
            {
                Logger.Log($"{ex}");
            }
            finally
            {
                ResetCommand();
            }
        }

        public string GetName()
            => "Command Executor";
    }
}