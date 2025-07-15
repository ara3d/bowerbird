using System;
using System.Collections.Generic;

namespace BIMOpenSchema;

public static class BIMOpenSchemaHelpers
{
    // Informal and incomplete mapping of IFC Relationships to relation types for documentation purposes 
    public static Dictionary<string, RelationType> IfcRelationToRelationType
        = new() {
            { "IfcRelAggregates", RelationType.MemberOf },
            { "IfcRelAssignsToGroup", RelationType.MemberOf },
            { "IfcRelDecomposes", RelationType.MemberOf },
            { "IfcRelNests", RelationType.MemberOf },
            { "IfcRelContainedInSpatialStructure", RelationType.ContainedIn },
            { "IfcRelDefinesByType", RelationType.InstanceOf },
            { "IfcRelVoidsElement", RelationType.HostedBy },
            { "IfcRelConnectsPortToPort", RelationType.ConnectsTo },
            { "IfcRelConnectsElements", RelationType.ConnectsTo },
            { "IfcRelFillsElement", RelationType.HostedBy },
            { "IfcPort", RelationType.HasConnector },
            { "IfcMaterialLayerSetUsage", RelationType.HasLayer },
            { "IfcRelAssociatesMaterial", RelationType.HasMaterial },
        };

    public record Unit
    (
        // UCUM symbol
        string Symbol,

        // ISO 80000 quantity name 
        string IsoQuantity,

        // Official symbol of the base SI unit
        string SiBaseUnitSymbol,

        // Conversion factor to the SI base unit
        double SiFactor,

        // Offset required to be added before conversion the SI base unit
        double SiOffset = 0
    );

    public static readonly Unit[] CommonUnits =
    [
        // ────────── Length ──────────
        new("m",       "length",             "m",       1.0                          ),
        new("ft",      "length",             "m",       0.3048                       ),
        new("mm",      "length",             "m",       0.001                        ),
        new("cm",      "length",             "m",       0.01                         ),
        new("dm",      "length",             "m",       0.1                          ),
        new("in",      "length",             "m",       0.0254                       ),
        new("yd",      "length",             "m",       0.9144                       ),
        new("mi",      "length",             "m",       1_609.344                    ),

        // ────────── Area ──────────
        new("m2",      "area",               "m²",      1.0                          ),
        new("ft2",     "area",               "m²",      0.092_903_04                 ),
        new("cm2",     "area",               "m²",      0.0001                       ),
        new("mm2",     "area",               "m²",      1e-6                         ),
        new("in2",     "area",               "m²",      0.000_645_16                 ),
        new("yd2",     "area",               "m²",      0.836_127_36                 ),
        new("acre",    "area",               "m²",      4_046.856_422_4              ),
        new("ha",      "area",               "m²",      10_000.0                     ),

        // ────────── Volume ──────────
        new("m3",      "volume",             "m³",      1.0                          ),
        new("ft3",     "volume",             "m³",      0.028_316_846_592            ),
        new("cm3",     "volume",             "m³",      1e-6                         ),
        new("mm3",     "volume",             "m³",      1e-9                         ),
        new("in3",     "volume",             "m³",      0.000_016_387_064            ),
        new("yd3",     "volume",             "m³",      0.764_554_857_984            ),
        new("L",       "volume",             "m³",      0.001                        ),
        new("gal",     "volume",             "m³",      0.003_785_411_784            ),   // US liquid gallon

        // ────────── Mass ──────────
        new("kg",      "mass",               "kg",      1.0                          ),
        new("g",       "mass",               "kg",      0.001                        ),
        new("lb",      "mass",               "kg",      0.453_592_37                 ),
        new("t",       "mass",               "kg",      1_000.0                      ),   // metric tonne
        new("ton_us",  "mass",               "kg",      907.184_74                   ),   // short ton (US)

        // ────────── Force ──────────
        new("N",       "force",              "N",       1.0                          ),
        new("kN",      "force",              "N",       1_000.0                      ),
        new("lbf",     "force",              "N",       4.448_221_615_260_5          ),
        new("kip",     "force",              "N",       4_448.221_615_260_5          ),   // 1 000 lbf

        // ────────── Pressure / Stress ──────────
        new("Pa",      "pressure‑stress",    "Pa",      1.0                          ),
        new("kPa",     "pressure‑stress",    "Pa",      1_000.0                      ),
        new("MPa",     "pressure‑stress",    "Pa",      1_000_000.0                  ),
        new("psi",     "pressure‑stress",    "Pa",      6_894.757_293_168            ),
        new("ksi",     "pressure‑stress",    "Pa",      6_894_757.293_168            ),
        new("psf",     "pressure‑stress",    "Pa",      47.880_258_98                ),
        new("bar",     "pressure‑stress",    "Pa",      100_000.0                    ),
        new("atm",     "pressure‑stress",    "Pa",      101_325.0                    ),

        // ────────── Temperature ──────────
        new("K",       "thermodynamic T",    "K",       1.0                          ),   // Kelvin (reference)
        new("°C",      "thermodynamic T",    "K",       1.0,         273.15          ),   // Celsius
        new("°F",      "thermodynamic T",    "K",       5.0/9.0,     255.372_222_222 ),   // Fahrenheit

        // ────────── Plane angle ──────────
        new("rad",     "plane angle",        "rad",     1.0                          ),
        new("deg",     "plane angle",        "rad",     Math.PI / 180.0              ),

        // ────────── Speed ──────────
        new("m/s",     "speed",              "m/s",     1.0                          ),
        new("km/h",    "speed",              "m/s",     0.277_777_777_777_8          ),
        new("mph",     "speed",              "m/s",     0.447_04                     ),

        // ────────── Volumetric flow ──────────
        new("m3/s",    "volume flow",        "m³/s",    1.0                          ),
        new("L/s",     "volume flow",        "m³/s",    0.001                        ),
        new("L/min",   "volume flow",        "m³/s",    0.000_016_666_666_666_7      ),
        new("ft3/s",   "volume flow",        "m³/s",    0.028_316_846_592            ),
        new("ft3/min", "volume flow",        "m³/s",    0.000_471_947_443_2          ),   // CFM
        new("gpm",     "volume flow",        "m³/s",    0.000_063_090_196_4          ),   // US gallon/min

        // ────────── Power ──────────
        new("W",       "power",              "W",       1.0                          ),
        new("kW",      "power",              "W",       1_000.0                      ),
        new("hp",      "power",              "W",       745.699_871_582              ),

        // ────────── Energy ──────────
        new("J",       "energy",             "J",       1.0                          ),
        new("kJ",      "energy",             "J",       1_000.0                      ),
        new("Wh",      "energy",             "J",       3_600.0                      ),
        new("kWh",     "energy",             "J",       3_600_000.0                  ),
        new("Btu",     "energy",             "J",       1_055.055_852_62             ),

        // ────────── Frequency ──────────
        new("Hz",      "frequency",          "Hz",      1.0                          ),
        new("kHz",     "frequency",          "Hz",      1_000.0                      ),

        // ────────── Electrical ──────────
        new("V",       "electric potential", "V",       1.0                          ),
        new("kV",      "electric potential", "V",       1_000.0                      ),
        new("A",       "electric current",   "A",       1.0                          ),
        new("kA",      "electric current",   "A",       1_000.0                      ),
        new("Ω",       "electric resistance","Ω",       1.0                          ),
        new("kΩ",      "electric resistance","Ω",       1_000.0                      ),
        new("VA",      "apparent power",     "VA",      1.0                          ),
        new("kVA",     "apparent power",     "VA",      1_000.0                      ),

        // ────────── Lighting ──────────
        new("lx",      "illuminance",        "lx",      1.0                          ),
        new("fc",      "illuminance",        "lx",      10.763_910_416_71            )
     ];
}