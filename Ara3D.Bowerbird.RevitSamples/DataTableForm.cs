using System;
using System.Collections;
using System.Reflection;
using System.Windows.Forms;

namespace Ara3D.Bowerbird.RevitSamples
{
    public class DataTableForm : System.Windows.Forms.Form
    {
        public DataGridView DataGridView;
        public DataTableBuilder Builder;

        public DataTableForm(DataTableBuilder builder)
        {
            Builder = builder;
            DataGridView = new DataGridView
            {
                Dock = DockStyle.Fill,
                DataSource = builder.DataTable,
                ReadOnly = true,
            };
            foreach (DataGridViewColumn col in DataGridView.Columns)
            {
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            }
            typeof(DataGridView).InvokeMember("DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null, DataGridView, new object[] { true });
            Controls.Add(DataGridView);
        }

        public void AddItemsToDataTable(IEnumerable items)
        {
            BeginInvoke(new Action(() => Builder.AddRows(items)));
        }
    }
}