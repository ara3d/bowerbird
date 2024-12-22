using System.Linq;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Utils;
using Autodesk.Revit.UI;
using Plato.DoublePrecision;
using Plato.Geometry.Revit;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class BuildingLayoutExporter : IBowerbirdCommand
    {
        public string Name => "Building Layout Exporter";

        public void Execute(object arg)
        {
            var uiapp = (arg as UIApplication);
            if (uiapp == null)
                return;
            var doc  = uiapp.ActiveUIDocument.Document;
            var rooms = doc.GetRooms().ToList();
            var levels = doc.GetLevels().ToList();
            var doors = doc.GetDoors().ToList();

            var bldg = new BuildingLayout();
            foreach (var room in rooms)
            {
                bldg.Rooms.Add(room.Id.IntegerValue, new RoomStruct
                {
                    Id = room.Id.IntegerValue,
                    Level = room.LevelId.IntegerValue,
                    Name = room.Name,
                    Bounds = room.get_BoundingBox(null)?.ToPlato() ?? Bounds3D.Default,
                });
            }

            var phase = doc.GetLastPhase();

            foreach (var door in doors)
            {
                bldg.Doors.Add(door.Id.IntegerValue, new DoorStruct()
                {
                    Id = door.Id.IntegerValue,
                    Level = door.LevelId.IntegerValue,
                    Name = door.Name,
                    FromRoom = door.get_FromRoom(phase)?.Id.IntegerValue ?? -1,
                    ToRoom = door.get_ToRoom(phase)?.Id.IntegerValue ?? -1, 
                    Bounds = door.get_BoundingBox(null)?.ToPlato() ?? Bounds3D.Default,
                });
            }

            foreach (var level in levels)
            {
                bldg.Levels.Add(level.Id.IntegerValue, new LevelStruct()
                {
                    Id = level.Id.IntegerValue,
                    Elevation = level.Elevation,
                    Name = level.Name
                });
            }

            var json = bldg.ToJsonFieldsOnly();
            LayoutFile.WriteAllText(json);
        }

        public static FilePath LayoutFile
            => new FilePath(@"C:\Users\cdigg\AppData\Local\Temp\layout.json");
    }
}