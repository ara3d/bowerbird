using System;
using System.Diagnostics;
using System.Windows.Forms;
using Ara3D.Bowerbird.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using Plato.Geometry.Graphics;
using Plato.Geometry.IO;
using PrimitiveType = Autodesk.Revit.DB.DirectContext3D.PrimitiveType;
using View = Autodesk.Revit.DB.View;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class DirectContextDemo : IBowerbirdCommand, IDirectContext3DServer
    {
        public string Name => "Direct Context Demo";

        public Guid Guid { get; } = Guid.NewGuid();
        public Outline m_boundingBox;

        public void Execute(object argument)
        {
            var app = (UIApplication)argument;

            //Set bounding box
            m_boundingBox = new Outline(new XYZ(0, 0, 0), new XYZ(10, 10, 10));

            LoadPlyFile();
            if (Mesh == null)
                return;

            // Register this class as a server with the DirectContext3D service.
            var directContext3DService = ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            directContext3DService.AddServer(this);

            var msDirectContext3DService = directContext3DService as MultiServerService;
            if (msDirectContext3DService == null)
                throw new Exception("Expected a MultiServerService");

            // Get current list 
            var serverIds = msDirectContext3DService.GetActiveServerIds();
            serverIds.Add(GetServerId());

            // Add the new server to the list of active servers.
            msDirectContext3DService.SetActiveServers(serverIds);

            app.ActiveUIDocument?.UpdateAllOpenViews();
        }

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
        public RenderMesh Mesh;
        public BufferStorage FaceBufferStorage;
        //public BufferStorage EdgeBufferStorage;

        public void RenderScene(View dBView, DisplayStyle displayStyle)
        {
            if (Mesh == null)
                return;
            if (FaceBufferStorage == null)
                FaceBufferStorage = new BufferStorage(PrimitiveType.TriangleList, Mesh.Vertices, Mesh.Indices);
            FaceBufferStorage.Render();
        }

        public OpenFileDialog PlyOpenFileDialog;

        public void LoadPlyFile()
        {
            if (PlyOpenFileDialog == null)
            {
                PlyOpenFileDialog = new OpenFileDialog()
                {
                    InitialDirectory = "C:\\Users\\cdigg\\git\\3d-format-shootout\\data\\big\\ply",
                    DefaultExt = ".ply",
                    Filter = "PLY Files (*.ply)|*.ply|All Files (*.*)|*.*",
                    Title = "Open PLY File"
                };
            }
            if (PlyOpenFileDialog.ShowDialog() != DialogResult.OK)
                return;
            var plyFile = PlyOpenFileDialog.FileName;
            var mesh = PlyImporter.LoadMesh(plyFile);
            Mesh = mesh.ToRenderMesh(Colors.BlueViolet);
        }
    }
}
