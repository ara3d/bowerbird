using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Bowerbird.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class DrawingServer : IBowerbirdCommand, IDirectContext3DServer
    {
        public string Name => "Drawing Server";

        public Guid Guid { get; } = Guid.NewGuid();
        public Outline m_boundingBox;

        public void Execute(object argument)
        {
            var app = (UIApplication)argument;

            //Set bounding box
            m_boundingBox = new Outline(new XYZ(0, 0, 0), new XYZ(m_CubeLength, m_CubeLength, m_CubeLength));

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

        public void RenderScene(View dBView, DisplayStyle displayStyle)
        {
            try
            {
                // Populate geometry buffers if they are not initialized or need updating.
                if (m_FaceBufferStorage == null || m_FaceBufferStorage.needsUpdate(displayStyle) ||
                    m_edgeBufferStorage == null || m_edgeBufferStorage.needsUpdate(displayStyle))
                {
                    CreateBufferStorageForMesh(displayStyle);
                }

                // Submit a subset of the geometry for drawing. Determine what geometry should be submitted based on
                // the type of the rendering pass (opaque or transparent) and DisplayStyle (wireframe or shaded).

                // If the server is requested to submit transparent geometry, DrawContext().IsTransparentPass()
                // will indicate that the current rendering pass is for transparent objects.

                // Conditionally submit triangle primitives (for non-wireframe views).
                if (displayStyle != DisplayStyle.Wireframe && m_FaceBufferStorage.PrimitiveCount > 0)
                    DrawContext.FlushBuffer(m_FaceBufferStorage.VertexBuffer,
                                            m_FaceBufferStorage.VertexBufferCount,
                                            m_FaceBufferStorage.IndexBuffer,
                                            m_FaceBufferStorage.IndexBufferCount,
                                            m_FaceBufferStorage.VertexFormat,
                                            m_FaceBufferStorage.EffectInstance, PrimitiveType.TriangleList, 0,
                                            m_FaceBufferStorage.PrimitiveCount);

                // Conditionally submit line segment primitives.
                if (m_edgeBufferStorage.PrimitiveCount > 0)
                    DrawContext.FlushBuffer(m_edgeBufferStorage.VertexBuffer,
                                            m_edgeBufferStorage.VertexBufferCount,
                                            m_edgeBufferStorage.IndexBuffer,
                                            m_edgeBufferStorage.IndexBufferCount,
                                            m_edgeBufferStorage.VertexFormat,
                                            m_edgeBufferStorage.EffectInstance, PrimitiveType.LineList, 0,
                                            m_edgeBufferStorage.PrimitiveCount);

            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
            }
        }

        private CustomBufferStorage m_FaceBufferStorage; //Not transparent
        private CustomBufferStorage m_edgeBufferStorage;
        private const double m_CubeLength = 20.0;
        private Random m_random = new Random();

        private void CreateBufferStorageForMesh(DisplayStyle displayStyle)
        {
            //Initialize the buffer storages
            m_FaceBufferStorage = new CustomBufferStorage(displayStyle);
            m_edgeBufferStorage = new CustomBufferStorage(displayStyle);

            //Read and get mesh info
            var customMesh = new MeshCube(m_CubeLength);

            //Populate face buffer
            m_FaceBufferStorage.VertexBufferCount += customMesh.VertexBufferCount;
            m_FaceBufferStorage.PrimitiveCount += customMesh.NumTriangles;

            //Set format bits
            m_FaceBufferStorage.FormatBits = VertexFormatBits.PositionNormalColored;

            //Set the buffer size -- The format of the vertices determines the size of the vertex buffer.
            var vertexBufferSizeInFloats = VertexPositionNormalColored.GetSizeInFloats() * m_FaceBufferStorage.VertexBufferCount;

            //Create Vertex Buffer and map so that the vertex data can be written into it
            m_FaceBufferStorage.VertexBuffer = new VertexBuffer(vertexBufferSizeInFloats);
            m_FaceBufferStorage.VertexBuffer.Map(vertexBufferSizeInFloats);

            try
            {
                //A VertexStream is used to write data into a VertexBuffer.
                var vertexStream = m_FaceBufferStorage.VertexBuffer.GetVertexStreamPositionNormalColored();

                var index = 0;
                foreach (var vertex in customMesh.Vertices)
                {
                    var normal = customMesh.Normals[index];

                    vertexStream.AddVertex(new VertexPositionNormalColored(vertex, normal,
                        new ColorWithTransparency((byte)m_random.Next(0, 255), (byte)m_random.Next(0, 255),
                            (byte)m_random.Next(0, 255), 0)));

                    index++;
                }
            }
            finally
            {
                //Unmap
                m_FaceBufferStorage.VertexBuffer.Unmap();
            }

            //Get the size of the index buffer
            m_FaceBufferStorage.IndexBufferCount = customMesh.NumTriangles * IndexTriangle.GetSizeInShortInts();
            var indexBufferSizeInShortInts = 1 * m_FaceBufferStorage.IndexBufferCount;

            //Create Index Buffer and map so that the vertex data can be written into it
            m_FaceBufferStorage.IndexBuffer = new IndexBuffer(indexBufferSizeInShortInts);
            m_FaceBufferStorage.IndexBuffer.Map(indexBufferSizeInShortInts);

            try
            {
                // An IndexStream is used to write data into an IndexBuffer.
                var indexStream = m_FaceBufferStorage.IndexBuffer.GetIndexStreamTriangle();

                foreach (var indexTri in customMesh.Triangles)
                {
                    indexStream.AddTriangle(new IndexTriangle(indexTri.a, indexTri.b, indexTri.c));
                }
            }
            finally
            {

                //Unmap the buffers so they can be used for rendering
                m_FaceBufferStorage.IndexBuffer.Unmap();
            }

            // VertexFormat is a specification of the data that is associated with a vertex (e.g., position)
            m_FaceBufferStorage.VertexFormat = new VertexFormat(m_FaceBufferStorage.FormatBits);

            // Effect instance is a specification of the appearance of geometry. For example, it may be used to specify color, if there is no color information provided with the vertices.
            m_FaceBufferStorage.EffectInstance = new EffectInstance(m_FaceBufferStorage.FormatBits);

            //https://bassemtodary.wordpress.com/2013/04/13/ambient-diffuse-specular-and-emissive-lighting/
            m_FaceBufferStorage.EffectInstance.SetColor(new Color(0, 255, 0));
            m_FaceBufferStorage.EffectInstance.SetEmissiveColor(new Color((byte)m_random.Next(0, 255), (byte)m_random.Next(0, 255), (byte)m_random.Next(0, 255)));
            m_FaceBufferStorage.EffectInstance.SetSpecularColor(new Color((byte)m_random.Next(0, 255), (byte)m_random.Next(0, 255), (byte)m_random.Next(0, 255)));
            m_FaceBufferStorage.EffectInstance.SetAmbientColor(new Color((byte)m_random.Next(0, 255), (byte)m_random.Next(0, 255), (byte)m_random.Next(0, 255)));



            //------------------------- Edges ------------

            //Populate edge buffer
            m_edgeBufferStorage.VertexBufferCount += customMesh.VertexBufferCount; //The same vertex buffer count
            m_edgeBufferStorage.PrimitiveCount += customMesh.DistinctEdgeCount;

            //Set format bits
            m_edgeBufferStorage.FormatBits = VertexFormatBits.Position;

            //Set the buffer size
            var edgeVertexBufferSizeInFloats = VertexPosition.GetSizeInFloats() * m_edgeBufferStorage.VertexBufferCount;

            //Create Vertex Buffer and map so that the vertex data can be written into it
            m_edgeBufferStorage.VertexBuffer = new VertexBuffer(edgeVertexBufferSizeInFloats);
            m_edgeBufferStorage.VertexBuffer.Map(edgeVertexBufferSizeInFloats);

            var vertexStreamEdge = m_edgeBufferStorage.VertexBuffer.GetVertexStreamPosition();
            foreach (var vertex in customMesh.Vertices)
            {
                vertexStreamEdge.AddVertex(new VertexPosition(vertex));
            }
            //Unmap the buffers so they can be used for rendering
            m_edgeBufferStorage.VertexBuffer.Unmap();

            //Get the size of the index buffer
            m_edgeBufferStorage.IndexBufferCount = m_edgeBufferStorage.PrimitiveCount * IndexLine.GetSizeInShortInts();
            var indexEdgeBufferSizeInShortInts = 1 * m_edgeBufferStorage.IndexBufferCount;

            m_edgeBufferStorage.IndexBuffer = new IndexBuffer(indexEdgeBufferSizeInShortInts);
            m_edgeBufferStorage.IndexBuffer.Map(indexEdgeBufferSizeInShortInts);

            var indexStreamEdge = m_edgeBufferStorage.IndexBuffer.GetIndexStreamLine();

            foreach (var edge in customMesh.DistinctEdges)
            {
                // Add two indices that define a line segment.
                indexStreamEdge.AddLine(new IndexLine(edge.a, edge.b)); //A and be points to the vertices of the edge
            }

            //Unmap the buffer so they can be used for rendering
            m_edgeBufferStorage.IndexBuffer.Unmap();

            // VertexFormat and Effect Instance
            m_edgeBufferStorage.VertexFormat = new VertexFormat(m_edgeBufferStorage.FormatBits);
            m_edgeBufferStorage.EffectInstance = new EffectInstance(m_edgeBufferStorage.FormatBits);
        }
    }

    public class MeshCube
    {
        // https://github.com/varolomer/DirectContext3DAPI/blob/master/DirectContext3DAPI/Assets/SS/MeshCube.png
        public int VertexBufferCount;
        public int NumTriangles;
        public int EdgeCount;
        public int DistinctEdgeCount;

        public List<XYZ> Vertices;
        public List<XYZ> Normals;
        public List<Index3d> Triangles;
        public List<Index2d> Edges;
        public List<Index2d> DistinctEdges;

        public MeshCube(double a) //A is cube length
        {
            Vertices = new List<XYZ>()
            {
                //Surface 1
                new XYZ(0, 0, 0), //0
                new XYZ(a, 0, 0), //1
                new XYZ(0, 0, a), //2
                new XYZ(a, 0, a), //3

                //Surface 2
                new XYZ(0, 0, 0), //4
                new XYZ(0, a, 0), //5
                new XYZ(0, 0, a), //6
                new XYZ(0, a, a), //7

                //Surface 3
                new XYZ(a, 0, 0), //8
                new XYZ(a, a, 0), //9
                new XYZ(a, 0, a), //10
                new XYZ(a, a, a), //11

                //Surface 4
                new XYZ(0, a, 0), //12
                new XYZ(a, a, 0), //13
                new XYZ(0, a, a), //14
                new XYZ(a, a, a), //15

                //Surface a
                new XYZ(0, 0, 0), //16
                new XYZ(a, 0, 0), //17
                new XYZ(0, a, 0), //18
                new XYZ(a, a, 0), //19

                //Surface 6
                new XYZ(0, 0, a), //20
                new XYZ(0, a, a), //21
                new XYZ(a, 0, a), //22
                new XYZ(a, a, a), //23
            };

            Normals = new List<XYZ>()
            {
                //Surface 1 
                new XYZ(0, -1, 0), //0
                new XYZ(0, -1, 0), //1
                new XYZ(0, -1, 0), //2
                new XYZ(0, -1, 0), //3

                //Surface 2
                new XYZ(-1, 0, 0), //4
                new XYZ(-1, 0, 0), //5
                new XYZ(-1, 0, 0), //6
                new XYZ(-1, 0, 0), //7

                //Surface 3
                new XYZ(0, 1, 0), //8
                new XYZ(0, 1, 0), //9
                new XYZ(0, 1, 0), //10
                new XYZ(0, 1, 0), //11

                //Surface 4
                new XYZ(0, 1, 0), //12
                new XYZ(0, 1, 0), //13
                new XYZ(0, 1, 0), //14
                new XYZ(0, 1, 0), //15

                //Surface 5
                new XYZ(0, 0, -1), //16
                new XYZ(0, 0, -1), //17
                new XYZ(0, 0, -1), //18
                new XYZ(0, 0, -1), //19

                //Surface 6
                new XYZ(0, 0, 1), //20
                new XYZ(0, 0, 1), //21
                new XYZ(0, 0, 1), //22
                new XYZ(0, 0, 1), //23
            };

            Triangles = new List<Index3d>()
            {
                //Surface 1
                new Index3d(0, 1, 2),
                new Index3d(3, 1, 2),

                //Surface 2
                new Index3d(4, 5, 6),
                new Index3d(7, 5, 6),

                //Surface 3
                new Index3d(8, 9, 10),
                new Index3d(11, 9, 10),

                //Surface 4
                new Index3d(12, 13, 14),
                new Index3d(15, 13, 14),

                //Surface 5
                new Index3d(16, 17, 18),
                new Index3d(19, 17, 18),

                //Surface 6
                new Index3d(20, 21, 22),
                new Index3d(23, 21, 22),

            };

            Edges = new List<Index2d>();

            foreach (var vertex1 in Vertices)
            {
                foreach (var vertex2 in Vertices)
                {
                    if (vertex1.DistanceTo(vertex2) == a)
                    {
                        var index1 = Vertices.IndexOf(vertex1);
                        var index2 = Vertices.IndexOf(vertex2);

                        Edges.Add(new Index2d(index1, index2));
                    }
                }
            }

            DistinctEdges = Edges.Distinct().ToList();

            VertexBufferCount = Vertices.Count();
            NumTriangles = Triangles.Count();
            EdgeCount = Edges.Count();
            DistinctEdgeCount = Edges.Count();
        }
    }

    public struct Index2d : IEquatable<Index2d>
    {
        public int a;
        public int b;

        public Index2d(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

        public bool Equals(Index2d other)
        {
            return (this.a + this.b) == (other.a + other.b);
        }
    }

    public struct Index3d
    {
        public int a;
        public int b;
        public int c;

        public Index3d(int a, int b, int c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    public class CustomBufferStorage
    {
        public DisplayStyle DisplayStyle { get; set; }

        //Counts
        public int PrimitiveCount { get; set; }
        public int VertexBufferCount { get; set; }
        public int IndexBufferCount { get; set; }

        //Buffers
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }

        //Formatting -- See the image: https://github.com/varolomer/DirectContext3DAPI/blob/master/DirectContext3DAPI/Assets/SS/VertexFormat.PNG
        public VertexFormatBits
            FormatBits
        {
            get;
            set;
        } //Formatting is used in the creation of both of the VertexFormat and EffectInstance objects

        public VertexFormat VertexFormat { get; set; }
        public EffectInstance EffectInstance { get; set; }


        public CustomBufferStorage(DisplayStyle displayStyle)
        {
            DisplayStyle = displayStyle;
        }

        /// <summary>
        /// If the user changes the display style (i.e. from hidden line to shaded) the graphics
        /// is needed to be re-rendered. The same applies if the low-level vertex buffer loses validity
        /// or if it gets null.
        /// </summary>
        /// <param name="newDisplayStyle"></param>
        /// <returns></returns>
        public bool needsUpdate(DisplayStyle newDisplayStyle)
        {
            if (newDisplayStyle != DisplayStyle)
                return true;

            if (PrimitiveCount > 0)
                if (VertexBuffer == null || !VertexBuffer.IsValid() ||
                    IndexBuffer == null || !IndexBuffer.IsValid() ||
                    VertexFormat == null || !VertexFormat.IsValid() ||
                    EffectInstance == null || !EffectInstance.IsValid())
                    return true;

            return false;
        }

        // A class that brings together all the data and rendering parameters that are needed to draw one sequence
        // of primitives (e.g., triangles) with the same format and appearance.
        class RenderingPassBufferStorage
        {
            public DisplayStyle DisplayStyle { get; set; }

            //Geometry Info
            public List<MeshInfo> Meshes { get; set; }
            public List<IList<XYZ>> EdgeXYZs { get; set; }

            //Counts
            public int PrimitiveCount { get; set; }
            public int VertexBufferCount { get; set; }
            public int IndexBufferCount { get; set; }

            //Buffers
            public VertexBuffer VertexBuffer { get; set; }
            public IndexBuffer IndexBuffer { get; set; }

            //Formatting -- See the image: https://github.com/varolomer/DirectContext3DAPI/blob/master/DirectContext3DAPI/Assets/SS/VertexFormat.PNG
            public VertexFormatBits
                FormatBits
            {
                get;
                set;
            } //Formatting is used in the creation of both of the VertexFormat and EffectInstance objects

            public VertexFormat VertexFormat { get; set; }
            public EffectInstance EffectInstance { get; set; }


            //VertexFormatBits is not to be confused with VertexFormat. The latter type of object is
            //associated with low-level graphics functionality and may become invalid. VertexFormat is
            //needed to submit a set of vertex and index buffers for rendering (see Autodesk::Revit::DB::DirectContext3D::DrawContext).


            public RenderingPassBufferStorage(DisplayStyle displayStyle)
            {
                DisplayStyle = displayStyle;
                Meshes = new List<MeshInfo>();
                EdgeXYZs = new List<IList<XYZ>>();
            }

            /// <summary>
            /// If the user changes the display style (i.e. from hidden line to shaded) the graphics
            /// is needed to be re-rendered. The same applies if the low-level vertex buffer loses validity
            /// or if it gets null.
            /// </summary>
            /// <param name="newDisplayStyle"></param>
            /// <returns></returns>
            public bool needsUpdate(DisplayStyle newDisplayStyle)
            {
                if (newDisplayStyle != DisplayStyle)
                    return true;

                if (PrimitiveCount > 0)
                    if (VertexBuffer == null || !VertexBuffer.IsValid() ||
                        IndexBuffer == null || !IndexBuffer.IsValid() ||
                        VertexFormat == null || !VertexFormat.IsValid() ||
                        EffectInstance == null || !EffectInstance.IsValid())
                        return true;

                return false;
            }

        }
    }

    class MeshInfo
    {
        public Mesh Mesh;
        public XYZ Normal;
        public ColorWithTransparency ColorWithTransparency;

        public MeshInfo(Mesh mesh, XYZ normal, ColorWithTransparency color)
        {
            Mesh = mesh;
            Normal = normal;
            ColorWithTransparency = color;
        }
    }
}
