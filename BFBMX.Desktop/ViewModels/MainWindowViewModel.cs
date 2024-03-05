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
#pragma warning disable CS8601 // Possible null reference assignment.
        private ILogger<DesktopLogger> _logger = App.Current.Services.GetService<ILogger<DesktopLogger>>();
#pragma warning restore CS8601 // Possible null reference assignment.

        [ObservableProperty]
        public string? _statusMessageLabel;
        [ObservableProperty]
        public DiscoveredFilesCollection? _discoveredFiles = new();

        /***** Global Monitor Functions *****/
        public async void HandleFileCreatedAsync(object sender, FileSystemEventArgs e)
        {
            await Task.Run(() =>
            {
                string? discoveredFilepath = e.FullPath ?? "unknown - check logs!";
                string msg = $"HandleFileCreatedAsync: Discovered file at {discoveredFilepath}";
                try
                {
                    DiscoveredFileModel newFile = new(discoveredFilepath);
                    DiscoveredFiles!.Enqueue(newFile);
                    _logger.LogInformation($"Enqueued path {discoveredFilepath}", discoveredFilepath);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error enqueuing path {0}", discoveredFilepath);
                    _logger.LogError($"Error enqueuing path continued: {0}", ex.Message);
                }
            });
        }

        public void HandleError(object sender, ErrorEventArgs e)
        {
            string? errorMessage = e.GetException().Message;
            string msg = $"HandleError: {errorMessage}";
            StatusMessageLabel = msg;
            _logger.LogWarning($"HandleError called: {0}", errorMessage);
        }
        /***** End Global Monitor Functions *****/

        /***** Alpha Monitor Configuration *****/
        //CancellationTokenSource alphaCancellationToken = new();
        private static FileSystemWatcher? _alphaMonitor;

        [ObservableProperty]
        public bool? _alphaMonitorPathEnabled = true;

        [ObservableProperty]
        public string? _alphaMonitorPath;

        [RelayCommand(CanExecute = nameof(CanInitAlphaMonitor))]
        public void InitAlphaMonitor()
        {
            if (_alphaMonitor is not null)
            {
                _alphaMonitor.EnableRaisingEvents = false;
                _alphaMonitor.Dispose();
            }

            try
            {
                _alphaMonitor = FSWatcherFactory.Create(HandleFileCreatedAsync, HandleError, AlphaMonitorPath!);
                AlphaMonitorPathEnabled = false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Alpha Monitor unable to initialize for {0}, exception msg: {1}", 
                    AlphaMonitorPath, ex.Message);
                AlphaMonitorPathEnabled = true;
            }
        }

        public bool CanInitAlphaMonitor()
        {
            // so long as path is valid, a new or existing monitor can be initialized
            return !string.IsNullOrWhiteSpace(AlphaMonitorPath) && File.Exists(AlphaMonitorPath);
        }

        [RelayCommand(CanExecute = nameof(CanStartAlphaMonitor))]
        public void StartAlphaMonitor()
        {
            try
            {
                _alphaMonitor!.EnableRaisingEvents = true;
                _logger.LogInformation($"Alpha Monitor started for {0}", AlphaMonitorPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Alpha Monitor unable to enable raising events for {0}, exception msg: {1}",
                                 AlphaMonitorPath, ex.Message);
            }
        }

        public bool CanStartAlphaMonitor()
        {
            return _alphaMonitor is not null 
                    && _alphaMonitor.EnableRaisingEvents == false 
                    && _alphaMonitor.Path == AlphaMonitorPath;
        }

        [RelayCommand(CanExecute = nameof(CanStopAlphaMonitor))]
        public void StopAlphaMonitor()
        {

        }

        public bool CanStopAlphaMonitor()
        {
            return _alphaMonitor is not null 
                && _alphaMonitor.EnableRaisingEvents == true 
                && _alphaMonitor.Path == AlphaMonitorPath;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyAlphaMonitor))]
        public void DestroyAlphaMonitor()
        {

        }

        public bool CanDestroyAlphaMonitor()
        {
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
