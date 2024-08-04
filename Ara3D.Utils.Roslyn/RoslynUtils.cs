using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Ara3D.Utils.Roslyn
{
    public static partial class RoslynUtils
    {
        public static CompilerInput ToCompilerInput(this ParsedSourceFile sourceFile,
            CompilerOptions options = default)
            => new[] { sourceFile }.ToCompilerInput(options);

        public static CompilerInput ToCompilerInput(this IEnumerable<ParsedSourceFile> sourceFiles,
            CompilerOptions options = default)
            => new CompilerInput(sourceFiles, options ?? CompilerOptions.CreateDefault());

        public static IEnumerable<MetadataReference> ReferencesFromFiles(IEnumerable<FilePath> files)
            => files.Select(x => MetadataReference.CreateFromFile(x));

        public static IEnumerable<MetadataReference> ReferencesFromLoadedAssemblies()
            => ReferencesFromFiles(LoadedAssemblyLocations());

        public static IEnumerable<FilePath> LoadedAssemblyLocations(AppDomain domain = null)
            => (domain ?? AppDomain.CurrentDomain).GetAssemblies().Where(x => !x.IsDynamic)
                .Select(x => new FilePath(x.Location));

        public static string ToPackageReference(this AssemblyIdentity asm)
            => $"<PackageReference Include=\"{asm.Name}\" Version=\"{asm.Version}\" />";

        public static DirectoryPath GetOrCreateDir(DirectoryPath path)
            => Directory.Exists(path)
                ? path
                : new DirectoryPath(Directory.CreateDirectory(path).FullName);

        public static FilePath GenerateNewDllFileName()
            => PathUtil.CreateTempFile("dll");

        public static FilePath GenerateNewSourceFileName()
            => PathUtil.CreateTempFile("cs");

        public static FilePath WriteToTempFile(string source)
        {
            var path = GenerateNewSourceFileName();
            File.WriteAllText(path, source);
            return path;
        }
    }
}