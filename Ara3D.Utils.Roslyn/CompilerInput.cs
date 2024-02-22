using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Ara3D.Utils.Roslyn
{
    public class CompilerInput
    {
        public IReadOnlyList<ParsedSourceFile> SourceFiles { get; }
        public CompilerOptions Options { get; }
        public IEnumerable<SyntaxTree> SyntaxTrees => SourceFiles.Select(sf => sf.SyntaxTree);
        public IEnumerable<EmbeddedText> EmbeddedTexts => SourceFiles.Select(sf => sf.EmbeddedText);
        public bool HasParseErrors => SourceFiles.Any(sf => !sf.Success);
        
        public CompilerInput(IEnumerable<ParsedSourceFile> sourceFiles, CompilerOptions options)
        {
            SourceFiles = sourceFiles.ToList();
            Options = options;
        }
    }
}