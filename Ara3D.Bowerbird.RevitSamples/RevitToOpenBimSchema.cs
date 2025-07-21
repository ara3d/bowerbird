using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Ara3D.BimOpenSchema;
using Document = Autodesk.Revit.DB.Document;

namespace Ara3D.Bowerbird.RevitSamples;

public class RevitToOpenBimSchema
{
    public RevitToOpenBimSchema(Document rootDocument, bool includeLinks) 
    {
        IncludeLinks = includeLinks;
        CreateCommonDescriptors();
        ProcessDocument(rootDocument);
    }

    public class StructuralLayer
    {
        public int LayerIndex;
        public MaterialFunctionAssignment Function;
        public double Width;
        public ElementId MaterialId;
        public bool IsCore;
    }

    public BimDataBuilder bdb = new();
    public bool IncludeLinks;
    public Document CurrentDocument;
    public DocumentIndex CurrentDocumentIndex;
    public Dictionary<(DocumentIndex, long), EntityIndex> ProcessedEntities = new();
    public Dictionary<(string, string), DocumentIndex> ProcessedDocuments = new();
    public Dictionary<long, EntityIndex> ProcessedCategories = new();

    public bool TryGetEntity(DocumentIndex di, ElementId id, out EntityIndex index)
        => ProcessedEntities.TryGetValue((di, id.Value), out index);

    public static (XYZ min, XYZ max)? GetBoundingBoxMinMax(Element element, View view = null)
    {
        if (element == null) return null;
        var bb = element.get_BoundingBox(view);
        return bb == null ? null : (bb.Min, bb.Max);
    }

    public PointIndex AddPoint(BimDataBuilder bdb, XYZ xyz)
        => bdb.AddPoint(new(xyz.X, xyz.Y, xyz.Z));

    private DescriptorIndex _apiTypeDescriptor;

    private DescriptorIndex _elementLevel;
    private DescriptorIndex _elementLocation;
    private DescriptorIndex _elementLocationStartPoint;
    private DescriptorIndex _elementLocationEndPoint;
    private DescriptorIndex _elementBoundsMin;
    private DescriptorIndex _elementBoundsMax;
    private DescriptorIndex _elementAssemblyInstance;
    private DescriptorIndex _elementDesignOption;
    private DescriptorIndex _elementGroup;
    private DescriptorIndex _elementWorkset;
    private DescriptorIndex _elementCreatedPhase;
    private DescriptorIndex _elementDemolishedPhase;
    private DescriptorIndex _elementCategory;

    private DescriptorIndex _familyInstanceToRoomDesc;
    private DescriptorIndex _familyInstanceFromRoomDesc;
    private DescriptorIndex _familyInstanceRoom;
    private DescriptorIndex _familyInstanceSpace;
    private DescriptorIndex _familyInstanceHost;
    private DescriptorIndex _familyInstanceSymbol;
    private DescriptorIndex _familyInstanceStructuralUsage;
    private DescriptorIndex _familyInstanceStructuralMaterialType;
    private DescriptorIndex _familyInstanceStructuralMaterialId;
    private DescriptorIndex _familyInstanceStructuralType;

    private DescriptorIndex _familyStructuralCodeName;
    private DescriptorIndex _familyStructuralMaterialType;

    private DescriptorIndex _roomBaseOffset;
    private DescriptorIndex _roomLimitOffset;
    private DescriptorIndex _roomUnboundedHeight;
    private DescriptorIndex _roomVolume;
    private DescriptorIndex _roomUpperLimit;
    private DescriptorIndex _roomNumber;

    private DescriptorIndex _levelElevation;
    private DescriptorIndex _levelProjectElevation;

    private DescriptorIndex _materialColorRed;
    private DescriptorIndex _materialColorGreen;
    private DescriptorIndex _materialColorBlue;
    private DescriptorIndex _materialShininess;
    private DescriptorIndex _materialSmoothness;
    private DescriptorIndex _materialCategory;
    private DescriptorIndex _materialClass;
    private DescriptorIndex _materialTransparency;

    private DescriptorIndex _worksetKind;

    private DescriptorIndex _layerIndex;
    private DescriptorIndex _layerFunction;
    private DescriptorIndex _layerWidth;
    private DescriptorIndex _layerMaterialId;
    private DescriptorIndex _layerIsCore;

    private DescriptorIndex _documentTitle;
    private DescriptorIndex _documentPath;
    private DescriptorIndex _documentWorksharingGuid;
    private DescriptorIndex _documentCreationGuid;
    private DescriptorIndex _documentElevation;
    private DescriptorIndex _documentLatitude;
    private DescriptorIndex _documentLongitude;
    private DescriptorIndex _documentPlaceName;
    private DescriptorIndex _documentWeatherStationName;
    private DescriptorIndex _documentTimeZone;

    private DescriptorIndex _projectName;
    private DescriptorIndex _projectNumber;
    private DescriptorIndex _projectStatus;
    private DescriptorIndex _projectAddress;
    private DescriptorIndex _projectClientName;
    private DescriptorIndex _projectIssueDate;
    private DescriptorIndex _projectAuthor;
    private DescriptorIndex _projectBuildingName;
    private DescriptorIndex _projectOrgDescription;
    private DescriptorIndex _projectOrgName;

    private DescriptorIndex _categoryCategoryType;
    private DescriptorIndex _categoryBuiltInType;

    private void AddDesc(ref DescriptorIndex desc, string name, ParameterType pt)
    {
        desc = bdb.AddDescriptor(name, "", "RevitAPI", pt);
    }

    public void CreateCommonDescriptors()
    {
        AddDesc(ref _apiTypeDescriptor, "rvt:Object:TypeName", ParameterType.String);

        AddDesc(ref _elementLevel, "rvt:Element:Level", ParameterType.Entity);
        AddDesc(ref _elementLocation, "rvt:Element:Location.Point", ParameterType.Point);
        AddDesc(ref _elementLocationStartPoint, "rvt:Element:Location.StartPoint", ParameterType.Point);
        AddDesc(ref _elementLocationEndPoint, "rvt:Element:Location.EndPoint", ParameterType.Point);
        AddDesc(ref _elementBoundsMin, "rvt:Element:Bounds.Min", ParameterType.Point);
        AddDesc(ref _elementBoundsMax, "rvt:Element:Bounds.Max", ParameterType.Point);
        AddDesc(ref _elementAssemblyInstance, "rvt:Element:AssemblyInstance", ParameterType.Entity);
        AddDesc(ref _elementDesignOption, "rvt:Element:DesignOption", ParameterType.Entity);
        AddDesc(ref _elementGroup, "rvt:Element:Group", ParameterType.Entity);
        AddDesc(ref _elementWorkset, "rvt:Element:Workset", ParameterType.Int);
        AddDesc(ref _elementCreatedPhase, "rvt:Element:CreatedPhase", ParameterType.Entity);
        AddDesc(ref _elementDemolishedPhase, "rvt:Element:DemolishedPhase", ParameterType.Entity);
        AddDesc(ref _elementCategory, "rvt:Element:Category", ParameterType.Entity);

        AddDesc(ref _familyInstanceToRoomDesc, "rvt:FamilyInstance:ToRoom", ParameterType.Entity);
        AddDesc(ref _familyInstanceFromRoomDesc, "rvt:FamilyInstance:FromRoom", ParameterType.Entity);
        AddDesc(ref _familyInstanceHost, "rvt:FamilyInstance:Host", ParameterType.Entity);
        AddDesc(ref _familyInstanceSpace, "rvt:FamilyInstance:Space", ParameterType.Entity);
        AddDesc(ref _familyInstanceRoom, "rvt:FamilyInstance:Room", ParameterType.Entity);
        AddDesc(ref _familyInstanceSymbol, "rvt:FamilyInstance:Symbol", ParameterType.Entity);
        AddDesc(ref _familyInstanceStructuralUsage, "rvt:FamilyInstance:StructuralUsage", ParameterType.String);
        AddDesc(ref _familyInstanceStructuralMaterialType, "rvt:FamilyInstance:StructuralMaterialType", ParameterType.String);
        AddDesc(ref _familyInstanceStructuralMaterialId, "rvt:FamilyInstance:StructuralMaterialId", ParameterType.Entity);
        AddDesc(ref _familyInstanceStructuralType, "rvt:FamilyInstance:StructuralType", ParameterType.String);

        AddDesc(ref _familyStructuralCodeName, "rvt:Family:StructuralCodeName", ParameterType.String);
        AddDesc(ref _familyStructuralMaterialType, "rvt:Family:StructuralMaterialType", ParameterType.String);

        AddDesc(ref _roomNumber, "rvt:Room:Number", ParameterType.String);
        AddDesc(ref _roomBaseOffset, "rvt:Room:BaseOffset", ParameterType.Double);
        AddDesc(ref _roomLimitOffset, "rvt:Room:LimitOffset", ParameterType.Double);
        AddDesc(ref _roomUnboundedHeight, "rvt:Room:UnboundedHeight", ParameterType.Double);
        AddDesc(ref _roomVolume, "rvt:Room:Volume", ParameterType.Double);
        AddDesc(ref _roomUpperLimit, "rvt:Room:UpperLimit", ParameterType.Entity);

        AddDesc(ref _levelProjectElevation, "rvt:Level:ProjectElevation", ParameterType.Double);
        AddDesc(ref _levelElevation, "rvt:Level:Elevation", ParameterType.Double);

        AddDesc(ref _materialColorRed, "rvt:Material:Color.Red", ParameterType.Double);
        AddDesc(ref _materialColorGreen, "rvt:Material:Color.Green", ParameterType.Double);
        AddDesc(ref _materialColorBlue, "rvt:Material:Color.Blue", ParameterType.Double);
        AddDesc(ref _materialShininess, "rvt:Material:Shininess", ParameterType.Double);
        AddDesc(ref _materialSmoothness, "rvt:Material:Smoothness", ParameterType.Double);
        AddDesc(ref _materialCategory, "rvt:Material:Category", ParameterType.String);
        AddDesc(ref _materialClass, "rvt:Material:Class", ParameterType.String);
        AddDesc(ref _materialTransparency, "rvt:Material:Transparency", ParameterType.Double);

        AddDesc(ref _worksetKind, "rvt:Workset:Kind", ParameterType.Double);

        AddDesc(ref _layerIndex, "rvt:Layer:Index", ParameterType.Int);
        AddDesc(ref _layerFunction, "rvt:Layer:Function", ParameterType.String);
        AddDesc(ref _layerWidth, "rvt:Layer:Width", ParameterType.Double);
        AddDesc(ref _layerMaterialId, "rvt:Layer:MaterialId", ParameterType.Entity); 
        AddDesc(ref _layerIsCore, "rvt:Layer:IsCore", ParameterType.Int);

        AddDesc(ref _documentCreationGuid, "rvt:Document:CreationGuid", ParameterType.String);
        AddDesc(ref _documentWorksharingGuid, "rvt:Document:WorksharingGuid", ParameterType.String);
        AddDesc(ref _documentTitle, "rvt;Document:Title", ParameterType.String);
        AddDesc(ref _documentPath, "rvt:Document:Path", ParameterType.String);
        AddDesc(ref _documentElevation, "rvt:Document:Elevation", ParameterType.Double);
        AddDesc(ref _documentLatitude, "rvt:Document:Latitude", ParameterType.Double);
        AddDesc(ref _documentLongitude, "rvt:Document:Longitude", ParameterType.Double);
        AddDesc(ref _documentPlaceName, "rvt:Document:PlaceName", ParameterType.String);
        AddDesc(ref _documentWeatherStationName, "rvt:Document:WeatherStationName", ParameterType.String);
        AddDesc(ref _documentTimeZone, "rvt:Document:TimeZone", ParameterType.Double);

        AddDesc(ref _projectName, "rvt:Document:Project:Name", ParameterType.String);
        AddDesc(ref _projectNumber, "rvt:Document:Project:Number", ParameterType.String);
        AddDesc(ref _projectStatus, "rvt:Document:Project:Status", ParameterType.String);
        AddDesc(ref _projectAddress, "rvt:Document:Project:Address", ParameterType.String);
        AddDesc(ref _projectClientName, "rvt:Document:Project:Client", ParameterType.String);
        AddDesc(ref _projectIssueDate, "rvt:Document:Project:IssueDate", ParameterType.String);
        AddDesc(ref _projectAuthor, "rvt:Document:Project:Author", ParameterType.String);
        AddDesc(ref _projectBuildingName, "rvt:Document:Project:BuildingName", ParameterType.String);
        AddDesc(ref _projectOrgDescription, "rvt:Document:Project:OrganizationDescription", ParameterType.String);
        AddDesc(ref _projectOrgName, "rvt:Document:Project:OrganizationName", ParameterType.String);

        AddDesc(ref _categoryCategoryType, "rvt:Category:CategoryType", ParameterType.String);
        AddDesc(ref _categoryBuiltInType, "rvt:Category:BuiltInType", ParameterType.String);
    }

    public List<StructuralLayer> GetLayers(HostObjAttributes host)
    {
        var compound = host.GetCompoundStructure();
        if (compound == null) return [];
        var r = new List<StructuralLayer>();
        for (var i = 0; i < compound.LayerCount; i++)
        {
            r.Add(new StructuralLayer()
            {
                Function = compound.GetLayerFunction(i),
                IsCore = compound.IsCoreLayer(i),
                LayerIndex = i,
                MaterialId = compound.GetMaterialId(i),
                Width = compound.GetLayerWidth(i)
            });
        }

        return r;
    }

    public void AddParameter(EntityIndex ei, DescriptorIndex di, string val)
    {
        var d = bdb.Data.Get(di);
        if (d.Type != ParameterType.String) throw new Exception($"Expected string not {d.Type}");
        bdb.AddParameter(ei, val, di);
    }

    public void AddParameter(EntityIndex ei, DescriptorIndex di, EntityIndex val)
    {
        var d = bdb.Data.Get(di);
        if (d.Type != ParameterType.Entity) throw new Exception($"Expected entity not {d.Type}");
        bdb.AddParameter(ei, val, di);
    }

    public void AddParameter(EntityIndex ei, DescriptorIndex di, PointIndex val)
    {
        var d = bdb.Data.Get(di);
        if (d.Type != ParameterType.Point) throw new Exception($"Expected point not {d.Type}");
        bdb.AddParameter(ei, val, di);
    }

    public void AddParameter(EntityIndex ei, DescriptorIndex di, int val)
    {
        var d = bdb.Data.Get(di);
        if (d.Type != ParameterType.Int) throw new Exception($"Expected int not {d.Type}");
        bdb.AddParameter(ei, val, di);
    }

    public void AddParameter(EntityIndex ei, DescriptorIndex di, double val)
    {
        var d = bdb.Data.Get(di);
        if (d.Type != ParameterType.Double) throw new Exception($"Expected double not {d.Type}");
        bdb.AddParameter(ei, val, di);
    }

    public void AddTypeAsParameter(EntityIndex ei, object o)
    {
        AddParameter(ei, _apiTypeDescriptor, o.GetType().Name);
    }

    public EntityIndex ProcessCategory(Category category)
    {
        if (!ProcessedCategories.TryGetValue(category.Id.Value, out var result))
            return result;

        var r = bdb.AddEntity(category.Id.Value, category.Id.ToString(), CurrentDocumentIndex, category.Name,
            category.BuiltInCategory.ToString());

        AddParameter(r, _apiTypeDescriptor, category.GetType().Name);
        AddParameter(r, _categoryCategoryType, category.CategoryType.ToString());
        AddParameter(r, _categoryCategoryType, category.BuiltInCategory.ToString());

        foreach (Category subCategory in category.SubCategories)
        {
            var subCatId = ProcessCategory(subCategory);
            bdb.AddRelation(subCatId, r, RelationType.ChildOf);
        }

        return r;
    }

    public void ProcessCompoundStructure(EntityIndex ei, HostObjAttributes host)
    {
        var layers = GetLayers(host);
        if (layers == null) return;

        foreach (var layer in layers)
        {
            var index = layer.LayerIndex;
            var layerEi = bdb.AddEntity(
                0, 
                $"{host.UniqueId}${index}", 
                CurrentDocumentIndex, 
                $"{host.Name}[{index}]", 
                layer.Function.ToString());

            AddParameter(layerEi, _layerIndex, index);
            AddParameter(layerEi, _layerFunction, layer.Function.ToString());
            AddParameter(layerEi, _layerWidth, layer.Width);
            AddParameter(layerEi, _layerIsCore, layer.IsCore ? 1 : 0);

            var matId = layer.MaterialId;
            if (matId != ElementId.InvalidElementId)
            {
                var matIndex = ProcessElement(matId);
                AddParameter(layerEi, _layerMaterialId, matIndex);
                bdb.AddRelation(layerEi, matIndex, RelationType.HasMaterial);
            }

            AddTypeAsParameter(layerEi, layer);
            bdb.AddRelation(ei, layerEi, RelationType.HasLayer);
        }
    }

    public void ProcessMaterial(EntityIndex ei, Material m)
    {
        var color = m.Color;
        AddParameter(ei, _materialColorGreen, color.Red);
        AddParameter(ei, _materialColorGreen, color.Green);
        AddParameter(ei, _materialColorGreen, color.Blue);

        AddParameter(ei, _materialTransparency, m.Transparency);
        AddParameter(ei, _materialShininess, m.Shininess);
        AddParameter(ei, _materialSmoothness, m.Smoothness);
        AddParameter(ei, _materialCategory, m.MaterialCategory);
        AddParameter(ei, _materialClass, m.MaterialClass);
    }

    public void ProcessFamily(EntityIndex ei, Family f)
    {
        AddParameter(ei, _familyStructuralCodeName, f.StructuralCodeName);
        AddParameter(ei, _familyStructuralMaterialType, f.StructuralMaterialType.ToString());
    }
    
    public void ProcessFamilyInstance(EntityIndex ei, FamilyInstance f)
    {
        var typeId = f.GetTypeId();
        if (typeId != ElementId.InvalidElementId)
        {
            var type = ProcessElement(typeId);
            bdb.AddRelation(ei, type, RelationType.InstanceOf);
        }

        var toRoom = f.ToRoom;
        if (toRoom != null && toRoom.IsValidObject)
            AddParameter(ei, _familyInstanceToRoomDesc, ProcessElement(toRoom));

        var fromRoom = f.FromRoom;
        if (fromRoom != null && fromRoom.IsValidObject)
            AddParameter(ei, _familyInstanceFromRoomDesc, ProcessElement(fromRoom));

        var host = f.Host;
        if (host != null && host.IsValidObject)
        {
            var hostIndex = ProcessElement(host);
            AddParameter(ei, _familyInstanceHost, hostIndex);
            bdb.AddRelation(ei, hostIndex, RelationType.HostedBy);
        }

        var space = f.Space;
        if (space != null && space.IsValidObject)
        {
            var spaceIndex = ProcessElement(space);
            AddParameter(ei, _familyInstanceSpace, spaceIndex);
            bdb.AddRelation(ei, spaceIndex, RelationType.ContainedIn);
        }

        var room = f.Room;
        if (room != null && room.IsValidObject)
        {
            var roomIndex = ProcessElement(room);
            AddParameter(ei, _familyInstanceSpace, roomIndex);
            bdb.AddRelation(ei, roomIndex, RelationType.ContainedIn);
        }

        var matId = f.StructuralMaterialId;
        if (matId != ElementId.InvalidElementId)
        {
            var matIndex = ProcessElement(matId);
            AddParameter(ei, _familyInstanceStructuralMaterialId, matIndex);
            bdb.AddRelation(ei, matIndex, RelationType.HasMaterial);
        }

        AddParameter(ei, _familyInstanceStructuralUsage, f.StructuralUsage.ToString());
        AddParameter(ei, _familyInstanceStructuralType, f.StructuralMaterialType.ToString());
    }

    public void ProcessRoom(EntityIndex ei, Room room)
    {
        AddParameter(ei, _roomBaseOffset, room.BaseOffset);
        AddParameter(ei, _roomLimitOffset, room.LimitOffset);
        AddParameter(ei, _roomNumber, room.Number);
        AddParameter(ei, _roomUnboundedHeight, room.UnboundedHeight);
        if (room.UpperLimit != null && room.UpperLimit.IsValidObject)
            AddParameter(ei, _roomUpperLimit, ProcessElement(room.UpperLimit));
        AddParameter(ei, _roomVolume, room.Volume);
    }
    
    public void ProcessLevel(EntityIndex ei, Level level)
    {
        AddParameter(ei, _levelElevation, level.Elevation);
        AddParameter(ei, _levelProjectElevation, level.ProjectElevation);
    }

    public void ProcessMaterials(EntityIndex ei, Element e)
    {
        var matIds = e.GetMaterialIds(false);
        foreach (var id in matIds)
        {
            var matId = ProcessElement(id);
            bdb.AddRelation(ei, matId, RelationType.HasMaterial);
        }
    }

    public static string GetUnitLabel(Parameter p)
    {
        var spec = p.Definition.GetDataType();
        if (!UnitUtils.IsMeasurableSpec(spec))
            return "";
        var unitId = p.GetUnitTypeId();
        return UnitUtils.GetTypeCatalogStringForUnit(unitId);
    }

    public void ProcessParameters(EntityIndex entityIndex, Element element)
    {
        foreach (Parameter p in element.Parameters)
        {
            if (p == null) continue;
            var def = p.Definition;
            if (def == null) continue;
            var groupId = def.GetGroupTypeId();
            var groupLabel = LabelUtils.GetLabelForGroup(groupId);
            var unitLabel = GetUnitLabel(p);
            switch (p.StorageType)
            {
                case StorageType.Integer:
                    AddParameter(entityIndex, bdb.AddDescriptor(def.Name, unitLabel, groupLabel, ParameterType.Int), p.AsInteger());
                    break;
                case StorageType.Double:
                    AddParameter(entityIndex, bdb.AddDescriptor(def.Name, unitLabel, groupLabel, ParameterType.Double), p.AsDouble());
                    break;
                case StorageType.String:
                    AddParameter(entityIndex, bdb.AddDescriptor(def.Name, unitLabel, groupLabel, ParameterType.String), p.AsString());
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
                        var valIndex = ProcessElement(e);
                        AddParameter(entityIndex, bdb.AddDescriptor(def.Name, unitLabel, groupLabel, ParameterType.Entity), valIndex);
                    }
                    break;
                case StorageType.None:
                default:
                    AddParameter(entityIndex, bdb.AddDescriptor(def.Name, unitLabel, groupLabel, ParameterType.String), p.AsValueString());
                    break;
            }
        }
    }

    public (DocumentIndex, long) GetKey(ElementId id)
        => (CurrentDocumentIndex, id.Value);

    public EntityIndex ProcessElement(ElementId id)
    {
        if (id == null || id == ElementId.InvalidElementId)
            throw new Exception("Invalid element");
        var key = GetKey(id);
        if (ProcessedEntities.TryGetValue(key, out var entityIndex))
            return entityIndex;
        var element = CurrentDocument.GetElement(id);
        return ProcessElement(element);
    }

    public static bool TryGetLocationEndpoints(
        LocationCurve lc,
        out XYZ startPoint,
        out XYZ endPoint)
    {
        startPoint = null;
        endPoint = null;
        var curve = lc?.Curve;
        if (curve == null) return false;
        if (!curve.IsBound) return false;
        startPoint = curve.GetEndPoint(0);
        endPoint = curve.GetEndPoint(1);
        return true;
    }

    public EntityIndex ProcessElement(Element e)
    {
        if (e == null || !e.IsValidObject)
            throw new Exception("Invalid element");

        var di = CurrentDocumentIndex; 
        if (TryGetEntity(di, e.Id, out var index))
            return index;

        var category = e.Category;
        var catName = (category != null && category.IsValid) ? category.Name : "";

        var entityIndex = bdb.AddEntity(e.Id.Value, e.UniqueId, di, e.Name, catName);
        ProcessedEntities.Add((di, e.Id.Value), entityIndex);

        if (category != null && category.IsValid)
        {
            var catIndex = ProcessCategory(category);
            AddParameter(entityIndex, _elementCategory, catIndex);
            bdb.AddRelation(entityIndex, catIndex, RelationType.ContainedIn);
        }

        ProcessParameters(entityIndex, e);
        ProcessMaterials(entityIndex, e);

        var bounds = GetBoundingBoxMinMax(e);
        if (bounds.HasValue)
        {
            var min = AddPoint(bdb, bounds.Value.min);
            var max = AddPoint(bdb, bounds.Value.max);
            AddParameter(entityIndex, _elementBoundsMin, min);
            AddParameter(entityIndex, _elementBoundsMax, max);
        }

        var levelId = e.LevelId;
        if (levelId != ElementId.InvalidElementId)
        {
            var levelIndex = ProcessElement(levelId);
            AddParameter(entityIndex, _elementLevel, levelIndex);
            bdb.AddRelation(entityIndex, levelIndex, RelationType.ContainedIn);
        }

        var assemblyInstanceId = e.AssemblyInstanceId;
        if (assemblyInstanceId != ElementId.InvalidElementId)
        {
            var assemblyIndex = ProcessElement(assemblyInstanceId);
            AddParameter(entityIndex, _elementAssemblyInstance, assemblyIndex);
            bdb.AddRelation(entityIndex, assemblyIndex, RelationType.PartOf);
        }
        
        var location = e.Location;
        if (location != null)
        {
            if (location is LocationPoint lp)
            {
                AddParameter(entityIndex, _elementLocation, AddPoint(bdb, lp.Point));
            }

            if (location is LocationCurve lc)
            {
                if (TryGetLocationEndpoints(lc, out var startPoint, out var endPoint))
                {
                    AddParameter(entityIndex, _elementLocationStartPoint, AddPoint(bdb, startPoint));
                    AddParameter(entityIndex, _elementLocationEndPoint, AddPoint(bdb, endPoint));
                }
            }
        }

        if (e.CreatedPhaseId != ElementId.InvalidElementId)
        {
            var createdPhase = ProcessElement(e.CreatedPhaseId);
            AddParameter(entityIndex, _elementCreatedPhase, createdPhase);
        }

        if (e.DemolishedPhaseId != ElementId.InvalidElementId)
        {
            var demolishedPhase = ProcessElement(e.DemolishedPhaseId);
            AddParameter(entityIndex, _elementDemolishedPhase, demolishedPhase);
        }

        var designOption = e.DesignOption;
        if (designOption != null && designOption.IsValidObject)
        {
            var doIndex = ProcessElement(designOption);
            bdb.AddRelation(entityIndex, doIndex, RelationType.ElementOf);
            AddParameter(entityIndex, _elementDesignOption, doIndex);
        }

        var groupId = e.GroupId;
        if (groupId != ElementId.InvalidElementId)
        {
            var group = ProcessElement(groupId);
            bdb.AddRelation(entityIndex, group, RelationType.ElementOf);
        }

        if (e.WorksetId != null)
        {
            AddParameter(entityIndex, _elementWorkset, e.WorksetId.IntegerValue);
        }

        if (e is HostObjAttributes host)
            ProcessCompoundStructure(entityIndex, host);

        if (e is Room r)
            ProcessRoom(entityIndex, r);

        if (e is Level level)
            ProcessLevel(entityIndex, level);

        if (e is Family family)
            ProcessFamily(entityIndex, family);

        if (e is FamilyInstance familyInstance)
            ProcessFamilyInstance(entityIndex, familyInstance);

        return entityIndex;
    }

    public static (string, string) GetDocumentKey(Document d)
        => (d.Title, d.PathName);

    public void ProcessDocument(Document d)
    {
        var key = GetDocumentKey(d);
        if (ProcessedDocuments.ContainsKey(key))
            return;

        CurrentDocument = d;
        CurrentDocumentIndex = bdb.AddDocument(d.Title, d.PathName);

        // NOTE: this creates a pseduo-entity for the document, which is used so that we can associate parameters and meta-data with it. 
        var ei = bdb.AddEntity(ProcessedDocuments.Count, CurrentDocument.CreationGUID.ToString(), CurrentDocumentIndex, d.Title, "__DOCUMENT__");
        ProcessedDocuments.Add(key, CurrentDocumentIndex);

        var siteLocation = CurrentDocument.SiteLocation;
            
        AddParameter(ei, _documentPath, CurrentDocument.PathName);
        AddParameter(ei, _documentTitle, CurrentDocument.Title);

        if (CurrentDocument.IsWorkshared)
            AddParameter(ei, _documentWorksharingGuid, CurrentDocument.WorksharingCentralGUID.ToString());
        AddParameter(ei, _documentCreationGuid, CurrentDocument.CreationGUID.ToString());
        AddParameter(ei, _documentElevation, siteLocation.Elevation);
        AddParameter(ei, _documentLatitude, siteLocation.Latitude);
        AddParameter(ei, _documentLongitude, siteLocation.Longitude);
        AddParameter(ei, _documentPlaceName, siteLocation.PlaceName);
        AddParameter(ei, _documentWeatherStationName, siteLocation.WeatherStationName);
        AddParameter(ei, _documentTimeZone, siteLocation.TimeZone);

        var projectInfo = CurrentDocument.ProjectInformation;

        AddParameter(ei, _projectAddress, projectInfo.Address);
        AddParameter(ei, _projectAuthor, projectInfo.Author);
        AddParameter(ei, _projectBuildingName, projectInfo.BuildingName);
        AddParameter(ei, _projectClientName, projectInfo.ClientName);
        AddParameter(ei, _projectIssueDate, projectInfo.IssueDate);
        AddParameter(ei, _projectName, projectInfo.Name);
        AddParameter(ei, _projectNumber, projectInfo.Number);
        AddParameter(ei, _projectOrgDescription, projectInfo.OrganizationDescription);
        AddParameter(ei, _projectOrgName, projectInfo.OrganizationName);
        AddParameter(ei, _projectStatus, projectInfo.Status);

        foreach (var e in CurrentDocument.GetElements())
            ProcessElement(e);

        if (IncludeLinks)
            foreach (var linkedDoc in d.GetLinkedDocuments())
                ProcessDocument(linkedDoc);
    }
}