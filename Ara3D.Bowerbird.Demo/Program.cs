using System;
using System.Windows.Forms;
using Ara3D.Logging;
using Ara3D.ScriptService;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Demo
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            using var mainForm = new BowerbirdForm();
            Application.Run(mainForm);
        }

    }
}
