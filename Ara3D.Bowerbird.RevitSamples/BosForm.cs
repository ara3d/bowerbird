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
    public partial class BosForm : Form
    {
        public BosForm()
        {
            InitializeComponent();
        }

        public void SetIdle(string txt)
        {
            textBoxIdle.BeginInvoke(() => textBoxIdle.Text = txt);
        }

        public void SetHeartBeat(string txt)
        {
            textBoxIdle.BeginInvoke(() => textBoxIdle.Text = txt);
        }

        public void SetId(string txt)
        {
            textBoxIdle.BeginInvoke(() => textBoxId.Text = txt);
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        public event EventHandler OnReset
        {
            add => button1.Click += value;
            remove => button1.Click -= value;
        }
    }
}
