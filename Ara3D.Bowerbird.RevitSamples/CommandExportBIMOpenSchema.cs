using Autodesk.Revit.UI;
using MessagePack.Resolvers;
using MessagePack;
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using BIMOpenSchema;
using RevitDocument = Autodesk.Revit.DB.Document;
using RevitLevel = Autodesk.Revit.DB.Level;
using RevitRoom = Autodesk.Revit.DB.Architecture.Room;
using Autodesk.Revit.DB.Architecture;

namespace Ara3D.Bowerbird.RevitSamples;

public class CommandExportBIMOpenSchema : NamedCommand
{
    public override string Name => "Export BIM Open Schema";

    public override void Execute(object arg)
    {
        ProcessedEntities.Clear();
        ProcessedDocuments.Clear();
        ExportData(arg as UIApplication);
    }

    public Dictionary<(DocumentIndex, long), EntityIndex> ProcessedEntities = new();
    public Dictionary<(string, string), DocumentIndex> ProcessedDocuments = new();

    public bool TryGetEntity(DocumentIndex di, ElementId id, out EntityIndex index)
        => ProcessedEntities.TryGetValue((di, id.Value), out index);

    public static XYZ? GetLocationPoint(Element element)
    {
        if (element == null) return null;
        return (element.Location as LocationPoint)?.Point;
    }

    public static (XYZ min, XYZ max)? GetBoundingBoxMinMax(Element element, View view = null)
    {
        if (element == null) return null;
        var bb = element.get_BoundingBox(view);   
        return bb == null ? null : (bb.Min, bb.Max);
    }

    public PointIndex AddPoint(BIMDataBuilder bdb, XYZ xyz)
        => bdb.AddPoint(xyz.X, xyz.Y, xyz.Z);

    public EntityIndex ProcessElement(DocumentIndex di, Element e, BIMDataBuilder bdb)
    {
        if (TryGetEntity(di, e.Id, out var index))
            return index;
        
        var entityIndex = bdb.AddEntity(e.UniqueId, di, e.Name, e.Category?.Name ?? "");
        ProcessedEntities.Add((di, e.Id.Value), entityIndex);

        AddParameters(di, bdb, e, entityIndex);

        var location = GetLocationPoint(e);
        if (location != null)
        {
            var pi = AddPoint(bdb, location);
            bdb.SetLocation(entityIndex, pi);
        }

        var bounds = GetBoundingBoxMinMax(e);
        if (bounds.HasValue)
        {
            var min = AddPoint(bdb, bounds.Value.min);
            var max = AddPoint(bdb, bounds.Value.max);
            bdb.SetBounds(entityIndex, min, max);
        }

        if (e is RevitRoom r)
        {
            bdb.AddRoom(entityIndex, r.BaseOffset, r.LimitOffset, r.Number);
        }

        if (e is RevitLevel l)
        {
            bdb.AddLevel(entityIndex, l.Elevation);
        }

        /*
        if (e.LevelId != ElementId.InvalidElementId)
        {
            var level = e.Document.GetElement(e.LevelId);
            var levelIndex = ProcessElement(di, level, bdb);
            bdb.SetLevel(entityIndex, levelIndex);
        }
        */
        return entityIndex;
    }

    public static (string, string) GetDocumentKey(RevitDocument d)
        => (d.Title, d.PathName);

    public DocumentIndex GetDocumentIndex(RevitDocument d)
    {
        var key = GetDocumentKey(d);
        if (!ProcessedDocuments.TryGetValue(key, out var index))
            throw new Exception($"Could not find document from key {key}");
        return index;
    }

    public void ProcessDocument(RevitDocument d, BIMDataBuilder bdb)
    {
        var key = GetDocumentKey(d);
        if (ProcessedDocuments.ContainsKey(key))
            return;

        var docIndex = bdb.AddDocument(d.Title, d.PathName);
        ProcessedDocuments.Add(key, docIndex);

        d.ProcessElements(e => ProcessElement(docIndex, e, bdb), false);

        foreach (var linkedDoc in d.GetLinkedDocuments())
            ProcessDocument(linkedDoc, bdb);
    }

    public void ExportData(UIApplication app)
    {
        var uiDoc = app.ActiveUIDocument;
        var doc = uiDoc.Document;

        var timer = Stopwatch.StartNew();
        var bdb = new BIMDataBuilder();

        ProcessDocument(doc, bdb);

        var processingTime = timer.Elapsed;
        timer.Restart();
        var bd = bdb.Build();
        var buildTime = timer.Elapsed;

        var s2 = new SerializationHelper("JSON with indenting", "parameters.json",
            fs => { JsonSerializer.Serialize(fs, bd, new JsonSerializerOptions() { WriteIndented = true }); });

        var s3 = new SerializationHelper("Message pack", "parameters.mp", fs =>
        {
            var options = MessagePackSerializerOptions.Standard
                .WithResolver(ContractlessStandardResolver.Instance);
            MessagePackSerializer.Serialize(fs, bd, options);
        });

        var s4 = new SerializationHelper("Message pack with compression", "parameters.mpz", fs =>
        {
            var options = MessagePackSerializerOptions.Standard
                .WithResolver(ContractlessStandardResolver.Instance)
                .WithCompression(MessagePackCompression.Lz4Block);
            MessagePackSerializer.Serialize(fs, bd, options);
        });

        OutputData(bd, processingTime, buildTime, s2, s3, s4);
    }

    public static void OutputData(BIMData bd, TimeSpan processingTime, TimeSpan buildTime, params SerializationHelper[] shs)
    {
        var text = $"Processed {bd.Documents.Count} documents\r\n" +
                   $"{bd.Entities.Count} entities\r\n" +
                   $"{bd.Descriptors.Count} descriptors\r\n" +
                   $"{bd.ParameterData.Integers.Count} integer parameters\r\n" +
                   $"{bd.ParameterData.Doubles.Count} double parameters\r\n" +
                   $"{bd.ParameterData.Entities.Count} entity parameters\r\n" +
                   $"{bd.ParameterData.Strings.Count} string parameters\r\n" +
                   $"{bd.Rooms.Count} rooms\r\n" +
                   $"{bd.Levels.Count} levels\r\n" +
                   $"{bd.Points.Count} points\r\n" +
                   $"{bd.Bounds.Count} bounds\r\n" +
                   $"{bd.Locations.Count} locations\r\n" +
                   $"Processing took {processingTime.TotalSeconds:F} seconds\r\n";

        foreach (var sh in shs)
        {
            text += $"{sh.Method} took {sh.Elapsed.TotalSeconds:F} seconds and output {sh.FileSize} data\r\n";
        }
        MessageBox.Show(text);
    }

    public static string GetUnitLabel(Parameter p)
    {
        var spec = p.Definition.GetDataType();
        if (!UnitUtils.IsMeasurableSpec(spec))
            return "";
        var unitId = p.GetUnitTypeId();
        return UnitUtils.GetTypeCatalogStringForUnit(unitId);
    }

    public void AddParameters(
        DocumentIndex di,
        BIMDataBuilder bdb,
        Element element,
        EntityIndex entityIndex)
    {
        foreach (Parameter p in element.Parameters)
        {
            if (p == null) continue;
            var def = p.Definition;
            if (def == null) continue;
            var groupId = def.GetGroupTypeId();
            var groupLabel = LabelUtils.GetLabelForGroup(groupId);
            var unitLabel = GetUnitLabel(p);
            var descIndex = bdb.AddDescriptor(def.Name, unitLabel, groupLabel);
            switch (p.StorageType)
            {
                case StorageType.Integer:
                    bdb.AddParameter(entityIndex, descIndex, p.AsInteger());
                    break;
                case StorageType.Double:
                    bdb.AddParameter(entityIndex, descIndex, p.AsDouble());
                    break;
                case StorageType.String:
                    bdb.AddParameter(entityIndex, descIndex, p.AsString());
                    break;
                case StorageType.ElementId:
                    {
                        var val = p.AsElementId();
                        if (val == ElementId.InvalidElementId)
                            continue;
                        var e = element.Document.GetElement(val);
                        if (e == null)
                            continue;
                        
                        // We recursively process the element 
                        var valIndex = ProcessElement(di, e, bdb);
                        bdb.AddParameter(entityIndex, descIndex, valIndex);
                    }
                    break;
                case StorageType.None:
                default:
                    bdb.AddParameter(entityIndex, descIndex, p.AsValueString());
                    break;
            }
        }
    }
}