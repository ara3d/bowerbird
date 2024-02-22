using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Ara3D.Utils.Roslyn
{
    /// <summary>
    /// Represents the input and output.
    /// However, we need to compile multiple things. So the service. 
    /// </summary>
    public class Compilation
    {
        public CompilerInput Input { get; }
        public EmitResult EmitResult { get; }
        public CSharpCompilation Compiler { get; }
        public CompilerOptions Options => Input.Options;
        public FilePath OutputFile => Input.Options.OutputFile;
        public IReadOnlyList<SemanticModel> SemanticModels { get; }
        public IEnumerable<Diagnostic> Diagnostics => Compiler.GetDiagnostics();

        public Compilation(CompilerInput input,
            CSharpCompilation compiler,
            EmitResult emitResult)
        {
            Input = input;
            Compiler = compiler;
            EmitResult = emitResult;
            SemanticModels = input.SourceFiles.Select(sf => Compiler?.GetSemanticModel(sf.SyntaxTree)).ToList();
        }
    }

    public static partial class RoslynUtils
    {
        public static Compilation CompileCSharpStandard(this CompilerInput input,
            CSharpCompilation compiler = default,
            CancellationToken token = default)
        {
            compiler = compiler ?? CSharpCompilation.Create(
                input.Options.AssemblyName, 
                input.SyntaxTrees,
                input.Options.MetadataReferences, 
                input.Options.CompilationOptions);

            var outputPath = input.Options.OutputFile;
            outputPath.DeleteAndCreateDirectory();

            EmitResult emitResult;
            using (var peStream = File.OpenWrite(outputPath))
            {
                var emitOptions = new EmitOptions(false, DebugInformationFormat.Embedded);
                emitResult = compiler.Emit(peStream, null, null, null, null, emitOptions, 
                    null, null, input.EmbeddedTexts, token);
            }
            if (!emitResult.Success)
                outputPath.Delete();
            return new Compilation(input, compiler, emitResult);
        }

        public static Compilation CompileCSharpStandard(this FilePath path, CompilerOptions options = default, CancellationToken token = default)
            => path.ParseCSharp().ToCompilerInput(options).CompileCSharpStandard(default, token);

        public static Compilation CompileCSharpStandard(string source, CompilerOptions options = default, CancellationToken token = default)
            => ParseCSharp(source).ToCompilerInput(options).CompileCSharpStandard(default, token);

        public static Compilation CompileCSharpStandard(this ParsedSourceFile inputFile, CompilerOptions options = default, CancellationToken token = default)
            => inputFile.ToCompilerInput(options).CompileCSharpStandard(default, token);

        public static Compilation CompileCSharpStandard(this IEnumerable<ParsedSourceFile> inputFiles, CompilerOptions options = default, CancellationToken token = default)
            => inputFiles.ToCompilerInput(options).CompileCSharpStandard(default, token);
    }
}
