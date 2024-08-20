using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class RoomData
    {
        public double Perimeter;
        public double Area;
        public int NumWalls;
        public int NumDoors;
        public double LongestWall;
        public int NumSharedRooms;
        public double BoundingBoxAspectRatio;
        public double BoundingBoxLongestSide;
        public double BoundingBoxShortestSide;
        public int NumOutlets;
        public double RatioOfAreaToBoundingBox;
        public double RatioOfPerimeterToBoundingBox;
        public bool HasChildRoom;
    }

    public static class RoomDataExtensions
    {
        public static IEnumerable<Room> GetRooms(this Document doc)
            => doc.CollectElements()
                .OfClass(typeof(SpatialElement))
                .OfType<Room>();

        public static Dictionary<int, List<FamilyInstance>> GroupByRoom(this IEnumerable<FamilyInstance> instances)
            => instances.GroupBy(fi => fi.GetRoomId()).ToDictionary(g => g.Key, g => g.ToList());

        public static FilteredElementCollector CollectElements(this Document doc)
            => new FilteredElementCollector(doc);

        public static IEnumerable<FamilyInstance> GetFamilyInstances(this Document doc)
            => doc.CollectElements()
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>();

        public static IEnumerable<FamilyInstance> GetFamilyInstances(this Document doc, BuiltInCategory cat)
            => doc.CollectElements()
                .OfCategory(cat)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>();

        public static IEnumerable<FamilyInstance> GetLights(this Document doc)
            => doc.GetFamilyInstances(BuiltInCategory.OST_LightingFixtures);

        public static IEnumerable<FamilyInstance> GetSockets(this Document doc)
            => doc.GetFamilyInstances(BuiltInCategory.OST_ElectricalFixtures);

        public static IEnumerable<FamilyInstance> GetDoors(this Document doc)
            => doc.GetFamilyInstances(BuiltInCategory.OST_Doors);

        public static IEnumerable<FamilyInstance> GetWindows(this Document doc)
            => doc.GetFamilyInstances(BuiltInCategory.OST_Windows);

        public static IEnumerable<FamilyInstance> GetWalls(this Document doc)
            => doc.GetFamilyInstances(BuiltInCategory.OST_Walls);

        public static int CountFamilyInstance(this Room room, BuiltInCategory cat)
            => room.Document.GetFamilyInstances(cat).Count();

        public static IEnumerable<Wall> GetWalls(this Room room)
        {
            foreach (var segment in room.GetBoundarySegments())
            {
                var elementId = segment.ElementId;
                var element = room.Document.GetElement(elementId);
                if (element is Wall wall)
                    yield return wall;
            }
        }

        public static bool IsCategoryType(this Element element, BuiltInCategory cat)
            => element.Category.Id.IntegerValue == (int)cat
               || element is FamilyInstance fi && fi.Symbol.IsCategoryType(cat);

        public static IEnumerable<Element> GetHostedElements(this HostObject self)
            => self.Document.GetElements(self.FindInserts(true, false, true, true));

        public static IEnumerable<Element> GetElements(this Document doc, IEnumerable<ElementId> ids)
            => ids.Select(doc.GetElement);

        public static IEnumerable<BoundarySegment> GetBoundarySegments(this Room room)
        {
            var options = new SpatialElementBoundaryOptions();
            var boundaries = room.GetBoundarySegments(options);
            if (boundaries == null)
                yield break;
            foreach (var boundaryList in boundaries)
                foreach (var segment in boundaryList)
                    yield return segment;
        }

        public static int GetRoomId(this FamilyInstance self)
        {
            try { return self.Room?.Id.IntegerValue ?? -1; }
            catch { return -1; }
        }

        public static IEnumerable<FamilyInstance> BelongingToRoom(this IEnumerable<FamilyInstance> self, Room room)
            => self.Where(fi => fi.GetRoomId() == room.Id.IntegerValue);

        public static IEnumerable<FamilyInstance> GetLights(this Room room)
            => room.Document.GetLights().BelongingToRoom(room);

        public static IEnumerable<FamilyInstance> GetDoors(this Room room)
            => room.Document.GetDoors().BelongingToRoom(room);

        public static IEnumerable<FamilyInstance> GetWindows(this Room room)
            => room.Document.GetWindows().BelongingToRoom(room);

        public static IEnumerable<FamilyInstance> GetSockets(this Room room)
            => room.Document.GetSockets().BelongingToRoom(room);

        public static double SpaceArea(Document doc, Room room)
        {
            var calculator = new SpatialElementGeometryCalculator(doc);

            // compute the room geometry
            var results = calculator.CalculateSpatialElementGeometry(room);

            // get the solid representing the room's geometry
            var roomSolid = results.GetGeometry();

            var result = 0.0;

            foreach (Face face in roomSolid.Faces)
            {
                // TODO: I am not convinced that this computation is correct. 
                var faceArea = face.Area;

                // get the sub-faces for the face of the room
                var subfaceList = results.GetBoundaryFaceInfo(face);
                if (subfaceList.Count > 0) // there are multiple sub-faces that define the face
                {
                    // get the area of each sub-face
                    // sub-faces exist in situations such as when a room-bounding wall has been
                    // horizontally split and the faces of each split wall combine to create the 
                    // entire face of the room
                    foreach (var subface in subfaceList)
                    {
                        var subfaceArea = subface.GetSubface().Area;
                        result += subfaceArea;
                    }
                }
                else
                {
                    result += faceArea;
                }

            }

            return result;
        }

        public static Solid GetGeometry(this SpatialElement space)
        {
            var calculator = new SpatialElementGeometryCalculator(space.Document);
            var results = calculator.CalculateSpatialElementGeometry(space);
            return results.GetGeometry();
        }

        public static IEnumerable<SpatialElementBoundarySubface> GetSubfaces(this SpatialElement space)
        {
            var calculator = new SpatialElementGeometryCalculator(space.Document);
            var results = calculator.CalculateSpatialElementGeometry(space);
            var roomSolid = results.GetGeometry();

            foreach (Face face in roomSolid.Faces)
            {
                // get the sub-faces for the face of the room
                var subfaceList = results.GetBoundaryFaceInfo(face);
                foreach (SpatialElementBoundarySubface subface in subfaceList)
                    yield return subface;

                // sub-faces exist in situations such as when a room-bounding wall has been
                // horizontally split and the faces of each split wall combine to create the 
                // entire face of the room
            }
        }
    }
}
