using System;
using System.Diagnostics;
using System.Globalization;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.Revit
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class BowerbirdExternalCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                if (BowerbirdRevitApp.Instance == null)
                    throw new Exception("Bowerbird application was never instantiated");
                
                Debug.WriteLine($"Current culture = {CultureInfo.CurrentCulture}");
                Debug.WriteLine($"Current UI culture = {CultureInfo.CurrentUICulture}");
                Debug.WriteLine($"Default current culture = {CultureInfo.DefaultThreadCurrentCulture}");
                Debug.WriteLine($"Default current UI culture = {CultureInfo.DefaultThreadCurrentUICulture}");

                BowerbirdRevitApp.Instance.Run(commandData.Application);
                Debug.WriteLine("Executed");
				
                return Result.Succeeded;
			}
			catch ( Exception ex)
            {
                TaskDialog.Show("Error",ex.ToString());
                return Result.Failed;
            }
        }
    }
}
