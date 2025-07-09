using System.Collections.Generic;
using System.Linq;

namespace BIMOpenSchema;

public static class BIMDataExtension
{
    public static Entity Get(this BIMData self, EntityIndex index) => self.Entities[(int)index];
    public static Level Get(this BIMData self, LevelIndex index) => self.Levels[(int)index];
    public static Document Get(this BIMData self, DocumentIndex index) => self.Documents[(int)index];
    public static Room Get(this BIMData self, RoomIndex index) => self.Rooms[(int)index];
    public static Descriptor Get(this BIMData self, DescriptorIndex index) => self.Descriptors[(int)index];

    public static IEnumerable<EntityIndex> EntityIndices(this BIMData self) 
        => Enumerable.Range(0, self.Entities.Count).Select(i => (EntityIndex)i);

    public static IEnumerable<LevelIndex> LevelIndices(this BIMData self)
        => Enumerable.Range(0, self.Levels.Count).Select(i => (LevelIndex)i);

    public static IEnumerable<RoomIndex> RoomIndices(this BIMData self)
        => Enumerable.Range(0, self.Rooms.Count).Select(i => (RoomIndex)i);

    public static IEnumerable<DocumentIndex> DocumentIndices(this BIMData self)
        => Enumerable.Range(0, self.Documents.Count).Select(i => (DocumentIndex)i);

    public static IEnumerable<DescriptorIndex> DescriptorIndices(this BIMData self)
        => Enumerable.Range(0, self.Descriptors.Count).Select(i => (DescriptorIndex)i);
}