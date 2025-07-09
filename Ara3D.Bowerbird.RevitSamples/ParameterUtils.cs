using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace Ara3D.Bowerbird.RevitSamples;

public static class ParameterUtils
{
    /// <summary>
    /// Returns a dictionary mapping every parameter's display name
    /// to a readable string value for the supplied element.
    /// </summary>
    public static Dictionary<string, string> GetParameterMap(this Element elem)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (elem == null || !elem.IsValidObject)
            return map;

        // Iterate over *all* parameters (built-in, shared, project, etc.)
        foreach (Parameter p in elem.Parameters)
        {
            if (p == null) continue;
            if (p.Definition == null) continue;
            var name = p.Definition?.Name ?? "(Unnamed)";
            //Debug.WriteLine($"Processing {name} + {p.Definition.GetDataType()}");
            var value = ParameterToString(p);
            // Last‐writer wins if duplicate names exist (rare but possible)
            map[name] = value;
        }

        return map;
    }

    /// <summary>
    /// Converts a single parameter to a readable string, handling all storage types.
    /// </summary>
    private static string ParameterToString(this Parameter p)
    {
        if (!p.HasValue)
            return string.Empty;

        switch (p.StorageType)
        {
            case StorageType.String:
                return p.AsString() ?? string.Empty;

            case StorageType.Integer:
                // Integers may be Booleans, enumerations, etc.
                // Attempt AsValueString first (respects unit/type formatting)
                return p.AsValueString() ?? p.AsInteger().ToString();

            case StorageType.Double:
                // AsValueString gives the user-visible, unit-aware representation
                return p.AsValueString() ?? p.AsDouble().ToString("G17");

            case StorageType.ElementId:
                var id = p.AsElementId();
                if (id == ElementId.InvalidElementId) return "—";
                return id.Value.ToString();

            case StorageType.None:
            default:
                return string.Empty;
        }
    }
}