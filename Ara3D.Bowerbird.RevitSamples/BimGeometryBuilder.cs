using System.Collections.Generic;
using System.Numerics;
using Ara3D.Models;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.RevitSamples;

public class Mesh
{
    public List<float> PointXData = new();
    public List<float> PointYData = new();
    public List<float> PointZData = new();
    public List<int> IndexData = new();

    // Helper functions 
    public int GetNumTriangles() => IndexData.Count / 3;
    public int GetNumPoints() => PointXData.Count / 3;
}

public class MeshWithMaterial
{
    public Mesh Mesh { get; set; }
    public Material Material { get; set; }
}

public class MeshGroup
{
    public Matrix4x4? Transform = null;
    public List<MeshGroup> Children { get; } = new();
    public List<MeshWithMaterial> Meshes { get; } = new();
}

public class BimGeometryBuilder
{
    public List<ElementStruct> Elements = new();
    public IndexedSet<Mesh> Meshes = new();
    public IndexedSet<Material> Materials = new();
    public IndexedSet<Matrix4x4> Matrices = new();

    public void Add(int objectId, Matrix4x4? parentTransform, MeshGroup group)
    {
        if (group == null)
            return;
        
        var currentTransform = parentTransform == null ? group.Transform : group.Transform * parentTransform;
        var matrixIndex = Matrices.Add(currentTransform ?? Matrix4x4.Identity);
        
        foreach (var child in group.Children)
            Add(objectId, currentTransform, child);
        
        foreach (var x in group.Meshes)
        {
            if (x == null)
                continue;
            var matId = Materials.Add(x.Material);
            var meshId = Meshes.Add(x.Mesh);
            var es = new ElementStruct(objectId, matId, meshId, matrixIndex);
            Elements.Add(es);
        }
    }

    public BimOpenGeometry BuildModel()
    {
        var r = new BimOpenGeometry();
        foreach (var es in Elements)
        {
            r.ElementEntityIds.Add(es.ElementIndex);
            r.ElementMaterialIndices.Add(es.MaterialIndex);
            r.ElementMeshIndices.Add(es.MeshIndex);
            r.ElementTransformIndices.Add(es.TransformIndex);
        }

        foreach (var m in Meshes.OrderedMembers())
        {
            if (m == null)
                continue;
            r.MeshVertexOffset.Add(r.VertexXData.Count);
            r.MeshIndexOffset.Add(r.FaceIndexData.Count);

            r.VertexXData.AddRange(m.PointXData);
            r.VertexYData.AddRange(m.PointYData);
            r.VertexZData.AddRange(m.PointZData);

            r.FaceIndexData.AddRange(m.IndexData);
        }

        foreach (var m in Materials.OrderedMembers())
        {
            if (m == null)
                continue;
            r.MaterialRed.Add(m.Color.R);
            r.MaterialGreen.Add(m.Color.G);
            r.MaterialBlue.Add(m.Color.B);
            r.MaterialAlpha.Add(m.Color.A);
            r.MaterialRoughness.Add(m.Roughness);
            r.MaterialMetallic.Add(m.Metallic);
        }

        foreach (var t in Matrices.OrderedMembers())
        {
            r.TransformData.Add(t.M11);
            r.TransformData.Add(t.M12);
            r.TransformData.Add(t.M13);
            r.TransformData.Add(t.M14);

            r.TransformData.Add(t.M21);
            r.TransformData.Add(t.M22);
            r.TransformData.Add(t.M23);
            r.TransformData.Add(t.M24);

            r.TransformData.Add(t.M31);
            r.TransformData.Add(t.M32);
            r.TransformData.Add(t.M33);
            r.TransformData.Add(t.M34);

            r.TransformData.Add(t.M41);
            r.TransformData.Add(t.M42);
            r.TransformData.Add(t.M43);
            r.TransformData.Add(t.M44);
        }

        return r;
    }
}