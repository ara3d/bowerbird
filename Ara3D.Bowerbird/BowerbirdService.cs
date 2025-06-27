using System.Diagnostics;
using Ara3D.Domo;
using Ara3D.Events;
using Ara3D.Logging;
using Ara3D.ScriptService;
using Ara3D.Services;
using Ara3D.Utils;

namespace Ara3D.Bowerbird;

/// <summary>
/// This wraps the ScriptingService interface provided byt the Ara 3D SDK,
/// which is unfortunately more complicated than it needs to be. 
/// </summary>
public class BowerbirdService
    : IServiceManager, IEventErrorHandler
{
    private readonly List<INamedCommand> _commands = new();
    private readonly List<IRepository> _repositories = new();
    private readonly List<IService> _services = new();
    public IReadOnlyList<INamedCommand> Commands => _commands;
    public IEventBus EventBus { get; }
    public ScriptingService ScriptingService { get; }
    public ScriptingOptions Options => ScriptingService.Options;
    public ILogger Logger => ScriptingService.Logger;
    public ScriptingDataModel ScriptingData => ScriptingService.Value;
    public event EventHandler RecompilationEvent;

    public BowerbirdService(ScriptingOptions options, ILogger logger)
    {
        EventBus = new EventBus(this);
        ScriptingService = new ScriptingService(this, logger ?? Logging.Logger.Debug, options);
        ScriptingService.Repository.OnModelChanged(DataModelChanged);
    }

    public void DataModelChanged(IModel<ScriptingDataModel> model)
    {
        try
        {
            UpdateCommands(model.Value);
            RecompilationEvent?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

    public void UpdateCommands(ScriptingDataModel model)
    {
        _commands.Clear();
        if (!model.LoadSuccess)
            return;
        var commandTypes = ScriptingService.Types.Where(t => t.ImplementsInterface(typeof(INamedCommand)));
        foreach (var t in commandTypes)
        {
            try
            {
                var command = Activator.CreateInstance(t);
                if (command != null)
                    _commands.Add(command as INamedCommand);
                else
                    Logger.Log($"Failed to create command from {t}, returned null");
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to create command from {t}, exception thrown {ex}");
            }
        }
    }

    public IReadOnlyList<IRepository> GetRepositories()
        => _repositories;

    public IReadOnlyList<IService> GetServices()
        => _services;

    public void AddService(IService service)
        => _services.Add(service);

    public void AddRepository(IRepository repository)
        => _repositories.Add(repository);

    public void OnError(ISubscriber sub, IEvent ev, Exception ex)
    {
        Debugger.Break();
        Debug.WriteLine($"Error occurred in {sub} with event {ev}: {ex}");
    }

    public void Compile()
        => ScriptingService.Compile();
}