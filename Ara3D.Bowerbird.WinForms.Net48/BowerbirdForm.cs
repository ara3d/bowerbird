using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Bowerbird.WinForms.Net48.Properties;
using Ara3D.Domo;
using Ara3D.Logging;
using Ara3D.Services;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.WinForms.Net48
{
    public partial class BowerbirdForm : Form
    {
        public IServiceManager App { get; } 
        public IBowerbirdService BowerbirdService { get; }
        public ILogger Logger { get; }
           
        public BowerbirdForm(IBowerbirdService service)
        {
            InitializeComponent();

            Closed += BowerbirdForm_Closed;
            Logger = service.Logger = Logger.Create("Bowerbird", OnLogMsg);
            Logger.Log($"Welcome to Bowerbird by https://ara3d.com");

            BowerbirdService = service;
            BowerbirdService.Repository.OnModelChanged(DataModelChanged);
            
            DataModelChanged(BowerbirdService.Model);

            checkBoxAutoRecompile.CheckedChanged += CheckBoxAutoRecompileOnCheckedChanged;

            BowerbirdService.Compile();
        }

        private void BowerbirdForm_Closed(object sender, EventArgs e)
        {
            BowerbirdService.Dispose();
        }

        public void UpdateListBox(ListBox listBox, IEnumerable<object> items)
        {
            listBox.Items.Clear();
            foreach (var x in items)
                listBox.Items.Add(x);
        }
        
        public void DataModelChanged(IModel<BowerbirdDataModel> model)
        {
            // Data model updates can happen on a separate thread, because the 
            
            var action = new Action(() =>
            {
                textBoxOutputDll.Text = model.Value.Dll;
                textBoxLibraryDir.Text = model.Value.Options.LibrariesFolder ?? "";
                textBoxSourceFiles.Text = model.Value.Options.ScriptsFolder ?? "";
                checkBoxAutoRecompile.Checked = BowerbirdService.AutoRecompile;

                checkBoxEmit.Checked = model.Value.EmitSuccess;
                checkBoxEmit.Text = "Emit " + (model.Value.EmitSuccess ? "Successful" : "Failed");

                checkBoxParse.Checked = model.Value.ParseSuccess;
                checkBoxParse.Text = "Parse " + (model.Value.ParseSuccess ? "Successful" : "Failed");

                checkBoxLoad.Checked = model.Value.LoadSuccess;
                checkBoxLoad.Text = "Load " + (model.Value.LoadSuccess ? "Successful" : "Failed");

                UpdateListBox(listBoxFiles, model.Value.Files);
                UpdateListBox(listBoxAssemblies, model.Value.Assemblies);
                UpdateListBox(listBoxTypes, model.Value.TypeNames);
                UpdateListBox(listBoxErrors, model.Value.Diagnostics);
                UpdateListBox(listBoxCommands, model.Value.Commands);
            });

            if (this.InvokeRequired)
                this.Invoke(action);
            else
                action();
        }

        private void CheckBoxAutoRecompileOnCheckedChanged(object sender, EventArgs e)
        {
            BowerbirdService.AutoRecompile = checkBoxAutoRecompile.Checked;
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

        public IBowerbirdCommand GetSelectedCommand()
        {
            var i = listBoxCommands.SelectedIndex;
            if (i < 0 || i >= BowerbirdService.Commands.Count) return null;
            return BowerbirdService.Commands[i];
        }

        private void listBoxCommands_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var cmd = GetSelectedCommand();
            if (cmd == null) return;
            BowerbirdService.ExecuteCommand(cmd);
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
            if (i < 0 || i >= BowerbirdService.Model.Value.Files.Count) return null;
            return BowerbirdService.Model.Value.Files[i];
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
            var cmd = GetSelectedCommand();
            if (cmd == null) return;
            BowerbirdService.ExecuteCommand(cmd);
        }

        private void openSelectedFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetSelectedFile()?.OpenDefaultProcess();
        }

        private void RecompileButton_Click(object sender, EventArgs e)
        {
            BowerbirdService.Compile();
        }
    }
}
