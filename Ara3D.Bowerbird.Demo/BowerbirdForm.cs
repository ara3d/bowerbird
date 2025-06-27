using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Ara3D.Logging;
using Ara3D.ScriptService;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Demo
{
   
    public partial class BowerbirdForm : Form
    {
        public BowerbirdService Service { get; }
        public ILogger Logger { get; }
        public ICommandExecutor Executor { get; }

        public BowerbirdForm(BowerbirdService service = null, ICommandExecutor executor = null)
        {
            InitializeComponent();

            Logger = Logger.Create("Bowerbird", OnLogMsg);
            Logger.Log($"Welcome to Bowerbird by https://ara3d.com");

            Executor = executor ?? new DefaultCommandExecutor();

            if (service == null)
            {
                var options = ScriptingOptions.CreateFromName("Bowerbird WinForms Demo");
                options.ScriptsFolder = AssemblyData.Current.LocationDir.RelativeFolder("..", "..", "..", "Samples");
                if (!options.ScriptsFolder.Exists())
                    throw new Exception($"Could not find folder {options.ScriptsFolder}");
                service = new BowerbirdService(options, Logger);
            }

            Service = service;
            
            checkBoxAutoRecompile.CheckedChanged += CheckBoxAutoRecompileOnCheckedChanged;

            Service.RecompilationEvent += Service_RecompilationEvent;
            Service.ScriptingService.Compile();
        }

        private void Service_RecompilationEvent(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(UpdateForm);
            else
                UpdateForm();
        }

        public void UpdateListBox(ListBox listBox, IEnumerable<object> items)
        {
            listBox.Items.Clear();
            if (items == null) 
                return;
            foreach (var x in items)
                listBox.Items.Add(x);
        }

        public void UpdateForm()
        {
            var data = Service.ScriptingData;
            textBoxOutputDll.Text = data.Dll;
            textBoxLibraryDir.Text = data.Options.LibrariesFolder ?? "";
            textBoxSourceFiles.Text = data.Options.ScriptsFolder ?? "";
            checkBoxAutoRecompile.Checked = Service.ScriptingService.AutoRecompile;

            checkBoxEmit.Checked = data.EmitSuccess;
            checkBoxEmit.Text = "Emit " + (data.EmitSuccess ? "Successful" : "Failed");

            checkBoxParse.Checked = data.ParseSuccess;
            checkBoxParse.Text = "Parse " + (data.ParseSuccess ? "Successful" : "Failed");

            checkBoxLoad.Checked = data.LoadSuccess;
            checkBoxLoad.Text = "Load " + (data.LoadSuccess ? "Successful" : "Failed");

            UpdateListBox(listBoxFiles, data.Files);
            UpdateListBox(listBoxAssemblies, data.Assemblies);
            UpdateListBox(listBoxTypes, data.TypeNames);
            UpdateListBox(listBoxErrors, data.Diagnostics);
            UpdateListBox(listBoxCommands, Service.Commands.Select(c => c.Name));
        }

        private void CheckBoxAutoRecompileOnCheckedChanged(object sender, EventArgs e)
        {
            Service.ScriptingService.AutoRecompile = checkBoxAutoRecompile.Checked;
        }

        public void OnLogMsg(string msg)
        {
            richTextBoxLog.AppendText(msg + Environment.NewLine);
        }

        private void aboutBowerbirdButtonClick(object sender, EventArgs e)
        {
            ProcessUtil.OpenUrl("http://github.com/ara3d/bowerbird");
        }

        private void clearLogButonClick(object sender, EventArgs e)
        {
            richTextBoxLog.Clear();
        }

        public INamedCommand GetSelectedCommand()
        {
            var i = listBoxCommands.SelectedIndex;
            return Service.Commands[i];
        }

        private void listBoxCommands_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RunSelectedCommand();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ProcessUtil.OpenFolderInExplorer(textBoxSourceFiles.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ProcessUtil.OpenFolderInExplorer(textBoxLibraryDir.Text);
        }

        private void listBoxFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        public FilePath GetSelectedFile()
        {
            var i = listBoxFiles.SelectedIndex;
            if (i < 0 || i >= Service.ScriptingData.Files.Count) return null;
            return Service.ScriptingData.Files[i];
        }

        private void listBoxFiles_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            GetSelectedFile()?.OpenDefaultProcess();
        }

        private void contextMenuStripCommands_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            contextMenuStripCommands.Items[0].Enabled = GetSelectedCommand() != null;
        }

        private void contextMenuStripFiles_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            contextMenuStripFiles.Items[0].Enabled = GetSelectedFile() != null;
        }

        private void runSelectedCommandToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RunSelectedCommand();
        }

        public void RunSelectedCommand()
        {
            Executor.Execute(GetSelectedCommand());
        }

        private void openSelectedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetSelectedFile()?.OpenDefaultProcess();
        }

        private void RecompileButton_Click(object sender, EventArgs e)
        {
            Service.ScriptingService.Compile();
        }
    }
}
