using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Logging;
using Ara3D.Services;
using Ara3D.Utils;
using Ara3D.Utils.Roslyn;

namespace Ara3D.Bowerbird.Core
{
    public class BowerbirdService : 
        SingletonModelBackedService<BowerbirdDataModel>, 
        IBowerbirdService
    {
        public DirectoryCompiler Compiler { get; }
        public ILogger Logger { get; set; }
        public BowerbirdOptions Options { get; }
        public Assembly Assembly => Compiler?.Assembly;
        public IBowerbirdHost Host { get; }
        public IReadOnlyList<IBowerbirdCommand> Commands { get; private set; }

        public BowerbirdService(IBowerbirdHost host, IApplication app, ILogger logger, BowerbirdOptions options)
            : base(app)
        {
            Host = host;
            Logger = logger ?? new Logger(LogWriter.DebugWriter, "Bowerbird");
            Options = options;
            CreateInitialFolders();
            Compiler = new DirectoryCompiler(Logger, Options.ScriptsFolder, Options.LibrariesFolder);
            Compiler.RecompileEvent += Compiler_RecompileEvent;
            UpdateDataModel();
            Commands = new List<IBowerbirdCommand>();
        }

        public void ExecuteCommand(IBowerbirdCommand command)
        {
            try
            {
                Logger.Log($"Starting command execution: {command.Name}");
                Host.ExecuteCommand(command);
                Logger.Log($"Finished command execution: {command.Name}");
            }
            catch (Exception e)
            {
                Logger.LogError($"Command execution failed: {e}");
            }
        }

        public void Compile()
        {
            Compiler.Compile();
        }

        public override void Dispose()
        {
            base.Dispose();
            Compiler.Dispose();
        }

        private void Compiler_RecompileEvent(object sender, EventArgs e)
        {
            // TODO: get the plugins from the assembly if things ar successful
            UpdateDataModel();
        }

        public void CreateInitialFolders()
        {
            Options.ScriptsFolder.Create();
            Options.LibrariesFolder.Create();
        }

        public bool AutoRecompile
        {
            get => Compiler.AutoRecompile;
            set => Compiler.AutoRecompile = value;
        }

        public void UpdateDataModel()
        {
            var types = Compiler?.ExportedTypes ?? Array.Empty<Type>();
            var cmds = new List<IBowerbirdCommand>();
            foreach (var type in types)
            {
                if (!typeof(IBowerbirdCommand).IsAssignableFrom(type))
                {
                    Logger.Log($"Not instantiating {type.Name} since it is not an IBowerbirdCommand");
                    continue;
                }

                try
                {
                    var cmd = Activator.CreateInstance(type);
                    if (cmd == null)
                    {
                        Logger.LogError($"Failed to create instance of type {type}");
                        continue;
                    }
                    var bbCmd = cmd as IBowerbirdCommand;
                    if (bbCmd == null)
                    {
                        Logger.LogError($"Failed to cast instance of type {type} to IBowerbirdCommand");
                        continue;
                    }
                    cmds.Add(bbCmd);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Exception occurred while processing type {type}");
                }
            }

            Commands = cmds;

            Repository.Value = new BowerbirdDataModel()
            {
                Dll = Assembly?.Location ?? "",
                Directory = Compiler?.Directory,
                TypeNames = types.Select(t => t.FullName).OrderBy(t => t).ToArray(),
                Files = Compiler?.Input?.SourceFiles?.Select(sf => sf.FilePath).OrderBy(x => x.Value).ToArray() ?? Array.Empty<FilePath>(),
                Assemblies = Compiler?.Refs?.Select(fp => fp.Value).ToList(),
                Diagnostics = Compiler?.Compilation?.Diagnostics?.Select(d => d.ToString()).ToArray() ?? Array.Empty<string>(),
                ParseSuccess = Compiler?.Input?.HasParseErrors == false,
                EmitSuccess = Compiler?.CompilationSuccess == true,
                LoadSuccess = Assembly != null,
                Options =  Options,
                Commands = cmds.Select(c => c.Name).ToList(),
            };
        }
    }
}