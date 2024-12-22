using System.Diagnostics;
using System.Linq;
using Ara3D.Utils;
using Ara3D.Bowerbird.Interfaces;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class AutoRun : IBowerbirdCommand
    {
        public string Name => "AutoRun";

        public void Execute(object arg)
        {
            var app = (UIApplication)arg;
            var uiDoc = app.ActiveUIDocument;
            var view = GetDefault3DView(uiDoc);
            if (view != null) 
                uiDoc.ActiveView = view;
            var output = PathUtil.CreateTempFile().ChangeExtension("png");
            ExportCurrentViewToPng(uiDoc.Document, output);
            output.OpenDefaultProcess();
            Process.GetCurrentProcess().Kill();
        }

       

        public static void ExportCurrentViewToPng(Document doc, Utils.FilePath filePath)
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
        }
    }
}