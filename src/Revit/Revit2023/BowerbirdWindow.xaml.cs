using System.Windows;
using Ara3D.Bowerbird.Core;

namespace Ara3D.Bowerbird.Revit
{
    public partial class BowerbirdWindow : Window
    {
        public BowerbirdRevitApp App { get; }

        public BowerbirdWindow(BowerbirdRevitApp app)
        {
            App = app;
            InitializeComponent();
        }

        private void CompileMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            App.Compile();
        }

        private void ClearMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            ConsoleListBox.Items.Clear();
        }
    }
}
