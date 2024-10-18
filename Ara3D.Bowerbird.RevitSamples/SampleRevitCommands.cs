using System;
using System.Collections;
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
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Plato.DoublePrecision;
using Plato.Geometry;
using Plato.Geometry.Graphics;
using Plato.Geometry.Memory;
using Plato.Geometry.Scenes;
using FilePath = Ara3D.Utils.FilePath;
using View = Autodesk.Revit.DB.View;
using XYZ = Autodesk.Revit.DB.XYZ;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class AutoRun : IBowerbirdCommand
    {
        public string Name => "AutoRun";

        public void Execute(object arg)
        {
            // TODO: uncomment the following code, if you want to automatically open one of the sample files, and go to the default 3D view. 
            /*
            var app = (UIApplication)arg;
            var path = new FilePath(Application.ExecutablePath);
            var sample = path.GetDirectory().RelativeFile("Samples", "rac_advanced_sample_project.rvt");
            if (!sample.Exists())
                return;
            app.OpenAndActivateDocument(sample);
            var uiDoc = app.ActiveUIDocument;
            var view = GetDefault3DView(uiDoc);
            if (view == null)
                MessageBox.Show("No 3D view found");
            else
                uiDoc.ActiveView = view;
            */
        }

        public static View3D GetDefault3DView(UIDocument uiDoc)
        {
            var doc = uiDoc.Document;

            // Retrieve all 3D views that are not templates
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .Where(v => !v.IsTemplate).ToList();

            var default3DView = collector.FirstOrDefault(v => v.Name.Equals("{3D}", StringComparison.InvariantCultureIgnoreCase));

            // If not found by name, return the first available non-template 3D view
            if (default3DView == null)
            {
                default3DView = collector.FirstOrDefault();
            }

            return default3DView;
        }
    }

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

    public class DataTableForm : System.Windows.Forms.Form
    {
        public DataGridView DataGridView;
        public DataTableBuilder Builder;

        public DataTableForm(DataTableBuilder builder)
        {
            Builder = builder;
            DataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = builder.DataTable,
                ReadOnly = true,
            };
            foreach (DataGridViewColumn col in DataGridView.Columns)
            {
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, DataGridView, new object[] { true });
            Controls.Add(DataGridView);
        }

        public void AddItemsToDataTable(IEnumerable items)
        {
            BeginInvoke(new Action(() => Builder.AddRows(items)));
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

            var builder = new DataTableBuilder(typeof(RoomData));
            builder.AddRows(GetAllRoomData());
            var form = new DataTableForm(builder);
            form.Show();

            //var text = string.Join("\r\n", GetAllRoomData());
            //TextDisplayForm.DisplayText(text);
        }

        public IEnumerable<RoomData> GetAllRoomData()
            => Rooms.Select(GetRoomData);

        public static int GetCount(Dictionary<int, List<FamilyInstance>> dict, Room r)
        {
            return dict.TryGetValue(r.Id.IntegerValue, out var list) ? list.Count : 0;
        }

        public class RoomData
        {
            public string Name;
            public int Id;
            public int Lights;
            public int Doors;
            public int Sockets;
            public int Windows;
            public int Walls;
            public double LimitOffset;
            public double Area;
            public double BoundingArea;
            public double BoundingVolume;
            public double BoundingHeight;
            public double AreaToBoundingArea;
            public double MaxSide;
            public double MinSide;
            public double Ratio;
            public double Volume;
            public double Elevation;
            public double Perimeter;
            public string LevelName;
            public double UnboundedHeight;
            public double UpperLevelElevation;
            public double BaseOffset;

            public override string ToString()
            {
                var sb = new StringBuilder();
                foreach (var fi in typeof(RoomData).GetFields())
                {
                    var val = fi.GetValue(this);
                    if (val is double d)
                        val = d.ToString("0.##");
                    sb.Append($"{fi.Name} = {val}; ");
                }

                return sb.ToString();
            }
        }

        public RoomData GetRoomData(Room r)
        {
            var walls = r.GetBoundaryWalls().ToList();
            var numWalls = walls.Count;
            var rd = new RoomData()
            {
                Name = r.Name ?? "",
                Id = r.Id.IntegerValue,
                Lights = GetCount(Lights, r),
                Doors = GetCount(Doors, r),
                Sockets = GetCount(Sockets, r),
                Windows = GetCount(Windows, r),
                Walls = numWalls,
                Elevation = r.Level?.Elevation ?? 0,
                Area = r.Area,
                Volume = r.Volume,
                Perimeter = r.Perimeter,
                LevelName = r.Level?.Name ?? "",
                UnboundedHeight = r.UnboundedHeight,
                UpperLevelElevation = r.UpperLimit?.Elevation ?? 0,
                BaseOffset = r.BaseOffset,
                LimitOffset = r.LimitOffset,
            };
            try
            {
                var bb = r.get_BoundingBox(null);
                if (bb != null)
                {
                    var extent = bb.Max - bb.Min;
                    extent = new XYZ(Math.Abs(extent.X), Math.Abs(extent.Y), Math.Abs(extent.Z));
                    rd.BoundingArea = extent.X * extent.Y;
                    rd.BoundingVolume = extent.X * extent.Y * extent.Z;
                    rd.BoundingHeight = extent.Z;
                    rd.AreaToBoundingArea = rd.Area / rd.BoundingArea;
                    rd.MinSide = Math.Min(extent.X, extent.Y);
                    rd.MaxSide = Math.Max(extent.X, extent.Y);
                    rd.Ratio = rd.MinSide / rd.MaxSide;
                }
            }
            catch
            {
                // DO Nothing.
            }

            foreach (var wall in walls)
            {
                foreach (var hosted in wall.GetHostedElements())
                {
                    if (hosted.IsCategoryType(BuiltInCategory.OST_LightingFixtures))
                        rd.Lights++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_Doors))
                        rd.Doors++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_ElectricalFixtures))
                        rd.Sockets++;
                    else if (hosted.IsCategoryType(BuiltInCategory.OST_Windows))
                        rd.Windows++;
                }
            }

            return rd;
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
                    else if (val is Parameter p)
                    {
                        val = p.AsValueString();
                    }

                    sb.AppendLine($"{indent + "  "}\"{m.Name}\": \"{val}\"");
                }
            }

            sb.AppendLine($"{indent}}}");
            return sb;
        }   
    }

    // This class creates JSON files representing the room boundaries, openings, and doors.
    // It uses the background processor to do work. 
    public class GeoJsonExporter : IBowerbirdCommand 
    {
        public string Name => "Export rooms as GeoJSON";

        public Document Document;
        public BackgroundUI Background;
        public Dictionary<int, List<Opening>> OpeningGroups;
        public Dictionary<int, List<FamilyInstance>> DoorGroups;
        
        public static readonly DirectoryPath OutputFolder = 
            SpecialFolders.LocalApplicationData.RelativeFolder("Temp", "Bowerbird");

        public void Execute(object arg)
        {
            var uiapp = (arg as UIApplication);
            Document = uiapp.ActiveUIDocument.Document;

            var rooms = Document.GetRooms().ToList();
            OpeningGroups = Document.GroupOpeningsByHost();
            DoorGroups = Document.GroupDoorsByHost();
            OutputFolder.CreateAndClearDirectory();
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

            var geoJson = ToGeoJson(room, openings, doors);
            var json = JsonConvert.SerializeObject(geoJson, Formatting.Indented);

            var f = OutputFolder.RelativeFile($"room-{room.Name.ToValidFileName()}-{room.Id.IntegerValue}.json");
            f.WriteAllText(json);

            // Add an artificial delay, otherwise the demo is finished too fast.  
            Thread.Sleep(250);
        }

        public GeoJson ToGeoJson(Room room, IEnumerable<Opening> openings, IEnumerable<FamilyInstance> doors)
        {
            var r = new GeoJson
            {
                PayloadGeoJson = new GeoPayload
                {
                    Coordinates = new List<List<List<double>>>()
                },
                Name = room?.Name ?? "_no_room_"
            };

            if (room != null)
            {
                var tmp = room.GetRoomBoundaryCoordinates();
                foreach (var loop in tmp)
                {
                    var loopCoords = new List<List<double>>();
                    foreach (var c in loop)
                    {
                        loopCoords.Add(new List<double> { c.X, c.Y, c.Z });
                    }

                    r.PayloadGeoJson.Coordinates.Add(loopCoords);
                }
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

    // This class creates JSON files representing the room boundaries, openings, and doors.
    // It uses the background processor to do work. 
    public class GeoJsonImporter : IBowerbirdCommand, IDirectContext3DServer
    {
        public class RoomData
        {
            public Vector3D Center;
            public List<Vector3D> DoorCenters = new List<Vector3D>();
        }

        public string Name => "Import GeoJSON rooms";

        public Document Document;
        public BackgroundUI Background;

        public List<RoomData> Rooms = new List<RoomData>();
        public DirectoryPath InputFolder = GeoJsonExporter.OutputFolder;

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

            var files = InputFolder.GetFiles("*.json").ToList();
            if (!files.Any())
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
                    var doorCenter = pts.Aggregate((a, b) => a + b) / pts.Count;
                    roomData.DoorCenters.Add(doorCenter);
                }
            }
                
            UpdateMesh();
            
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
                .ToTriangleMesh()
                .Faceted();

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
                        var rot = dir.LookRotation;

                        arrows.AddMesh(arrowMesh, new TRSTransform(new Transform3D(p1, rot, (1, 1, len))));
                    }
                }
            }

            Mesh = ToRenderMesh(scene);

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

        //==

        public static RenderMesh ToRenderMesh(Scene scene)
        {
            var vertices = new List<RenderVertex>();
            var indices = new List<Integer>();

            foreach (var node in GetDescendants(scene.Root))
            {
                var t = node.Transform;

                foreach (var o in node.GetMeshObjects())
                {
                    var offset = vertices.Count;
                    var mesh = o.Mesh;
                    var m = o.Material;
                    var c = (Color32)m.Color;

                    var normals = mesh.ComputeVertexNormalsFaceted();

                    for (var i = 0; i < mesh.Points.Count; ++i)
                    {
                        var p = mesh.Points[i];
                        var n = normals[i];
                        var p1 = t.TransformPoint(p);
                        var n1 = t.TransformVector(n);
                        var rv = new RenderVertex(p1, n1, Vector2D.Default, c);
                        vertices.Add(rv);
                    }

                    foreach (var i in mesh.Indices)
                    {
                        indices.Add(i + offset);
                    }
                }
            }

            return new RenderMesh(vertices.ToBuffer(), indices.ToBuffer());
        }

        public static IEnumerable<ISceneNode> GetDescendants(ISceneNode node)
            => node.Children.SelectMany(GetDescendants).Prepend(node);
    }
}
