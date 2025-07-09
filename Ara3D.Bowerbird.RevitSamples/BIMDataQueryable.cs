using System.Collections.Generic;
using System.Linq;

namespace BIMOpenSchema;

/// <summary>
/// This is a helper class that creates the reverse lookup tables that make it easier to work with BIMData. 
/// </summary>
public class BIMDataQueryable
{
    public BIMData Data { get; }

    // These are features of entities 
    public Dictionary<EntityIndex, ParameterData> Parameters { get; }
    public Dictionary<EntityIndex, Level> LevelFeatures { get; }
    public Dictionary<EntityIndex, BoundsComponent> BoundsFeatures { get; }
    public Dictionary<EntityIndex, LocationComponent> LocationFeatures { get; }

    // Some entities are specialized types 
    public Dictionary<EntityIndex, Level> LevelFromEntity { get; }
    public Dictionary<EntityIndex, Room> RoomFromEntity { get; }
    public Dictionary<EntityIndex, Type> TypeFromEntity { get; }

    public BIMDataQueryable(BIMData bimData)
    {
        Data = bimData;
        LevelFromEntity = Data.Levels.ToDictionary(l => l.LevelEntity, l => l);
        RoomFromEntity = Data.Rooms.ToDictionary(r => r.Entity, r => r);
        TypeFromEntity = Data.Types.ToDictionary(t => t.Entity, t => t);

        LevelFeatures = Data.LevelRelations.ToDictionary(lr => lr.Entity, lr => Data.Get(lr.Level));
        BoundsFeatures = Data.Bounds.ToDictionary(b => b.Entity, b => b);
        LocationFeatures = Data.Locations.ToDictionary(l => l.Entity, l => l);

        // Grouping of parameters by entity 
        Parameters = Data.EntityIndices().ToDictionary(e => e, _ => new ParameterData());
        foreach (var p in Data.ParameterData.Integers) Parameters[p.Entity].Integers.Add(p);
        foreach (var p in Data.ParameterData.Doubles) Parameters[p.Entity].Doubles.Add(p);
        foreach (var p in Data.ParameterData.Strings) Parameters[p.Entity].Strings.Add(p);
        foreach (var p in Data.ParameterData.Entities) Parameters[p.Entity].Entities.Add(p);
    }

    public bool TryGetBounds(EntityIndex index, out BoundsComponent bounds) => BoundsFeatures.TryGetValue(index, out bounds);
    public bool TryGetLocation(EntityIndex index, out LocationComponent loc) => LocationFeatures.TryGetValue(index, out loc);
    public bool TryGetLevel(EntityIndex index, out Level level) => LevelFeatures.TryGetValue(index, out level);

    public bool HasBounds(EntityIndex index) => TryGetBounds(index, out _);
    public bool HasLocation(EntityIndex index) => TryGetLocation(index, out _);
    public bool HasLevel(EntityIndex index) => TryGetLevel(index, out _);
}