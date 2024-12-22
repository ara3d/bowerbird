using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Ara3D.Logging;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class TextDisplayForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox textBox;

        public TextDisplayForm(string text)
        {
            Text = "Multi-line Text";
            Size = new System.Drawing.Size(400, 300);

            textBox = new System.Windows.Forms.TextBox();
            textBox.Multiline = true;
            textBox.Dock = DockStyle.Fill;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Text = text;

            Controls.Add(textBox);
        }

        public void AddLine(string s)
            => textBox.AppendText(s + Environment.NewLine);

        public ILogger CreateLogger()
            => new Logger(LogWriter.Create(AddLine), "");

        public static TextDisplayForm DisplayText(IEnumerable<string> lines)
            => DisplayText(string.Join("\r\n", lines));

        public static TextDisplayForm DisplayText(string text)
        {
            var form = new TextDisplayForm(text);
            form.Show();
            return form;
        }

        public void SetText(string s)
            => textBox.Text = s;
    }
}