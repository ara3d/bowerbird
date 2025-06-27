using Ara3D.Utils;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using FilePath = Ara3D.Utils.FilePath;

namespace Ara3D.Bowerbird.RevitSamples
{
    /// <summary>
    /// Save the current view as a PNG in the temp folder, and then opens
    /// it using the default registered application. 
    /// </summary>
    public class CommandSaveCurrentViewAsPng : NamedCommand
    {
        public override string Name => "Save to PNG";

        public override void Execute(object arg)
        {
            var doc = (arg as UIApplication)?.ActiveUIDocument?.Document;
            if (doc == null) return;
            var output = PathUtil.CreateTempFile().ChangeExtension("png");
            ExportCurrentViewToPng(doc, output);
            output.ShellExecute();
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