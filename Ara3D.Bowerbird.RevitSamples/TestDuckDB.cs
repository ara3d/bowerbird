using System.Reflection;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Ara3D.SpaceLinter.RevitSamples;

public class TestDuckDB : NamedCommand
{
    public override string Name => "Test DuckDB";

    public override void Execute(object arg)
    {
        var s1 = Assembly.GetExecutingAssembly().Location;
        var s2 = Environment.CurrentDirectory;
        MessageBox.Show(s1 + "\n" + s2);

        NativeLibrary.Load(@"C:\Users\cdigg\AppData\Roaming\Autodesk\Revit\Addins\2025\Ara3D.BIMOpenSchema\duckdb.dll");
        NativeLibrary.Load(@"C:\Users\cdigg\AppData\Roaming\Autodesk\Revit\Addins\2025\Ara3D.BIMOpenSchema\VCRUNTIME140.dll");
        NativeLibrary.Load(@"C:\Users\cdigg\AppData\Roaming\Autodesk\Revit\Addins\2025\Ara3D.BIMOpenSchema\nironcompress.dll");

        var sb = new DuckDB.NET.Data.DuckDBConnectionStringBuilder();
        sb.DataSource = "File";
    }
}