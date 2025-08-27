using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandListRooms : NamedCommand
    {
        public override string Name => "List Rooms";

        public List<Room> Rooms;

        public override void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            if (doc == null) return;
            Rooms = doc.GetRooms().ToList();

            var builder = new DataTableBuilder(typeof(RoomData));
            builder.AddRows(Rooms.Select(r => r.GetRoomData()));
            var form = new DataTableForm(builder);
            form.Show();
        }
    }
}