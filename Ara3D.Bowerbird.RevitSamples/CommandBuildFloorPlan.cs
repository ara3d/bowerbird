using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandBuildFloorPlan : NamedCommand
    {
        public override string Name => "Build Floor Plan";


        public override void Execute(object arg)
        {
            var uiapp = arg as UIApplication;
            var ui = uiapp.ActiveUIDocument;

            // Example: quick plan spec (meters) — replace with deserialized JSON/YAML/XML or UI-collected data
            var simpleSpec = new PlanSpec
            {
                Units = SourceUnits.Meters,
                LevelName = "Level 1",
                LevelElevation = 0,
                Shell = new OutlineSpec { Origin = new XYZ(0, 0, 0), Width = 40, Depth = 25, WallHeight = 3.6, ExteriorWallType = null },
                Partitions = new List<PartitionSpec>
                {
                    new PartitionSpec { P0 = new XYZ(-10, 0, 0), P1 = new XYZ(10, 0, 0), Height = 3.0 },
                    new PartitionSpec { P0 = new XYZ(0, -6, 0), P1 = new XYZ(0, 6, 0), Height = 3.0 }
                },
                Corridor = new CorridorSpec
                {
                    Path = new List<XYZ> { new XYZ(-18, 0, 0), new XYZ(18, 0, 0) },
                    Width = 2.4,
                    UseWalls = false
                },
                Rooms = new List<RoomSpec>
                {
                    new RoomSpec { Number = "101", Name = "Exam", Department = "Outpatient", SeedPoint = new XYZ(-8, 4, 0) },
                    new RoomSpec { Number = "102", Name = "Exam", Department = "Outpatient", SeedPoint = new XYZ( 8, 4, 0) },
                    new RoomSpec { Number = "103", Name = "Consult", Department = "Outpatient", SeedPoint = new XYZ(-8, -4, 0) },
                    new RoomSpec { Number = "104", Name = "Consult", Department = "Outpatient", SeedPoint = new XYZ( 8, -4, 0) }
                }
            };

            var spec = new PlanSpec
            {
                Units = SourceUnits.Meters,
                LevelName = "Level 1",
                LevelElevation = 0,
                Shell = new OutlineSpec { Origin = new XYZ(0, 0, 0), Width = 60, Depth = 36, WallHeight = 3.6 },

                // Long partitions — let CreatePartitionWallExtended extend them to the shell
                Partitions = new List<PartitionSpec>
                {
                    // north suite dividers
                    new PartitionSpec { P0 = new XYZ(-18,  8, 0), P1 = new XYZ( 18,  8, 0), Height = 3.2 },
                    new PartitionSpec { P0 = new XYZ(-18, 14, 0), P1 = new XYZ( 18, 14, 0), Height = 3.2 },
                    // south suite divider
                    new PartitionSpec { P0 = new XYZ(-24, -8, 0), P1 = new XYZ( 24, -8, 0), Height = 3.2 },
                    // vertical banks
                    new PartitionSpec { P0 = new XYZ(-12, -18, 0), P1 = new XYZ(-12, 18, 0), Height = 3.2 },
                    new PartitionSpec { P0 = new XYZ(   0, -18, 0), P1 = new XYZ(   0, 18, 0), Height = 3.2 },
                    new PartitionSpec { P0 = new XYZ(  12, -18, 0), P1 = new XYZ(  12, 18, 0), Height = 3.2 },
                },

                Corridor = new CorridorSpec
                {
                    // L-shaped corridor
                    Path = new List<XYZ>
                    {
                        new XYZ(-24, -10, 0),
                        new XYZ(  0, -10, 0),
                        new XYZ(  0,  12, 0),
                        new XYZ( 20,  12, 0)
                    },
                    Width = 2.6,
                    UseWalls = false
                },

                Rooms = new List<RoomSpec>
                {
                    // north wing, west to east
                    new RoomSpec { Number="101", Name="Exam",     Department="Outpatient", SeedPoint=new XYZ(-17, 15, 0) },
                    new RoomSpec { Number="102", Name="Exam",     Department="Outpatient", SeedPoint=new XYZ(-11, 15, 0) },
                    new RoomSpec { Number="103", Name="Consult",  Department="Outpatient", SeedPoint=new XYZ( -5, 15, 0) },
                    new RoomSpec { Number="104", Name="Consult",  Department="Outpatient", SeedPoint=new XYZ(  5, 15, 0) },
                    new RoomSpec { Number="105", Name="Storage",  Department="Ops",        SeedPoint=new XYZ( 11, 15, 0) },
                    new RoomSpec { Number="106", Name="Clean",    Department="Ops",        SeedPoint=new XYZ( 17, 15, 0) },

                    // central band
                    new RoomSpec { Number="107", Name="Waiting",  Department="Front Desk", SeedPoint=new XYZ(-10, -2, 0) },
                    new RoomSpec { Number="108", Name="Triage",   Department="Front Desk", SeedPoint=new XYZ(  0,  -2, 0) },
                    new RoomSpec { Number="109", Name="Admin",    Department="Admin",      SeedPoint=new XYZ( 10,  -2, 0) },

                    // south wing
                    new RoomSpec { Number="110", Name="Staff",    Department="Ops",        SeedPoint=new XYZ(-17, -12, 0) },
                    new RoomSpec { Number="111", Name="Toilet",   Department="Ops",        SeedPoint=new XYZ(-11, -12, 0) },
                    new RoomSpec { Number="112", Name="Storage",  Department="Ops",        SeedPoint=new XYZ( -5, -12, 0) },
                    new RoomSpec { Number="113", Name="Utility",  Department="Ops",        SeedPoint=new XYZ(  5, -12, 0) },
                    new RoomSpec { Number="114", Name="Office",   Department="Admin",      SeedPoint=new XYZ( 11, -12, 0) },
                    new RoomSpec { Number="115", Name="Office",   Department="Admin",      SeedPoint=new XYZ( 17, -12, 0) },
                }
            };

            try
            {
                HospitalPlanBuilder.BuildFromSpec(ui, spec);
            }
            catch (Exception ex)
            {
                // message = ex.ToString();
            }
        }
    }

    /// <summary>
    /// Source units used by incoming specs (JSON/YAML/XML/UI). All values converted to feet.
    /// </summary>
    public enum SourceUnits { Feet, Inches, Millimeters, Meters }

    public sealed class UnitContext
    {
        public SourceUnits Units { get; }
        public UnitContext(SourceUnits u) => Units = u;
        public double ToFeet(double v)
        {
            return Units switch
            {
                SourceUnits.Feet => v,
                SourceUnits.Inches => v / 12.0,
                SourceUnits.Millimeters => v / 304.8,
                SourceUnits.Meters => v * 3.280839895,
                _ => v
            };
        }
        public XYZ ToFeet(XYZ p) => new XYZ(ToFeet(p.X), ToFeet(p.Y), ToFeet(p.Z));
    }

    /// <summary>
    /// Minimal plan specification that can be deserialized from JSON/YAML/XML or built from a UI.
    /// Keep this intentionally small; you can extend without breaking the builder API.
    /// </summary>
    public class PlanSpec
    {
        public SourceUnits Units { get; set; } = SourceUnits.Meters;
        public string LevelName { get; set; } = "Level 1";
        public double LevelElevation { get; set; } = 0.0; // in Units
        public OutlineSpec Shell { get; set; } = new OutlineSpec();
        public List<PartitionSpec> Partitions { get; set; } = new();
        public List<RoomSpec> Rooms { get; set; } = new();
        public List<DoorSpec> Doors { get; set; } = new();
        public List<WindowSpec> Windows { get; set; } = new();
        public CorridorSpec Corridor { get; set; } = null; // optional
    }

    public class OutlineSpec
    {
        public XYZ Origin { get; set; } = new XYZ(0, 0, 0); // plan origin (Units)
        public double Width { get; set; } = 40; // Units
        public double Depth { get; set; } = 25; // Units
        public double WallHeight { get; set; } = 3.5; // Units (e.g., meters)
        public string ExteriorWallType { get; set; } = null; // name (optional)
    }

    public class PartitionSpec
    {
        public XYZ P0 { get; set; } // Units
        public XYZ P1 { get; set; } // Units
        public string WallType { get; set; } = null; // name (optional)
        public double Height { get; set; } = 3.0; // Units
    }

    public class RoomSpec
    {
        public string Number { get; set; } = null; // e.g., "101"
        public string Name { get; set; } = null;   // e.g., "Exam"
        public string Department { get; set; } = null; // e.g., "Radiology"
        public XYZ SeedPoint { get; set; } // Units; point inside the room boundary
    }

    public class DoorSpec
    {
        public string SymbolName { get; set; } = null; // FamilySymbol name; e.g., "Single-Flush 36" or full path
        public ElementId WallId { get; set; } = ElementId.InvalidElementId; // optional direct wall id
        public Guid WallUniqueId { get; set; } = Guid.Empty; // or unique id to resolve later
        public string WallByName { get; set; } = null; // or store a custom key you attach to walls you create
        public XYZ Location { get; set; } // Units; insertion point on wall's location curve
        public double OffsetFromWallStart { get; set; } = double.NaN; // alternative to Location
        public double SillHeight { get; set; } = 0; // Units
        public bool FlipHand { get; set; } = false;
        public bool FlipFacing { get; set; } = false;
    }

    public class WindowSpec
    {
        public string SymbolName { get; set; } = null; // e.g., "Fixed 36" or full path
        public ElementId WallId { get; set; } = ElementId.InvalidElementId;
        public XYZ Location { get; set; } // Units; on wall location curve
        public double OffsetFromWallStart { get; set; } = double.NaN;
        public double SillHeight { get; set; } = 1.0; // Units
        public bool CenterInWallThickness { get; set; } = true;
    }

    public class CorridorSpec
    {
        /// <summary>Centerline polyline in plan (Units).</summary>
        public List<XYZ> Path { get; set; } = new();
        /// <summary>Clear width (Units).</summary>
        public double Width { get; set; } = 2.0;
        /// <summary>If true, build the corridor boundaries with walls; otherwise use room separation lines.</summary>
        public bool UseWalls { get; set; } = false;
        public string WallType { get; set; } = null; // if UseWalls
    }


    public static class HospitalPlanBuilder
    {
        /// <summary>
        /// Build or update a hospital floor using a PlanSpec. Designed for idempotent, incremental runs:
        /// - Ensures level exists.
        /// - Creates a rectangular shell of exterior walls.
        /// - Adds partition walls.
        /// - Carves a corridor (walls or separation lines).
        /// - Creates rooms at seed points and sets parameters.
        /// - Places doors and windows.
        /// Extend as needed.
        /// </summary>
        public static void BuildFromSpec(UIDocument uidoc, PlanSpec spec)
        {
            var doc = uidoc.Document;
            var units = new UnitContext(spec.Units);

            using var tg = new TransactionGroup(doc, "Build Hospital Floor");
            tg.Start();

            var level = EnsureLevel(doc, spec.LevelName, units.ToFeet(spec.LevelElevation));
            var planView = EnsurePlanView(doc, level, out _);

            // Shell
            var shell = spec.Shell ?? new OutlineSpec();
            var shellWalls = CreateRectangularShellWalls(
                doc,
                level,
                origin: units.ToFeet(shell.Origin),
                widthFeet: units.ToFeet(shell.Width),
                depthFeet: units.ToFeet(shell.Depth),
                wallHeightFeet: units.ToFeet(shell.WallHeight),
                wallTypeName: shell.ExteriorWallType
            );

            // Partitions
            foreach (var p in spec.Partitions)
            {
                CreatePartitionWall(
                    doc,
                    level,
                    units.ToFeet(p.P0),
                    units.ToFeet(p.P1),
                    heightFeet: units.ToFeet(p.Height),
                    wallTypeName: p.WallType
                );
            }

            // Corridor
            if (spec.Corridor != null && spec.Corridor.Path.Count >= 2)
            {
                var path = spec.Corridor.Path.Select(units.ToFeet).ToList();
                if (spec.Corridor.UseWalls)
                {
                    CreateCorridorWalls(doc, level, path, units.ToFeet(spec.Corridor.Width), spec.Corridor.WallType, heightFeet: units.ToFeet(shell.WallHeight));
                }
                else
                {
                    CreateCorridorSeparationLines(doc, planView, path, units.ToFeet(spec.Corridor.Width));
                }
            }

            // Rooms
            foreach (var r in spec.Rooms)
            {
                var room = CreateRoomAtPoint(doc, level, new UV(units.ToFeet(r.SeedPoint.X), units.ToFeet(r.SeedPoint.Y)));
                if (room == null)
                    continue;
                using (var t = new Transaction(doc, "Set Room Params"))
                {
                    t.Start();
                    SetStringParam(room, BuiltInParameter.ROOM_NUMBER, r.Number);
                    SetStringParam(room, BuiltInParameter.ROOM_NAME, r.Name);
                    if (!string.IsNullOrWhiteSpace(r.Department))
                    {
                        var p = room.get_Parameter(BuiltInParameter.ROOM_DEPARTMENT);
                        if (p is { IsReadOnly: false }) p.Set(r.Department);
                    }
                    t.Commit();
                }
            }

            // Doors
            foreach (var d in spec.Doors)
            {
                PlaceDoorOnWall(
                    doc,
                    level,
                    symbolName: d.SymbolName,
                    wallId: ResolveWallId(doc, d),
                    locationFeet: units.ToFeet(d.Location),
                    offsetFromWallStartFeet: double.IsNaN(d.OffsetFromWallStart) ? (double?)null : units.ToFeet(d.OffsetFromWallStart),
                    sillHeightFeet: units.ToFeet(d.SillHeight),
                    flipHand: d.FlipHand,
                    flipFacing: d.FlipFacing
                );
            }

            // Windows
            foreach (var w in spec.Windows)
            {
                PlaceWindowOnWall(
                    doc,
                    level,
                    symbolName: w.SymbolName,
                    wallId: w.WallId,
                    locationFeet: units.ToFeet(w.Location),
                    offsetFromWallStartFeet: double.IsNaN(w.OffsetFromWallStart) ? (double?)null : units.ToFeet(w.OffsetFromWallStart),
                    sillHeightFeet: units.ToFeet(w.SillHeight),
                    centerInWallThickness: w.CenterInWallThickness
                );
            }

            tg.Assimilate();
        }

        public static Level EnsureLevel(Document doc, string name, double elevationFeet)
        {
            var level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault(l => l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (level != null) return level;

            using var t = new Transaction(doc, $"Create Level {name}");
            t.Start();
            level = Level.Create(doc, elevationFeet);
            level.Name = name;
            t.Commit();
            return level;
        }

        public static ViewPlan EnsurePlanView(Document doc, Level level, out bool created)
        {
            created = false;
            // Try to find existing plan on this level
            var plan = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewPlan))
                .Cast<ViewPlan>()
                .FirstOrDefault(v => v.GenLevel?.Id == level.Id && v.ViewType == ViewType.FloorPlan);
            if (plan != null) return plan;

            // Create one
            var vft = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(x => x.ViewFamily == ViewFamily.FloorPlan);

            if (vft == null)
                throw new InvalidOperationException("No ViewFamilyType for FloorPlan found.");

            using var t = new Transaction(doc, $"Create Plan for {level.Name}");
            t.Start();
            plan = ViewPlan.Create(doc, vft.Id, level.Id);
            created = true;
            t.Commit();
            return plan;
        }

        public static SketchPlane EnsureSketchPlaneOnLevel(Document doc, Level level)
        {
            var origin = new XYZ(0, 0, level.Elevation);
            var plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, origin);
            using var t = new Transaction(doc, "Create SketchPlane");
            t.Start();
            var sp = SketchPlane.Create(doc, plane);
            t.Commit();
            return sp;
        }

        public static WallType FindWallType(Document doc, string typeName)
            => string.IsNullOrWhiteSpace(typeName)
                ? new FilteredElementCollector(doc).OfClass(typeof(WallType)).Cast<WallType>().FirstOrDefault(wt => wt.Kind == WallKind.Basic)
                : new FilteredElementCollector(doc).OfClass(typeof(WallType)).Cast<WallType>().FirstOrDefault(wt => wt.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));

        public static FamilySymbol FindFamilySymbol(Document doc, BuiltInCategory bic, string symbolName)
        {
            var col = new FilteredElementCollector(doc)
                .OfCategory(bic)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>();

            if (string.IsNullOrWhiteSpace(symbolName))
                return col.FirstOrDefault();

            var s = col.FirstOrDefault(f => f.Name.Equals(symbolName, StringComparison.OrdinalIgnoreCase) || ($"{f.Family?.Name} : {f.Name}").Equals(symbolName, StringComparison.OrdinalIgnoreCase));
            return s ?? col.FirstOrDefault();
        }

        public static void EnsureActivated(this FamilySymbol symbol, Document doc)
        {
            if (symbol == null) throw new InvalidOperationException("FamilySymbol not found.");
            if (symbol.IsActive) return;
            using var t = new Transaction(doc, $"Activate {symbol.Name}");
            t.Start();
            symbol.Activate();
            t.Commit();
        }

        /// <summary>Create four exterior walls around a rectangle, counter-clockwise from bottom edge.</summary>
        public static IList<Wall> CreateRectangularShellWalls(this Document doc, Level level, XYZ origin, double widthFeet, double depthFeet, double wallHeightFeet, string wallTypeName)
        {
            var wt = FindWallType(doc, wallTypeName) ?? throw new InvalidOperationException("No WallType found.");

            var p0 = new XYZ(origin.X - widthFeet / 2, origin.Y - depthFeet / 2, level.Elevation);
            var p1 = new XYZ(origin.X + widthFeet / 2, origin.Y - depthFeet / 2, level.Elevation);
            var p2 = new XYZ(origin.X + widthFeet / 2, origin.Y + depthFeet / 2, level.Elevation);
            var p3 = new XYZ(origin.X - widthFeet / 2, origin.Y + depthFeet / 2, level.Elevation);

            var curves = new List<Curve>
            {
                Line.CreateBound(p0, p1),
                Line.CreateBound(p1, p2),
                Line.CreateBound(p2, p3),
                Line.CreateBound(p3, p0)
            };

            var result = new List<Wall>(4);
            using var t = new Transaction(doc, "Create Shell Walls");
            t.Start();
            foreach (var c in curves)
            {
                var w = Wall.Create(doc, c, wt.Id, level.Id, wallHeightFeet, 0, false, false);
                result.Add(w);
            }
            t.Commit();

            // TODO: Try joining adjacent corners for cleaner geometry.
            //TryJoinConsecutiveWalls(doc, result);

            return result;
        }

        public static Wall CreatePartitionWall(Document doc, Level level, XYZ p0, XYZ p1, double heightFeet, string wallTypeName)
        {
            var wt = FindWallType(doc, wallTypeName) ?? FindWallType(doc, null);
            using var t = new Transaction(doc, "Create Partition Wall");
            t.Start();
            var w = Wall.Create(doc, Line.CreateBound(p0, p1), wt.Id, level.Id, heightFeet, 0, false, false);
            t.Commit();
            return w;
        }

        public static void CreateCorridorWalls(Document doc, Level level, IList<XYZ> centerline, double widthFeet, string wallTypeName, double heightFeet)
        {
            if (centerline.Count < 2) return;
            var wt = FindWallType(doc, wallTypeName) ?? FindWallType(doc, null);
            var half = widthFeet / 2.0;

            // For each segment, build two offset walls.
            var allWalls = new List<Wall>();
            using var t = new Transaction(doc, "Create Corridor Walls");
            t.Start();
            for (int i = 0; i < centerline.Count - 1; i++)
            {
                var a = centerline[i];
                var b = centerline[i + 1];
                var dir = (b - a).Normalize();
                var n = new XYZ(-dir.Y, dir.X, 0); // left-normal in XY
                var pL0 = a + n * half; var pL1 = b + n * half;
                var pR0 = a - n * half; var pR1 = b - n * half;

                var wL = Wall.Create(doc, Line.CreateBound(pL0, pL1), wt.Id, level.Id, heightFeet, 0, false, false);
                var wR = Wall.Create(doc, Line.CreateBound(pR0, pR1), wt.Id, level.Id, heightFeet, 0, false, false);
                allWalls.Add(wL); allWalls.Add(wR);
            }
            t.Commit();
            // TODO:
            //TryJoinConsecutiveWalls(doc, allWalls);
        }

        public static void TryJoinConsecutiveWalls(Document doc, IList<Wall> walls)
        {
            using var t = new Transaction(doc, "Join Walls");
            t.Start();
            for (int i = 0; i < walls.Count; i++)
            {
                for (int j = i + 1; j < walls.Count; j++)
                {
                    try { JoinGeometryUtils.JoinGeometry(doc, walls[i], walls[j]); }
                    catch { /* ignore */ }
                }
            }
            t.Commit();
        }

        public static CurveArray ToCurveArray(IEnumerable<Curve> curves)
        {
            var ca = new CurveArray();
            foreach (var c in curves) ca.Append(c);
            return ca;
        }

        /// <summary>
        /// Creates separation lines offset from the centerline path by width/2 to carve a corridor.
        /// Requires a plan view. Lines will live on that view.
        /// </summary>
        public static void CreateCorridorSeparationLines(Document doc, ViewPlan plan, IList<XYZ> centerline, double widthFeet)
        {
            if (centerline.Count < 2) return;
            var half = widthFeet / 2.0;
            var level = plan.GenLevel;
            var sp = EnsureSketchPlaneOnLevel(doc, level);

            var left = new List<Curve>();
            var right = new List<Curve>();

            // Build offsets segment-by-segment (simple approach; for right-angle layouts this is fine).
            for (int i = 0; i < centerline.Count - 1; i++)
            {
                var a = centerline[i];
                var b = centerline[i + 1];
                var dir = (b - a).Normalize();
                var n = new XYZ(-dir.Y, dir.X, 0);
                left.Add(Line.CreateBound(a + n * half, b + n * half));
                right.Add(Line.CreateBound(a - n * half, b - n * half));
            }

            using var t = new Transaction(doc, "Create Corridor Separation Lines");
            t.Start();
            // NewRoomBoundaryLines creates CurveElements in OST_RoomSeparationLines

            var _ = doc.Create.NewRoomBoundaryLines(sp, ToCurveArray(left), plan);
            var __ = doc.Create.NewRoomBoundaryLines(sp, ToCurveArray(right), plan);
            t.Commit();
        }

        /// <summary>
        /// Creates a room at the given UV on the provided level. The point must lie within closed boundaries
        /// (walls or separation lines). Returns null if creation fails.
        /// </summary>
        public static Room CreateRoomAtPoint(Document doc, Level level, UV uv)
        {
            using var t = new Transaction(doc, "Create Room");
            t.Start();
            Room r = null;
            try { r = doc.Create.NewRoom(level, uv); } catch { /* fails if not enclosed */ }
            if (r != null) t.Commit(); else t.RollBack();
            return r;
        }

        public static FamilyInstance PlaceDoorOnWall(Document doc, Level level, string symbolName, ElementId wallId, XYZ locationFeet, double? offsetFromWallStartFeet, double sillHeightFeet, bool flipHand, bool flipFacing)
        {
            if (wallId == ElementId.InvalidElementId) throw new ArgumentException("WallId required for placing a door.");
            var wall = doc.GetElement(wallId) as Wall;
            if (wall == null) throw new InvalidOperationException("Wall not found.");

            var symbol = FindFamilySymbol(doc, BuiltInCategory.OST_Doors, symbolName);
            symbol.EnsureActivated(doc);

            var insertPoint = ResolvePointOnWall(wall, locationFeet, offsetFromWallStartFeet);

            using var t = new Transaction(doc, "Place Door");
            t.Start();
            var inst = doc.Create.NewFamilyInstance(insertPoint, symbol, wall, level, StructuralType.NonStructural);

            // Sill height parameter (rough opening height positioning may vary per family)
            var sill = inst.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM) ?? inst.LookupParameter("Sill Height");
            if (sill != null && !sill.IsReadOnly) sill.Set(sillHeightFeet);

            if (flipHand) inst.flipHand();
            if (flipFacing) inst.flipFacing();
            t.Commit();
            return inst;
        }

        public static FamilyInstance PlaceWindowOnWall(Document doc, Level level, string symbolName, ElementId wallId, XYZ locationFeet, double? offsetFromWallStartFeet, double sillHeightFeet, bool centerInWallThickness)
        {
            if (wallId == ElementId.InvalidElementId) throw new ArgumentException("WallId required for placing a window.");
            var wall = doc.GetElement(wallId) as Wall;
            if (wall == null) throw new InvalidOperationException("Wall not found.");

            var symbol = FindFamilySymbol(doc, BuiltInCategory.OST_Windows, symbolName);
            symbol.EnsureActivated(doc);

            var insertPoint = ResolvePointOnWall(wall, locationFeet, offsetFromWallStartFeet);

            using var t = new Transaction(doc, "Place Window");
            t.Start();
            var inst = doc.Create.NewFamilyInstance(insertPoint, symbol, wall, level, StructuralType.NonStructural);

            // Sill height
            var sill = inst.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM) ?? inst.LookupParameter("Sill Height");
            if (sill != null && !sill.IsReadOnly) sill.Set(sillHeightFeet);

            // Center in wall thickness (flip around wall thickness using the host face orientation parameter if needed)
            if (centerInWallThickness)
            {
                try
                {
                    var loc = inst.Location as LocationPoint;
                    var p = loc?.Point ?? insertPoint;
                    var wallThickness = (doc.GetElement(wall.GetTypeId()) as WallType)?.GetCompoundStructure()?.GetLayers().Sum(l => l.Width) ?? wall.Width;
                    if (wallThickness > 0)
                    {
                        // move instance along its local X by half the thickness towards wall center
                        var n = wall.Orientation; // unit vector along wall's local X (perpendicular to curve normal)
                        var move = -n * (wallThickness / 2.0);
                        ElementTransformUtils.MoveElement(doc, inst.Id, move);
                    }
                }
                catch { /* optional */ }
            }

            t.Commit();
            return inst;
        }

        /// <summary>Resolve a point on the wall's location curve, using either absolute XYZ or an offset from start.</summary>
        private static XYZ ResolvePointOnWall(Wall wall, XYZ locationFeet, double? offsetFromWallStartFeet)
        {
            var lc = (wall.Location as LocationCurve)?.Curve;
            if (lc == null) throw new InvalidOperationException("Wall has no LocationCurve.");

            if (offsetFromWallStartFeet.HasValue)
            {
                var s = lc.GetEndPoint(0);
                var e = lc.GetEndPoint(1);
                var dir = (e - s).Normalize();
                return s + dir * offsetFromWallStartFeet.Value;
            }

            // Project the given point onto the wall curve
            var proj = lc.Project(locationFeet);
            return proj != null ? proj.XYZPoint : lc.Evaluate(0.5, true);
        }

        public static void SetStringParam(Element e, BuiltInParameter bip, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var p = e.get_Parameter(bip);
            if (p != null && !p.IsReadOnly) p.Set(value);
        }

        /// <summary>Try to resolve a wall id from DoorSpec: explicit id, unique id, or a named key you attach to walls.</summary>
        public static ElementId ResolveWallId(Document doc, DoorSpec spec)
        {
            if (spec.WallId != ElementId.InvalidElementId) return spec.WallId;
            if (spec.WallUniqueId != Guid.Empty)
            {
                var el = doc.GetElement(spec.WallUniqueId.ToString());
                if (el is Wall w) return w.Id;
            }
            if (!string.IsNullOrWhiteSpace(spec.WallByName))
            {
                var w = new FilteredElementCollector(doc).OfClass(typeof(Wall)).Cast<Wall>()
                    .FirstOrDefault(x => x.Name.Equals(spec.WallByName, StringComparison.OrdinalIgnoreCase));
                if (w != null) return w.Id;
            }
            return ElementId.InvalidElementId;
        }
    }
}
