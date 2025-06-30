using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Bowerbird.RevitSamples;

public record UVData(double U, double V);
public record XYZData(double X, double Y, double Z);
public record UVLoop(List<UVData> Points);

public record TransformData(
    XYZData Origin,
    XYZData BasisX,
    XYZData BasisY,
    XYZData BasisZ,
    bool HasReflection,
    bool IsConformal,
    bool IsTranslation);

public enum GeometryObjectType
{
    Face,
    Solid,
    Element,
    Instance,
    Mesh,
}

public record GeometryObjectData(
    int ContextId,
    GeometryObjectType Type);

public enum FaceType
{
    Conical,
    Cylindrical,
    Hermite,
    Planar,
    Revolved,
    Ruled,
}

public record FaceData(
    int ContextId,
    UVData Min,
    UVData Max,
    List<UVLoop> Boundaries,
    bool CyclicU,
    bool CyclicV,
    double PeriodU,
    double PeriodV,
    List<FaceData> Regions,
    FaceType FaceType,
    bool OrientationMatchesParametrization,
    bool TwoSided,
    object SpecificFaceData
) 
    : GeometryObjectData(ContextId, GeometryObjectType.Face);

public record HermiteFaceData(
    List<XYZData> MixedDerivatives,
    List<XYZData> Points,
    List<double> ParamsU,
    List<double> ParamsV,
    List<XYZData> TangentsU,
    List<XYZData> TangentsV);

public record ConicalFaceData(XYZData Axis, double HalfAngle, XYZData Origin, XYZData Radius0, XYZData Radius1);
public record CylindricalFaceData(XYZData Axis, XYZData Origin, XYZData Radius0, XYZData Radius1);
public record PlanarFaceData(XYZData Origin, XYZData Normal, XYZData XVector, XYZData YVector);
public record RevolvedFaceData(XYZData Axis, CurveData Curve, XYZData Origin, XYZData Radius0, XYZData Radius1);
public record RuledFaceData(bool IsExtruded, bool IsParallel, CurveData Curve0, CurveData Curve1, XYZData Point0, XYZData Point1);

public record SolidGeometryData(int ContextId, List<int> FaceIndices) 
    : GeometryObjectData(ContextId, GeometryObjectType.Solid);

public record GeometryInstanceData(int ContextId, TransformData Transform, string SymbolUniqueId)
    : GeometryObjectData(ContextId, GeometryObjectType.Instance);

public record GeometryElementData(int ContextId, List<int> GeometryObjectIndexes)
    : GeometryObjectData(ContextId, GeometryObjectType.Element);

public record CurveData(
    double ApproximateLength,
    bool IsBound,
    bool IsClosed,
    bool IsCyclic,
    double Period,
    List<XYZData> Points);

public record MeshData(
    int ContextId,
    List<XYZData> Points,
    List<int> Indices) : GeometryObjectData(ContextId, GeometryObjectType.Mesh);

public record ElementData(long Id, int GeometryElementIndex, TransformData Transform);

public class GeometryData
{   
    public Dictionary<string, int> SymbolIdsToGeometry = new();
    public List<object> GeometryObjects = new();
    public List<ElementData> Elements = new();
    
    public int Add(GeometryObjectData geometryObject)
    {
        GeometryObjects.Add(geometryObject);
        return GeometryObjects.Count - 1;
    }

    public List<int> AddRange(IEnumerable<GeometryObjectData> geometryObjects)
        => geometryObjects.Select(Add).ToList();
}

public class DocumentData
{
    public string DocumentName;
    public TransformData Transform;
    public GeometryData Geometry = new();
    public List<DocumentData> Documents = new();
};