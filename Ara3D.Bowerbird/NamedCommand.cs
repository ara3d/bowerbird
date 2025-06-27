using Ara3D.Utils;

namespace Ara3D.Bowerbird;

/// <summary>
/// This is a useful base-class for named commands
/// </summary>
public abstract class NamedCommand : INamedCommand
{
    public void NotifyCanExecuteChanged()
        => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    public virtual bool CanExecute(object parameter)
        => true;

    public event EventHandler CanExecuteChanged;

    public virtual string Name
        => GetType().Name;

    public virtual void Execute(object _)
        => Execute();

    public virtual void Execute()
    { }
}