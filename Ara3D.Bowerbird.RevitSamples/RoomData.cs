using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Autodesk.Revit.DB.Architecture;
using System;

namespace Ara3D.Bowerbird.RevitSamples;

public enum GoalScope
{
    Door,
    Room,
    Corridor,
    Department,
    Classification,
    Level,
    Building,
}

public enum GoalOperator
{
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterOrEqual,
    NearTo,
    Between,
    EqualTo,
}

public class GoalEvaluation
{
    public string EntityId { get; set; }
    public Goal Goal { get; set; }
    public double Value { get; set; }
}

public abstract class Goal
{
    public string Name { get; set; }
    public string Description { get; set; }
    public double Target { get; set; }
    public double Range { get; set; }
    public GoalOperator Operator { get; set; }
    public GoalScope Scope { get; set; }
    public abstract GoalEvaluation Evaluate(Layout layout);
}

public class Layout
{
    public List<RoomData> Rooms { get; set; } = [];
}

public class RoomDataComputedMetrics
{
    public bool IsCorridor { get; set; }
    public bool IsBathroom { get; set; }
    public double Width { get; set; }
    public int NumAccessPoints { get; set; }
    public bool IsClassified { get; set; }
    public double MinDoorWidth { get; set; }
    public double MaxDoorWidth { get; set; }
    public List<(string, double)> RoomTypeNavDistance { get; set; }
}

public class RoomData
{
    public string Name;
    public string Department;
    public string Classification;
    public string SubClassification;
    public string RoomId;

    public int Lights;
    public int Doors;
    public int Sockets;
    public int Windows;
    public int Walls;

    public Dictionary<string, string> Parameters = new();
    public List<AccessPoint> AccessPoints = new();
    public Vector2 Center;
    public long Id;
    public double LimitOffset;
    public double Area;
    public double BoundingArea;
    public double BoundingVolume;
    public double BoundingHeight;
    public double AreaToBoundingArea;
    public double MaxSide;
    public double MinSide;
    public double Ratio;
    public double Volume;
    public double Elevation;
    public double Perimeter;
    public string LevelName;
    public double UnboundedHeight;
    public double UpperLevelElevation;
    public double BaseOffset;

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var fi in typeof(RoomData).GetFields())
        {
            var val = fi.GetValue(this);
            if (val is double d)
                val = d.ToString("0.##");
            sb.Append($"{fi.Name} = {val}; ");
        }

        return sb.ToString();
    }
}

public enum AccessType
{
    Door,
    WallOpening
}

public sealed class AccessPoint
{
    /// <summary>The aperture element (Door FamilyInstance or Opening).</summary>
    public ElementId ElementId { get; set; }

    /// <summary>Type of access.</summary>
    public AccessType Type { get; set; }

    /// <summary>Host wall of the aperture, if any.</summary>
    public ElementId HostWallId { get; set; }

    /// <summary>Representative point on the wall (projected onto wall line if possible; else element location/bbox center).</summary>
    public XYZ Point { get; set; }

    /// <summary>Door facing direction (BasisY) if Type == Door; otherwise null.</summary>
    public XYZ FacingDirection { get; set; }

    /// <summary>True if the given room is on the "To" side (for doors) or the wall's +normal side (for openings); otherwise false.</summary>
    public bool IsOnToSide { get; set; }

    /// <summary>Door/opening width</summary>
    public double Width { get; set; }

    /// <summary>Door/opening height</summary>
    public double Height { get; set; }

    /// <summary>Parameters if this is </summary>
    public Dictionary<string, string> Parameters = new();
}

public static class RoomAccessPoints
{
    /// <summary>
    /// Returns all access points (doors + wall openings) that connect to the specified Room (phase-aware).
    /// </summary>
    public static IList<AccessPoint> GetAccessPoints(Document doc, Room room)
    {
        if (doc is null) throw new ArgumentNullException(nameof(doc));
        if (room is null) throw new ArgumentNullException(nameof(room));

        var result = new List<AccessPoint>(64);

        // ---- DOORS (FamilyInstance, category OST_Doors) ----
        var doors = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Doors)
            .WhereElementIsNotElementType()
            .OfType<FamilyInstance>();

        foreach (var door in doors)
        {
            Room toRoom = null, fromRoom = null;
            try
            {
                toRoom = door.ToRoom;
                fromRoom = door.FromRoom;
            }
            catch
            {
                /* some edge cases can throw; ignore */
            }

            var touchesRoom =
                (toRoom != null && toRoom.Id == room.Id) ||
                (fromRoom != null && fromRoom.Id == room.Id);

            if (!touchesRoom)
                continue;

            var (hostWallId, accessPoint) = ComputeWallProjectionPoint(door);

            XYZ facingDir = null;
            try
            {
                var t = door.GetTransform();
                if (t != null) facingDir = t.BasisY; // Revit convention
            }
            catch
            {
            }
            
            var (width, height) = GetOpeningSize(door);

            result.Add(new AccessPoint
            {
                ElementId = door.Id,
                Type = AccessType.Door,
                HostWallId = hostWallId,
                Width = width,
                Height = height,
                Point = accessPoint ?? GuessElementPoint(doc, door),
                FacingDirection = facingDir,
                IsOnToSide = (toRoom != null && toRoom.Id == room.Id)
            });
        }

        // ---- WALL OPENINGS (Opening hosted by Wall) ----
        // Opening elements represent rectangular/linear holes in hosts like Wall/Floor/Roof/Ceiling.
        // We only consider those hosted by a Wall and that actually separate the given room from another space.
        var openings = new FilteredElementCollector(doc)
            .OfClass(typeof(Opening))
            .Cast<Opening>()
            .Where(op => op.Host is Wall);

        var roomsInPhase = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_Rooms)
            .WhereElementIsNotElementType()
            .OfType<Room>()
            .ToList();

        foreach (var op in openings)
        {
            var hostWall = op.Host as Wall;
            if (hostWall == null) continue;

            // A representative point: try projecting bbox center to the wall line.
            var center = GuessElementPoint(doc, op);
            var wallLine = (hostWall.Location as LocationCurve)?.Curve;
            if (center == null) continue; // extremely rare

            var projected = center;
            if (wallLine != null)
            {
                var proj = wallLine.Project(center);
                if (proj != null) projected = proj.XYZPoint;
            }

            // Determine rooms on each side by offsetting along wall's normal.
            // Wall.Orientation is a unit vector perpendicular to the wall's location line, pointing to the "exterior" side.
            var normal = hostWall.Orientation ?? XYZ.BasisX;
            const double offset = 0.15; // ~15 cm, adjust as needed

            var pPlus = projected + normal * offset;
            var pMinus = projected - normal * offset;

            var roomPlus = FindRoomContainingPoint(roomsInPhase, pPlus);
            var roomMinus = FindRoomContainingPoint(roomsInPhase, pMinus);

            // If neither side is the target room, skip.
            if ((roomPlus?.Id != room.Id) && (roomMinus?.Id != room.Id))
                continue;

            // Ensure it actually connects two spaces (room-to-room or room-to-non-room void).
            var connectsSomething =
                (roomPlus != null && roomMinus != null && roomPlus.Id != roomMinus.Id)
                || (roomPlus != null ^ roomMinus != null); // one side room, other side corridor/void

            if (!connectsSomething)
                continue;

            var isOnToSide = (roomPlus != null && roomPlus.Id == room.Id); // define "+" as "To"

            var (width, height) = GetOpeningSize(op);

            result.Add(new AccessPoint
            {
                ElementId = op.Id,
                Type = AccessType.WallOpening,
                HostWallId = hostWall.Id,
                Point = projected,
                Width = width,
                Height = height,
                FacingDirection = null,
                IsOnToSide = isOnToSide
            });
        }

        return result;
    }

    /// <summary>
    /// For a hosted element (door), returns (host wall id, projected point on wall line) when possible.
    /// </summary>
    public static (ElementId HostWallId, XYZ ProjectedPoint) ComputeWallProjectionPoint(FamilyInstance inst)
    {
        var locPt = (inst.Location as LocationPoint)?.Point;
        var hostWall = inst.Host as Wall;
        if (hostWall != null)
        {
            var lc = hostWall.Location as LocationCurve;
            var curve = lc?.Curve;
            if (curve != null && locPt != null)
            {
                var proj = curve.Project(locPt);
                if (proj != null)
                    return (hostWall.Id, proj.XYZPoint);
            }

            return (hostWall.Id, locPt);
        }

        return (ElementId.InvalidElementId, locPt);
    }

    /// <summary>
    /// Tries to get a reasonable representative point for any element (LocationPoint if available, otherwise bbox center).
    /// </summary>
    public static XYZ GuessElementPoint(Document doc, Element e)
    {
        var lp = (e.Location as LocationPoint)?.Point;
        if (lp != null) return lp;

        var bb = e.get_BoundingBox(null);
        if (bb != null) return (bb.Min + bb.Max) * 0.5;

        // Fallback: project geometry & compute centroid of first solid
        var opts = new Options
            { ComputeReferences = false, IncludeNonVisibleObjects = false, DetailLevel = ViewDetailLevel.Fine };
        var geo = e.get_Geometry(opts);
        if (geo != null)
        {
            foreach (var obj in geo)
            {
                if (obj is GeometryInstance gi)
                {
                    var g2 = gi.GetInstanceGeometry();
                    var c = FirstCentroid(g2);
                    if (c != null) return c;
                }

                var c1 = FirstCentroid(new List<GeometryObject> { obj });
                if (c1 != null) return c1;
            }
        }

        return XYZ.Zero;
    }

    public static XYZ FirstCentroid(IEnumerable<GeometryObject> gos)
    {
        foreach (var go in gos)
        {
            if (go is Solid s && s.Volume > 1e-9)
            {
                return s.ComputeCentroid();
            }

            if (go is Curve c)
            {
                return c.Evaluate(0.5, true);
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the room (from a pre-filtered list) that contains the given point using Room.IsPointInRoom.
    /// </summary>
    public static Room FindRoomContainingPoint(IEnumerable<Room> rooms, XYZ p)
    {
        if (p == null) return null;
        foreach (var r in rooms)
        {
            try
            {
                if (r.IsPointInRoom(p)) return r;
            }
            catch
            {
                // Some malformed rooms can throw; ignore and continue.
            }
        }

        return null;
    }


    /// <summary>
    /// Returns (width, height) in feet for a door, window, cased opening, or Opening element.
    /// For hosted families it prefers DOOR_/WINDOW_ built-ins, then FAMILY_* fallbacks.
    /// For Autodesk.Revit.DB.Opening (wall/floor opening), it measures from geometry.
    /// </summary>
    public static (double widthFt, double heightFt) GetOpeningSize(Element e)
    {
        if (e == null) throw new ArgumentNullException(nameof(e));

        // 1) Family instances (doors, windows, cased openings, generic)
        if (e is FamilyInstance fi)
        {
            // instance-level built-ins (best source for Doors/Windows):
            if (TryGetParam(fi, BuiltInParameter.DOOR_WIDTH, out var w) &
                TryGetParam(fi, BuiltInParameter.DOOR_HEIGHT, out var h))
                return (w, h);

            if (TryGetParam(fi, BuiltInParameter.WINDOW_WIDTH, out w) &
                TryGetParam(fi, BuiltInParameter.WINDOW_HEIGHT, out h))
                return (w, h);

            // generic family width/height (many cased openings use these)
            if (TryGetParam(fi, BuiltInParameter.FAMILY_WIDTH_PARAM, out w) &
                TryGetParam(fi, BuiltInParameter.FAMILY_HEIGHT_PARAM, out h))
                return (w, h);

            // some families keep size on the TYPE; try the symbol
            var sym = fi.Symbol;
            if (sym != null)
            {
                if (TryGetParam(sym, BuiltInParameter.DOOR_WIDTH, out w) &
                    TryGetParam(sym, BuiltInParameter.DOOR_HEIGHT, out h))
                    return (w, h);

                if (TryGetParam(sym, BuiltInParameter.WINDOW_WIDTH, out w) &
                    TryGetParam(sym, BuiltInParameter.WINDOW_HEIGHT, out h))
                    return (w, h);

                if (TryGetParam(sym, BuiltInParameter.FAMILY_WIDTH_PARAM, out w) &
                    TryGetParam(sym, BuiltInParameter.FAMILY_HEIGHT_PARAM, out h))
                    return (w, h);
            }

            // last resort for family instances: measure from bounding box projected to host frame
            return MeasureFromBoundingBox(fi);
        }

        // 2) System Opening (Autodesk.Revit.DB.Opening) — wall/floor/roof opening
        if (e is Opening sysOpening)
        {
            return MeasureFromBoundingBox(sysOpening);
        }

        // Unknown kind; fall back to bbox in world (width = max XY extent, height = Z)
        var bb = e.get_BoundingBox(null);
        if (bb == null) return (0, 0);
        var size = bb.Max - bb.Min;
        var height = size.Z;
        var width = Math.Max(Math.Abs(size.X), Math.Abs(size.Y));
        return (width, height);
    }

    // ---------- helpers ----------

    static bool TryGetParam(Element e, BuiltInParameter bip, out double val)
    {
        val = 0;
        var p = e.get_Parameter(bip);
        if (p == null || p.StorageType != StorageType.Double) return false;
        val = p.AsDouble();
        return val > 0;
    }

    static (double widthFt, double heightFt) MeasureFromBoundingBox(Element e)
    {
        var doc = e.Document;
        var bb = e.get_BoundingBox(null);
        if (bb == null) return (0, 0);

        // If hosted by a wall, compute width along wall direction, height vertical
        HostObject host = null;
        if (e is FamilyInstance fi) host = fi.Host as HostObject;
        if (e is Opening op) host = op.Host as HostObject;

        if (host is Wall wall)
        {
            var lc = (wall.Location as LocationCurve)?.Curve;
            if (lc != null)
            {
                var dir = (lc.GetEndPoint(1) - lc.GetEndPoint(0)).Normalize();
                var dx = bb.Max.X - bb.Min.X;
                var dy = bb.Max.Y - bb.Min.Y;
                var dz = bb.Max.Z - bb.Min.Z;

                // Build three world axes and project the bbox edges onto wall’s axis
                // Better: sample all 8 corners and get extent along 'dir' and Z
                var corners = new[]
                {
                    new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z), new XYZ(bb.Max.X, bb.Min.Y, bb.Min.Z),
                    new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z), new XYZ(bb.Max.X, bb.Max.Y, bb.Min.Z),
                    new XYZ(bb.Min.X, bb.Min.Y, bb.Max.Z), new XYZ(bb.Max.X, bb.Min.Y, bb.Max.Z),
                    new XYZ(bb.Min.X, bb.Max.Y, bb.Max.Z), new XYZ(bb.Max.X, bb.Max.Y, bb.Max.Z)
                };

                // project to wall axis and vertical
                double minAlong = double.PositiveInfinity, maxAlong = double.NegativeInfinity;
                double minZ = double.PositiveInfinity, maxZ = double.NegativeInfinity;
                foreach (var c in corners)
                {
                    var along = c.DotProduct(dir); // projection length along wall axis
                    if (along < minAlong) minAlong = along;
                    if (along > maxAlong) maxAlong = along;

                    if (c.Z < minZ) minZ = c.Z;
                    if (c.Z > maxZ) maxZ = c.Z;
                }

                var width = maxAlong - minAlong;
                var height = maxZ - minZ;
                return (Math.Abs(width), Math.Abs(height));
            }
        }

        // If not wall-hosted, just use bbox (XY max span as width, Z as height)
        var size = bb.Max - bb.Min;
        var height2 = size.Z;
        var width2 = Math.Max(Math.Abs(size.X), Math.Abs(size.Y));
        return (width2, height2);
    }

    /// <summary>
    /// Collects all parameters on an element (instance + type) into a dictionary.
    /// Keys are "Instance:ParamName" or "Type:ParamName".
    /// Values are stringified parameter values.
    /// </summary>
    public static Dictionary<string, string> GetAllParametersAsDictionary(Element e)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (e == null) return result;

        // Instance parameters
        foreach (Parameter p in e.Parameters)
        {
            var key = SafeName(p);
            result[key] = ParamToString(p);
        }

        // Type parameters (FamilySymbol / ElementType)
        var type = e.Document.GetElement(e.GetTypeId()) as ElementType;
        if (type != null)
        {
            foreach (Parameter p in type.Parameters)
            {
                var key = "Type:" + SafeName(p);
                result[key] = ParamToString(p);
            }
        }

        return result;
    }

    // Helpers

    private static string SafeName(Parameter p)
    {
        try
        {
            if (!string.IsNullOrEmpty(p.Definition?.Name))
                return p.Definition.Name;
        }
        catch
        {
        }

        return $"P_{p.Id.Value}";
    }

    private static string ParamToString(Parameter p)
    {
        if (p == null) return "";
        try
        {
            switch (p.StorageType)
            {
                case StorageType.Double:
                    // format with display units if available
                    return p.AsValueString() ?? p.AsDouble().ToString();

                case StorageType.Integer:
                    // handle Yes/No params
                    return p.AsInteger().ToString();

                case StorageType.String:
                    return p.AsString() ?? "";

                case StorageType.ElementId:
                    var id = p.AsElementId();
                    if (id == ElementId.InvalidElementId) return "";
                    var el = p.Element.Document.GetElement(id);
                    return el != null ? el.Name : id.Value.ToString();

                default:
                    return "";
            }
        }
        catch
        {
            return "";
        }
    }

    public static RoomData GetRoomData(this Room r)
    {
        var walls = r.GetBoundaryWalls().ToList();
        var numWalls = walls.Count;
        var rd = new RoomData()
        {
            Name = r.Name ?? "",
            Id = r.Id.Value,
            Walls = numWalls,
            Elevation = r.Level?.Elevation ?? 0,
            Area = r.Area,
            Volume = r.Volume,
            Perimeter = r.Perimeter,
            LevelName = r.Level?.Name ?? "",
            UnboundedHeight = r.UnboundedHeight,
            UpperLevelElevation = r.UpperLimit?.Elevation ?? 0,
            BaseOffset = r.BaseOffset,
            LimitOffset = r.LimitOffset,
        };
        try
        {
            var bb = r.get_BoundingBox(null);
            if (bb != null)
            {
                var extent = bb.Max - bb.Min;
                extent = new Autodesk.Revit.DB.XYZ(Math.Abs(extent.X), Math.Abs(extent.Y), Math.Abs(extent.Z));
                rd.BoundingArea = extent.X * extent.Y;
                rd.BoundingVolume = extent.X * extent.Y * extent.Z;
                rd.BoundingHeight = extent.Z;
                rd.AreaToBoundingArea = rd.Area / rd.BoundingArea;
                rd.MinSide = Math.Min(extent.X, extent.Y);
                rd.MaxSide = Math.Max(extent.X, extent.Y);
                rd.Ratio = rd.MinSide / rd.MaxSide;
            }
        }
        catch
        {
            // DO Nothing.
        }

        foreach (var wall in walls)
        {
            foreach (var hosted in wall.GetHostedElements())
            {
                if (hosted.IsCategoryType(BuiltInCategory.OST_Doors))
                    rd.Doors++;
            }
        }

        return rd;
    }
}

