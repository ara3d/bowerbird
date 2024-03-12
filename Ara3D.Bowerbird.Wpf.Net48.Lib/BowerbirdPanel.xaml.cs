using System.Windows.Controls;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Domo;
using Ara3D.Services;
using Ara3D.Logging;

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
        }

        public void LogEntryAdded(IModel<LogEntry> entry)
        {
        }
    }
}
