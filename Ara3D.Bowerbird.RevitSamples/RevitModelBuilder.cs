using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Geometry;
using Autodesk.Revit.DB;
using Element = Autodesk.Revit.DB.Element;
using Material = Ara3D.Models.Material;
using RevitMesh = Autodesk.Revit.DB.Mesh;

namespace Ara3D.Bowerbird.RevitSamples;

public class RevitModelBuilder
{
    public SerializableModel3D Build()
    {
        var bldr = new SerializableModel3DBuilder();
        foreach (var g in DocumentMeshGroups.Values)
        {
            bldr.Add(-1, g.Transform, g);
        }

        return bldr.BuildModel();
    }

    public Dictionary<(Guid, long), Material> MaterialLookup = new();
    public Document CurrentDoc { get; private set; }
    public Guid CurrentDocId => CurrentDoc.CreationGUID;
    public Options Options = new()
    {
        ComputeReferences = false,
        DetailLevel = ViewDetailLevel.Fine, 
        IncludeNonVisibleObjects = false
    };
    public Dictionary<Guid, MeshGroup> DocumentMeshGroups = new();
    public Dictionary<Solid, MeshGroup> SolidLookup = new();
    public Dictionary<(Guid, long), MeshGroup> ElementLookup = new();
    public Dictionary<(Guid, string), MeshGroup> SymbolGeometryLookup = new();
    public List<string> Errors = new();

    public static Point3D ToPoint3D(XYZ xyz)
        => ((float)xyz.X, (float)xyz.Y, (float)xyz.Z);

    public static Vector3 ToVector3(XYZData xyz)
        => ((float)xyz.X, (float)xyz.Y, (float)xyz.Z);

    public static Mesh ToMesh(RevitMesh m)
    {
        if (m == null)
            return null;

        var r = new Mesh();
        var n = m.NumTriangles;
        for (var i = 0; i < n; i++)
        {
            var tri = m.get_Triangle(i);
            var v0 = (int)tri.get_Index(0);
            var v1 = (int)tri.get_Index(1);
            var v2 = (int)tri.get_Index(2);
            r.IndexData.Add(v0);
            r.IndexData.Add(v1);
            r.IndexData.Add(v2);
        }

        foreach (var v in m.Vertices)
        {
            r.PointXData.Add((float)v.X);
            r.PointYData.Add((float)v.Y);
            r.PointZData.Add((float)v.Z);
        }

        return r;
    }

    public Material ToMaterial(PbrMaterialInfo pbr) 
        => pbr == null
            ? Material.Default
            : new Material(pbr.BaseColor ?? pbr.ShadingColor, (float)(pbr.Metallic ?? 0),
                (float)(pbr.Roughness ?? 0));

    public Material ToMaterial(long? materialId)
    {
        if (materialId == null || materialId == -1)
            return Material.Default;
        if (MaterialLookup.TryGetValue((CurrentDocId, materialId.Value), out var mat))
            return mat;
        var pbrMatInfo = CurrentDoc.GetPbrInfo(materialId.Value);
        var r = ToMaterial(pbrMatInfo);
        MaterialLookup.Add((CurrentDocId, materialId.Value), r);
        return r;
    }

    public MeshWithMaterial ToMeshWithMaterial(RevitMesh mesh, long? materialId)
        => ToMeshWithMaterial(ToMesh(mesh), materialId);

    public MeshWithMaterial ToMeshWithMaterial(Mesh mesh, long? materialId)
    {
        if (mesh == null || mesh.GetNumTriangles() == 0)
            return null;
        return new MeshWithMaterial() { Mesh = mesh, Material = ToMaterial(materialId) };
    }

    public MeshGroup ToMeshGroup(RevitMesh mesh, long? materialId)
        => ToMeshGroup(ToMeshWithMaterial(ToMesh(mesh), materialId));

    public MeshGroup ToMeshGroup(MeshWithMaterial mat)
    {
        if (mat == null) return null;
        var meshGroup = new MeshGroup();
        meshGroup.Meshes.Add(mat);
        return meshGroup;
    }

    public MeshGroup ToMeshGroup(Mesh mesh, long? materialId)
        => ToMeshGroup(ToMeshWithMaterial(mesh, materialId));

    public MeshGroup ProcessGeometryInstance(GeometryInstance gi)
    {
        if (gi == null)
            return null;

        var symId = gi.GetSymbolGeometryId().AsUniqueIdentifier();
        if (SymbolGeometryLookup.TryGetValue((CurrentDocId, symId), out var instance))
            return instance;

        try
        {
            var r = new MeshGroup();
            SymbolGeometryLookup.Add((CurrentDocId, symId), r);

            r.Transform = ToMatrix(gi.Transform);
            var symbolGeometryElement = gi.SymbolGeometry;
            if (symbolGeometryElement == null)
                return null;
            r.Children.Add(ProcessGeometryElement(symbolGeometryElement));

            return r;
        }
        catch (Exception ex)
        {
            Errors.Add(ex.Message);
            return null;
        }
    }

    public MeshGroup ProcessGeometryElement(GeometryElement ee)
    {
        try
        {
            var r = new MeshGroup();
            foreach (GeometryObject go in ee)
            {
                var child = ProcessGeometryObject(go);
                if (child != null)
                    r.Children.Add(child);
            }

            return r;
        }
        catch (Exception e)
        {
            Errors.Add(e.Message);
            return null;
        }
    }

    public MeshGroup ProcessMesh(RevitMesh mesh)
    {
        if (mesh == null || mesh.NumTriangles == 0)
            return null;
        return ToMeshGroup(mesh, mesh.MaterialElementId?.Value);
    }

    public MeshGroup ProcessFace(Face face)
    {
        if (face == null)
            return null;

        try
        {
            var mesh = ToMeshWithMaterial(face.Triangulate(1.0), face.MaterialElementId?.Value);

            var r = new MeshGroup();

            r.Meshes.Add(mesh);

            if (face.HasRegions)
            {
                foreach (var region in face.GetRegions())
                {
                    var regionMesh = ProcessFace(region);
                    if (regionMesh != null)
                        r.Children.Add(regionMesh);
                }
            }

            return r;
        }
        catch (Exception ex)
        {
            Errors.Add(ex.Message);
            return null;
        }
    }

    public MeshGroup ProcessSolid(Solid solid)
    {
        try 
        {
            if (SolidLookup.TryGetValue(solid, out var meshGroup))
                return meshGroup;

            var r = new MeshGroup();
            foreach (Face f in solid.Faces)
            {
                var faceGroup = ProcessFace(f);
                if (faceGroup != null)
                    r.Children.Add(faceGroup);
            }

            SolidLookup.Add(solid, r);
            return r;
        }
        catch (Exception ex)
        {
            Errors.Add(ex.Message);
            return null;
        }
    }

    public MeshGroup ProcessGeometryObject(GeometryObject geom)
    {
        switch (geom)
        {
            case Solid solid:
                return ProcessSolid(solid);
                
            case RevitMesh mesh:
                return ProcessMesh(mesh);

            case GeometryInstance inst:
                return ProcessGeometryInstance(inst);

            case GeometryElement geomElem:
                return ProcessGeometryElement(geomElem);
                
            case Face face:
                return ProcessFace(face);
        }

        // Things like: lines, curves, profiles, etc. return null
        return null;
    }

    public static Matrix4x4? ToMatrix(Transform self)
        => self == null 
            ? null 
            : self.IsIdentity 
                ? null 
                : ToMatrix(self.ToData());

    public static Matrix4x4? ToMatrix(TransformData self)
    {
        if (self == null)
            return null;
        
        var o = ToVector3(self.Origin);
        if (self.IsTranslation)
        {
            if (o.X == 0f && o.Y == 0f && o.Z == 0f)
                return null;

            return Matrix4x4.CreateTranslation(o);
        }

        var x = ToVector3(self.BasisX);
        var y = ToVector3(self.BasisY);
        var z = ToVector3(self.BasisZ);
        return new Matrix4x4(
            x.X, x.Y, x.Z, 0f,
            y.X, y.Y, y.Z, 0f,
            z.X, z.Y, z.Z, 0f,
            o.X, o.Y, o.Z, 1f);
    }

    public MeshGroup ProcessElement(Element e)
    {
        if (e == null || !e.IsValidObject || e.Id == ElementId.InvalidElementId)
            return null;

        try
        {
            var guid = e.Document.CreationGUID;
            var id = e.Id.Value;

            if (ElementLookup.ContainsKey((guid, id)))
                return ElementLookup[(guid, id)];

            var ge = e.get_Geometry(Options);
            if (ge == null)
                return null;

            var r = ProcessGeometryElement(ge);
            if (r == null)
                return null;

            ElementLookup[(guid, id)] = r;
            return r;
        }
        catch (Exception ex)
        {
            Errors.Add(ex.Message);
            return null;
        }
    }

    public MeshGroup ProcessElements(Document doc)
    {
        var collector = new FilteredElementCollector(doc).WhereElementIsNotElementType();
        var r = new MeshGroup();
        foreach (Element e in collector)
        {
            var tmp = ProcessElement(e);
            if (tmp != null)
                r.Children.Add(tmp);
        }
        return r;
    }

    public MeshGroup ProcessDocument(Document doc)
    {
        var oldDoc = CurrentDoc;
        try
        {
            CurrentDoc = doc;
            if (DocumentMeshGroups.TryGetValue(CurrentDocId, out var docMeshGroup))
                return docMeshGroup;

            try
            {
                var r = ProcessElements(doc);
                DocumentMeshGroups.Add(CurrentDocId, r);

                foreach (var link in new FilteredElementCollector(doc)
                             .OfClass(typeof(RevitLinkInstance))
                             .OfType<RevitLinkInstance>())
                {
                    var linkDoc = link.GetLinkDocument();
                    if (linkDoc == null) continue;
                    var tmp = ProcessDocument(linkDoc);
                    tmp.Transform = ToMatrix(link.GetTotalTransform());
                    r.Children.Add(tmp);
                }

                return r;
            }
            catch (Exception ex)
            {
                Errors.Add(ex.Message);
                return null;
            }
        }
        finally
        {
            CurrentDoc = oldDoc;
        }
    }
}