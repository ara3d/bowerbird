using System.Windows.Forms;
using Ara3D.Logging;
using Ara3D.Utils;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.Revit
{
    public class CommandExecutor : IExternalEventHandler, ICommandExecutor
    {
        private INamedCommand _command;
        private readonly ExternalEvent _event;
        public ILogger Logger { get; }

        public CommandExecutor(ILogger logger)
        {
            Logger = logger;
            _event = ExternalEvent.Create(this);
        }

        public void Raise(INamedCommand command)
        {
            SetCommand(command);
            _event.Raise();
        }

        public void SetCommand(INamedCommand command)
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
                MessageBox.Show($"Caught exception: {ex}");
            }
            finally
            {
                ResetCommand();
            }
        }

        public string GetName()
            => "Command Executor";

        public void Execute(INamedCommand command, object parameter = null)
        {
            Raise(command);
        }
    }
}