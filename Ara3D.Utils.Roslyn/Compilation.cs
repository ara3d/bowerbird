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

}
