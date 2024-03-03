using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace BFBMX.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
        }

        public new static App Current => (App)Application.Current;


        public IServiceProvider Services { get; }


        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            // inject services here e.g. services.addsingleton<TInterface, TImplementation>();

            // inject viewmodels here as transient services
            services.AddTransient<ViewModels.MainWindowViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
