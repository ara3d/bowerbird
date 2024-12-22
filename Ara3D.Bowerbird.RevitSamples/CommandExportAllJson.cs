﻿using Autodesk.Revit.UI;
using System;
using System.Linq;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Utils;
using Newtonsoft.Json.Linq;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandExportAllJson : IBowerbirdCommand
    {
        public string Name => "Export all elements JSON";

        public UIApplication app { get; private set; }

        public void Execute(object arg)
        {
            app = (arg as UIApplication);
            if (app == null)
            {
                throw new Exception($"Passed argument {arg} is either null or not a UI application");
            }

            var uidoc = app.ActiveUIDocument;
            var doc = uidoc.Document;
            var outputFilePath = doc.CurrentFileName().ChangeDirectoryAndExt(Config.OutputDir, ".json");
            var elements = doc.GetElements().OrderBy(e => e.Id.IntegerValue);
            var array = new JArray(elements.ToJson());
            outputFilePath.WriteAllText(array.ToString());  
        }
    }
}
