using System.Collections.Generic;
using System.Reflection;
using Ara3D.Services;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Interfaces
{
    /*
     TODO: this was a theory on how I would like it to work.

    public interface IPlugin
    {
        void Initialize(IBowerbirdService service);
        void ShutDown();
        IReadOnlyList<INamedCommand> Commands { get; }
    }


    public interface IPluginService
    {
        IReadOnlyList<IPlugin> GetPlugins();
    }

    public interface ICommandService
    {
        IReadOnlyList<INamedCommand> GetCommands();
    }

    public interface IBowerbirdService
    {
        ICommandService CommandService { get; }
        IPluginService PluginService { get; }
    }
    */

    public interface IBowerbirdService 
        : ISingletonModelBackedService<BowerbirdDataModel>
    {
        BowerbirdOptions Options { get; }

        // TODO: I think eventually this will be abstracted away into a "plug-in" system.
        Assembly Assembly { get; }

        void Compile();
    }
}
