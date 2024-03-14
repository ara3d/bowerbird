using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Ara3D.Logging;

namespace Ara3D.Utils.Roslyn
{
    // TODO: make a class called "LongRunningTask" 
    public class DirectoryCompiler
    {
        public ILogger Logger { get; }

        public Assembly Assembly { get; private set; }
        public Compilation Compilation { get; private set; }
        public CompilerOptions Options { get; private set; }
        public List<FilePath> Refs { get; } = new List<FilePath>();

        public CompilerInput Input => Compilation?.Input;
        public DirectoryPath Directory => Watcher.Directory;
        public DirectoryWatcher Watcher { get; }
        public bool AutoRecompile { get; set; } = true;
        public CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();
        public bool CompilationSuccess => Compilation?.EmitResult?.Success == true;
        public FilePath OutputFile => Options.OutputFile;
        public IEnumerable<Type> ExportedTypes => Assembly?.ExportedTypes ?? Enumerable.Empty<Type>();
        
        public void Log(string s)
            => Logger?.Log(s);

        public event EventHandler RecompileEvent;
        public event EventHandler DirectoryChange;

        public const string BinaryFolderName = "bin";
        public static string BaseFileName => "output.dll";
        public DirectoryPath LibsDirectoryPath { get; }
        public DirectoryPath OutputFolder => Directory.RelativeFolder(BinaryFolderName);
        public FilePath BaseFilePath => OutputFolder.RelativeFile(BaseFileName);

        public FilePath GenerateUniqueFileName()
            => BaseFilePath.ToUniqueTimeStampedFileName();

        public DirectoryCompiler(ILogger logger, DirectoryPath inputDir, DirectoryPath libsDir, bool recursive = false, 
            CompilerOptions options = null)
        {
            Logger = logger;
            Log("Creating directory compiler");
            Options = options ?? CompilerOptions.CreateDefault();
            if (!inputDir.Exists())
                throw new Exception($"Directory {inputDir} does not exist");
            LibsDirectoryPath = libsDir;
            Watcher = new DirectoryWatcher(inputDir, "*.cs", recursive, OnChange);
        }

        public void OnChange()            
        {
            DirectoryChange?.Invoke(this, EventArgs.Empty);
            if (AutoRecompile)
                Compile();
        }

        public const string RefsFileName = "refs.txt";

        public void Compile()
        {
            try
            {
                Log("Requesting cancel of existing work...");
                TokenSource.Cancel();
                TokenSource = new CancellationTokenSource();
                var token = TokenSource.Token;

                Compilation = null;
                Refs.Clear();
                Assembly = null;

                Log($"Creating and clearing the output folder, if possible: {Directory}");
                OutputFolder.TryToCreateAndClearDirectory();

                Log($"Compilation task started of {Directory}");
                var refsFile = Directory.RelativeFile(RefsFileName);

                if (refsFile.Exists())
                {
                    Refs.AddRange(refsFile.ReadAllLines().Select(f => new FilePath(f)));
                    Log($"Loaded {Refs.Count} references from '{RefsFileName}'");

                    foreach (var fp in Refs)
                    {
                        if (!fp.Exists())
                            Logger.LogError($"Could not find referenced file: {fp}");
                    }
                }
                else
                {
                    Log($"No references file name found '{RefsFileName}'");
                }

                if (LibsDirectoryPath == null)
                {
                    Log("No library directory provided");
                }
                else if (LibsDirectoryPath?.Exists() == false)
                {
                    Log($"Library directory not found: {LibsDirectoryPath}");
                }
                else
                {
                    Log($"Loading references from: {LibsDirectoryPath}");
                    foreach (var f in LibsDirectoryPath?.GetAllFilesRecursively())
                    {
                        Refs.Add(f);
                    }
                }

                // Add all locally loaded assemblies
                foreach (var f in RoslynUtils.LoadedAssemblyLocations())
                    Refs.Add(f);

                Options = Options.WithNewReferences(Refs);

                Log($"All references:");
                foreach (var f in Options.MetadataReferences)
                    Log($"  Reference: {f.Display}");

                Options = Options.WithNewOutputFilePath(GenerateUniqueFileName());
                Log($"Generated new output file name = {OutputFile}");

                var inputFiles = Watcher.GetFiles().ToArray();
                foreach (var f in inputFiles)
                    Log($"  Input file {f}");

                Log("Parsing input files");
                var sourceFiles = inputFiles.ParseCSharp(Options, token);
                
                if (token.IsCancellationRequested)
                {
                    Log($"Compilation canceled");
                    return;
                }

                Log("Generating compiler input");
                var input = sourceFiles.ToCompilerInput(Options);

                Log("Compiling");
                Compilation = input.CompileCSharpStandard(Compilation?.Compiler, token);
                if (token.IsCancellationRequested)
                {
                    Log($"Canceled");
                    return;
                }

                Log($"Diagnostics:");
                foreach (var x in Compilation.Diagnostics)
                    Log($"  Diagnostic: {x}");

                if (CompilationSuccess)
                {
                    Log("Compilation succeeded");
                    Assembly = FunctionUtils.TryGetValue(() => Assembly.LoadFile(OutputFile));

                    if (Assembly == null)
                    {
                        Log($"Failed to load assembly from {OutputFile}");
                    }
                    else
                    {
                        Log($"Loaded assembly from {OutputFile}");
                    }
                }
                else
                {
                    Log("Compilation failed");
                }

                Log($"Completed compilation");
            }
            finally
            {
                RecompileEvent?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
