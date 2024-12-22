using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
using Enumerable = System.Linq.Enumerable;
using View = Autodesk.Revit.DB.View;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class BowerbirdGeoJsonImporter : IBowerbirdCommand, IDirectContext3DServer
    {
        public string Name => "Import GeoJSON rooms";

        public Document Document;
        public BackgroundUI Background;

        public List<RoomData> Rooms = new List<RoomData>();
        public DirectoryPath InputFolder = BowerbirdGeoJsonExporter.OutputFolder;

        public static IReadOnlyList<Vector3D> GetPointList(List<List<double>> values)
        {
            var pts = new List<Vector3D>();
            for (var i = 0; i < values.Count; i++)
            {
                var p0 = values[i];
                var v0 = new Vector3D(p0[0], p0[1], p0[2]);
                pts.Add(v0);
            }

            return pts;
        }

        public void Execute(object arg)
        {
            var uiapp = (arg as UIApplication);
            Document = uiapp.ActiveUIDocument.Document;

            if (!InputFolder.Exists())
            {
                MessageBox.Show($"No GeoJSON folder found at {InputFolder}");
                return;
            }

            var files = Enumerable.ToList(InputFolder.GetFiles("*.json"));
            if (!Enumerable.Any(files))
            {
                MessageBox.Show($"No GeoJSON files found in {InputFolder}");
                return;
            }

            Rooms.Clear();
            foreach (var file in files)
            {
                var text = System.IO.File.ReadAllText(file);
                var geoJson = JsonConvert.DeserializeObject<GeoJson>(text);
                var roomData = new RoomData();
                Rooms.Add(roomData);

                var allPts = new List<Vector3D>();
                foreach (var list in geoJson.PayloadGeoJson.Coordinates)
                {
                    var pts = GetPointList(list);
                    allPts.AddRange(pts);
                }

                roomData.Center = allPts.ToIArray().Average();
                foreach (var list in geoJson.Doors.Coordinates)
                {
                    var pts = GetPointList(list);
                    var doorCenter = Enumerable.Aggregate(pts, (a, b) => a + b) / pts.Count;
                    roomData.DoorCenters.Add(doorCenter);
                }
            }

            UpdateMesh();

            // Register this class as a server with the DirectContext3D service.
            var directContext3DService =
                ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            directContext3DService.AddServer(this);

            var msDirectContext3DService = directContext3DService as MultiServerService;
            if (msDirectContext3DService == null)
                throw new Exception("Expected a MultiServerService");

            // Get current list 
            var serverIds = msDirectContext3DService.GetActiveServerIds();
            serverIds.Add(GetServerId());

            // Add the new server to the list of active servers.
            msDirectContext3DService.SetActiveServers(serverIds);

            uiapp.ActiveUIDocument?.UpdateAllOpenViews();
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

        public void UpdateMesh()
        {
            var scene = new Scene();
            var arrows = scene.Root.AddNode("Arrows");

            var arrowMesh = Extensions2
                .UpArrow(1, 0.2, 0.4, 12, 0.8)
                .ToTriangleMesh();

            foreach (var room in Rooms)
            {
                foreach (var doorCenter in room.DoorCenters)
                {
                    var pos = room.Center + Vector3D.UnitZ;
                    var target = doorCenter + Vector3D.UnitZ;

                    for (var i = 0; i < 5; i++)
                    {
                        var t = i * 0.2;
                        var p1 = pos.Lerp(target, t);
                        var dir = target - pos;
                        var len = dir.Length / 10;
                        var rot = dir.LookRotation(Vector3D.UnitZ);

                        arrows.AddMesh(arrowMesh, new Transform3D(p1, rot, (1, 1, len)));
                    }
                }
            }

            Mesh = scene.ToRenderMesh();

            var min = new XYZ();
            var max = new XYZ();
            m_boundingBox = new Outline(min, max);
            foreach (var v in Mesh.Vertices)
            {
                m_boundingBox.AddPoint(new XYZ(v.PX, v.PY, v.PZ));
            }
        }

        public void RenderScene(View dBView, DisplayStyle displayStyle)
        {
            if (Mesh == null) return;
            // NOTE: do not try to create the FaceBufferStorage when not in this call.
            // It will have undefined behavior. 
            if (FaceBufferStorage == null)
                FaceBufferStorage = new BufferStorage(Mesh);
            FaceBufferStorage.Render();
        }
    }

    // This class creates JSON files representing the room boundaries, openings, and doors.
        // It uses the background processor to do work. 
}