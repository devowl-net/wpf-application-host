using System.Windows;

using WpfApplicationHost.ViewModels;

namespace WpfApplicationHost.Views
{
    /// <summary>
    /// Interaction logic for <see cref="MainWindow"/>.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// <see cref="MainWindow"/> Constructor.
        /// </summary>
        public MainWindow()
        {
            DataContext = new MainWindowViewModel();
            InitializeComponent();
        }
    }
}