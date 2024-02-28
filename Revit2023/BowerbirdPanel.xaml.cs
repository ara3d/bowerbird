using System.Windows.Controls;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Domo;
using Ara3D.Services;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Wpf.Net48.Lib
{
    public partial class BowerbirdPanel : UserControl
    {
        public BowerbirdPanel()
        {
            InitializeComponent();
        }

        public void RegisterServices(IBowerbirdService bowerbird, ILoggingService logging)
        {
            logging.Repository.OnModelAdded(LogEntryAdded);
            bowerbird.Repository.OnModelChanged(DataModelChanged);
        }

        public void DataModelChanged(IModel<BowerbirdDataModel> model)
        {
            ListView1.ItemsSource = model.Value.Files;
            ListView2.ItemsSource = model.Value.Diagnostics;
            ListView3.ItemsSource = model.Value.TypeNames;
        }

        public void LogEntryAdded(IModel<LogEntry> entry)
        {
            ListView0.Items.Add(entry.Value.Message);
        }
    }
}
