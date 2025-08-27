using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Form = System.Windows.Forms.Form;
using Outline = Autodesk.Revit.DB.Outline;
using View = Autodesk.Revit.DB.View;

namespace Ara3D.Bowerbird.RevitSamples;

public class Colorizer : NamedCommand
{
    public override string Name => "Colorizer";
    
    private static StairsDc3dServer _server;
    private static UIApplication _uiapp;

    public override void Execute(object arg)
    {
        _uiapp = (UIApplication)arg;
        var service = ExternalServiceRegistry.GetService(
            ExternalServices.BuiltInExternalServices.DirectContext3DService) as MultiServerService;
        EnsureServerRegistered(service);
        ActivateServer(service);
        _uiapp.ActiveUIDocument?.RefreshActiveView();
        UiHelpers.ShowOneButtonWindow("Stop Highlighting", Shutdown);
    }

    public static void EnsureServerRegistered(MultiServerService service)
    {
        if (_server != null) return;
        _server = new StairsDc3dServer();
        var already = service.GetRegisteredServerIds().Any(id => id == _server.GetServerId());
        if (!already)
            service.AddServer(_server);
    }

    public static void ActivateServer(MultiServerService service)
    {
        var active = service.GetActiveServerIds().ToList();
        if (!active.Contains(_server.GetServerId()))
        {
            active.Add(_server.GetServerId());
            service.SetActiveServers(active);
        }
    }
    public static void Shutdown()
    {
        // Optional: remove from active list
        var service = ExternalServiceRegistry.GetService(
            ExternalServices.BuiltInExternalServices.DirectContext3DService) as MultiServerService;

        var active = service.GetActiveServerIds().ToList();
        active.Remove(_server.GetServerId());
        service.SetActiveServers(active);
        _uiapp.ActiveUIDocument?.RefreshActiveView();
    }
}

public static class UiHelpers
{
    public static void ShowOneButtonWindow(string label, Action onClosed)
    {
        var form = new Form
        {
            Text = label,
            StartPosition = FormStartPosition.CenterScreen,
            MinimizeBox = false,
            MaximizeBox = false,
            FormBorderStyle = FormBorderStyle.FixedDialog
        };

        var btn = new Button
        {
            Text = label,
            Dock = DockStyle.Fill
        };

        btn.Click += (_, __) => form.Close();

        bool fired = false;
        form.FormClosed += (_, __) =>
        {
            if (fired) return;
            fired = true;
            try { onClosed?.Invoke(); } catch { /* swallow callback errors */ }
            form.Dispose();
        };

        form.Controls.Add(btn);
        form.Show(); // non-blocking; use form.ShowDialog() if you prefer modal
    }
}

public class StairsDc3dServer : IDirectContext3DServer
{
    private readonly Guid _id = new Guid("7966705D-19AC-49F4-8BCC-02624D25AC12");

    public Guid GetServerId() => _id;
    public string GetName() => "Stairs Highlighter (DC3D)";
    public string GetDescription() => "Draws translucent purple quads over stairs in floor plan views.";
    public string GetVendorId() => "Ara 3D";
    public string GetSourceId() => "";       // recommended to keep empty for third-party servers
    public string GetApplicationId() => "";  // recommended to keep empty for third-party servers
    public ExternalServiceId GetServiceId() => ExternalServices.BuiltInExternalServices.DirectContext3DService;
    public bool UsesHandles() => false;      // classic third-party usage – no handle elements. :contentReference[oaicite:3]{index=3}

    public bool CanExecute(View view)
    {
        var plan = view as ViewPlan;
        return plan != null && plan.ViewType == ViewType.FloorPlan;
    }

    public bool UseInTransparentPass(View view) => true; // we draw translucent geometry

    public Outline GetBoundingBox(View view)
    {
        // Coarse, view-space bounding box of all stairs in this plan
        var plan = view as ViewPlan;
        if (plan == null) return null;

        var stairs = new FilteredElementCollector(view.Document, view.Id)
            .OfCategory(BuiltInCategory.OST_Stairs)
            .WhereElementIsNotElementType()
            .ToElements();

        bool any = false;
        XYZ min = null, max = null;

        foreach (var e in stairs)
        {
            var bb = e.get_BoundingBox(plan);
            if (bb == null) continue;

            // Transform min/max to model coords in case bbox is oriented.
            var tf = bb.Transform ?? Transform.Identity;
            var corners = new[]
            {
                    tf.OfPoint(new XYZ(bb.Min.X, bb.Min.Y, bb.Min.Z)),
                    tf.OfPoint(new XYZ(bb.Max.X, bb.Max.Y, bb.Max.Z))
                };
            var bmin = new XYZ(Math.Min(corners[0].X, corners[1].X),
                               Math.Min(corners[0].Y, corners[1].Y),
                               Math.Min(corners[0].Z, corners[1].Z));
            var bmax = new XYZ(Math.Max(corners[0].X, corners[1].X),
                               Math.Max(corners[0].Y, corners[1].Y),
                               Math.Max(corners[0].Z, corners[1].Z));

            if (!any) { min = bmin; max = bmax; any = true; }
            else
            {
                min = new XYZ(Math.Min(min.X, bmin.X), Math.Min(min.Y, bmin.Y), Math.Min(min.Z, bmin.Z));
                max = new XYZ(Math.Max(max.X, bmax.X), Math.Max(max.Y, bmax.Y), Math.Max(max.Z, bmax.Z));
            }
        }

        return any ? new Outline(min, max) : null;
    }

    public void RenderScene(View view, DisplayStyle displayStyle)
    {
        if (!DrawContext.IsTransparentPass()) return; // only draw once in the transparent pass. :contentReference[oaicite:4]{index=4}

        var plan = view as ViewPlan;
        if (plan == null) return;

        var doc = plan.Document;

        // Collect stairs visible in this view
        var stairs = new FilteredElementCollector(doc, plan.Id)
            .OfCategory(BuiltInCategory.OST_Stairs)
            .WhereElementIsNotElementType()
            .ToElements();

        // Build quads from each stair's view bbox
        var quads = new List<XYZ[]>(capacity: stairs.Count);

        foreach (var e in stairs)
        {
            var bb = e.get_BoundingBox(plan);
            if (bb == null) continue;

            var tf = bb.Transform ?? Transform.Identity;

            // Lift slightly toward the camera to avoid z-fighting.
            // Plan view looks down -Z; lift +0.10' above the top of bbox.
            double z = bb.Max.Z + 0.10;

            var p0 = tf.OfPoint(new XYZ(bb.Min.X, bb.Min.Y, z));
            var p1 = tf.OfPoint(new XYZ(bb.Max.X, bb.Min.Y, z));
            var p2 = tf.OfPoint(new XYZ(bb.Max.X, bb.Max.Y, z));
            var p3 = tf.OfPoint(new XYZ(bb.Min.X, bb.Max.Y, z));

            quads.Add(new[] { p0, p1, p2, p3 });
        }

        if (quads.Count == 0) return;

        // Each quad = 4 vertices, 2 triangles = 6 indices (shorts)
        int vertexCount = quads.Count * 4;
        int triCount = quads.Count * 2;
        int indexCount = triCount * 3;

        // Allocate buffers
        var vfBits = VertexFormatBits.PositionColored; // per-vertex color with alpha. :contentReference[oaicite:5]{index=5}
        using var vformat = new VertexFormat(vfBits);
        using var vbuf = new VertexBuffer(vertexCount * VertexPositionColored.GetSizeInFloats()); // floats. :contentReference[oaicite:6]{index=6}
        using var ibuf = new IndexBuffer(indexCount * IndexTriangle.GetSizeInShortInts());        // shorts. :contentReference[oaicite:7]{index=7}
        using var effect = new EffectInstance(vfBits);
        // We rely on per-vertex alpha + transparent pass, no global transparency needed.
        // effect.SetTransparency(0.0); // optional; would multiply with vertex alpha. :contentReference[oaicite:8]{index=8}

        // Map & write vertices
        vbuf.Map(vertexCount * VertexPositionColored.GetSizeInFloats());
        var vstream = vbuf.GetVertexStreamPositionColored();

        var purple = new ColorWithTransparency(128, 0, 128, 160); // RGBA (alpha 160/255 ~ 0.63) :contentReference[oaicite:9]{index=9}
        foreach (var q in quads)
        {
            vstream.AddVertex(new VertexPositionColored(q[0], purple));
            vstream.AddVertex(new VertexPositionColored(q[1], purple));
            vstream.AddVertex(new VertexPositionColored(q[2], purple));
            vstream.AddVertex(new VertexPositionColored(q[3], purple));
        }
        vbuf.Unmap();

        // Map & write indices
        ibuf.Map(indexCount * 1); // size in shorts; each triangle index is a short. :contentReference[oaicite:10]{index=10}
        var istream = ibuf.GetIndexStreamTriangle();

        for (short i = 0; i < quads.Count; ++i)
        {
            short baseV = (short)(i * 4);
            // two triangles: (0,1,2) and (0,2,3)
            istream.AddTriangle(new IndexTriangle(baseV, (short)(baseV + 1), (short)(baseV + 2)));
            istream.AddTriangle(new IndexTriangle(baseV, (short)(baseV + 2), (short)(baseV + 3)));
        }
        ibuf.Unmap();

        // Submit
        DrawContext.FlushBuffer(vbuf, vertexCount, ibuf, indexCount, vformat, effect, PrimitiveType.TriangleList, 0, triCount);
    }
}