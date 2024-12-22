using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.RevitSamples
{
    // https://people.computing.clemson.edu/~dhouse/courses/405/docs/brief-obj-file-format.html
    // https://en.wikipedia.org/wiki/Wavefront_.obj_file
    // https://www.fileformat.info/format/wavefrontobj/egff.htm
    // https://paulbourke.net/dataformats/obj/
    public static class ObjFileWriter
    {
        public static IEnumerable<int> Range(this int count)
            => Enumerable.Range(0, count);

        public static IEnumerable<string> GetVertexLines(IReadOnlyList<double> vertexData)
            => (vertexData.Count / 3).Range().Select(i => $"v {vertexData[i*3]} {vertexData[i*3+1]} {vertexData[i*3+2]}");

        public static IEnumerable<string> GetFaceLines(IReadOnlyList<int> indexData)
            => (indexData.Count / 3).Range().Select(i => $"f {indexData[i * 3]} {indexData[i * 3 + 1]} {indexData[i * 3 + 2]}");

        public static IEnumerable<string> GetObjLines(IReadOnlyList<double> vertexData, IReadOnlyList<int> indexData)
            => GetVertexLines(vertexData).Concat(GetFaceLines(indexData));

        public static Ara3D.Utils.FilePath WriteObjFile(this Ara3D.Utils.FilePath filePath, IReadOnlyList<double> vertexData, IReadOnlyList<int> indexData)
            => filePath.WriteAllLines(GetObjLines(vertexData, indexData));
    }
}