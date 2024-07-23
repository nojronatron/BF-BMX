using BFBMX.Desktop.ViewModels;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace BFBMX.Desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<MainWindowViewModel>();
        }
    }
}