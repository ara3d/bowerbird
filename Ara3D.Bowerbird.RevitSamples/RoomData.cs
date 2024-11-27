using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Reflection;
using Autodesk.Revit.UI;
using Plato.DoublePrecision;
using Plato.Geometry.Graphics;
using Plato.Geometry.Memory;
using Plato.Geometry.Scenes;
using Arc = Autodesk.Revit.DB.Arc;
using Type = System.Type;
using Autodesk.Revit.DB.ExternalService;

namespace Ara3D.Bowerbird.RevitSamples
{
    // TODO: move this into Ara3D.Utils
    public class DataTableBuilderOptions
    {
        public bool IncludeFields = true;
        public bool IncludeProps = true;
        public bool IncludeMethods = false;
        public bool PublicOnly = true;
        public bool DeclaredOnly = false;
    }

    public class DataTableBuilder
    {
        public readonly Type Type;
        public readonly IReadOnlyList<MemberInfo> Members;
        public readonly DataTableBuilderOptions Options;
        public readonly DataTable DataTable;

        public DataTableBuilder(Type type, DataTableBuilderOptions options = null)
        {
            Options = options ?? new DataTableBuilderOptions();
            Type = type;
            DataTable = new DataTable(Type.Name);

            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (!Options.PublicOnly)
                bindingFlags |= BindingFlags.NonPublic;
            if (Options.DeclaredOnly)
                bindingFlags |= BindingFlags.DeclaredOnly;

            var properties = Options.IncludeProps ? Type.GetProperties(bindingFlags).Where(DataTableExtensions.CanGetValue).ToArray() : Array.Empty<PropertyInfo>();
            foreach (var property in properties)
                DataTable.Columns.Add(property.Name, property.GetValueType());

            var fields = Options.IncludeFields ? Type.GetFields(bindingFlags).ToArray() : Array.Empty<FieldInfo>();
            foreach (var field in fields)
                DataTable.Columns.Add(field.Name, field.GetValueType());

            var methods = Options.IncludeMethods ? Type.GetMethods(bindingFlags).Where(DataTableExtensions.CanGetValue).ToArray() : Array.Empty<MethodInfo>();
            foreach (var method in methods)
                DataTable.Columns.Add(method.Name, method.GetValueType());

            Members = properties.Cast<MemberInfo>().Concat(fields).Concat(methods).ToArray();
        }

        public DataTableBuilder AddRows(IEnumerable items)
        {
            foreach (var item in items)
            {
                var row = DataTable.NewRow();
                foreach (var member in Members)
                {
                    try
                    {
                        row[member.Name] = member.GetValue(item) ?? DBNull.Value;
                    }
                    catch (Exception e)
                    {
                        row.SetColumnError(member.Name, $"{member.Name}: {e.Message}");
                    }
                }
                DataTable.Rows.Add(row);
            }
            return this;
        }
    }

    public static class DataTableExtensions
    {
        public static Type GetUnderlyingType(this Type type)
            => Nullable.GetUnderlyingType(type) ?? type;

        public static Type GetValueType(this MemberInfo mi)
        {
            if (mi is FieldInfo f) return f.FieldType.GetUnderlyingType();
            if (mi is PropertyInfo p) return p.PropertyType.GetUnderlyingType();
            if (mi is MethodInfo m) return m.ReturnType.GetUnderlyingType();
            throw new Exception("Not invokable member");
        }

        public static object GetValue(this MemberInfo mi, object obj)
        {
            if (mi is FieldInfo f) return f.GetValue(obj);
            if (mi is PropertyInfo p) return p.GetValue(obj);
            if (mi is MethodInfo m) return m.Invoke(obj, Array.Empty<object>());
            throw new Exception("Not invokable member");
        }

        public static bool CanGetValue(this MemberInfo mi)
            => mi is FieldInfo 
               || (mi is PropertyInfo pi && pi.CanRead && pi.GetIndexParameters().Length == 0) 
               || (mi is MethodInfo method && method.GetParameters().Length == 0);

        public static DataTableBuilder BuildDataTable(this Type self, DataTableBuilderOptions options = null)
            => new DataTableBuilder(self, options);

        public static DataTable ToDataTable<T>(this IEnumerable<T> self, DataTableBuilderOptions options = null)
            => BuildDataTable(typeof(T), options).AddRows(self).DataTable;
    }

    public class RoomData
    { }

    // TODO: consider maybe making an Ara3D.Utils.Revit for these functions and more. 
    public static class RoomDataExtensions
    {
        public static IEnumerable<Phase> GetPhases(this Document doc)
            => doc.CollectElements()
                .OfClass(typeof(Phase))
                .OfType<Phase>();

        public static int GetPhaseSequenceNumber(this Phase p)
            => p.get_Parameter(BuiltInParameter.PHASE_SEQUENCE_NUMBER).AsInteger();

        public static Phase GetLastPhase(this Document doc)
            => doc.GetPhases().OrderBy(GetPhaseSequenceNumber).LastOrDefault();

        public static IEnumerable<Room> GetRooms(this Document doc)
            => doc.CollectElements()
                .OfClass(typeof(SpatialElement))
                .OfType<Room>();

        public static IEnumerable<Level> GetLevels(this Document doc)
            => doc.CollectElements()
                .OfClass(typeof(Level))
                .OfType<Level>();

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

        public static IEnumerable<Wall> GetBoundaryWalls(this Room room)
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

        public static List<List<XYZ>> GetRoomBoundaryCoordinates(this Room room)
        {
            // List to hold all boundary loops
            var boundaries = new List<List<XYZ>>();

            // Create boundary options
            var options = new SpatialElementBoundaryOptions();

            // Get the boundary segments of the room
            var boundarySegments = room.GetBoundarySegments(options);

            // Check if boundary segments are available
            if (boundarySegments != null)
            {
                // Iterate over each boundary loop
                foreach (var segmentList in boundarySegments)
                {
                    // List to hold points of the current boundary loop
                    var boundaryPoints = new List<XYZ>();

                    // Iterate over each segment in the boundary loop
                    foreach (var segment in segmentList)
                    {
                        // Get the curve of the segment
                        var curve = segment.GetCurve();

                        // Get the start point of the curve
                        var startPoint = curve.GetEndPoint(0);

                        // Add the point to the boundary points list
                        boundaryPoints.Add(startPoint);
                    }

                    // Close the loop by adding the first point at the end if necessary
                    if (boundaryPoints.Count > 0 &&
                        !boundaryPoints[0].IsAlmostEqualTo(boundaryPoints[boundaryPoints.Count - 1]))
                    {
                        boundaryPoints.Add(boundaryPoints[0]);
                    }

                    // Add the current boundary loop to the boundaries list
                    boundaries.Add(boundaryPoints);
                }
            }

            return boundaries;
        }

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
            try
            {
                return self.Room?.Id.IntegerValue ?? -1;
            }
            catch
            {
                return -1;
            }
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
                foreach (var subface in subfaceList)
                    yield return subface;

                // sub-faces exist in situations such as when a room-bounding wall has been
                // horizontally split and the faces of each split wall combine to create the 
                // entire face of the room
            }
        }

        /// <summary>
        /// Retrieves the Opening elements grouped by their Host.
        /// </summary>
        public static Dictionary<int, List<Opening>> GroupOpeningsByHost(this Document doc)
            => new FilteredElementCollector(doc).OfClass(typeof(Opening)).Cast<Opening>()
                .GroupBy(opening => opening.Host?.Id ?? ElementId.InvalidElementId)
                .ToDictionary(g => g.Key.IntegerValue, g => g.ToList());

        /// <summary>
        /// Retrieves the Door elements grouped by their Host.
        /// </summary>
        public static Dictionary<int, List<FamilyInstance>> GroupDoorsByHost(this Document doc)
            => doc.GetDoors()
                .GroupBy(door => door.Host?.Id ?? ElementId.InvalidElementId)
                .ToDictionary(g => g.Key.IntegerValue, g => g.ToList());

        public static IEnumerable<FamilyInstance> GetBoundaryDoors(this Room room,
            Dictionary<int, List<FamilyInstance>> doorsByHost)
            => room.GetBoundaryWalls().SelectMany(bw => doorsByHost.TryGetValue(bw.Id.IntegerValue, out var value)
                ? value
                : Enumerable.Empty<FamilyInstance>());

        public static IEnumerable<Opening> GetBoundaryOpenings(this Room room,
            Dictionary<int, List<Opening>> openingsByHost)
            => room.GetBoundaryWalls().SelectMany(bw => openingsByHost.TryGetValue(bw.Id.IntegerValue, out var value)
                ? value
                : Enumerable.Empty<Opening>());

        public static XYZ[] GetBaseBox(this Element e)
        {
            var box = e.get_BoundingBox(null);
            if (box == null)
                return null;
            var z = box.Min.Z;
            var minX = box.Min.X;
            var minY = box.Min.Y;
            var maxX = box.Max.X;
            var maxY = box.Max.Y;
            return new[]
                { new XYZ(minX, minY, z), new XYZ(maxX, minY, z), new XYZ(maxX, maxY, z), new XYZ(minX, maxY, z) };
        }

        public static IList<XYZ> GetBaseBox(this Opening opening)
        {
            if (opening.IsRectBoundary)
                return opening.BoundaryRect.ToArray();

            var list = new List<XYZ>();
            foreach (var curve in opening.BoundaryCurves)
            {
                if (curve is Line line)
                {
                    list.Add(line.GetEndPoint(0));
                    list.Add(line.GetEndPoint(1));
                }
                else if (curve is Arc arc)
                {
                    list.Add(arc.GetEndPoint(0));
                    list.Add(arc.GetEndPoint(1));
                }
            }

            return list;
        }

        public static RenderMesh ToRenderMesh(this Scene scene)
        {
            var vertices = new List<RenderVertex>();
            var indices = new List<Integer>();

            foreach (var node in GetDescendants(scene.Root))
            {
                var t = node.Transform;

                foreach (var o in node.GetMeshes())
                {
                    var offset = vertices.Count;
                    var mesh = o.Mesh;
                    var m = o.Material;
                    var c = (Color32)m.Color;

                    var normals = mesh.ComputeVertexNormals();

                    for (var i = 0; i < mesh.Points.Count; ++i)
                    {
                        var p = mesh.Points[i];
                        var n = normals[i];
                        var p1 = t.Transform(p);
                        var n1 = t.TransformNormal(n);
                        var rv = new RenderVertex(p1, n1, Vector2D.Default, c);
                        vertices.Add(rv);
                    }

                    foreach (var i in mesh.Indices)
                    {
                        indices.Add(i + offset);
                    }
                }
            }

            if (vertices.Count == 0 || indices.Count == 0)
                return null;
            return new RenderMesh(vertices.ToBuffer(), indices.ToBuffer());
        }

        public static IEnumerable<ISceneNode> GetDescendants(this ISceneNode node)
            => node.Children.SelectMany(GetDescendants).Prepend(node);

        public static XYZ Current3DCameraPosition(this UIDocument uidoc)
        {
            var view = uidoc.ActiveView as View3D;
            if (view == null)
                return XYZ.Zero;
            return view.GetOrientation().EyePosition;
        }

        public static void Update3DCameraPosition(this UIDocument uidoc, XYZ pos)
        {
            var view = uidoc.ActiveView as View3D;
            if (view == null)
                return;

            // Start a transaction to modify the view
            using (var t = new Transaction(uidoc.Document, "Move Camera"))
            {
                t.Start();

                // Get the current camera orientation
                var orientation = view.GetOrientation();

                // Create the new orientation with the updated eye position
                var newOrientation = new ViewOrientation3D(
                    pos,
                    orientation.UpDirection,
                    orientation.ForwardDirection);

                // Apply the updated orientation back to the view
                view.SetOrientation(newOrientation);

                t.Commit();
            }

            // Refresh the active view to reflect changes
            uidoc.RefreshActiveView();
        }

        public static void RegisterDirectDrawServer(this IExternalServer self)
        {
            // Register this class as a server with the DirectContext3D service.
            var directContext3DService =
                ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            directContext3DService.AddServer(self);

            var msDirectContext3DService = directContext3DService as MultiServerService;
            if (msDirectContext3DService == null)
                throw new Exception("Expected a MultiServerService");

            // Get current list 
            var serverIds = msDirectContext3DService.GetActiveServerIds();
            serverIds.Add(self.GetServerId());

            // Add the new server to the list of active servers.
            msDirectContext3DService.SetActiveServers(serverIds);
        }
        
        // TODO: this throws an exception if the server was not registered. Should check.
        public static void UnregisterDirectDrawServer(this IExternalServer self)
        {
            // Register this class as a server with the DirectContext3D service.
            var directContext3DService =
                ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            directContext3DService.RemoveServer(self.GetServerId());

            var msDirectContext3DService = directContext3DService as MultiServerService;
            if (msDirectContext3DService == null)
                throw new Exception("Expected a MultiServerService");

            // Get current list 
            var serverIds = msDirectContext3DService.GetActiveServerIds();
            serverIds.Remove(self.GetServerId());

            // Remove the new server from the list of active servers.
            msDirectContext3DService.SetActiveServers(serverIds);
            
        }
    }
}
