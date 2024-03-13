using BFBMX.Desktop.Helpers;
using BFBMX.Service.Collections;
using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BFBMX.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableValidator
    {
        private ILogger<MainWindowViewModel> _logger;

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger)
        {
            _logger = logger;
        }

        [ObservableProperty]
        public string? _statusMessageLabel;
        [ObservableProperty]
        public DiscoveredFilesCollection? _discoveredFiles = new();

        /***** Global Monitor Functions *****/
        public async void HandleFileCreatedAsync(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("HandleFileCreatedAsync called.");

            await Task.Run(() =>
            {
                string? discoveredFilepath = e.FullPath ?? "unknown - check logs!";
                string msg = $"HandleFileCreatedAsync: Discovered file at {discoveredFilepath}";
                try
                {
                    DiscoveredFileModel newFile = new(discoveredFilepath);
                    DiscoveredFiles!.Enqueue(newFile);
                    _logger.LogInformation("Enqueued path {discoveredFilepath}", discoveredFilepath);
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Error enqueuing path {discPath}", discoveredFilepath);
                    _logger.LogInformation("Error enqueuing path continued: {msg}", ex.Message);
                }
            });
        }

        public void HandleError(object sender, ErrorEventArgs e)
        {
            string? errorMessage = e.GetException().Message;
            string msg = $"HandleError: {errorMessage}";
            StatusMessageLabel = msg;
            _logger.LogInformation("HandleError called: {errMsg}", errorMessage);
        }

        /***** End Global Monitor Functions *****/

        /***** Alpha Monitor Configuration *****/
        //CancellationTokenSource alphaCancellationToken = new();
        private static FileSystemWatcher? _alphaMonitor;

        [ObservableProperty]
        public bool? _alphaMonitorPathEnabled = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitAlphaMonitorCommand))]
        public string? _alphaMonitorPath;

        [RelayCommand(CanExecute = nameof(CanInitAlphaMonitor))]
        public void InitAlphaMonitor()
        {
            _logger.LogInformation("Initialize Alpha Monitor button pressed.");
            if (_alphaMonitor is not null)
            {
                _alphaMonitor.EnableRaisingEvents = false;
                _alphaMonitor.Dispose();
                _logger.LogInformation("An existing Alpha Monitor was disposed.");
            }

            try
            {
                _alphaMonitor = FSWatcherFactory.Create(HandleFileCreatedAsync, HandleError, AlphaMonitorPath!);
                AlphaMonitorPathEnabled = false;
                CanStartAlphaMonitor();
                CanDestroyAlphaMonitor();
                _logger.LogInformation("Alpha Monitor initialized for {0}", AlphaMonitorPath);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Alpha Monitor unable to initialize for {0}, exception msg: {1}", 
                    AlphaMonitorPath, ex.Message);
                AlphaMonitorPathEnabled = true;
            }
        }

        public bool CanInitAlphaMonitor()
        {
            // so long as path is valid, a new or existing monitor can be initialized
            return !string.IsNullOrWhiteSpace(AlphaMonitorPath) && Directory.Exists(AlphaMonitorPath);
        }

        [RelayCommand(CanExecute = nameof(CanStartAlphaMonitor))]
        public void StartAlphaMonitor()
        {
            try
            {
                _alphaMonitor!.EnableRaisingEvents = true;
                _logger.LogInformation("Alpha Monitor started for {0}", AlphaMonitorPath);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Alpha Monitor unable to enable raising events for {0}, exception msg: {1}",
                                 AlphaMonitorPath, ex.Message);
            }
        }

        public bool CanStartAlphaMonitor()
        {
            _logger.LogInformation("CanStartAlphaMonitor called.");
            return _alphaMonitor is not null 
                    && _alphaMonitor.EnableRaisingEvents == false 
                    && _alphaMonitor.Path == AlphaMonitorPath;
        }

        [RelayCommand(CanExecute = nameof(CanStopAlphaMonitor))]
        public void StopAlphaMonitor()
        {
            _logger.LogInformation("Stop Alpha Monitor button pressed.");

        }

        public bool CanStopAlphaMonitor()
        {
            _logger.LogInformation("CanStopAlphaMonitor called.");
            return _alphaMonitor is not null 
                && _alphaMonitor.EnableRaisingEvents == true 
                && _alphaMonitor.Path == AlphaMonitorPath;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyAlphaMonitor))]
        public void DestroyAlphaMonitor()
        {
            _logger.LogInformation("Destroy Alpha Monitor button pressed.");

        }

        public bool CanDestroyAlphaMonitor()
        {
            _logger.LogInformation("CanDestroyAlphaMonitor called.");
            return (_alphaMonitor is not null)
                    && (_alphaMonitor.EnableRaisingEvents == false)
                    && (_alphaMonitor.Path == AlphaMonitorPath);
        }
        /***** End Alpha Monitor Configuration *****/

        /***** Bravo Monitor Configuration *****/
        [RelayCommand(CanExecute = nameof(CanInitBravoMonitor))]
        public void InitBravoMonitor()
        {

        }

        public bool CanInitBravoMonitor()
        {
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStartBravoMonitor))]
        public void StartBravoMonitor()
        {

        }

        public bool CanStartBravoMonitor()
        {
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopBravoMonitor))]
        public void StopBravoMonitor()
        {

        }

        public bool CanStopBravoMonitor()
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyBravoMonitor))]
        public void DestroyBravoMonitor()
        {

        }

        public bool CanDestroyBravoMonitor()
        {
            return true;
        }
        /***** End Bravo Monitor Commands *****/

        /***** Charlie Monitor Commands *****/
        [RelayCommand(CanExecute = nameof(CanInitCharlieMonitor))]
        public void InitCharlieMonitor()
        {

        }

        public bool CanInitCharlieMonitor()
        {
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStartCharlieMonitor))]
        public void StartCharlieMonitor()
        {

        }

        public bool CanStartCharlieMonitor()
        {
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopCharlieMonitor))]
        public void StopCharlieMonitor()
        {

        }

        public bool CanStopCharlieMonitor()
        {
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyCharlieMonitor))]
        public void DestroyCharlieMonitor()
        {

        }

        public bool CanDestroyCharlieMonitor()
        {
            return true;
        }
        /***** End Charlie Monitor Commands *****/

    }
}
