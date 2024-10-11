using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using Ara3D.Logging;

namespace Ara3D.Utils.Roslyn
{
    public class DirectoryCompiler : IDisposable
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
        public readonly List<FilePath> LoadedAssemblies;

        public FilePath GenerateUniqueFileName()
            => BaseFilePath.ToUniqueTimeStampedFileName();

        public DirectoryCompiler(ILogger logger, DirectoryPath inputDir, DirectoryPath libsDir, bool recursive = false, 
            CompilerOptions options = null)
        {
            LoadedAssemblies = RoslynUtils.LoadedAssemblyLocations().ToList();
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
        public const string IncludesFileName = "includes.txt";

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
                    var refs = refsFile.ReadAllLines();
                    
                    var thisFolder = new FilePath(typeof(DirectoryCompiler).Assembly.Location).GetDirectory();

                    Log($"Found {refs.Length} references in '{RefsFileName}'");
                    foreach (var line in refs)
                    {
                        if (line.IsNullOrWhiteSpace())
                            continue;

                        var fp = LoadedAssemblies.FirstOrDefault(f => f.Value.EndsWith(line));
                        if (fp != null && fp.Exists())
                        {
                            Refs.Add(fp);
                            continue;
                        }

                        fp = new FilePath(line);
                        if (fp.Exists())
                        {
                            Refs.Add(fp);
                            Assembly.LoadFile(fp.GetFullPath());
                            continue;
                        }

                        fp = thisFolder.RelativeFile(line);
                        if (fp.Exists())
                        {
                            Refs.Add(fp);
                            Assembly.LoadFile(fp.GetFullPath());
                            continue;
                        }

                        Logger.LogError($"Could not find referenced file: {line}");
                    }
                }   
                else
                {
                    Log($"No references file provided ({RefsFileName})");
                }

                if (LibsDirectoryPath == null)
                {
                    Log("No library directory provided");
                }
                else if (LibsDirectoryPath.Exists() == false)
                {
                    Log($"Library directory not found: {LibsDirectoryPath}");
                }
                else
                {
                    Log($"Loading references from: {LibsDirectoryPath}");
                    foreach (var f in LibsDirectoryPath.GetAllFilesRecursively())
                    {
                        Refs.Add(f);
                    }
                }

                // Add all locally loaded assemblies
                Options = Options.WithNewReferences(Refs);

                Log($"All references:");
                foreach (var f in Options.MetadataReferences)
                    Log($"  Reference: {f.Display}");

                Options = Options.WithNewOutputFilePath(GenerateUniqueFileName());
                Log($"Generated new output file name = {OutputFile}");

                var inputFiles = Watcher.GetFiles().ToList();
                foreach (var f in inputFiles)
                    Log($"  Input file {f} found in directory");

                var includesFile = Directory.RelativeFile(IncludesFileName);
                if (includesFile.Exists())
                {
                    Log($"Loading additional input files from {IncludesFileName}");
                    foreach (var line in includesFile.ReadAllLines())
                    {
                        if (!line.IsNullOrWhiteSpace())
                        {
                            var fp = new FilePath(line);
                            if (!fp.Exists())
                            {
                                Log($"  Error: included file not found - {line}");
                            }
                            else
                            {
                                inputFiles.Add(line);
                                Log($"   Included file found {line}");
                            }
                        }
                    }
                }
                else
                {
                    Log($"No {IncludesFileName} file provided");
                }

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

        public void Dispose()
        {
            Watcher.Dispose();
        }
    }
}
