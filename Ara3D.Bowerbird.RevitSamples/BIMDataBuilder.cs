using System.Collections.Generic;
using System.Diagnostics;

namespace BIMOpenSchema;

// This is a helper class for incrementally constructing a BIMData object.
public class BIMDataBuilder
{
    private readonly Dictionary<Entity, int> _entities = new();
    private readonly Dictionary<Document, int> _documents = new();
    private readonly Dictionary<Point, int> _points = new();
    private readonly Dictionary<ParameterDescriptor, int> _descriptors = new();
    private readonly Dictionary<string, int> _strings = new();

    private BIMData _data = new BIMData();

    public BIMData Build()
    {
        var r = _data;
        _data = null;
        return r;
    }

    private int AddParameter<T>(Dictionary<T, int> d, List<T> list, T val)
    {
        Debug.Assert(val != null);
        if (val == null)
            Debugger.Break();
        if (d.TryGetValue(val, out var index))
            return index;
        var r = d.Count;
        d.Add(val, r);
        list.Add(val);
        Debug.Assert(d.Count == list.Count);
        return r;
    }
    
    public void AddRelation(EntityIndex a, EntityIndex b, RelationType rt)
        => _data.Relations.Add(new(a, b, rt));

    public EntityIndex AddEntity(long localId, string globalId, DocumentIndex d, string name, string category)
        => (EntityIndex)AddParameter(_entities, _data.Entities, new(localId, globalId, d, AddString(name), AddString(category)));

    public DocumentIndex AddDocument(string title, string pathName)
        => (DocumentIndex)AddParameter(_documents, _data.Documents, new(AddString(title), AddString(pathName)));

    public PointIndex AddPoint(double x, double y, double z)
        => (PointIndex)AddParameter(_points, _data.Points, new(x, y, z));

    public DescriptorIndex AddDescriptor(string name, string units, string group)
        => (DescriptorIndex)AddParameter(_descriptors, _data.Descriptors, new(AddString(name), AddString(units), AddString(group)));

    public StringIndex AddString(string name)
        => (StringIndex)AddParameter(_strings, _data.Strings, name ?? "");

    public void AddParameter(EntityIndex e, DescriptorIndex d, double val)
        => _data.DoubleParameters.Add(new(e, d, val));

    public void AddParameter(EntityIndex e, DescriptorIndex d, int val)
        => _data.IntegerParameters.Add(new(e, d, val));

    public void AddParameter(EntityIndex e, DescriptorIndex d, EntityIndex val)
        => _data.EntityParameters.Add(new(e, d, val));

    public void AddParameter(EntityIndex e, DescriptorIndex d, string val)
        => _data.StringParameters.Add(new(e, d, AddString(val)));

    public void AddParameter(EntityIndex e, DescriptorIndex d, PointIndex pi)
        => _data.PointParameters.Add(new(e, d, pi));

    public void AddParameter(EntityIndex e, DescriptorIndex d, double x, double y, double z)
        => _data.PointParameters.Add(new(e, d, AddPoint(x, y, z)));
}

