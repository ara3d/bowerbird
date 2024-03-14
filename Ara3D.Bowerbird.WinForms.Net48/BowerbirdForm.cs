using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Ara3D.Bowerbird.Core;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Domo;
using Ara3D.Logging;
using Ara3D.Services;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.WinForms.Net48
{
    public partial class BowerbirdForm : Form
    {
        public IApplication App { get; } 
        public IBowerbirdService BowerbirdService { get; }
        public ILogger Logger { get; }

        public static readonly DirectoryPath SamplesSrcFolder
            = PathUtil.GetCallerSourceFolder().RelativeFolder("Samples");

        public static readonly BowerbirdOptions Options =
            BowerbirdOptions.CreateFromName("Ara 3D", "Bowerbird WinForms Demo");

        public BowerbirdForm()
        {
            InitializeComponent();
            App = new Services.Application();

            Logger = Logger.Create("Bowerbird", OnLogMsg);
            Logger.Log($"Welcome to Bowerbird by https://ara3d.com");
            
            Logger.Log($"Copying script files from {SamplesSrcFolder} to {Options.ScriptsFolder}");
            SamplesSrcFolder.CopyDirectory(Options.ScriptsFolder);
            
            BowerbirdService = new BowerbirdService(App, Logger, Options);
            BowerbirdService.Repository.OnModelChanged(DataModelChanged);
            DataModelChanged(BowerbirdService.Model);

            checkBoxAutoRecompile.CheckedChanged += CheckBoxAutoRecompileOnCheckedChanged;

            BowerbirdService.Compile();
        }

        
        public void UpdateListBox(ListBox listBox, IEnumerable<object> items)
        {
            listBox.Items.Clear();
            foreach (var x in items)
                listBox.Items.Add(x);
        }
        
        public void DataModelChanged(IModel<BowerbirdDataModel> model)
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

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void listBoxCommands_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listBoxCommands_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var i = listBoxCommands.SelectedIndex;
            if (i < 0) return;
            var command = BowerbirdService.Commands[i];
            command.Execute();
        }
    }
}
