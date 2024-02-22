using System;
using System.Collections.Generic;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Core
{
    public class AssemblyLoadingService
    { }

    public class ReferencedAssemblies 
    { }

    public class AssemblyLookup
    {
        public bool IsLoaded(string name) => throw new NotImplementedException();
        public FilePath FindAssembly(string name) => throw new NotImplementedException();
    }

    public class DirectoryWatcherService
    { }

    public class CompiledAssemblyService
    {
        public DirectoryPath Directory { get; }
        public FilePath Assembly { get; }
    }

    public class CompiledAssemblyCached
    {
        public FilePath GetCompiledAssembly { get; }
    }

    public class FoldersService
    {
        public DirectoryPath UserScriptsRootPath { get; }
        public DirectoryPath CompiledAssembliesPath { get; }
        public DirectoryPath SettingsPath { get; }
    }

    public class DirectoryState
    {
        public DirectoryPath Path { get; }
        public IReadOnlyList<FileState> Files { get; }
        public IReadOnlyList<DirectoryState> Subfolders { get; }

        public DirectoryState GetDiff(DirectoryState other) => throw new NotImplementedException();
        public DirectoryState(DirectoryPath path, string filter) => throw new NotImplementedException();
    }

    public class FileState
    {
        public FilePath File { get; }
        public DateTimeOffset Modified { get; }
        public long Size { get; }

        public bool Equals(FileState other) => throw new NotImplementedException();
    }

    // Loaded when the application starts.
    // Saved whenever it changes 
    public class ApplicationStateService
    {
    }
}
