using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Ara3D.Utils.Roslyn
{
    public class CompilerOptions
    {
        public CompilerOptions(IEnumerable<FilePath> fileReferences, FilePath outputFileName, bool debug)
            => (FileReferences, OutputFile, Debug) = (fileReferences.ToList(), outputFileName, debug);

        public FilePath OutputFile { get; }
        public bool Debug { get; }
        public IReadOnlyList<FilePath> FileReferences { get; }

        public string AssemblyName
            => OutputFile.GetFileNameWithoutExtension();

        public LanguageVersion Language
            => LanguageVersion.CSharp7_3;

        public CSharpParseOptions ParseOptions
            => new CSharpParseOptions(Language);

        public CSharpCompilationOptions CompilationOptions
            => new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOverflowChecks(true)                
                .WithOptimizationLevel(Debug ? OptimizationLevel.Debug : OptimizationLevel.Release);
    
        public IEnumerable<MetadataReference> MetadataReferences
            => RoslynUtils.ReferencesFromFiles(FileReferences);

        public CompilerOptions WithNewOutputFilePath(string fileName = null)
            => new CompilerOptions(FileReferences, fileName, Debug);

        public CompilerOptions WithNewReferences(IEnumerable<FilePath> fileReferences)
            => new CompilerOptions(fileReferences, OutputFile, Debug);

        public static CompilerOptions CreateDefault()
            => new CompilerOptions(RoslynUtils.LoadedAssemblyLocations(), 
                RoslynUtils.GenerateNewDllFileName(), true);

        public static CompilerOptions CreateDefault(Type[] types)
            => new CompilerOptions(types.Select(t => (FilePath)t.Assembly.Location),
                RoslynUtils.GenerateNewDllFileName(), true);
    }
}
