using System.Collections.Generic;
using Ara3D.Services;

namespace Ara3D.Bowerbird.Interfaces
{
    public interface IBowerbirdService 
        : ISingletonModelBackedService<BowerbirdDataModel>
    {
        BowerbirdOptions Options { get; }
        bool AutoRecompile { get; set; }
        void Compile();
        IReadOnlyList<IBowerbirdCommand> Commands { get; }
    }
}
