using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitExporter;

namespace Ara3D.Bowerbird.Revit.Samples
{
    [Transaction(TransactionMode.Manual)]
    public class MyExportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uiapp = commandData.Application;
            var uidoc = uiapp.ActiveUIDocument;
            var app = uiapp.Application;
            var doc = uidoc.Document;

            if (doc.ActiveView as View3D != null)
                ExportView3D(doc, doc.ActiveView as View3D);
            else
                MessageBox.Show("You must be in 3D view to export.");

            return Result.Succeeded;
        }

        public static void ExportView3D(Document document, View3D view3D)
        {
            var context = new MyExportContext(document);

            // Create an instance of a custom exporter by giving it a document and the context.
            var exporter = new CustomExporter(document, context);

            exporter.IncludeGeometricObjects = true;

            exporter.ShouldStopOnError = false;
            
            exporter.Export(view3D);
        }

    }
}
