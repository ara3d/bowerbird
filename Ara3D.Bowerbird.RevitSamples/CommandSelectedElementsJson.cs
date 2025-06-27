using System;
using System.Linq;
using Ara3D.Bowerbird.Revit;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Events;

namespace Ara3D.Bowerbird.RevitSamples
{
    /// <summary>
    /// Demonstrates how to create an event handler when the current selection changes,
    /// and to display the current element data as JSON. 
    /// </summary>
    public class CommandSelectedElementsJson : NamedCommand
    {
        public override string Name => "Selected elements JSON";

        public TextDisplayForm TextForm { get; private set; }
        public UIApplication app { get; private set; }

        public override void Execute(object arg)
        {
            app = (arg as UIApplication);
            if (app == null)
            {
                throw new Exception($"Passed argument {arg} is either null or not a UI application");
            }

            // Set up an event handler
            app.SelectionChanged += SelectionChanged;

            // Create a form for displaying text
            TextForm = new TextDisplayForm("");

            // Detach the event handler when the form is closing
            // If we don't do this, then when we will have a small memory leak (the lambda stays resident in memory)
            // And the events will start to pile up every time we relaunch this command
            TextForm.Closing += (sender, args) =>
            {
                // This has to happen in a execute context 
                BowerbirdRevitApp.Instance.Schedule(_ =>
                {
                    app.SelectionChanged -= SelectionChanged;
                }, "Detaching selection changed event");
            };

            // Show the form 
            TextForm.Show();
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check that the form is non-null, visible, and not disposed 
            if (TextForm == null || TextForm.IsDisposed || !TextForm.Visible) 
                return;

            // Get the element IDs in the selection, retrieve elements and convert to JSON 
            var uidoc = app.ActiveUIDocument;
            var doc = uidoc.Document;
            var sel = uidoc.Selection;
            var elementIds = sel.GetElementIds().ToList();
            var elements = elementIds.Select(doc.GetElement);
            var text = elements.ToJson().ToString();
            TextForm.SetText(text);
        }
    }
}