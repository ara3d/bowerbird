using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Ara3D.Logging;

namespace Ara3D.Utils.Roslyn
{
    public class DirectoryWatchingCompiler : IDisposable
    {
        public ILogger Logger { get; }
        public Compiler Compiler { get; private set; }

        public DirectoryPath Directory => Watcher.Directory;
        public DirectoryWatcher Watcher { get; }

        public bool AutoRecompile { get; set; } = true;
        public CancellationTokenSource TokenSource { get; private set; } = new CancellationTokenSource();
        
        public void Log(string s) => Logger?.Log(s);

        public event EventHandler RecompileEvent;
        public event EventHandler DirectoryChange;

        public const string BinaryFolderName = "bin";
        public static string BaseFileName => "output.dll";
        public DirectoryPath LibsDirectoryPath { get; }
        public DirectoryPath OutputFolder => Directory.RelativeFolder(BinaryFolderName);
        public FilePath BaseFilePath => OutputFolder.RelativeFile(BaseFileName);
        public readonly List<FilePath> LoadedAssemblies;
        public CompilerOptions Options { get; private set; }

        public FilePath GenerateUniqueFileName()
            => BaseFilePath.ToUniqueTimeStampedFileName();

        public DirectoryWatchingCompiler(ILogger logger, DirectoryPath inputDir, DirectoryPath libsDir, bool recursive = false, 
            CompilerOptions options = null)
        {
            LoadedAssemblies = RoslynUtils.LoadedAssemblyLocations().ToList();
            Logger = logger;
            Log("Creating directory compiler");
            Options = options ?? CompilerOptions.CreateDefault();
            if (!inputDir.Exists())
                throw new System.Exception($"Directory {inputDir} does not exist");
            LibsDirectoryPath = libsDir;

            var dirFile = inputDir.RelativeFile("dir.txt");
            Log($"Looking for redirection file {dirFile}");
            if (dirFile.Exists())
            {
                var redirPath = new DirectoryPath(dirFile.ReadAllText().Trim());

                try
                {
                    if (redirPath.Exists())
                    {
                        Log($"Redirecting to {redirPath}");
                        inputDir = new DirectoryPath(redirPath);
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Log($"Error {e} occured when loading redirect file");
                }
            }

            Watcher = new DirectoryWatcher(inputDir, "*.cs", recursive, OnChange);
        }

        public void OnChange()            
        {
            DirectoryChange?.Invoke(this, EventArgs.Empty);
            if (AutoRecompile)
                Compile();
        }

        public const string RefsFileName = "refs.txt";

        public CompilerOptions GetOptionsWithNewName() 
            => Options.WithNewOutputFilePath(GenerateUniqueFileName());

        public void Compile()
        {
            try
            {
                Log("Requesting cancel of existing work...");
                TokenSource.Cancel();
                TokenSource = new CancellationTokenSource();
                var token = TokenSource.Token;

                Compiler = null;

                Log($"Creating and clearing the output folder, if possible: {Directory}");
                OutputFolder.TryToCreateAndClearDirectory();

                Log($"Compilation task started of {Directory}");
                var refsFile = Directory.RelativeFile(RefsFileName);

                Log($"Loading references from {refsFile}, exists is {refsFile.Exists()}");
                
                // Set up the refs with the default references
                var refs = new List<FilePath>(Options.FileReferences);

                if (refsFile.Exists())
                {
                    var lines = refsFile.ReadAllLines();

                    var thisFolder = new FilePath(typeof(DirectoryWatchingCompiler).Assembly.Location).GetDirectory();

                    Log($"Found {refs.Count} references in '{RefsFileName}'");
                    foreach (var line in lines)
                    {
                        if (line.IsNullOrWhiteSpace())
                            continue;

                        var fp = LoadedAssemblies.FirstOrDefault(f => f.Value.EndsWith(line));
                        if (fp != null && fp.Exists())
                        {
                            refs.Add(fp);
                            continue;
                        }

                        fp = new FilePath(line);
                        if (fp.Exists())
                        {
                            refs.Add(fp);
                            Assembly.LoadFile(fp.GetFullPath());
                            continue;
                        }

                        fp = thisFolder.RelativeFile(line);
                        if (fp.Exists())
                        {
                            refs.Add(fp);
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
                        refs.Add(f);
                    }
                }

                // Add all locally loaded assemblies
                Options = Options.WithNewReferences(refs.Select(r => new FilePath(r)));

                Log($"All references:");
                foreach (var f in Options.MetadataReferences)
                    Log($"  Reference: {f.Display}");

                Options = GetOptionsWithNewName();
                Log($"Generated new output file name = {Options.OutputFile}");

                var inputFiles = Watcher.GetFiles().ToList();
                foreach (var f in inputFiles)
                    Log($"  Input file {f} found in directory");

                try
                {
                    Log("Creating compiler");
                    Compiler = new Compiler(inputFiles, refs, Options, Logger, token);
                }
                catch (Exception ex)
                {
                    Log($"Exception during compilation: {ex}");
                }
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
