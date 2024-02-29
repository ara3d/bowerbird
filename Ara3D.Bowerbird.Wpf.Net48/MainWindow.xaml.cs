using System.Windows;

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
