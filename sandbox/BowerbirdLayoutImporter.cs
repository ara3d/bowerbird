using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Plato.DoublePrecision;
using Plato.Geometry.Graphics;
using Plato.Geometry.Scenes;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class BowerbirdLayoutImporter : IBowerbirdCommand, IDirectContext3DServer
    {
        public string Name => "Import layout";

        public Document Document;
        public UIDocument UIDocument;
        public BackgroundUI Background;
        public ChooseRoomForm Form;
        public ChooseRoomForm Form2;
        public int SrcRoom;
        public int DestRoom;
        public BuildingLayout Layout = new BuildingLayout();
        public XYZ NewPos;
        public ExternalEvent UpdateCameraEvent;
            
        public void UpdateCameraCallback(UIApplication app)
        {
            app.ActiveUIDocument.Update3DCameraPosition(NewPos);
        }

        public void UpdateCameraFromPosition(XYZ pos)
        {
            NewPos = pos;
            UpdateCameraEvent.Raise();
        }

        public void OnSrcRoomChanged(RoomStruct roomStruct)
        {
            SrcRoom = roomStruct.Id;
            UpdateMesh();
        }

        public void OnDestRoomChanged(RoomStruct roomStruct)
        {
            DestRoom = roomStruct.Id;
            UpdateMesh();
        }
        
        public void Execute(object arg)
        {
            var uiapp = (arg as UIApplication);
            UIDocument = uiapp.ActiveUIDocument;
            Document = UIDocument.Document;
            UpdateCameraEvent = ApiContext.CreateEvent(UpdateCameraCallback, "Update camera");

            var filePath = BuildingLayoutExporter.LayoutFile;
            var json = filePath.ReadAllText();

            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>() { new MiscExtensions.BoundsConverter() }
            };

            Layout = JsonConvert.DeserializeObject<BuildingLayout>(json, settings);
                
            foreach (var room in Layout.Rooms.Values)
                RoomCenters[room.Id] = room.Bounds.Center;

            foreach (var door in Layout.Doors.Values)
                DoorCenters[door.Id] = door.Bounds.Center;

            ComputeAllPaths();

            Form = new ChooseRoomForm(Layout, OnSrcRoomChanged);
            Form.Closing += Form_Closing;
            Form.Show();

            Form2 = new ChooseRoomForm(Layout, OnDestRoomChanged);
            Form2.Show();

            // Register this class as a server with the DirectContext3D service.
            this.RegisterDirectDrawServer();

            UIDocument?.UpdateAllOpenViews();
        }

        private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SrcRoom = -1;
            try 
            {
                this.UnregisterDirectDrawServer();
            }
            catch (Exception ex)
            { 
                Debug.WriteLine(ex.Message);
            }
        }

        public Guid Guid { get; } = Guid.NewGuid();
        public Outline m_boundingBox;
        public RenderMesh Mesh;
        public BufferStorage FaceBufferStorage;

        public Guid GetServerId() => Guid;
        public ExternalServiceId GetServiceId() => ExternalServices.BuiltInExternalServices.DirectContext3DService;
        public string GetName() => Name;
        public string GetVendorId() => "Ara 3D Inc.";
        public string GetDescription() => "Demonstrates using the DirectContext3D API";
        public bool CanExecute(View dBView) => dBView.ViewType == ViewType.ThreeD;
        public string GetApplicationId() => "Bowerbird";
        public string GetSourceId() => "";
        public bool UsesHandles() => false;
        public Outline GetBoundingBox(View dBView) => m_boundingBox;
        public bool UseInTransparentPass(View dBView) => true;
        public bool NeedsUpdate { get; set; }

        public Dictionary<int, Vector3D> RoomCenters = new Dictionary<int, Vector3D>();
        public Dictionary<int, Vector3D> DoorCenters = new Dictionary<int, Vector3D>();

        public void UpdateMeshWithAllArrows()
        {
            var scene = new Scene();
            var lines = new List<Line3D>();

            foreach (var door in Layout.Doors.Values)
            {
                var doorCenter = door.Bounds.Center;

                if (door.FromRoom == SrcRoom || door.FromRoom == DestRoom)
                    if (RoomCenters.TryGetValue(door.FromRoom, out var a))
                        lines.Add(new Line3D(a, doorCenter));

                if (door.ToRoom == SrcRoom || door.ToRoom == DestRoom)
                    if (RoomCenters.TryGetValue(door.ToRoom, out var b))
                        lines.Add(new Line3D(b, doorCenter));
            }

            AddArrowsFromLine(scene.Root.AddNode("Arrows"), lines);
            UpdateMeshAndBoundingBoxFromScene(scene);
        }

        public void UpdateMeshAndBoundingBoxFromScene(Scene scene)
        {
            Mesh = scene.ToRenderMesh();
            if (Mesh != null)
            {
                var min = new XYZ();
                var max = new XYZ();
                m_boundingBox = new Outline(min, max);
                foreach (var v in Mesh.Vertices)
                {
                    m_boundingBox.AddPoint(new XYZ(v.PX, v.PY, v.PZ));
                }
            }

            UIDocument.RefreshActiveView();
        }

        public void AddArrowsFromLine(SceneNode root, IEnumerable<Line3D> lines)
        {
            var arrowMesh = Extensions2
                .UpArrow(1, 0.4, 0.8, 12, 0.75)
                .ToTriangleMesh();

            foreach (var line in lines)
            {
                var dir = line.Direction;
                var len = dir.Length;

                // We are going to skip empty arrows
                if (len <= double.Epsilon)
                    continue;

                var rot = dir.LookRotation(Vector3D.UnitZ);
                var mesh = root.AddMesh(arrowMesh, new Transform3D(line.A, rot, (1, 1, len)));
                mesh.Material = Colors.Red;
            }
        }

        public void UpdateMesh()
        {
            Mesh = null;
            NeedsUpdate = true;
            var path = GetShortestPath(SrcRoom, DestRoom);

            if (path == null)
            {
                UpdateMeshWithAllArrows();
                return;
            }

            var lines = new List<Line3D>();

            if (RoomCenters.TryGetValue(SrcRoom, out var a))
            {
                foreach (var door in path.Doors)
                {
                    if (DoorCenters.TryGetValue(door, out var b))
                    {
                        lines.Add((a, b));
                        a = b;
                    }
                }

                var final = RoomCenters[DestRoom];
                lines.Add((a, final));
            }

            var scene = new Scene();
            AddArrowsFromLine(scene.Root.AddNode("Arrows"), lines);
            UpdateMeshAndBoundingBoxFromScene(scene);
        }

        public void RenderScene(View dBView, DisplayStyle displayStyle)
        {
            if (Mesh == null) return;

            // NOTE: do not try to create the FaceBufferStorage when not in this call.
            // It will have undefined behavior. 
            if (FaceBufferStorage == null || NeedsUpdate)
            {
                FaceBufferStorage = new BufferStorage(Mesh);
            }

            FaceBufferStorage.Render();
        }

        public class Path
        {
            public readonly int SrcRoom;
            public readonly int DestRoom;
            public readonly IReadOnlyList<int> Doors;

            public Path(int src, int dest, IReadOnlyList<int> doors)
            {
                SrcRoom = src;
                DestRoom = dest;
                Doors = doors;
                Debug.Assert(src != dest);
                Debug.Assert(Enumerable.Count(Enumerable.Distinct(doors)) == doors.Count);
            }

            public Path Reverse()
                => new Path(DestRoom, SrcRoom, Enumerable.ToList(Enumerable.Reverse(Doors)));

            public Path ExtendWithDoorIfValid(DoorStruct door)
            {
                if (door.FromRoom == DestRoom 
                    && door.ToRoom != SrcRoom 
                    && !Enumerable.Contains(Doors, door.Id))
                {
                    return new Path(SrcRoom, door.ToRoom, Enumerable.ToList(LinqUtil.Append(Doors, door.Id)));
                }

                return null;
            }

            public static Path CreateFromDoor(DoorStruct door)
                => new Path(door.FromRoom, door.ToRoom, new[] { door.Id });
        }

        public Dictionary<int, Dictionary<int, Path>> ShortestPaths = new Dictionary<int, Dictionary<int, Path>>();

        public Path AddOrGetPathIfShortest(Path path)
        {
            if (path == null)
                return null;
            if (!ShortestPaths.ContainsKey(path.SrcRoom))
                ShortestPaths.Add(path.SrcRoom, new Dictionary<int, Path>());
            var d = ShortestPaths[path.SrcRoom];
            if (d.TryGetValue(path.DestRoom, out var shortest))
                return shortest;
            d.Add(path.DestRoom, path);
            return path;
        }

        public IEnumerable<DoorStruct> GetDoors()
            => Layout.Doors.Values;

        public IEnumerable<Path> GetAllPaths()
            => Enumerable.SelectMany(ShortestPaths, d => d.Value.Values);

        public Path GetShortestPath(int src, int dest)
            => ShortestPaths.TryGetValue(src, out var d) 
               && d.TryGetValue(dest, out var path) ? path : null;

        public DoorStruct Reverse(DoorStruct door)
            => new DoorStruct()
            {
                Bounds = door.Bounds, FromRoom = door.ToRoom, ToRoom = door.FromRoom, Id = door.Id,
                Level = door.Level, Name = door.Name
            }; 

        public void ComputeAllPaths()
        {
            ShortestPaths = new Dictionary<int, Dictionary<int, Path>>();

            // Seed it with the initial paths (door) 
            foreach (var door in GetDoors())
            {
                // Skip doors to nowhere 
                if (door.ToRoom == -1 || door.FromRoom == -1)
                    continue;

                var path = Path.CreateFromDoor(door);
                AddOrGetPathIfShortest(path);
                AddOrGetPathIfShortest(path.Reverse());
            }

            // Extend all paths by one
            var paths = Enumerable.ToList(GetAllPaths());

            foreach (var path in paths)
            {
                foreach (var door in GetDoors())
                {
                    var path1 = path.ExtendWithDoorIfValid(door);
                    AddOrGetPathIfShortest(path1);

                    var path2 = path.ExtendWithDoorIfValid(Reverse(door));
                    AddOrGetPathIfShortest(path2);
                }
            }

            // Extend all paths by one
            paths = Enumerable.ToList(GetAllPaths());
            foreach (var path in paths)
            {
                foreach (var door in GetDoors())
                {
                    var path1 = path.ExtendWithDoorIfValid(door);
                    AddOrGetPathIfShortest(path1);

                    var path2 = path.ExtendWithDoorIfValid(Reverse(door));
                    AddOrGetPathIfShortest(path2);
                }
            }

            // TODO: this only considers paths with up to two doors.
        }
  
    }
}