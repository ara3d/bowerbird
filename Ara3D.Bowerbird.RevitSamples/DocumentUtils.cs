using System;
using System.Collections.Generic;
using System.Linq;
using Ara3D.Utils;
using Autodesk.Revit.DB;

namespace Ara3D.Bowerbird.RevitSamples;

public static class DocumentUtils
{
    public static IEnumerable<Document> GetLinkedDocuments(this Document doc)
        => new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .Cast<RevitLinkInstance>()
            .Select(li => li.GetLinkDocument())
            .WhereNotNull();

    public static IEnumerable<Element> GetElementsThatAreNotTypes(this Document doc)
        => new FilteredElementCollector(doc).WhereElementIsNotElementType();

    public static IEnumerable<Element> GetElementsThatAreTypes(this Document doc)
        => new FilteredElementCollector(doc).WhereElementIsElementType();

    public static void ProcessElements(this Document rootDoc, Action<Element> process, bool includeLinks = true)
    {
        // Track which docs we've already processed so nested links aren't repeated
        // doc.PathName is unique even for detached files
        var visited = new HashSet<string>();          

        void Local_ProcessElements(Document doc)
        {
            if (doc == null || !visited.Add(doc.PathName ?? doc.Title)) return;

            foreach (var e in doc.GetElements())
                process(e);

            if (includeLinks)
                foreach (var d in doc.GetLinkedDocuments())
                    Local_ProcessElements(d);
        }

        Local_ProcessElements(rootDoc);
    }
}