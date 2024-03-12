using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Domo;
using Ara3D.Logging;
using Ara3D.Services;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.WinForms.Net48
{
    public partial class BowerbirdForm : Form
    {
        public IBowerbirdService BowerbirdService { get; private set; }
        public ILoggingService LoggingService { get; private set; }

        public BowerbirdForm()
        {
            InitializeComponent();
        }

        public void RegisterServices(IBowerbirdService bowerbird, ILoggingService logging)
        {
            if (BowerbirdService != null) throw new Exception("Bowerbird service was already initialized");
            if (LoggingService != null) throw new Exception("Logging service was already initialized");
            BowerbirdService = bowerbird;
            LoggingService = logging;
            LoggingService.Repository.OnModelAdded(LogEntryAdded);
            BowerbirdService.Repository.OnModelChanged(DataModelChanged);
        }

        public void DataModelChanged(IModel<BowerbirdDataModel> model)
        {
        }

        public void LogEntryAdded(IModel<LogEntry> entry)
        {
            listBoxLog.Items.Add(entry.ToString());
        }


        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void listBoxFiles_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void compileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void recompileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void clearConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void aboutBowerbirdButtonClick(object sender, EventArgs e)
        {
            ProcessUtil.OpenUrl("http://github.com/ara3d/bowerbird");
        }

        private void clearLogButonClick(object sender, EventArgs e)
        {
            listBoxLog.Items.Clear();
        }
    }
}
