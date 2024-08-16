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
        public static FilteredElementCollector CollectElements(this Document doc)
            => new FilteredElementCollector(doc);

        public static IEnumerable<FamilyInstance> GetFamilyInstances(this Document doc, BuiltInCategory cat)
            => doc.CollectElements()
                .OfCategory(cat)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>();

        public static IEnumerable<FamilyInstance> GetLights(this Document doc)
            => doc.GetFamilyInstances(BuiltInCategory.OST_LightingFixtures);

        public static IEnumerable<FamilyInstance> BelongingToRoom(this IEnumerable<FamilyInstance> self, Room room)
            => self.Where(fi => fi.Room.Id == room.Id);

        public static int GetNumberOfLights(this Room room)
            => room.Document.GetLights().BelongingToRoom(room).Count();

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
