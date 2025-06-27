using Ara3D.Utils;

namespace Ara3D.Bowerbird;

/// <summary>
/// This can be used to assure that the command is executed on the correct thread,
/// and to assure that exceptions don't crash the application
/// </summary>
public interface ICommandExecutor
{
    void Execute(INamedCommand command, object parameter = null);
}