using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;
using BFBMX.Desktop.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
                // override LogLevelToTextOutputMap configuration if wanted
                string baseProfilePath = Environment.GetEnvironmentVariable("USERPROFILE") ?? @"C:\";
                string basePath = baseProfilePath == @"C:\" ? baseProfilePath : Path.Combine(baseProfilePath, "Documents");
                string? envLogPath = Environment.GetEnvironmentVariable("BFBMX_FOLDER_NAME");
                string logPath = string.IsNullOrEmpty(envLogPath) ? "BFBMX" : envLogPath;
                string logfileName = "bfbmx-desktop.log";
                config.LogfilePath = Path.Combine(basePath, logPath, logfileName);
            });
            // add custom logging provider to the IoC container/collection
            services.AddSingleton<ILoggerProvider, DesktopLoggerProvider>();

            // inject services here e.g. services.addsingleton<TInterface, TImplementation>();
            
            // inject viewmodels here as transient services
            services.AddTransient<ViewModels.MainWindowViewModel>();

            return services.BuildServiceProvider();
        }
    }
}
