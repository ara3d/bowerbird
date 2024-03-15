using System.Windows.Forms;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FilePath = Ara3D.Utils.FilePath;

namespace Ara3D.Bowerbird.Revit.Samples
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
}
