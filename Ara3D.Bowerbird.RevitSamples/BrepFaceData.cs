using System.Collections.Generic;

namespace Ara3D.Brep;

public enum UvIndex;
public enum XyzIndex;
public enum UvListIndex;
public enum FaceIndex;
public enum GeometryObjectIndex;

public class BrepData
{
    public List<Uv> UvPoints { get; set; } = new();
    public List<Xyz> XyzPoints { get; set; } = new();
    public List<UvList> Loops { get; set; } = new();
    public List<Transform> Transforms { get; set; } = new();
    public List<HermiteFaceData> HermiteFaces { get; set; } = new();
    public List<ConicalFaceData> ConicalFaces { get; set; } = new();
    public List<CylindricalFaceData> CylindricalFaces { get; set; } = new();
    public List<PlanarFaceData> PlanarFaces { get; set; } = new();
    public List<RevolvedFaceData> RevolvedFaces { get; set; } = new();
    public List<RuledFaceData> RuledFaces { get; set; } = new();
    public List<FaceData> Faces { get; set; } = new();

    public List<GeometryInstance> Instances { get; set; } = new();
    public List<GeometrySolid> Solids { get; set; } = new();
    public List<GeometryElement> Elements { get; set; } = new();
    public List<GeometryMesh> Meshes { get; set; } = new();

    public List<GeometryObject> Objects { get; set; } = new();
}

public enum GeometryType
{
    Instance,
    Solid,
    Element,
    Mesh
}

public struct GeometryObject
{
    public int Index;
    public GeometryType Type;
}

public struct GeometryObjectList
{
    public GeometryObjectIndex First;
    public int Count;
}

public struct Uv
{
    public float U;
    public float V;
}

public struct UvList
{
    public UvIndex Begin;
    public int Count;
}

public struct UvLoopList
{
    public UvListIndex First;
    public int Count;
}

public struct Xyz
{
    public float X;
    public float Y;
    public float Z;
}

public struct XyzList
{
    public XyzIndex First;
    public int Count;
}

public struct Transform
{
    public Xyz Origin;
    public Xyz BasisX;
    public Xyz BasisY;
    public Xyz BasisZ;
    public bool HasReflection;
    public bool IsConformal;
    public bool IsTranslation;
}

public struct CurveData
{
    public float ApproximateLength;
    public bool IsBound;
    public bool IsClosed;
    public bool IsCyclic;
    public float Period;
    public XyzList Points;
}

public enum FaceType
{
    Conical,
    Cylindrical,
    Hermite,
    Planar,
    Revolved,
    Ruled,
}

public struct FaceData
{
    public int ContextId;
    public Uv Min;
    public Uv Max;
    public UvLoopList Boundaries;
    public bool CyclicU;
    public bool CyclicV;
    public float PeriodU;
    public float PeriodV;
    public bool OrientationMatchesParametrization;
    public bool TwoSided;
    public FaceType FaceType;
    public FaceIndex FaceIndex;
}

public struct HermiteFaceData
{
    public XyzList MixedDerivatives;
    public XyzList Points;
    public UvList ParamsUv;
    public XyzList TangentsU;
    public XyzList TangentsV;
}

public struct ConicalFaceData
{
    public Xyz Axis;
    public float HalfAngle;
    public Xyz Origin;
    public Xyz Radius0;
    public Xyz Radius1;
}

public record CylindricalFaceData
{
    public Xyz Axis;
    public Xyz Origin;
    public Xyz Radius0;
    public Xyz Radius1;
}

public struct PlanarFaceData
{
    public Xyz Origin;
    public Xyz Normal;
    public Xyz XVector;
    public Xyz YVectors;
}

public struct RevolvedFaceData
{
    public Xyz Axis;
    public CurveData Curve;
    public Xyz Origin;
    public Xyz Radius0;
    public Xyz Radius1;
}

public struct RuledFaceData
{
    public bool IsExtruded;
    public bool IsParallel;
    public CurveData Curve0;
    public CurveData Curve1;
    public Xyz Point0;
    public Xyz Point1;
}

public struct GeometrySolid
{
    public int FaceIndexOffset;
    public int FaceCount;
}

public struct GeometryInstance
{
    public Transform Transform;
    public string SymbolUniqueId;
}

public struct GeometryElement
{
    public int ContextId;
    public GeometryObjectList Objects;
}

public struct GeometryMesh
{
    public List<Xyz> Points;
    public List<int> Indices;
}

public struct Element
{
    public long Id;
    public int GeometryElementIndex;
    public Transform Transform;
}

public class Document
{
    public string DocumentName;
    public Transform Transform;
    public List<Document> Documents = new();
};