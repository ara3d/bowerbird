using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.RevitSamples
{
    // Contains configuration options for the samples
    public static class Config
    {
        public static Ara3D.Utils.DirectoryPath OutputDir
            => SpecialFolders.MyDocuments.RelativeFolder("Bowerbird Output").Create();
    }
}
