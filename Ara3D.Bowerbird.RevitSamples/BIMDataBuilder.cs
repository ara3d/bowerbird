using System.Collections.Generic;
using System.Diagnostics;

namespace BIMOpenSchema;

// This is a helper class for incrementally constructing a BIMData object.
public class BIMDataBuilder
{
    private readonly Dictionary<Entity, int> _entities = new();
    private readonly Dictionary<Type, int> _types = new();
    private readonly Dictionary<Document, int> _documents = new();
    private readonly Dictionary<Point, int> _points = new();
    private readonly Dictionary<Descriptor, int> _descriptors = new();
    private readonly Dictionary<Level, int> _levels = new();
    
    private BIMData _data = new BIMData();

    public BIMData Build()
    {
        var r = _data;
        _data = null;
        return r;
    }

    private int Add<T>(Dictionary<T, int> d, List<T> list, T val)
    {
        if (d.TryGetValue(val, out var index))
            return index;
        var r = d.Count;
        d.Add(val, r);
        list.Add(val);
        Debug.Assert(d.Count == list.Count);
        return r;
    }
    
    private int Add<T>(List<T> list, T val)
    {
        var r = list.Count;
        list.Add(val);
        return r;
    }

    public EntityIndex AddEntity(string id, DocumentIndex d, string name, string category)
        => (EntityIndex)Add(_entities, _data.Entities, new(id, d, name, category));

    public DocumentIndex AddDocument(string title, string pathName)
        => (DocumentIndex)Add(_documents, _data.Documents, new(title, pathName));

    public PointIndex AddPoint(double x, double y, double z)
        => (PointIndex)Add(_points, _data.Points, new(x, y, z));

    public DescriptorIndex AddDescriptor(string name, string units, string group)
        => (DescriptorIndex)Add(_descriptors, _data.Descriptors, new(name, units, group));

    public LevelIndex AddLevel(EntityIndex e, double elevation)
        => (LevelIndex)Add(_levels, _data.Levels, new(e, elevation));

    public TypeIndex AddType(EntityIndex e, string category)
        => (TypeIndex)Add(_types, _data.Types, new(e, category));

    public int AddRoom(EntityIndex e, double baseOffset, double limitOffset, string roomNumber)
        => Add(_data.Rooms, new(e, baseOffset, limitOffset, roomNumber));

    public void SetLevel(EntityIndex e, LevelIndex level)
        => _data.LevelRelations.Add(new(e, level));

    public void SetBounds(EntityIndex e, PointIndex min, PointIndex max)
        => _data.Bounds.Add(new(e, min, max));

    public void SetLocation(EntityIndex e, PointIndex p)
        => _data.Locations.Add(new(e, p));

    public void AddParameter(EntityIndex e, DescriptorIndex d, double val)
        => _data.ParameterData.Doubles.Add(new(e, d, val));

    public void AddParameter(EntityIndex e, DescriptorIndex d, int val)
        => _data.ParameterData.Integers.Add(new(e, d, val));

    public void AddParameter(EntityIndex e, DescriptorIndex d, EntityIndex val)
        => _data.ParameterData.Entities.Add(new(e, d, val));

    public void AddParameter(EntityIndex e, DescriptorIndex d, string val)
        => _data.ParameterData.Strings.Add(new(e, d, val));
}

