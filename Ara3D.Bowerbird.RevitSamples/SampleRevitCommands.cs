using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Logging;
using Ara3D.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using FilePath = Ara3D.Utils.FilePath;

namespace Ara3D.Bowerbird.RevitSamples
{
    /// <summary>
    /// Displays the current active document in a window
    /// </summary>
    public class CurrentDocument : IBowerbirdCommand
    {
        public string Name => "Current Open Document";

        public void Execute(object arg)
        {
            var app = (UIApplication)arg;
            var doc = app.ActiveUIDocument?.Document;
            if (doc == null)
            {
                MessageBox.Show("No document open");
            }
            else
            {
                MessageBox.Show($"Open document: {doc.PathName}");
            }
        }
    }

    /// <summary>
    /// Save the current view as a PNG in the temp folder, and then opens
    /// it using the default registered application. 
    /// </summary>
    public class SaveCurrentViewAsPng : IBowerbirdCommand
    {
        public string Name => "Save to PNG";

        public void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            if (doc == null) return;
            var output = PathUtil.CreateTempFile().ChangeExtension("png");
            ExportCurrentViewToPng(doc, output);
            output.OpenDefaultProcess();
        }

        public static FilePath ExportCurrentViewToPng(Document doc, FilePath filePath)
        {
            var img = new ImageExportOptions();
            img.ZoomType = ZoomFitType.FitToPage;
            img.PixelSize = 1024;
            img.ImageResolution = ImageResolution.DPI_600;
            img.FitDirection = FitDirectionType.Horizontal;
            img.ExportRange = ExportRange.CurrentView;
            img.HLRandWFViewsFileType = ImageFileType.PNG;
            img.FilePath = filePath;
            img.ShadowViewsFileType = ImageFileType.PNG;
            doc.ExportImage(img);
            return filePath;
        }
    }

    public class FamilyInstanceData : IBowerbirdCommand
    {
        public string Name => "Family Instances";

        public void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            var instances = doc.GetFamilyInstances();
            var grps = instances.GroupBy(fi => fi.Symbol.Category.Name).OrderBy(g => g.Key);
            var text = string.Join("\r\n", grps.Select(g => $"{g.Key} = {g.Count()}"));
            TextDisplayForm.DisplayText(text);
        }
    }

    public class ElectricalFixtures : IBowerbirdCommand
    {
        public string Name => "Electrical Fixtures";

        public void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            var sockets = doc.GetSockets();
            var text = string.Join("\r\n", sockets.Select(s => s.GetRoomId().ToString()));
            TextDisplayForm.DisplayText(text);
        }
    }

    public class ListRooms : IBowerbirdCommand
    {
        public string Name => "List Rooms";

        public List<Room> Rooms;
        public Dictionary<int, List<FamilyInstance>> Lights;
        public Dictionary<int, List<FamilyInstance>> Doors;
        public Dictionary<int, List<FamilyInstance>> Sockets;
        public Dictionary<int, List<FamilyInstance>> Windows;

        public void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            if (doc == null) return;
            Rooms = doc.GetRooms().ToList();
            Lights = doc.GetLights().GroupByRoom();
            Doors = doc.GetDoors().GroupByRoom();
            Sockets = doc.GetSockets().GroupByRoom();
            Windows = doc.GetWindows().GroupByRoom();
            var text = string.Join("\r\n", Rooms.Select(RoomToString));
            TextDisplayForm.DisplayText(text);
        }

        public static int GetCount(Dictionary<int, List<FamilyInstance>> dict, Room r)
        {
            return dict.TryGetValue(r.Id.IntegerValue, out var list) ? list.Count : 0;
        }

        public string RoomToString(Room r)
        {
            var numLights = GetCount(Lights, r);
            var numDoors = GetCount(Doors, r);
            var numSockets = GetCount(Sockets, r);
            var numWindows = GetCount(Windows, r);
            
            var walls = r.GetWalls().ToList();
            var numWalls = walls.Count;

            foreach (var wall in walls)
            {
                foreach (var hosted in wall.GetHostedElements())
                {
                    if (hosted.IsCategoryType(BuiltInCategory.OST_LightingFixtures))
                        numLights++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_Doors))
                        numDoors++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_ElectricalFixtures))
                        numSockets++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_Windows))
                        numWindows++;
                }
            }

            var elevation = r.Level?.Elevation ?? 0;

            return $"Name = {r.Name}, " +
                   $"Level = {r.Level?.Name}, " +
                   $"Elevation = {elevation:0.##}, " +
                   $"Perimeter = {r.Perimeter:0.##}, " +
                   $"Area = {r.Area:0.##}, " +
                   $"Volume = {r.Volume:0.##}, " +
                   $"Height = {r.UnboundedHeight:0.##}, " +
                   $"Doors = {numDoors}, " +
                   $"Windows = {numWindows}, " +
                   $"Walls = {numWalls}, " +
                   $"Lights = {numLights}, " +
                   $"Sockets = {numSockets}";
        }
    }

    public class ExternalEventExample : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            TaskDialog.Show("External Event", "Click Close to close.");
        }

        public string GetName()
        {
            return "External Event Example";
        }
    }

    public class ExternalEventDemo : IBowerbirdCommand
    {
        public string Name => "External event";

        public void Execute(object arg)
        {
            var handler = new ExternalEventExample(); 
            var ev = ExternalEvent.Create(handler);
            ev.Raise();
        }
    }

    public class SelectedElementsJson : IBowerbirdCommand
    {
        public string Name => "Selected elements JSON";

        public void Execute(object arg)
        {
            var uidoc = (arg as UIApplication)?.ActiveUIDocument;
            var doc = uidoc.Document;
            var sel = uidoc.Selection;
            var elements = sel.GetElementIds().Select(id => doc.GetElement(id));
            var text = ElementData.ToJson(elements).ToString();
            TextDisplayForm.DisplayText(text);
        }
    }

    public class IdlingDemo : IBowerbirdCommand
    {
        public TextDisplayForm Form;
        public ILogger Logger;
        public int MSecElapsed;
        public const int WORK_ITEM_MSEC = 100;
        public const int WORK_TOTAL_MSEC = 1000;
        public string Name => "Idling Demo";
        public Stopwatch Stopwatch = new Stopwatch();

        public void Log(string msg)
        {
            Logger.Log(msg);
        }

        public void Execute(object arg)
        {
            Form = new TextDisplayForm("");
            var uiApp = arg as UIApplication; ;
            uiApp.Idling += Application_Idling;
            Logger = Form.CreateLogger();
            Form.Show();
            Stopwatch.Start();
        }

        private void Application_Idling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {   
            var uiApp = sender as UIApplication;
            if (uiApp == null) return;

            // Simulate work 
            Thread.Sleep(WORK_ITEM_MSEC);
            MSecElapsed += WORK_ITEM_MSEC;
            if (MSecElapsed > WORK_TOTAL_MSEC)
            {
                var n1 = MSecElapsed / 1000f;
                Log($"{n1:##.00} seconds elapsed during idling");
                MSecElapsed = 0;
                var n2 = Stopwatch.ElapsedMilliseconds / 1000f;
                Log($"{n2:##.00} seconds elapsed in real-time");
                Stopwatch.Reset();
                Stopwatch.Start();
            }
            e.SetRaiseWithoutDelay();
        }
    }

    public class SelectedItemGeometry : IBowerbirdCommand
    {
        public string Name => "Selected Item Geometry";

        public static TextDisplayForm Form;
        public static ILogger Logger;

        public static void Log(string msg)
        {
            Logger.Log(msg);
        }

        public void Execute(object arg)
        {
            if (Form == null)
            {
                Form = new TextDisplayForm("");
                Form.FormClosing += (sender, args) =>
                {
                    args.Cancel = true;
                    Form.Hide();
                };
            }

            Logger = Form.CreateLogger();
            try
            {
                var uidoc = (arg as UIApplication)?.ActiveUIDocument; 
                var doc = uidoc.Document;
                var sel = uidoc.Selection;
                if (sel == null)
                {
                    Log($"No selection");
                    return;
                }
                var selId = sel.GetElementIds().FirstOrDefault();
                if (selId == null)
                {
                    Log($"No selection ID found");
                    return;
                }
                Log($"Found selection {selId}");

                var element = doc.GetElement(selId);
                if (element == null)
                {
                    Log($"No element found");
                    return;
                }
                
                Log($"Element found {element.Name}");
                var ge = element.get_Geometry(new Options()
                {
                    ComputeReferences = false, 
                    DetailLevel = ViewDetailLevel.Coarse,
                    IncludeNonVisibleObjects = false
                });
                
                Log($"Retrieved geometry element {ge.Id}");
                foreach (var go in ge)
                {
                    Log($"Geometry object {go.Id} is {go.GetType().Name}");
                    var expr = go.ToExpr();
                    Log($"Retrieved AST Expr");

                    Log($"Formatting AST");
                    expr = GeometryAbstractSyntaxTree.PrettyPrintAst(expr);
                    Log($"{expr}");
                }

                Log($"Completed");
            }
            catch (Exception e)
            {
                Log($"Exception occurred {e}");
            }
            Form.Show();
        }
    }

    public class TextDisplayForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox textBox;

        public TextDisplayForm(string text)
        {
            Text = "Multi-line Text";
            Size = new System.Drawing.Size(400, 300);

            textBox = new System.Windows.Forms.TextBox();
            textBox.Multiline = true;
            textBox.Dock = DockStyle.Fill;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Text = text;

            Controls.Add(textBox);
        }

        public void AddLine(string s)
            => textBox.AppendText(s + Environment.NewLine);

        public ILogger CreateLogger()
            => new Logger(LogWriter.Create(AddLine), "");

        public static TextDisplayForm DisplayText(IEnumerable<string> lines)
            => DisplayText(string.Join("\r\n", lines));

        public static TextDisplayForm DisplayText(string text)
        {
            var form = new TextDisplayForm(text);
            form.Show();
            return form;
        }

        public void SetText(string s)
            => textBox.Text = s;
    }

    public static class MiscExtensions
    {
        public static IReadOnlyList<int> GetIndexData(this TriangulatedShellComponent self)
        {
            var r = new List<int>();
            for (var i = 0; i < self.TriangleCount; i++)
            {
                var tri = self.GetTriangle(i);
                r.Add(tri.VertexIndex0);
                r.Add(tri.VertexIndex1);
                r.Add(tri.VertexIndex2);
            }
            return r;
        }

        public static IReadOnlyList<double> GetVertexData(this TriangulatedShellComponent self)
        {
            var r = new List<double>();
            for (var i = 0; i < self.VertexCount; i++)
            {
                var v = self.GetVertex(i);
                r.Add(v.X);
                r.Add(v.Y);
                r.Add(v.Z);
            }
            return r;
        }

        public static FilePath WriteToFileAsObj(this TriangulatedShellComponent self, FilePath filePath)
            => filePath.WriteObjFile(GetVertexData(self), GetIndexData(self));

        public static IReadOnlyList<TriangulatedShellComponent> TriangulatedComponents(
            this TriangulatedSolidOrShell solid)
            => Enumerable.Range(0, solid.ShellComponentCount).Select(solid.GetShellComponent).ToList();

        public static TriangulatedSolidOrShell Tessellate(this Solid solid)
        {
            var controls = new SolidOrShellTessellationControls();

            // https://www.revitapidocs.com/2020.1/720f75c5-8a11-bfc6-d698-a200ffc28be9.htm
            /*
            controls.MinAngleInTriangle = 0.01; // Max value is (Math.PI * 3)
            controls.MinExternalAngleBetweenTriangles = 0.2; // 
            controls.LevelOfDetail = 0.5; // 0 to 1 
            controls.Accuracy = 0.1;
            */

            /*
            // https://github.com/Autodesk/revit-ifc/blob/master/Source/Revit.IFC.Export/Utility/ExporterUtil.cs
            controls.Accuracy = 0.6;
            controls.LevelOfDetail = 0.1;
            controls.MinAngleInTriangle = 0.13;
                controls.MinExternalAngleBetweenTriangles = 1.2;
            */

            // https://github.com/Autodesk/revit-ifc/blob/master/Source/Revit.IFC.Export/Utility/ExporterUtil.cs
            controls.Accuracy = 0.5;
            controls.LevelOfDetail = 0.4;
            controls.MinAngleInTriangle = 0.13;
            controls.MinExternalAngleBetweenTriangles = 0.55;

            return SolidUtils.TessellateSolidOrShell(solid, controls);
        }
    }

    // https://people.computing.clemson.edu/~dhouse/courses/405/docs/brief-obj-file-format.html
    // https://en.wikipedia.org/wiki/Wavefront_.obj_file
    // https://www.fileformat.info/format/wavefrontobj/egff.htm
    // https://paulbourke.net/dataformats/obj/
    public static class ObjFileWriter
    {
        public static IEnumerable<int> Range(this int count)
            => Enumerable.Range(0, count);

        public static IEnumerable<string> GetVertexLines(IReadOnlyList<double> vertexData)
            => (vertexData.Count / 3).Range().Select(i => $"v {vertexData[i*3]} {vertexData[i*3+1]} {vertexData[i*3+2]}");

        public static IEnumerable<string> GetFaceLines(IReadOnlyList<int> indexData)
            => (indexData.Count / 3).Range().Select(i => $"f {indexData[i * 3]} {indexData[i * 3 + 1]} {indexData[i * 3 + 2]}");

        public static IEnumerable<string> GetObjLines(IReadOnlyList<double> vertexData, IReadOnlyList<int> indexData)
            => GetVertexLines(vertexData).Concat(GetFaceLines(indexData));

        public static FilePath WriteObjFile(this FilePath filePath, IReadOnlyList<double> vertexData, IReadOnlyList<int> indexData)
            => filePath.WriteAllLines(GetObjLines(vertexData, indexData));
    }

    public static class ElementData
    {
        public static StringBuilder ToJson(IEnumerable<Element> elements, StringBuilder sb = null, string indent = "")
        {
            if (sb == null)
                sb = new StringBuilder();
            sb.AppendLine($"{indent}[");
            var first = true;
            foreach (var e in elements)
            {
                if (!first)
                    sb.AppendLine($"{indent}, ");
                else
                    first = false;
                ToJson(e, sb, indent + "  ");
            }
            sb.AppendLine($"{indent}]");
            return sb;
        }

        public static string ParameterToString(Parameter parameter)
        {
            if (parameter == null)
                return null;

            var paramName = parameter.Definition.Name;
            
            if (!parameter.HasValue)
                return $"\"{paramName}\" : null";

            switch (parameter.StorageType)
            {
                case StorageType.Integer:
                    return $"\"{paramName}\" : {parameter.AsInteger()}";

                case StorageType.Double:
                    return $"\"{paramName}\" : {parameter.AsDouble()}";

                case StorageType.String:
                    return $"\"{paramName}\" : \"{parameter.AsString()}\"";

                case StorageType.ElementId:
                    return $"\"{paramName}\" : {parameter.AsElementId().IntegerValue}";
            }

            return $"\"{paramName}\" : {parameter.AsValueString()}";
        }


        public static StringBuilder ToJson(Element e, StringBuilder sb = null, string indent = "")
        {
            if (sb == null)
                sb = new StringBuilder();
            sb.AppendLine($"{indent}{{");
            foreach (var m in e.GetType().GetMembers())
            {
                object val;
                try
                {
                    if (m is PropertyInfo pi)
                    {
                        if (pi.GetIndexParameters().Length == 0)
                            val = pi.GetValue(e);
                        else 
                            continue;
                    }
                    else if (m is MethodInfo mi)
                    {
                        if (mi.GetParameters().Length == 0)
                            val = mi.Invoke(e, null);
                        else 
                            continue;
                    }
                    else if (m is FieldInfo fi)
                    {
                        val = fi.GetValue(e);
                    }
                    else
                    {
                        continue;
                    }
                }
                catch
                {
                    continue;
                }

                if (val is Element subElement)
                {
                    sb.AppendLine($"{indent + "  "}\"{m.Name}\":");
                    ToJson(subElement, sb, indent + "  ");
                }
                else if (val is IEnumerable<Element> subElements)
                {
                    sb.AppendLine($"{indent + "  "}\"{m.Name}\":");
                    ToJson(subElements, sb, indent + "  ");
                }
                else if (val is IEnumerable<ElementId> subElementIds)
                {
                    var ids = subElementIds.Select(eId => eId.IntegerValue);
                    sb.AppendLine($"{indent + "  "}\"{m.Name}\": [{ids.JoinStringsWithComma()}]");
                }
                else if (val is IEnumerable<Parameter> parameters)
                {
                    var paramStr = parameters.Select(ParameterToString).JoinStringsWithComma();
                    sb.AppendLine($"{indent + "  "}\"{m.Name}\": {{ {paramStr} }}");
                }
                else
                {
                    if (val is ElementId id)
                        val = id.IntegerValue;
                    else if (val is Parameter)
                    {

                    }

                    sb.AppendLine($"{indent + "  "}\"{m.Name}\": \"{val}\"");
                }
            }

            sb.AppendLine($"{indent}}}");
            return sb;
        }
    }
}
