using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;

namespace Ara3D.Bowerbird.RevitSamples
{
    /// <summary>
    /// A geometry abstract syntax tree (AST) is an abstract tree representation of the geometry of
    /// a BIM element that removes some of the details, but captures the general structure
    /// of the geometry.
    ///
    /// It is created from the components of a geometric element. 
    ///
    /// The geometry AST can be expressed as a string, that looks like a function call expression.
    /// Accessing a string representation of the geometry AST is achieved through the extension method ".ToExpr()"
    /// If there is a change to the geometry by a Revit user then the string is highly likely to change.
    ///
    /// The advantage of computing the geometry AST is that it can be done very quickly. Two structures that have changed
    /// can be determined very quickly. It can also acts as a hash to enable finding similar structures that might need to
    /// be merged.
    ///
    /// The disadvantage of a geometry AST is that it is possible in some cases that two geometric structures might be
    /// different, but still returns the same abstract syntax tree, 
    /// </summary>
    public static class GeometryAbstractSyntaxTree
    {
        public static string ToExpr(this IList<Autodesk.Revit.DB.XYZ> points)
            => $"Points({string.Join(", ", points.Select(xyz => xyz.ToExpr()))})";

        public static string ToExpr(this DoubleArray vals)
            => $"DoubleArray({string.Join(", ", vals)})";

        public static string ToExpr(this CurveArray ca)
            => $"Curves({string.Join(", ", ca.OfType<Curve>().Select(x => x.ToExpr()))})";

        public static string ToExpr(this Autodesk.Revit.DB.XYZ xyz)
            => $"[{xyz.X},{xyz.Y},{xyz.Z}]";

        public static string ToExpr(this BoundingBoxXYZ bb)
            => $"BoundingBoxXYZ({bb.Min.ToExpr()}, {bb.Max.ToExpr()})";

        public static string ToExpr(this FaceArray faces)
            => $"Faces({string.Join(", ", faces.OfType<Face>().Select(f => f.ToExpr()))})";
        
        /// <summary>
        /// This provides a string representation of an object that will **likely** change if the geometry changes. 
        /// NOTE: it might not change even if the geometry changes underneath, it will require careful analysis and testing. 
        /// </summary>
        public static string ToExpr(this GeometryObject o)
        {
            if (o == null)
                return "";

            var type = o.GetType().Name;
            switch (o)
            {
                case Arc arc:
                    return $"{type}({arc.Center}, {arc.Radius}, {arc.Normal})";

                case CylindricalHelix ch:
                    return $"{type}({ch.Radius}, {ch.BasePoint.ToExpr()}, {ch.Height}, {ch.Pitch})";
                
                case Ellipse e:
                    return $"{type}({e.Center.ToExpr()}, {e.Normal.ToExpr()}, {e.RadiusX}, {e.RadiusY})";
                
                case HermiteSpline hs:
                    return $"{type}({hs.IsPeriodic}, {hs.ControlPoints.ToExpr()}, {hs.Tangents.ToExpr()}, {hs.Parameters.ToExpr()})";
                
                case Line line:
                    return $"{type}({line.Direction.ToExpr()}, {line.Origin.ToExpr()}, {line.Length})";
                
                case NurbSpline ns:
                    return $"{type}({ns.CtrlPoints.ToExpr()}, {ns.Degree}, {ns.Knots.ToExpr()}, {ns.Weights.ToExpr()}, {ns.isRational})";
                
                case Curve c:
                    return $"{type}({c.IsBound}, {c.IsClosed}, {c.IsCyclic}, {c.Period})";
                
                /*
                case ConicalFace cf:
                case CylindricalFace cyf:
                case HermiteFace hermiteFace:
                case PlanarFace planarFace:
                case RevolvedFace revolvedFace:
                case RuledFace ruledFace:
                */
                case Face f:
                    return $"{type}({f.Area}, {f.EdgeLoops.Size}, {f.HasRegions}, {f.IsTwoSided})";

                case Edge e:
                case Point p:
                    return $"{type}()";

                case GeometryElement ge:
                    return $"{type}({ge.GetBoundingBox().ToExpr()},{string.Join(",", ge.Select(ToExpr))})";
                    
                case GeometryInstance gi:
                    // TODO: this looks like it could be optimized.
                    // We don't want to retrieve a symbol geometry multiple times. 
                    // Does a symbol geometry even ever change? If so when?  
                    return $"{type}({gi.GetSymbolGeometry().ToExpr()})";
                
                case Autodesk.Revit.DB.Mesh mesh:
                    return $"{type}({mesh.NumTriangles}, {mesh.Vertices.Count}, {mesh.IsClosed})";

                case PolyLine pl:
                    return $"{type}({pl.GetCoordinates().ToExpr()})";
                
                case Profile pr:
                    return $"{type}({pr.Filled}, {pr.Curves.ToExpr()}";
                
                case Solid s:
                    // TODO: validate how sensitive this is to actual geometry changes
                    // It is probably possible to make a change to geometry that returns the same values throughout
                    return $"{type}({s.GetBoundingBox().ToExpr()}, {s.Faces.Size}, {s.Edges.Size}, {s.SurfaceArea}, {s.Volume}, {s.Faces.ToExpr()})";
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(o));
            }
        }

        public static string PrettyPrintAst(string input)
        {
            var sb = new StringBuilder();
            var indentLevel = 0;
            var i = 0;
            var indent = "";
            while (i < input.Length)
            {
                var c = input[i++];
                if (c == '(')
                {
                    if (input[i] == ')')
                    {
                        i++;
                        indent = new string(' ', indentLevel * 2);
                        sb.AppendLine("()");
                        sb.Append(indent);
                        continue;
                    }

                    indent = new string(' ', ++indentLevel * 2);
                    sb.AppendLine("(");
                    sb.Append(indent);
                    continue;
                }

                if (c == ')')
                {
                    sb.Append(")");
                    indent = new string(' ', --indentLevel * 2);

                    if (i < input.Length && input[i] == ',')
                    {
                        sb.Append(",");
                        i++;
                    }

                    sb.AppendLine();
                    sb.Append(indent);
                    continue;
                }

                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
