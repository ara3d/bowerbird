using System;
using System.Diagnostics;
using System.IO;

namespace Ara3D.Bowerbird.Revit.CLI
{
    public static class RevitCLIProgram
    {
        public static void Main(string[] args)
        {
            try
            {
                var revitPath = args[0];
                if (!File.Exists(revitPath))
                    throw new Exception($"Revit path not found {revitPath}"); 
                if (!revitPath.EndsWith("exe"))
                    throw new Exception($"Revit path is not an executable {revitPath}");
                var scriptPath = args[1];
                if (!File.Exists(scriptPath))
                    throw new Exception($"Script path not found {scriptPath}");
                var files = args.Length > 2 ? args[2..] : [];
                if (files.Length == 0)
                    throw new Exception("No Revit files specified");
                foreach (var f in files)
                {
                    if (!File.Exists(f))
                        throw new Exception($"Revit file not found {f}");
                }

                foreach (var f in files)
                {
                    var psi = new ProcessStartInfo(revitPath);
                    psi.ArgumentList.Add(f);
                    psi.EnvironmentVariables.Add("BOWERBIRD_AUTORUN_ONLOAD", scriptPath);
                    var p = new Process() { StartInfo = psi };
                    if (!p.Start())
                        throw new Exception("Failed to start process");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Usage();
            }
        }

        public static void Usage()
        {
            Console.WriteLine("Usage: ara3d-revit-cli <path/revit.exe> <path/script.cs> [<file1.rvt> <file2.rvt> ...]");
            Console.WriteLine();
            Console.WriteLine("  This program assumes that the Bowerbird Revit plug-in is installed for the specified version of Revit.");
            Console.WriteLine("  It will set launch Revit with each input file. The bowerbird plug-in will compile the script, and execute it");
            Console.WriteLine("  when the specified file is completed loading. It is up to the script to shut-down Revit afterwards.");
        }
    }
}
