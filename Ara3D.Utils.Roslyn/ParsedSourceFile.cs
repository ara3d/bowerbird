using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Ara3D.Utils.Roslyn
{
    /// <summary>
    /// Contains C# source code, the associated syntax tree, and the source file path.
    /// If not source path is provided then it creates a temp file. 
    /// It also creates an embedded text for the purpose of creating PDBs. 
    /// </summary>
    public class ParsedSourceFile
    {
        public readonly SourceText SourceText;
        public readonly SyntaxTree SyntaxTree;
        public readonly FilePath FilePath;
        public readonly EmbeddedText EmbeddedText;
        public ParseOptions ParseOptions => SyntaxTree.Options;
        public IEnumerable<Diagnostic> Diagnostics => SyntaxTree.GetDiagnostics();
        public bool HasErrors => Diagnostics.Any(st => st.WarningLevel == 0);
        public bool Success => SyntaxTree != null && !HasErrors;

        public ParsedSourceFile(SourceText source, SyntaxTree tree, FilePath filePath)
        {
            SourceText = source;
            FilePath = filePath;
            EmbeddedText = EmbeddedText.FromSource(FilePath, SourceText);
            SyntaxTree = tree;
        }

        public ParsedSourceFile Update(SourceText newText)
            => new ParsedSourceFile(newText, SyntaxTree.WithChangedText(newText), FilePath);

        public ParsedSourceFile UpdateFromFile()
            => Update(SourceText.From(FilePath.ReadAllText(), Encoding.UTF8));

        public static ParsedSourceFile Create(FilePath filePath, CSharpParseOptions options, CancellationToken token = default)
        {
            var newSource = SourceText.From(filePath.ReadAllText(), Encoding.UTF8);
            var newTree = CSharpSyntaxTree.ParseText(newSource, options, filePath, token);
            return new ParsedSourceFile(newSource, newTree, filePath);
        }

        public static ParsedSourceFile CreateFromSource(string source, CSharpParseOptions options, CancellationToken token = default)
        {
            var newSource = SourceText.From(source, Encoding.UTF8);
            var filePath = PathUtil.CreateTempFileWithContents(source);
            var newTree = CSharpSyntaxTree.ParseText(newSource, options, filePath, token);
            return new ParsedSourceFile(newSource, newTree, filePath);
        }
    }

    public static partial class RoslynUtils
    {
        public static CSharpParseOptions CSharpStandardParseOptions 
            = new CSharpParseOptions(LanguageVersion.CSharp7_3);

        public static ParsedSourceFile ParseCSharp(string source, CompilerOptions options = default, CancellationToken token = default)
            => ParsedSourceFile.CreateFromSource(source, options?.ParseOptions ?? CSharpStandardParseOptions, token);
        
        public static IReadOnlyList<ParsedSourceFile> ParseCSharp(this IEnumerable<FilePath> files, CompilerOptions options = default, CancellationToken token = default)
            => files.Select(f => ParseCSharp(f, options, token)).ToList();

        public static ParsedSourceFile ParseCSharp(this FilePath filePath, CompilerOptions options = null, CancellationToken token = default)
            => ParsedSourceFile.Create(filePath, options?.ParseOptions ?? CSharpStandardParseOptions ?? CSharpStandardParseOptions, token);
    }
}
