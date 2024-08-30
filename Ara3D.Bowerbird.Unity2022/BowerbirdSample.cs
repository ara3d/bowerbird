using Ara3D.Bowerbird.Core;
using Ara3D.Domo;
using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Logging;
using Ara3D.Services;
using Ara3D.Utils;
using Ara3D.Utils.Roslyn;

namespace Ara3D.Bowerbird.Unity2022
{
    public static class BowerbirdSample
    {
        public static BowerbirdService Service;
        public static IApplication App;
        public static IBowerbirdHost Host;
        public static ILogger Logger;
        public static BowerbirdOptions Options;

        public static void StartBowerbird()
        {
            Host = BowerbirdHost.CreateDefault();
            App = new Application();
            Logger = Logger.Create("Bowerbird", Console.WriteLine);
            Options = BowerbirdOptions.CreateFromName("Bowerbird for Unity 2022");
            Service = new BowerbirdService(Host, App, Logger, Options);
        }

        public static Compilation CompileScript(string s, IEnumerable<string> filePaths)
            => RoslynUtils.CompileCSharpStandard(s, new CompilerOptions(filePaths.Select(f => new FilePath(f)), RoslynUtils.GenerateNewDllFileName(), true));
    }
}
