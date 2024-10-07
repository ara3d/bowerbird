using System;
using System.Collections.Generic;
using Ara3D.Logging;
using Ara3D.Services;

namespace Ara3D.Bowerbird.Interfaces
{
    public interface IBowerbirdService 
        : IBowerbirdHost, ISingletonModelBackedService<BowerbirdDataModel>, IDisposable
    {
        BowerbirdOptions Options { get; }
        bool AutoRecompile { get; set; }
        ILogger Logger { get; set; }
        void Compile();
        IReadOnlyList<IBowerbirdCommand> Commands { get; }
    }
}
