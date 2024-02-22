using System.Collections.Generic;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Interfaces
{
    public interface IPlugin
    {
        void Initialize(IBowerbirdService service);
        void ShutDown();
        IReadOnlyList<INamedCommand> Commands { get; }
    }
    
    public interface IBowerbirdService
    {
        ICommandService CommandService { get; }
        IPluginService PluginService { get; }
    }

    public interface IPluginService
    {
        IReadOnlyList<IPlugin> GetPlugins();
    }

    public interface ICommandService
    {
        IReadOnlyList<INamedCommand> GetCommands();
    }
}
