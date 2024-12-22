﻿using Ara3D.Bowerbird.Interfaces;
using Autodesk.Revit.UI;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class CommandExternalEventDemo : IBowerbirdCommand
    {
        public class ExternalEventExample : IExternalEventHandler
        {
            public void Execute(UIApplication app)
            {
                TaskDialog.Show("External Event", "Click Close to close.");
            }

            public string GetName()
            {
                return "External Event Example";
            }
        }

        public string Name => "External event";

        public void Execute(object arg)
        {
            var handler = new ExternalEventExample(); 
            var ev = ExternalEvent.Create(handler);
            ev.Raise();
        }
    }
}