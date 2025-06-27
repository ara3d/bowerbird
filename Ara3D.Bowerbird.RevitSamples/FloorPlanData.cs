using System.Collections.Generic;
using Ara3D.Geometry;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class LevelStruct
    {
        public int Id;
        public string Name;
        public override string ToString() => $"{Name} - {Id}";
    }

    public class Polygon
    {
       public List<Vector3> Points = new List<Vector3>();
    }

    public class RoomStruct
    {
        public int Id;
        public string Name;
        public int Level;
        public Bounds3D Bounds;
        public List<Polygon> Polygons = new List<Polygon>();
        public override string ToString() => $"{Name} - {Id}";
    }

    public class DoorStruct
    {
        public int Id;
        public string Name;
        public int Level;
        public int FromRoom;
        public int ToRoom;
        public Bounds3D Bounds;
        public override string ToString() => $"{Name} - {Id}";
    }

    public class FloorPlanStruct
    {
        public Dictionary<int, RoomStruct> Rooms = new Dictionary<int, RoomStruct>();
        public Dictionary<int, DoorStruct> Doors = new Dictionary<int, DoorStruct>();
        public Dictionary<int, LevelStruct> Levels = new Dictionary<int, LevelStruct>();
    }
}
