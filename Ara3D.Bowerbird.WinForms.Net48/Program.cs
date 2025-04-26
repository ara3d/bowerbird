using System;
using System.Windows.Forms;
using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Services;

namespace Ara3D.Bowerbird.WinForms.Net48
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

            ServiceApp = new ServiceManager();
            Options = BowerbirdOptions.CreateFromName("Bowerbird WinForms Demo");
            Host = new BowerbirdHost(ExecuteCommand);
            BowerbirdService = new BowerbirdService(Host, ServiceApp, null, Options);
        }

        public static void ExecuteCommand(IBowerbirdCommand command) => BowerbirdService.ExecuteCommand(null);
        public static BowerbirdHost Host { get; private set; }
        public static ServiceManager ServiceApp { get; private set; }
        public static BowerbirdOptions Options { get; private set; }
        public static BowerbirdService BowerbirdService { get; private set; }
    }
}
