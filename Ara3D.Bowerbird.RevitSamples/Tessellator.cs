using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class UVPointData
    {
        public double U { get; set; }
        public double V { get; set; }
    }

    public class UVLoop
    {
        public List<UVPointData> Points { get; set; } = new();
    }

    public class UVFaceData
    {
        public UVPointData Min { get; set; }
        public UVPointData Max { get; set; }
        public List<UVLoop> Boundaries { get; set; } = new();
        public bool OrientationMatchesParametrization { get; set; }
        public double PeriodU { get; set; }
        public double PeriodV { get; set; }
        public bool CyclicU { get; set; }
        public bool CyclicV { get; set; }
        public List<UVFaceData> Regions { get; set; } = new();
    }

    public class UVElementData
    {
        public long ElementId { get; set; }
        public List<UVFaceData> Faces { get; set; } = new();
    }

    public class CommandFacesAndBoundaries : NamedCommand
    {
        public override string Name => "Export faces and boundaries";

        public UIApplication app { get; private set; }

        public override void Execute(object arg)
        {
            app = (arg as UIApplication);

            if (app == null)
                throw new Exception($"Passed argument {arg} is either null or not a UI application");

            var uiDoc = app.ActiveUIDocument;
            var doc = uiDoc.Document;

            var elements = doc.GetElements();
            var r = new List<UVElementData>();
            var geoOptions = new Options() { DetailLevel = ViewDetailLevel.Fine };

            foreach (var element in elements)
            {
                var tmp = GetElementData(element, geoOptions);
                if (tmp.Faces.Count > 0)
                    r.Add(GetElementData(element, geoOptions));
            }

            var s = JsonConvert.SerializeObject(r, Formatting.Indented);
            var outputFilePath = Path.Combine(Path.GetTempPath(), "faces.json");
            File.WriteAllText(outputFilePath, s);
        }

        public static UVPointData ToData(UV uv)
            => new() { U = uv.U, V = uv.V };

        public static UVElementData GetElementData(Element e, Options geometryOptions)
        {
            var faces = GetFaces(e, geometryOptions).Select(GetFaceData).ToList();
            return new() { ElementId = e.Id.Value, Faces = faces };
        }

        public static List<UVPointData> FaceEdgePoints(Face face, Edge edge)
            => edge.TessellateOnFace(face).Select(ToData).ToList();

        public static List<UVPointData> FaceEdgePoints(Face face, EdgeArray edgeArray)
        {
            var r = new List<UVPointData>();
            foreach (Edge edge in edgeArray)
                r.AddRange(FaceEdgePoints(face, edge));
            return r;
        }

        public static UVLoop GetUVLoop(Face face, EdgeArray edgeArray)
            => new() { Points = FaceEdgePoints(face, edgeArray) };

        public static List<UVLoop> GetUVLoops(Face face)
        {
            var r = new List<UVLoop>();
            var edgeLoops = face.EdgeLoops;
            foreach (EdgeArray edgeArray in edgeLoops)
                r.Add(GetUVLoop(face, edgeArray));
            return r;
        }

        public static UVFaceData GetFaceData(Face face)
        {
            var r = new UVFaceData();
            r.CyclicU = face.get_IsCyclic(0);
            r.CyclicV = face.get_IsCyclic(1);
            r.Boundaries = GetUVLoops(face);
            r.OrientationMatchesParametrization = face.OrientationMatchesSurfaceOrientation;
            if (r.CyclicU) r.PeriodU = face.get_Period(0);
            if (r.CyclicV) r.PeriodV = face.get_Period(1);
            if (face.HasRegions)
            {
                r.Regions = face.GetRegions().Select(GetFaceData).ToList();
            }
            return r;
        }

        public static List<Face> GetFaces(Element e, Options geometryOptions)
        {
            var r = new List<Face>();
            GeometryElement geometry = e.get_Geometry(geometryOptions);
            if (geometry != null)
            {
                foreach (GeometryObject obj in geometry)
                {
                    CollectFaces(obj, r);
                }
            }

            return r;
        }

        public static void CollectFaces(GeometryObject obj, List<Face> faces )
        {
            if (obj is GeometryInstance inst)
            {
                foreach (GeometryObject instObj in inst.GetInstanceGeometry())
                    CollectFaces(instObj, faces);
            }

            if (obj is Solid solid)
            {
                foreach (Face face in solid.Faces)
                    faces.Add(face);
            }
        }
    }
}