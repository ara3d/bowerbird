using System;
using System.Linq;
using Ara3D.Logging;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandSelectedItemGeometry : NamedCommand
    {
        public override string Name => "Selected Item Geometry";

        public static TextDisplayForm Form;
        public static ILogger Logger;

        public static void Log(string msg)
        {
            Logger.Log(msg);
        }

        public override void Execute(object arg)
        {
            if (Form == null)
            {
                Form = new TextDisplayForm("");
                Form.TopMost = true;
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
}