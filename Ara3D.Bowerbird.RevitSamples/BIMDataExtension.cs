using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry;

namespace BIMOpenSchema;

public static class BIMDataExtension
{
    public static String Get(this BIMData self, StringIndex index) => self.Strings[(int)index];
    public static Entity Get(this BIMData self, EntityIndex index) => self.Entities[(int)index];
    public static Document Get(this BIMData self, DocumentIndex index) => self.Documents[(int)index];
    public static Point Get(this BIMData self, PointIndex index) => self.Points[(int)index];

    public static ParameterDescriptor Get(this BIMData self, DescriptorIndex index) => self.Descriptors[(int)index];

    public static IEnumerable<EntityIndex> EntityIndices(this BIMData self) 
        => Enumerable.Range(0, self.Entities.Count).Select(i => (EntityIndex)i);

    public static IEnumerable<DocumentIndex> DocumentIndices(this BIMData self)
        => Enumerable.Range(0, self.Documents.Count).Select(i => (DocumentIndex)i);

    public static IEnumerable<DescriptorIndex> DescriptorIndices(this BIMData self)
        => Enumerable.Range(0, self.Descriptors.Count).Select(i => (DescriptorIndex)i);
}