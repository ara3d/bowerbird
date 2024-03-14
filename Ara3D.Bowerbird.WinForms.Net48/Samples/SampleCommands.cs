using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.WinForms.Net48.Samples
{
    public class HelloWorld : IBowerbirdCommand
    {
        public string Name => "Hello World!";

        public void Execute()
            => MessageBox.Show(Name);
    }

    public class Counter : IBowerbirdCommand
    {
        public static int Count;

        public string Name => "Count";

        public void Execute()
            => MessageBox.Show($"You have executed this command {++Count} time(s)");
    }

    public class OpenUrl : IBowerbirdCommand
    {
        public string Name => "Open URL";

        public void Execute()
            => ProcessUtil.OpenUrl("https://ara3d.com");
    }

    public class LaunchDebugger : IBowerbirdCommand
    {
        public string Name => "Launch Debugger";

        public void Execute()
            => Debugger.Break();
    }

    public class HttpServer : IBowerbirdCommand
    {
        public string Name => "HTTP Server";
        public WebServer Server
        {
            get;
            private set;
        }

        public void Execute()
        {
            var localHostUri = "http://localhost:8074/";
            Server = new WebServer(Callback, localHostUri);
            Server.Start();
            ProcessUtil.OpenUrl(localHostUri);
        }

        private void Callback(string verb, string path, IDictionary<string, string> parameters, Stream inputstream, Stream outputstream)
        {
            using (var writer = new StreamWriter(outputstream))
            {
                writer.WriteLine($"Hello, thanks for using the HTTP server. I will shut myself down now.");
            }
            Server.Stop();
        }
    }
}
