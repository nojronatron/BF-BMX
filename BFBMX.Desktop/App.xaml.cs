using System.Configuration;
using System.Data;
using System.Windows;
using BFBMX.Service.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            // add console logging
            services.AddLogging();
            services.AddSingleton<Helpers.DesktopLogger>();
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            // inject services here e.g. services.addsingleton<TInterface, TImplementation>();
            services.AddTransient(sp => new LoggerFactory().CreateLogger("BFBMX.Desktop"));
            
            // inject viewmodels here as transient services
            services.AddTransient<ViewModels.MainWindowViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
