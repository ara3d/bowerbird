using System.Collections.Generic;

namespace Ara3D.Bowerbird.RevitSamples;

public class BimOpenGeometry
{
    //==
    // Model data 
    //
    // A model is a set of elements (e.g., discipline models, or different buildings) 

    // The index of the first element associated with each model
    public List<int> ModelElementOffset { get; set; } = new();

    //==
    // Element data
    //
    // An element consists of a mesh, material, transform associated with a specific object
    // An object may have different elements.
    // Meshes, materials, and transforms may all be shared to reduce repetition 

    // Entity ids are unique within a given model. Used for referencing extra data 
    // An entity id may be repeated across different models, but with different meaning
    public List<int> ElementEntityIds { get; set; } = new();

    // Index of the material associated with this element
    public List<int> ElementMaterialIndices { get; set; } = new();

    // Index of the mesh associated with this element
    public List<int> ElementMeshIndices { get; set; } = new();

    // Index of the transform associated with this element 
    public List<int> ElementTransformIndices { get; set; } = new();

    //==
    // Vertex point data
    //
    // Each buffer is the same size. 

    // X position of each vertex
    public List<float> VertexXData { get; set; } = new();
    
    // Y position of each vertex
    public List<float> VertexYData { get; set; } = new();
    
    // Z position of each vertex
    public List<float> VertexZData { get; set; } = new();

    //==
    // Index data

    // A multiple of the face size, used for indirectly referencing the vertices 
    // Needs to be added to the appropriate mesh vertex offset. 
    public List<int> FaceIndexData { get; set; } = new();
    
    //==
    // Mesh data 
    
    // The offset into the vertex buffer where each mesh starts 
    public List<int> MeshVertexOffset { get; set; } = new();
    
    // The offset into the index buffer where each index starts. Indices reference  
    public List<int> MeshIndexOffset { get; set; } = new();

    // (1=points, 2=lines, 3=triangles, 4=quads) if 0 or absent, face index size of 3 is assumed.
    public List<int> MeshFaceSize { get; set; } = new(); 
    
    //==
    // Material data 

    public List<float> MaterialRed { get; set; } = new();
    public List<float> MaterialGreen { get; set; } = new();
    public List<float> MaterialBlue { get; set; } = new();
    public List<float> MaterialAlpha { get; set; } = new();
    public List<float> MaterialRoughness { get; set; } = new();
    public List<float> MaterialMetallic { get; set; } = new();
    
    //==
    // Transform data.

    // Transforms are stored as 4x4 matrices following the System.Numerics layout
    public List<float> TransformData { get; set; }= new();
}
