using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Diagnostics;
using RevitMesh = Autodesk.Revit.DB.Mesh;

namespace Ara3D.Bowerbird.RevitSamples;

public static class FaceDataExtensions
{
    public static FaceType GetFaceType(this Face face)
    {
        switch (face)
        {
            case ConicalFace conicalFace:
                return FaceType.Conical;
            case CylindricalFace cylindricalFace:
                return FaceType.Cylindrical;
            case HermiteFace hermiteFace:
                return FaceType.Hermite;
            case PlanarFace planarFace:
                return FaceType.Planar;
            case RevolvedFace revolvedFace:
                return FaceType.Revolved;
            case RuledFace ruledFace:
                return FaceType.Ruled;
            default:
                throw new ArgumentOutOfRangeException(nameof(face));
        }
    }

    public static object ToSpecificFaceData(this Face face)
    {
        switch (face)
        {
            case ConicalFace conicalFace:
                return new ConicalFaceData(
                    conicalFace.Axis.ToData(),
                    conicalFace.HalfAngle,
                    conicalFace.Origin.ToData(),
                    conicalFace.get_Radius(0).ToData(),
                    conicalFace.get_Radius(1).ToData());
            case CylindricalFace cylindricalFace:
                return new CylindricalFaceData(
                    cylindricalFace.Axis.ToData(),
                    cylindricalFace.Origin.ToData(),
                    cylindricalFace.get_Radius(0).ToData(),
                    cylindricalFace.get_Radius(1).ToData());
                break;
            case HermiteFace hermiteFace:
                return new HermiteFaceData(
                    hermiteFace.MixedDerivs.ToData(),
                    hermiteFace.Points.ToData(),
                    hermiteFace.get_Params(0).ToData(),
                    hermiteFace.get_Params(1).ToData(),
                    hermiteFace.get_Tangents(0).ToData(),
                    hermiteFace.get_Tangents(1).ToData());
            case PlanarFace planarFace:
                return new PlanarFaceData(
                    planarFace.Origin.ToData(),
                    planarFace.FaceNormal.ToData(),
                    planarFace.XVector.ToData(),
                    planarFace.YVector.ToData());
            case RevolvedFace revolvedFace:
                return new RevolvedFaceData(
                    revolvedFace.Axis.ToData(),
                    revolvedFace.Curve.ToData(),
                    revolvedFace.Origin.ToData(),
                    revolvedFace.get_Radius(0).ToData(),
                    revolvedFace.get_Radius(1).ToData());
            case RuledFace ruledFace:
                return new RuledFaceData(
                    ruledFace.IsExtruded,
                    ruledFace.RulingsAreParallel,
                    ruledFace.get_Curve(0).ToData(),
                    ruledFace.get_Curve(1).ToData(),
                    ruledFace.get_Point(0).ToData(),
                    ruledFace.get_Point(1).ToData());
            default:
                throw new ArgumentOutOfRangeException(nameof(face));
        }
    }

    public static List<double> ToData(this DoubleArray self)
    {
        var r = new List<double>(self.Size);
        for (var i = 0; i < self.Size; i++)
            r.Add(self.get_Item(i));
        return r;
    }

    public static UVData ToData(this UV self)
        => new(self.U, self.V);

    public static List<UVData> ToData(this IEnumerable<UV> self)
        => self.Select(ToData).ToList();

    public static XYZData ToData(this XYZ self)
        => new(self.X, self.Y, self.Z);

    public static List<XYZData> ToData(this IEnumerable<XYZ> self)
        => self.Select(ToData).ToList();

    public static List<UVData> GetEdgePoints(this Face face, Edge edge)
        => edge.TessellateOnFace(face).Select(ToData).ToList();

    public static List<UVData> GetEdgePoints(this Face face, EdgeArray edgeArray)
    {
        var r = new List<UVData>();
        foreach (Edge edge in edgeArray)
            r.AddRange(face.GetEdgePoints(edge));
        return r;
    }

    public static UVLoop GetUVLoop(this Face face, EdgeArray edgeArray)
        => new(face.GetEdgePoints(edgeArray));

    public static List<UVLoop> GetUVLoops(this Face face)
    {
        var r = new List<UVLoop>();
        var edgeLoops = face.EdgeLoops;
        foreach (EdgeArray edgeArray in edgeLoops)
            r.Add(GetUVLoop(face, edgeArray));
        return r;
    }

    public static FaceData ToData(this Face face)
    {
        var bounds = face.GetBoundingBox();
        var regions = face.HasRegions ? face.GetRegions().Select(ToData).ToList() : [];
        return new(
            face.Id,
            bounds.Min.ToData(),
            bounds.Max.ToData(),
            GetUVLoops(face),
            face.get_IsCyclic(0),
            face.get_IsCyclic(1),
            face.get_IsCyclic(0) ? face.get_Period(0) : 0.0,
            face.get_IsCyclic(1) ? face.get_Period(1) : 0.0,
            regions,
            face.GetFaceType(),
            face.OrientationMatchesSurfaceOrientation,
            face.IsTwoSided,
            face.ToSpecificFaceData());
    }

    public static CurveData ToData(this Curve self)
    {
        var points = self.IsBound ? self.Tessellate().ToData() : null;
        return new(self.ApproximateLength, self.IsBound, self.IsClosed, self.IsCyclic, self.Period, points);
    }

    public static TransformData ToData(this Transform self)
    {
        if (self.IsIdentity) return null;
        return new TransformData(
            self.Origin.ToData(),
            self.BasisX.ToData(),
            self.BasisY.ToData(),
            self.BasisZ.ToData(),
            self.HasReflection,
            self.IsConformal,
            self.IsTranslation);
    }

    public static string GetUniqueIdString(this GeometryInstance self)
        => self.GetSymbolGeometryId().AsUniqueIdentifier();

    public static TransformData GetTransform(this GeometryInstance self)
        => (self.Transform).ToData();

    public static int ProcessGeometryElement(this GeometryElement self, GeometryData data)
    {
        var childIds = self.Select(g => ProcessGeometryObject(g, data)).Where(index => index >= 0).ToList();
        if (childIds.Count == 0) return -1;
        var tmp = new GeometryElementData(self.Id, childIds);
        return data.Add(tmp);
    }

    public static int NotProcessedMessage<T>(T self)
    {
        Debug.WriteLine($"Object {self} of type {typeof(T)} is not processed");
        return -1;
    }

    public static int ProcessFace(this Face self, GeometryData data)
        => data.Add(self.ToData());

    public static int ProcessGeometryInstance(this GeometryInstance self, GeometryData data)
    {
        var r = new GeometryInstanceData(self.Id, self.GetTransform(), self.GetUniqueIdString());
        if (!data.SymbolIdsToGeometry.ContainsKey(r.SymbolUniqueId))
            ProcessSymbolGeometry(self.SymbolGeometry, r.SymbolUniqueId, data);
        return data.Add(r);
    }

    public static int ProcessSolid(this Solid self, GeometryData data)
    {
        var faces = new List<FaceData>();
        foreach (Face face in self.Faces)
            faces.Add(face.ToData());
        var faceIds = data.AddRange(faces);
        var tmp = new SolidGeometryData(self.Id, faceIds);
        return data.Add(tmp);
    }

    public static MeshData ToData(this RevitMesh mesh)
    {
        var numTris = mesh.NumTriangles;
        var points = mesh.Vertices.ToData();
        var indices = new List<int>();
        for (var i = 0; i < numTris; i++)
        {
            var tri = mesh.get_Triangle(i);
            indices.Add((int)tri.get_Index(0));
            indices.Add((int)tri.get_Index(1));
            indices.Add((int)tri.get_Index(2));
        }

        return new MeshData(mesh.Id, points, indices);
    }

    public static int ProcessMesh(this RevitMesh mesh, GeometryData data)
        => data.Add(mesh.ToData());

    public static void ProcessSymbolGeometry(this GeometryElement element, string id, GeometryData data)
    {
        if (data.SymbolIdsToGeometry.ContainsKey(id))
            throw new Exception("Internal error: ID already exists.");

        var index = ProcessGeometryElement(element, data);
        data.SymbolIdsToGeometry.Add(id, index);
    }

    public static int ProcessGeometryObject(this GeometryObject self, GeometryData data)
    {
        switch (self)
        {
            case Arc arc:
                return NotProcessedMessage(arc);
            case CylindricalHelix cylindricalHelix:
                return NotProcessedMessage(cylindricalHelix);
            case Ellipse ellipse:
                return NotProcessedMessage(ellipse);
            case HermiteSpline hermiteSpline:
                return NotProcessedMessage(hermiteSpline);
            case Line line:
                return NotProcessedMessage(line);
            case NurbSpline nurbSpline:
                return NotProcessedMessage(nurbSpline);
            case Curve curve:
                return NotProcessedMessage(curve);
            case Edge edge:
                return NotProcessedMessage(edge);
            case Face face:
                return ProcessFace(face, data);
            case GeometryElement geometryElement:
                return ProcessGeometryElement(geometryElement, data);
            case GeometryInstance geometryInstance:
                return ProcessGeometryInstance(geometryInstance, data);
            case Autodesk.Revit.DB.Mesh mesh:
                return ProcessMesh(mesh, data);
            case Point point:
                return NotProcessedMessage(point);
            case PolyLine polyLine:
                return NotProcessedMessage(polyLine);
            case Profile profile:
                return NotProcessedMessage(profile);
            case Solid solid:
                return ProcessSolid(solid, data);
            default:
                throw new ArgumentOutOfRangeException(nameof(self));
        }

        return -1;
    }

    public static void ProcessElement(this Element e, GeometryData data, Options options)
    {
        try
        {
            if (e == null || e.get_BoundingBox(null) == null)
                return;
            var geo = e.get_Geometry(options);
            if (geo == null) return;
            var tmp = ProcessGeometryElement(geo, data);
            var transform = e is Instance inst
                ? inst.GetTransform().ToData()
                : null;
            var ed = new ElementData(e.Id.Value, tmp, transform);
            data.Elements.Add(ed);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error occured while processing element {e.Id.Value}");
            Debugger.Break();
        }
    }

    public static void ProcessGeometryElements(this Document doc, GeometryData data, Options options)
    {
        var collector = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType();

        foreach (var e in collector)
            ProcessElement(e, data, options);
    }

    public static void ProcessDocument(this Document doc, DocumentData data, Options options)
    {
        data.DocumentName = doc.PathName;
        ProcessGeometryElements(doc, data.Geometry, options);

        foreach (var link in new FilteredElementCollector(doc)
                     .OfClass(typeof(RevitLinkInstance))
                     .OfType<RevitLinkInstance>())
        {
            var linkDoc = link.GetLinkDocument();
            if (linkDoc == null) continue;
            var childDoc = new DocumentData();
            data.Documents.Add(childDoc);
            childDoc.Transform = link.GetTransform().ToData();
            ProcessDocument(linkDoc, childDoc, options);
        }
    }

    public static DocumentData ProcessDocumentBrep(this Document doc)
    {
        var options = new Options
        {
            DetailLevel = ViewDetailLevel.Fine,
            IncludeNonVisibleObjects = false,
            ComputeReferences = false // huge: speed-up unless you need reference objects
        };

        var dd = new DocumentData();
        ProcessDocument(doc, dd, options);
        return dd;
    }
}