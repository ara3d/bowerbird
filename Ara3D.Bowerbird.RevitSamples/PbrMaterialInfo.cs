using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.DB;
using Color = Ara3D.Geometry.Color;

namespace Ara3D.Bowerbird.RevitSamples;

/// <summary>PBR parameters we care about.</summary>
public sealed record PbrMaterialInfo
(
    string Name,
    Color ShadingColor,    // Legacy material.Color (always present)
    Color? BaseColor,       // “generic_diffuse” / “UnifiedDiffuse”
    double? Metallic,        // “generic_metallic” / “UnifiedMetallic”  (0–1)
    double? Roughness,       // “generic_roughness”/ “UnifiedRoughness” (0–1)
    Color? EmissiveColor    // “generic_emission” / “UnifiedEmission”
);

public static class MaterialExtensions
{
    /// <summary>
    /// Retrieves the basic PBR data from a material ID.
    /// </summary>
    public static PbrMaterialInfo? GetPbrInfo(this Document doc, long materialId)
    {
        // 1. Look up the Material
        var mat = doc.GetElement(new ElementId((int)materialId)) as Material;
        if (mat is null) return null;

        // 2. Always-available legacy shading colour
        var legacyColor = new Color(mat.Color.Red / 255f, mat.Color.Green / 255f, mat.Color.Blue / 255f, 1);

        // 3. Try to reach the rendering (appearance) asset
        var assetEl = doc.GetElement(mat.AppearanceAssetId) as AppearanceAssetElement;
        if (assetEl is null)
            return new PbrMaterialInfo(mat.Name, legacyColor, null, null, null, null);

        var asset = assetEl.GetRenderingAsset();

        // 4. Map the parameters we care about
        Color? baseCol = null;
        Color? emissive = null;
        double? metallic = null;
        double? roughness = null;
        double? opacity = null;

        for (var i=0; i < asset.Size; i++)
        {
            var prop = asset[i];

            // Colours -----------------------------------------------------------------
            if (prop is AssetPropertyDoubleArray4d col)
            {
                var c = ToDrawingColor(col);
                switch (prop.Name)
                {
                    case "generic_diffuse":
                    case "UnifiedDiffuse":
                        baseCol = c; break;

                    case "generic_emission":
                    case "UnifiedEmission":
                        emissive = c; break;
                }
            }

            // Scalar parameters --------------------------------------------------------
            else if (prop is AssetPropertyDouble d)
            {
                switch (prop.Name)
                {
                    case "generic_metallic":
                    case "UnifiedMetallic":
                        metallic = d.Value; break;

                    case "generic_roughness":
                    case "UnifiedRoughness":
                        roughness = d.Value; break;

                    case "generic_opacity":
                    case "UnifiedOpacity":
                        opacity = d.Value; break;
                }
            }
        }

        var alpha = opacity.HasValue ? (float)opacity.Value : 1.0f;
        if (baseCol != null)
            baseCol = baseCol.Value.WithA(alpha);

        legacyColor = legacyColor.WithA(alpha);
        return new PbrMaterialInfo(mat.Name, legacyColor, baseCol, metallic, roughness, emissive);
    }

    // Helper – converts a Revit colour array (R,G,B,A, each 0-1) to System.Drawing.Color
    private static Color ToDrawingColor(AssetPropertyDoubleArray4d col)
    {
        // AssetPropertyDoubleArray4d always stores 4 doubles
        var dbls = col.GetValueAsDoubles();
        var r = (float)dbls[0];
        var g = (float)dbls[1];
        var b = (float)dbls[2];
        var a = (float)dbls[3];
        return new Color(r, g, b, a);
    }
}