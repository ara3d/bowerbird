using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Ara3D.Logging;

namespace Ara3D.Utils.Roslyn
{
    public class Compiler
    {
        public ILogger Logger { get; }
        public Assembly Assembly { get; }
        public Compilation Compilation { get;  }
        public CompilerOptions Options { get; }
        public IReadOnlyList<FilePath> Refs { get; } 
        public IReadOnlyList<ParsedSourceFile> SourceFiles { get; } 
        public CompilerInput Input { get; }
        public bool CompilationSuccess => Compilation?.EmitResult?.Success == true;
        public FilePath OutputFile => Options.OutputFile;
        public IEnumerable<Type> ExportedTypes => Assembly?.ExportedTypes ?? Enumerable.Empty<Type>();

        public void Log(string s) => Logger?.Log(s);

        public Compiler(IReadOnlyList<FilePath> inputFiles, IReadOnlyList<FilePath> refs, CompilerOptions options, ILogger logger, CancellationToken token)
        {
            Logger = logger;
            Options = options;
            Refs = refs;

            Log("Parsing input files");
            SourceFiles = inputFiles.ParseCSharp(Options, token);

            if (token.IsCancellationRequested)
            {
                Log($"Compilation canceled");
                return;
            }

            Log("Compiling");
            Input = SourceFiles.ToCompilerInput(Options);
            Compilation = Input.CompileCSharpStandard(Compilation?.Compiler, token);
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

                Log(Assembly == null
                    ? $"Failed to load assembly from {OutputFile}"
                    : $"Loaded assembly from {OutputFile}");
            }
            else
            {
                Log("Compilation failed");
            }

            Log($"Completed compilation");
        }
    }
}