using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ara3D.Bowerbird.Core;
using Ara3D.Domo;
using Ara3D.Services;

namespace Ara3D.Bowerbird.Wpf.Net48
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BowerBirdDemoApp App { get; } = new BowerBirdDemoApp();

        public MainWindow()
        {
            var repo = App.Service.Repo;
            DataContext = repo;
            InitializeComponent();
            App.Service.Repo.OnModelChanged(model => ModelChanged(model.Value));
            App.Service.Compiler.Compile();
        }

        public void NewLogEntry(IModel<LogEntry> entry)
        {
            ListView0.Items.Add(entry.Value.Text);
        }

        public void ModelChanged(BowerbirdDataModel dataModel)
        {
            ListView1.ItemsSource = dataModel.Files;
            ListView2.ItemsSource = dataModel.Diagnostics;
            ListView3.ItemsSource = dataModel.Types;
        }
    }
}
