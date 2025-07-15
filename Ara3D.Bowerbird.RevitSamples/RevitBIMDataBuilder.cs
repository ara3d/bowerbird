using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using BIMOpenSchema;
using Document = Autodesk.Revit.DB.Document;

namespace Ara3D.Bowerbird.RevitSamples;

public class RevitBIMDataBuilder
{
    public RevitBIMDataBuilder(Document rootDocument, bool includeLinks)
    {
        IncludeLinks = includeLinks;
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

    public BIMDataBuilder bdb = new();
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

    public PointIndex AddPoint(BIMDataBuilder bdb, XYZ xyz)
        => bdb.AddPoint(xyz.X, xyz.Y, xyz.Z);

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

    private void AddDesc(ref DescriptorIndex desc, string name)
    {
        desc = bdb.AddDescriptor(name, "", "RevitAPI");
    }

    public void CreateCommonDescriptors()
    {
        AddDesc(ref _apiTypeDescriptor, "rvt:Object:TypeName");

        AddDesc(ref _elementLevel, "rvt:Element:Level");
        AddDesc(ref _elementLocation, "rvt:Element:Location.Point");
        AddDesc(ref _elementLocationStartPoint, "rvt:Element:Location.StartPoint");
        AddDesc(ref _elementLocationEndPoint, "rvt:Element:Location.EndPoint");
        AddDesc(ref _elementBoundsMin, "rvt:Element:Bounds.Min");
        AddDesc(ref _elementBoundsMax, "rvt:Element:Bounds.Max");
        AddDesc(ref _elementAssemblyInstance, "rvt:Element:AssemblyInstance");
        AddDesc(ref _elementDesignOption, "rvt:Element:DesignOption");
        AddDesc(ref _elementGroup, "rvt:Element:Group");
        AddDesc(ref _elementWorkset, "rvt:Element:Workset");
        AddDesc(ref _elementCreatedPhase, "rvt:Element:CreatedPhase");
        AddDesc(ref _elementDemolishedPhase, "rvt:Element:DemolishedPhase");
        AddDesc(ref _elementCategory, "rvt:Element:Category");

        AddDesc(ref _familyInstanceToRoomDesc, "rvt:FamilyInstance:ToRoom");
        AddDesc(ref _familyInstanceFromRoomDesc, "rvt:FamilyInstance:FromRoom");
        AddDesc(ref _familyInstanceHost, "rvt:FamilyInstance:Host");
        AddDesc(ref _familyInstanceSpace, "rvt:FamilyInstance:Space");
        AddDesc(ref _familyInstanceRoom, "rvt:FamilyInstance:Room");
        AddDesc(ref _familyInstanceSymbol, "rvt:FamilyInstance:Symbol");
        AddDesc(ref _familyInstanceStructuralUsage, "rvt:FamilyInstance:StructuralUsage");
        AddDesc(ref _familyInstanceStructuralMaterialType, "rvt:FamilyInstance:StructuralMaterialType");
        AddDesc(ref _familyInstanceStructuralMaterialId, "rvt:FamilyInstance:StructuralMaterialId");
        AddDesc(ref _familyInstanceStructuralType, "rvt:FamilyInstance:StructuralType");

        AddDesc(ref _familyStructuralCodeName, "rvt:Family:StructuralCodeName");
        AddDesc(ref _familyStructuralMaterialType, "rvt:Family:StructuralMaterialType");

        AddDesc(ref _roomNumber, "rvt:Room:Number");
        AddDesc(ref _roomBaseOffset, "rvt:Room:BaseOffset");
        AddDesc(ref _roomLimitOffset, "rvt:Room:LimitOffset");
        AddDesc(ref _roomUnboundedHeight, "rvt:Room:UnboundedHeight");
        AddDesc(ref _roomVolume, "rvt:Room:Volume");
        AddDesc(ref _roomUpperLimit, "rvt:Room:UpperLimit");

        AddDesc(ref _levelProjectElevation, "rvt:Level:ProjectElevation");
        AddDesc(ref _levelElevation, "rvt:Level:Elevation");

        AddDesc(ref _materialColorRed, "rvt:Material:Color.Red");
        AddDesc(ref _materialColorGreen, "rvt:Material:Color.Green");
        AddDesc(ref _materialColorBlue, "rvt:Material:Color.Blue");
        AddDesc(ref _materialShininess, "rvt:Material:Shininess");
        AddDesc(ref _materialSmoothness, "rvt:Material:Smoothness");
        AddDesc(ref _materialCategory, "rvt:Material:Category");
        AddDesc(ref _materialClass, "rvt:Material:Class");
        AddDesc(ref _materialTransparency, "rvt:Material:Transparency");

        AddDesc(ref _worksetKind, "rvt:Workset:Kind");

        AddDesc(ref _layerIndex, "rvt:Layer:Index");
        AddDesc(ref _layerFunction, "rvt:Layer:Function");
        AddDesc(ref _layerWidth, "rvt:Layer:Width");
        AddDesc(ref _layerMaterialId, "rvt:Layer:MaterialId"); 
        AddDesc(ref _layerIsCore, "rvt:Layer:IsCore");

        AddDesc(ref _documentCreationGuid, "rvt:Document:CreationGuid");
        AddDesc(ref _documentWorksharingGuid, "rvt:Document:WorksharingGuid");
        AddDesc(ref _documentTitle, "rvt;Document:Title");
        AddDesc(ref _documentPath, "rvt:Document:Path");
        AddDesc(ref _documentElevation, "rvt:Document:Elevation");
        AddDesc(ref _documentLatitude, "rvt:Document:Latitude");
        AddDesc(ref _documentLongitude, "rvt:Document:Longitude");
        AddDesc(ref _documentPlaceName, "rvt:Document:PlaceName");
        AddDesc(ref _documentWeatherStationName, "rvt:Document:WeatherStationName");
        AddDesc(ref _documentTimeZone, "rvt:Document:TimeZone");

        AddDesc(ref _projectName, "rvt:Document:Project:Name");
        AddDesc(ref _projectNumber, "rvt:Document:Project:Number");
        AddDesc(ref _projectStatus, "rvt:Document:Project:Status");
        AddDesc(ref _projectAddress, "rvt:Document:Project:Address");
        AddDesc(ref _projectClientName, "rvt:Document:Project:Client");
        AddDesc(ref _projectIssueDate, "rvt:Document:Project:IssueDate");
        AddDesc(ref _projectAuthor, "rvt:Document:Project:Author");
        AddDesc(ref _projectBuildingName, "rvt:Document:Project:BuildingName");
        AddDesc(ref _projectOrgDescription, "rvt:Document:Project:OrganizationDescription");
        AddDesc(ref _projectOrgName, "rvt:Document:Project:OrganizationName");

        AddDesc(ref _categoryCategoryType, "rvt:Category:CategoryType");
        AddDesc(ref _categoryBuiltInType, "rvt:Category:BuiltInType");
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

    public void AddTypeAsParameter(EntityIndex ei, object o)
    {
        bdb.AddParameter(ei, _apiTypeDescriptor, o.GetType().Name);
    }

    public EntityIndex ProcessCategory(Category category)
    {
        if (!ProcessedCategories.TryGetValue(category.Id.Value, out var result))
            return result;

        var r = bdb.AddEntity(category.Id.Value, category.Id.ToString(), CurrentDocumentIndex, category.Name,
            category.BuiltInCategory.ToString());

        bdb.AddParameter(r, _apiTypeDescriptor, category.GetType().Name);
        bdb.AddParameter(r, _categoryCategoryType, category.CategoryType.ToString());
        bdb.AddParameter(r, _categoryCategoryType, category.BuiltInCategory.ToString());

        foreach (Category subCategory in category.SubCategories)
        {
            var subCatId = ProcessCategory(subCategory);
            bdb.AddRelation(subCatId, r, RelationType.SubcategoryOf);
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

            bdb.AddParameter(layerEi, _layerIndex, index);
            bdb.AddParameter(layerEi, _layerFunction, layer.Function.ToString());
            bdb.AddParameter(layerEi, _layerWidth, layer.Width);
            bdb.AddParameter(layerEi, _layerIsCore, layer.IsCore ? 1 : 0);

            var matId = layer.MaterialId;
            if (matId != ElementId.InvalidElementId)
            {
                var matIndex = ProcessElement(matId);
                bdb.AddParameter(layerEi, _layerMaterialId, matIndex);
                bdb.AddRelation(layerEi, matIndex, RelationType.HasMaterial);
            }

            AddTypeAsParameter(layerEi, layer);
            bdb.AddRelation(ei, layerEi, RelationType.HasLayer);
        }
    }

    public void ProcessMaterial(EntityIndex ei, Material m)
    {
        var color = m.Color;
        bdb.AddParameter(ei, _materialColorGreen, color.Red);
        bdb.AddParameter(ei, _materialColorGreen, color.Green);
        bdb.AddParameter(ei, _materialColorGreen, color.Blue);

        bdb.AddParameter(ei, _materialTransparency, m.Transparency);
        bdb.AddParameter(ei, _materialShininess, m.Shininess);
        bdb.AddParameter(ei, _materialSmoothness, m.Smoothness);
        bdb.AddParameter(ei, _materialCategory, m.MaterialCategory);
        bdb.AddParameter(ei, _materialClass, m.MaterialClass);
    }

    public void ProcessFamily(EntityIndex ei, Family f)
    {
        bdb.AddParameter(ei, _familyStructuralCodeName, f.StructuralCodeName);
        bdb.AddParameter(ei, _familyStructuralMaterialType, f.StructuralMaterialType.ToString());
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
            bdb.AddParameter(ei, _familyInstanceToRoomDesc, ProcessElement(toRoom));

        var fromRoom = f.FromRoom;
        if (fromRoom != null && fromRoom.IsValidObject)
            bdb.AddParameter(ei, _familyInstanceFromRoomDesc, ProcessElement(fromRoom));

        var host = f.Host;
        if (host != null && host.IsValidObject)
        {
            var hostIndex = ProcessElement(host);
            bdb.AddParameter(ei, _familyInstanceHost, hostIndex);
            bdb.AddRelation(ei, hostIndex, RelationType.HostedBy);
        }

        var space = f.Space;
        if (space != null && space.IsValidObject)
        {
            var spaceIndex = ProcessElement(space);
            bdb.AddParameter(ei, _familyInstanceSpace, spaceIndex);
            bdb.AddRelation(ei, spaceIndex, RelationType.ContainedIn);
        }

        var room = f.Room;
        if (room != null && room.IsValidObject)
        {
            var roomIndex = ProcessElement(room);
            bdb.AddParameter(ei, _familyInstanceSpace, roomIndex);
            bdb.AddRelation(ei, roomIndex, RelationType.ContainedIn);
        }

        var matId = f.StructuralMaterialId;
        if (matId != ElementId.InvalidElementId)
        {
            var matIndex = ProcessElement(matId);
            bdb.AddParameter(ei, _familyInstanceStructuralMaterialId, matIndex);
            bdb.AddRelation(ei, matIndex, RelationType.HasMaterial);
        }

        bdb.AddParameter(ei, _familyInstanceStructuralUsage, f.StructuralUsage.ToString());
        bdb.AddParameter(ei, _familyInstanceStructuralType, f.StructuralMaterialType.ToString());
    }

    public void ProcessRoom(EntityIndex ei, Room room)
    {
        bdb.AddParameter(ei, _roomBaseOffset, room.BaseOffset);
        bdb.AddParameter(ei, _roomLimitOffset, room.LimitOffset);
        bdb.AddParameter(ei, _roomNumber, room.Number);
        bdb.AddParameter(ei, _roomUnboundedHeight, room.UnboundedHeight);
        if (room.UpperLimit != null && room.UpperLimit.IsValidObject)
            bdb.AddParameter(ei, _roomUpperLimit, ProcessElement(room.UpperLimit));
        bdb.AddParameter(ei, _roomVolume, room.Volume);
    }
    
    public void ProcessLevel(EntityIndex ei, Level level)
    {
        bdb.AddParameter(ei, _levelElevation, level.Elevation);
        bdb.AddParameter(ei, _levelProjectElevation, level.ProjectElevation);
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
                        var valIndex = ProcessElement(e);
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

    public EntityIndex ProcessElement(ElementId id)
    {
        if (id == null || id == ElementId.InvalidElementId)
            throw new Exception("Invalid element");
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
            bdb.AddParameter(entityIndex, _elementCategory, catIndex);
            bdb.AddRelation(entityIndex, catIndex, RelationType.MemberOf);
        }

        ProcessParameters(entityIndex, e);
        ProcessMaterials(entityIndex, e);

        var bounds = GetBoundingBoxMinMax(e);
        if (bounds.HasValue)
        {
            var min = AddPoint(bdb, bounds.Value.min);
            var max = AddPoint(bdb, bounds.Value.max);
            bdb.AddParameter(entityIndex, _elementBoundsMin, min);
            bdb.AddParameter(entityIndex, _elementBoundsMax, max);
        }

        var levelId = e.LevelId;
        if (levelId != ElementId.InvalidElementId)
        {
            var levelIndex = ProcessElement(levelId);
            bdb.AddParameter(entityIndex, _elementLevel, levelIndex);
            bdb.AddRelation(entityIndex, levelIndex, RelationType.ContainedIn);
        }

        var assemblyInstanceId = e.AssemblyInstanceId;
        if (assemblyInstanceId != ElementId.InvalidElementId)
        {
            var assemblyIndex = ProcessElement(assemblyInstanceId);
            bdb.AddParameter(entityIndex, _elementAssemblyInstance, assemblyIndex);
            bdb.AddRelation(entityIndex, assemblyIndex, RelationType.MemberOf);
        }
        
        var location = e.Location;
        if (location != null)
        {
            if (location is LocationPoint lp)
            {
                bdb.AddParameter(entityIndex, _elementLocation, AddPoint(bdb, lp.Point));
            }

            if (location is LocationCurve lc)
            {
                if (TryGetLocationEndpoints(lc, out var startPoint, out var endPoint))
                {
                    bdb.AddParameter(entityIndex, _elementLocationStartPoint, AddPoint(bdb, startPoint));
                    bdb.AddParameter(entityIndex, _elementLocationEndPoint, AddPoint(bdb, endPoint));
                }
            }
        }

        if (e.CreatedPhaseId != ElementId.InvalidElementId)
        {
            var createdPhase = ProcessElement(e.CreatedPhaseId);
            bdb.AddParameter(entityIndex, _elementCreatedPhase, createdPhase);
        }

        if (e.DemolishedPhaseId != ElementId.InvalidElementId)
        {
            var demolishedPhase = ProcessElement(e.DemolishedPhaseId);
            bdb.AddParameter(entityIndex, _elementDemolishedPhase, demolishedPhase);
        }

        var designOption = e.DesignOption;
        if (designOption != null && designOption.IsValidObject)
        {
            var doIndex = ProcessElement(designOption);
            bdb.AddRelation(entityIndex, doIndex, RelationType.MemberOf);
            bdb.AddParameter(entityIndex, _elementDesignOption, doIndex);
        }

        var groupId = e.GroupId;
        if (groupId != ElementId.InvalidElementId)
        {
            var group = ProcessElement(groupId);
            bdb.AddRelation(entityIndex, group, RelationType.MemberOf);
        }

        if (e.WorksetId != null)
        {
            bdb.AddParameter(entityIndex, _elementWorkset, e.WorksetId.IntegerValue);
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
            
        bdb.AddParameter(ei, _documentPath, CurrentDocument.PathName);
        bdb.AddParameter(ei, _documentTitle, CurrentDocument.Title);

        if (CurrentDocument.IsWorkshared)
            bdb.AddParameter(ei, _documentWorksharingGuid, CurrentDocument.WorksharingCentralGUID.ToString());
        bdb.AddParameter(ei, _documentCreationGuid, CurrentDocument.CreationGUID.ToString());
        bdb.AddParameter(ei, _documentElevation, siteLocation.Elevation);
        bdb.AddParameter(ei, _documentLatitude, siteLocation.Latitude);
        bdb.AddParameter(ei, _documentLongitude, siteLocation.Longitude);
        bdb.AddParameter(ei, _documentPlaceName, siteLocation.PlaceName);
        bdb.AddParameter(ei, _documentWeatherStationName, siteLocation.WeatherStationName);
        bdb.AddParameter(ei, _documentTimeZone, siteLocation.TimeZone);

        var projectInfo = CurrentDocument.ProjectInformation;

        bdb.AddParameter(ei, _projectAddress, projectInfo.Address);
        bdb.AddParameter(ei, _projectAuthor, projectInfo.Author);
        bdb.AddParameter(ei, _projectBuildingName, projectInfo.BuildingName);
        bdb.AddParameter(ei, _projectClientName, projectInfo.ClientName);
        bdb.AddParameter(ei, _projectIssueDate, projectInfo.IssueDate);
        bdb.AddParameter(ei, _projectName, projectInfo.Name);
        bdb.AddParameter(ei, _projectNumber, projectInfo.Number);
        bdb.AddParameter(ei, _projectOrgDescription, projectInfo.OrganizationDescription);
        bdb.AddParameter(ei, _projectOrgName, projectInfo.OrganizationName);
        bdb.AddParameter(ei, _projectStatus, projectInfo.Status);

        foreach (var e in CurrentDocument.GetElements())
            ProcessElement(e);

        if (IncludeLinks)
            foreach (var linkedDoc in d.GetLinkedDocuments())
                ProcessDocument(linkedDoc);
    }
}