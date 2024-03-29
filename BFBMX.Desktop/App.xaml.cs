using System.IO;
using System.Windows;
using BFBMX.Desktop.Collections;
using BFBMX.Desktop.Helpers;
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
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the current App instance in use.
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the IServiceProvider instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for teh application.
        /// </summary>
        /// <returns></returns>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();

            // add custom logging service configuration
            services.Configure<DesktopLoggerConfiguration>(config =>
            {
                config.EventId = 0;
                config.LogfilePath = Path.Combine(DesktopEnvFactory.GetBfBmxLogPath(), DesktopEnvFactory.GetBfBmxLogFileName()); //(basePath, logPath, logfileName);
            });
            // add custom logging provider to the IoC container/collection
            services.AddSingleton<ILoggerProvider, DesktopLoggerProvider>();

            // inject services here e.g. services.AddSingleton<TInterface, TImplementation>() etc
            string targetUrl = DesktopEnvFactory.GetServerHostnameAndPort();
            services.AddSingleton(new ApiClientSettings(targetUrl));
            services.AddSingleton<IApiClient, ApiClient>();
            services.AddSingleton<IDiscoveredFilesCollection, DiscoveredFilesCollection>();

            // inject viewmodels here as transient services
            services.AddTransient<ViewModels.MainWindowViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
