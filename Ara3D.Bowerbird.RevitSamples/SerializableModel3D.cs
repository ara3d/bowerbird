using System.Collections.Generic;
using Ara3D.Models;
using Ara3D.Utils;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace Ara3D.Bowerbird.RevitSamples;

public class SerializableModel3D
{
    // Element data
    public List<int> ElementObjectIds = new();
    public List<int> ElementMaterialIndices = new();
    public List<int> ElementMeshIndices = new();
    public List<int> ElementTransforms = new();

    // Point data
    public List<float> PointXData = new();
    public List<float> PointYData = new();
    public List<float> PointZData = new();
    
    // Index data 
    public List<int> IndexData = new();
    
    // Mesh data 
    public List<int> MeshPointOffset = new();
    public List<int> MeshIndexOffset = new();
    
    // Material data 
    public List<float> MaterialColorRed = new();
    public List<float> MaterialColorGreen = new();
    public List<float> MaterialColorBlue = new();
    public List<float> MaterialColorAlpha = new();
    public List<float> MaterialRoughness = new();
    public List<float> MaterialMetallic = new();
    
    // Transform data 
    public List<float> TransformData = new();

    // Helper functions 
    public int GetNumMatrices() => TransformData.Count / 16;
    public int GetNumMaterials() => MaterialColorRed.Count;
    public int GetNumMeshes() => MeshPointOffset.Count;
    public int GetNumElements() => ElementObjectIds.Count;
}

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

public class SerializableModel3DBuilder
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

    public SerializableModel3D BuildModel()
    {
        var r = new SerializableModel3D();
        foreach (var es in Elements)
        {
            r.ElementObjectIds.Add(es.ElementIndex);
            r.ElementMaterialIndices.Add(es.MaterialIndex);
            r.ElementMeshIndices.Add(es.MeshIndex);
            r.ElementTransforms.Add(es.TransformIndex);
        }

        foreach (var m in Meshes.OrderedMembers())
        {
            if (m == null)
                continue;
            r.MeshPointOffset.Add(r.PointXData.Count);
            r.MeshIndexOffset.Add(r.IndexData.Count);

            r.PointXData.AddRange(m.PointXData);
            r.PointYData.AddRange(m.PointYData);
            r.PointZData.AddRange(m.PointZData);
            r.IndexData.AddRange(m.IndexData);
        }

        foreach (var m in Materials.OrderedMembers())
        {
            if (m == null)
                continue;
            r.MaterialColorRed.Add(m.Color.R);
            r.MaterialColorGreen.Add(m.Color.G);
            r.MaterialColorBlue.Add(m.Color.B);
            r.MaterialColorAlpha.Add(m.Color.A);
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