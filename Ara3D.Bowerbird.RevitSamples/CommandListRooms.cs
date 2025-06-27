using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandListRooms : NamedCommand
    {
        public override string Name => "List Rooms";

        public List<Room> Rooms;
        public Dictionary<int, List<FamilyInstance>> Lights;
        public Dictionary<int, List<FamilyInstance>> Doors;
        public Dictionary<int, List<FamilyInstance>> Sockets;
        public Dictionary<int, List<FamilyInstance>> Windows;

        public override void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            if (doc == null) return;
            Rooms = doc.GetRooms().ToList();
            Lights = doc.GetLights().GroupByRoom();
            Doors = doc.GetDoors().GroupByRoom();
            Sockets = doc.GetSockets().GroupByRoom();
            Windows = doc.GetWindows().GroupByRoom();

            var builder = new DataTableBuilder(typeof(RoomData));
            builder.AddRows(GetAllRoomData());
            var form = new DataTableForm(builder);
            form.Show();

            //var text = string.Join("\r\n", GetAllRoomData());
            //TextDisplayForm.DisplayText(text);
        }

        public IEnumerable<RoomData> GetAllRoomData()
            => Rooms.Select(GetRoomData);

        public static int GetCount(Dictionary<int, List<FamilyInstance>> dict, Room r)
        {
            return dict.TryGetValue(r.Id.IntegerValue, out var list) ? list.Count : 0;
        }

        public class RoomData
        {
            public string Name;
            public int Id;
            public int Lights;
            public int Doors;
            public int Sockets;
            public int Windows;
            public int Walls;
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

        public RoomData GetRoomData(Room r)
        {
            var walls = r.GetBoundaryWalls().ToList();
            var numWalls = walls.Count;
            var rd = new RoomData()
            {
                Name = r.Name ?? "",
                Id = r.Id.IntegerValue,
                Lights = GetCount(Lights, r),
                Doors = GetCount(Doors, r),
                Sockets = GetCount(Sockets, r),
                Windows = GetCount(Windows, r),
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
                    extent = new XYZ(Math.Abs(extent.X), Math.Abs(extent.Y), Math.Abs(extent.Z));
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
                    if (hosted.IsCategoryType(BuiltInCategory.OST_LightingFixtures))
                        rd.Lights++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_Doors))
                        rd.Doors++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_ElectricalFixtures))
                        rd.Sockets++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_Windows))
                        rd.Windows++;
                }
            }

            return rd;
        }
    }
}