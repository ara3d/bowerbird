using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Ara3D.Domo;
using Ara3D.Services;
using Ara3D.Utils;
using Ara3D.Utils.Roslyn;

namespace Ara3D.Bowerbird.Core
{
    public class BowerbirdDataModel
    {
        public IReadOnlyList<Type> Types = Array.Empty<Type>();
        public IReadOnlyList<string> Files = Array.Empty<string>();
        public IReadOnlyList<string> Diagnostics = Array.Empty<string>();
        public string Dll = "";
        public BowerbirdOptions Options = new BowerbirdOptions();
        public bool ParseSuccess;
        public bool EmitSuccess;
        public bool LoadSuccess;
    }
        
    public class BowerbirdService : BaseService
    {
        public DirectoryPath Directory => Compiler.Directory; 
        public DirectoryCompiler Compiler { get; }
        public ILogger Logger { get; }
        public CancellationTokenSource TokenSource { get; private set; }
        public BowerbirdOptions Options { get; }
        public Assembly Assembly => Compiler?.Assembly;
        public SingletonRepository<BowerbirdDataModel> Repo { get; } = new SingletonRepository<BowerbirdDataModel>();

        public BowerbirdService(IApplication app, ILogger logger, BowerbirdOptions options)
            : base(app)
        {
            Logger = logger;
            Options = options;
            UpdateDataModel();
            CreateInitialFolders();
            Compiler = new DirectoryCompiler(Logger, Options.ScriptsFolder, Options.LibrariesFolder);
            Compiler.RecompileEvent += Compiler_RecompileEvent;
        }

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
            Repo.Value = new BowerbirdDataModel()
            {
                Dll = Assembly?.Location ?? "",
                Types = Compiler?.ExportedTypes?.OrderBy(t => t.FullName).ToArray() ?? Array.Empty<Type>(),
                Files = Compiler?.Input?.SourceFiles?.Select(sf => sf.FilePath.Value).OrderBy(x => x).ToArray() ?? Array.Empty<string>(),
                Diagnostics = Compiler?.Compilation?.Diagnostics?.Select(d => d.ToString()).ToArray() ?? Array.Empty<string>(),
                ParseSuccess = Compiler?.Input?.HasParseErrors == false,
                EmitSuccess = Compiler?.CompilationSuccess == true,
                LoadSuccess = Assembly != null,
            };
        }
    }
}