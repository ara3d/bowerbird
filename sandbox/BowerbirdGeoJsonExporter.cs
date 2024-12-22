using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Newtonsoft.Json;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class BowerbirdFloorplanExporter : IBowerbirdCommand 
    {
        public string Name => "Export floorplan";

        public Document Document;
        public BackgroundUI Background;
        public Dictionary<int, List<Opening>> OpeningGroups;
        public Dictionary<int, List<FamilyInstance>> DoorGroups;
        
        public void Execute(object arg)
        {
            var uiapp = (arg as UIApplication);
            Document = uiapp.ActiveUIDocument.Document;
            var rooms = Document.GetRooms().ToList();
            OpeningGroups = Document.GroupOpeningsByHost();
            DoorGroups = Document.GroupDoorsByHost();
            Background = new BackgroundUI(uiapp, ProcessRoom);
            Background.Processor.EnqueueWork(rooms.Select(r => r.Id.IntegerValue));
        }

        public void ProcessRoom(int id)
        {
            var room = Document.GetElement(new ElementId(id)) as Room;
            if (room == null)
                return;
            var openings = room.GetBoundaryOpenings(OpeningGroups);
            var doors  = room.GetBoundaryDoors(DoorGroups);
            var boundaries = room.GetRoomBoundaryCoordinates();
            foreach (var loop in boundaries)
            {
                var loopCoords = new List<List<double>>();
                foreach (var c in loop)
                {
                    loopCoords.Add(new List<double> { c.X, c.Y, c.Z });
                }

                .Coordinates.Add(loopCoords);
            }

            var geoJson = ToGeoJson(room, openings, doors);
            
            var json = JsonConvert.SerializeObject(geoJson, Formatting.Indented);
            var f = OutputFolder.RelativeFile($"room-{room.Name.ToValidFileName()}-{room.Id.IntegerValue}.json");
            f.WriteAllText(json);

            // Add an artificial delay, otherwise the demo is finished too fast.  
            Thread.Sleep(250);
        }


            foreach (var box in openings.Select(opening => opening.GetBaseBox()))
            {
                if (box == null)
                    continue;

                var minZ = box.Min(m => m.Z);
                var vals = box.Select(p => new List<double> { p.X, p.Y, minZ });
                r.Openings.Coordinates.Add(new List<List<double>>(vals));
            }

            foreach (var box in doors.Select(door => door.GetBaseBox()))
            {
                if (box == null)
                    continue;
                r.Doors.Coordinates.Add(new List<List<double>>
                {
                    new List<double> { box[0].X, box[0].Y, box[0].Z },
                    new List<double> { box[1].X, box[1].Y, box[1].Z },
                    new List<double> { box[2].X, box[2].Y, box[2].Z },
                    new List<double> { box[3].X, box[3].Y, box[3].Z },
                });
            }

            return r;
        }
    }
}