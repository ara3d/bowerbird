using System;
using System.Collections.Generic;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Core
{
    public class Settings
    {
        public string ProjectPath { get; }
        public List<string> AssemblySearchPath { get; }
    }

    public class Constants
    {
        public const string ReferenceFilesName = "references.txt";
    }   
    
    // One of these per folder. 
    public class PluginDescriptor
    {
        public string Name { get; }
        public string Author { get; }
        public string Description { get; }
        public string SupportUrl { get; }
        public Guid PluginId { get; }
        public Guid VersionId { get; }
        public Version Version { get; }
        List<string> ReferencedAssemblies { get; }
        List<Guid> ReferencedPlugins { get; }
    }



    public interface IPluginCommand : INamedCommand
    {
        Guid Id { get; }
        string Description { get; }
    }

    // Something that is run every X minutes, 
}
