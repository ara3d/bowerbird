using Ara3D.Logging;
using Ara3D.Utils;

namespace Ara3D.Bowerbird;

public class DefaultCommandExecutor : ICommandExecutor
{
    public ILogger Logger { get; set; }

    public void Execute(INamedCommand command, object parameter = null)
    {
        try
        {
            Logger.Log($"Executing command {command.Name}");
            command.Execute(parameter);
            Logger.Log($"Completed execution of command {command.Name}");
        }
        catch (Exception ex)
        {
            Logger.LogError($"Error occurred during command execution.", ex);
        }
    }
}