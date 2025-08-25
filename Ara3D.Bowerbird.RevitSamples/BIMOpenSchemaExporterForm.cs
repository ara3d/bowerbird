using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ara3D.Bowerbird.RevitSamples
{
   public partial class BIMOpenSchemaExporterForm : Form
    {
        public BIMOpenSchemaExporterForm()
        {
            InitializeComponent();
        }

        private void linkLabel1_Click(object sender, EventArgs e)
        {
            var url = @"https://github.com/ara3d/bim-open-schema";
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
