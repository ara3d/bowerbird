using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ara3D.Graphics;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Plato.DoublePrecision;
using Plato.Geometry.Graphics;
using Plato.Geometry.Revit;
using PrimitiveType = Autodesk.Revit.DB.DirectContext3D.PrimitiveType;
using RenderMesh = Plato.Geometry.Graphics.RenderMesh;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class BufferStorage
    {
        public PrimitiveType PrimitiveType { get; }
        public int PrimitiveSize => 
            PrimitiveType == PrimitiveType.TriangleList ? 3 : 
            PrimitiveType == PrimitiveType.LineList ? 2 : 
            1;
        public int PrimitiveCount => IndexCount / PrimitiveSize;
        public VertexBuffer VertexBuffer { get; private set; }
        public int VertexCount { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }
        public int IndexCount { get; private set; }

        public static readonly VertexFormatBits FormatBits = VertexFormatBits.PositionNormalColored;
        public VertexFormat VertexFormat => new VertexFormat(FormatBits);
        public EffectInstance EffectInstance { get; } = new EffectInstance(FormatBits);

        public int VertexBufferSizeInFloat => VertexPositionNormalColored.GetSizeInFloats() * VertexCount;
        public int IndexBufferSizeInShort => IndexTriangle.GetSizeInShortInts() * IndexCount;

        public BufferStorage(RenderMesh renderMesh)
            : this(PrimitiveType.TriangleList, renderMesh.Vertices, renderMesh.Indices)
        { }

        public BufferStorage(PrimitiveType primitiveType, IReadOnlyList<RenderVertex> vertices, IReadOnlyList<Integer> indices)
        {
            PrimitiveType = primitiveType;
            Debug.WriteLine("Setting the vertex buffer");
            SetVertexBuffer(vertices);
            Debug.WriteLine("Setting the index buffer");
            if (primitiveType == PrimitiveType.TriangleList)
                SetIndexBuffer(indices);
            else if (primitiveType == PrimitiveType.LineList)
                SetIndexBuffer(TriangleIndicesToEdgeIndices(indices));
            else if (primitiveType == PrimitiveType.PointList)
                throw new Exception("Not supported");
        }

        public void Render()
        {
            DrawContext.FlushBuffer(VertexBuffer,
                VertexCount,
                IndexBuffer,
                IndexCount,
                VertexFormat,
                EffectInstance, 
                PrimitiveType, 
                0,
                PrimitiveCount);
        }

        public void SetVertexBuffer(IReadOnlyList<RenderVertex> vertices)
        {
            if (VertexCount != vertices.Count)
            {
                Debug.WriteLine($"Recreating the vertex buffer to handle {vertices.Count}");

                VertexBuffer?.Dispose();
                VertexCount = vertices.Count;
                VertexBuffer = VertexCount == 0 ? null : 
                    new VertexBuffer(VertexBufferSizeInFloat);
            }

            if (VertexBuffer == null)
                return;

            Debug.WriteLine($"Mapping the vertex buffer");
            VertexBuffer.Map(VertexBufferSizeInFloat);
            try
            {
                Debug.WriteLine("Getting the vertex stream");
                var vertexStream = VertexBuffer.GetVertexStreamPositionNormalColored();
                Debug.WriteLine("Adding vertices");
                foreach (var vertex in vertices)
                    vertexStream.AddVertex(vertex.ToRevit());
                Debug.WriteLine("Finished writing vertices");
            }
            finally
            {
                Debug.WriteLine($"Unmapping the vertex buffer");
                VertexBuffer.Unmap();
            }
        }

        public static IReadOnlyList<Integer> TriangleIndicesToEdgeIndices(IReadOnlyList<Integer> indices)
        {
            var r = new List<Integer>();
            for (var i=0; i < indices.Count; i += 3)
            {
                r.Add(indices[i]);
                r.Add(indices[i+1]);
                r.Add(indices[i + 1]);
                r.Add(indices[i + 2]);
                r.Add(indices[i + 2]);
                r.Add(indices[i]);
            }

            return r;
        }

        public void SetIndexBuffer(IReadOnlyList<Integer> indices)
        {
            if (IndexCount != indices.Count)
            {
                if (indices.Count % PrimitiveSize != 0)
                    throw new Exception($"The number of indices {indices.Count} must be a multiple of {PrimitiveSize}");
                IndexCount = indices.Count;
                IndexBuffer?.Dispose();
                IndexBuffer = IndexCount == 0 ? null : 
                    new IndexBuffer(IndexBufferSizeInShort);
            }

            if (IndexBuffer == null)
                return;

            IndexBuffer.Map(IndexBufferSizeInShort);
            try
            {
                if (PrimitiveType == PrimitiveType.TriangleList)
                {
                    var indexStream = IndexBuffer.GetIndexStreamTriangle();
                    for (var i = 0; i < indices.Count; i += 3)
                    {
                        indexStream.AddTriangle(new IndexTriangle(indices[i], indices[i + 1], indices[i + 2]));
                    }
                }
                else if (PrimitiveType == PrimitiveType.LineList)
                {
                    var indexStream = IndexBuffer.GetIndexStreamLine();
                    for (var i = 0; i < indices.Count; i += 2)
                    {
                        indexStream.AddLine(new IndexLine(indices[i], indices[i+1]));
                    }
                }
                else if (PrimitiveType == PrimitiveType.PointList)
                {
                    var indexStream = IndexBuffer.GetIndexStreamPoint();
                    for (var i = 0; i < indices.Count; i++)
                    {
                        indexStream.AddPoint(new IndexPoint(i));
                    }
                }
            }
            finally
            {
                IndexBuffer.Unmap();
            }
        }
    }
}