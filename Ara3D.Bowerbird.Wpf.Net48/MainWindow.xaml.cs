using System.Windows;
using Ara3D.Bowerbird.Interfaces;
using Ara3D.Domo;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Wpf.Net48
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public BowerbirdDemoApp Bowerbird { get; } = new BowerbirdDemoApp();

        public MainWindow()
        {
            InitializeComponent();
            BowerbirdPanel.RegisterServices(Bowerbird.Service, Bowerbird.Logger);
            Bowerbird.Service.Compile();
        }
    }
}
