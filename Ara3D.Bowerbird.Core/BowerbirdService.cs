using System;
using System.Linq;
using System.Reflection;
using Ara3D.Bowerbird.Interfaces;
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
        public ILogger Logger { get; }
        public BowerbirdOptions Options { get; }
        public Assembly Assembly => Compiler?.Assembly;

        public BowerbirdService(IApplication app, ILoggingService logger, BowerbirdOptions options)
            : base(app)
        {
            Logger = logger;
            Options = options;
            UpdateDataModel();
            CreateInitialFolders();
            Compiler = new DirectoryCompiler(Logger, Options.ScriptsFolder, Options.LibrariesFolder);
            Compiler.RecompileEvent += Compiler_RecompileEvent;
        }

        // TODO: make this a command.
        public void Compile()
        {
            Compiler.Compile();
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

        public void UpdateDataModel()
        {
            Repository.Value = new BowerbirdDataModel()
            {
                Dll = Assembly?.Location ?? "",
                Directory = Compiler?.Directory,
                TypeNames = Compiler?.ExportedTypes?.Select(t => t.FullName).OrderBy(t => t).ToArray() ?? Array.Empty<string>(),
                Files = Compiler?.Input?.SourceFiles?.Select(sf => sf.FilePath).OrderBy(x => x.Value).ToArray() ?? Array.Empty<FilePath>(),
                Diagnostics = Compiler?.Compilation?.Diagnostics?.Select(d => d.ToString()).ToArray() ?? Array.Empty<string>(),
                ParseSuccess = Compiler?.Input?.HasParseErrors == false,
                EmitSuccess = Compiler?.CompilationSuccess == true,
                LoadSuccess = Assembly != null,
            };
        }
    }
}