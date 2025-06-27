using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Demo.Samples
{
    /// <summary>
    /// Shows a message box with the text: "Hello world!"
    /// </summary>
    public class HelloWorld : NamedCommand
    {
        public override void Execute()
            => MessageBox.Show("Hello World!");

    }

    /// <summary>
    /// Shows a message box with a counter value which is incremented each time. 
    /// </summary>
    public class Counter : NamedCommand
    {
        public static int Count;

        public override void Execute()
            => MessageBox.Show($"You have executed this command {++Count} time(s)");
    }

    /// <summary>
    /// Opens a URL in the default web browser. 
    /// </summary>
    public class OpenUrl : NamedCommand
    {
        public override void Execute()
            => ProcessUtil.OpenUrl("https://ara3d.com");
    }

    /// <summary>
    /// Triggers the debugger to break. 
    /// </summary>
    public class LaunchDebugger : NamedCommand
    {
        public override void Execute()
            => Debugger.Break();
    }

    /// <summary>
    /// Launches a simple web-server, returns a text response, and shuts down the web-server. 
    /// </summary>
    public class HttpServer : NamedCommand
    {
        public WebServer Server
        {
            get;
            private set;
        }

        public override void Execute()
        {
            Server = new WebServer(Callback);
            Server.Start();
            ProcessUtil.OpenUrl(Server.Uri);
        }

        private void Callback(string verb, string path, IDictionary<string, string> parameters, Stream inputStream, Stream outputStream)
        {
            using (var writer = new StreamWriter(outputStream))
            {
                writer.WriteLine($"Hello, thanks for using the HTTP server. I will shut myself down now.");
            }
            Server.Stop();
        }
    }
}
