using BFBMX.Desktop.Collections;
using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.IO;

namespace BFBMX.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableValidator
    {
        const string AlphaMonitorName = "Alpha";
        const string BravoMonitorName = "Bravo";
        const string CharlieMonitorName = "Charlie";

        private readonly ILogger<MainWindowViewModel> _logger;

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger,
            IDiscoveredFilesCollection discoveredFilesCollection)
        {
            _logger = logger;
            _discoveredFiles = discoveredFilesCollection;
        }

        [ObservableProperty]
        public string? _alphaStatusMessage;
        [ObservableProperty]
        public string? _bravoStatusMessage;
        [ObservableProperty]
        public string? _charlieStatusMessage = "Monitor #3 is in development.";

        [ObservableProperty]
        public IDiscoveredFilesCollection _discoveredFiles;

        /***** Global Monitor Functions *****/
        public async void HandleFileCreatedAsync(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("HandleFileCreatedAsync called.");
            string? discoveredFilepath = e.FullPath ?? "unknown - check logs!";
            // put the discovered filepath info into the queue and be done
            DiscoveredFileModel newFile = new(discoveredFilepath);
            await _discoveredFiles.EnqueueAsync(newFile);
            _logger.LogInformation("Enqueued path {discoveredFilepath}", discoveredFilepath);
        }

        public void HandleError(object sender, ErrorEventArgs e)
        {
            string? errorMessage = e.GetException().Message;
            string msg = $"HandleError: {errorMessage}";
            AlphaStatusMessage = msg;
            _logger.LogInformation("HandleError called: {errMsg}", errorMessage);
        }

        /***** End Global Monitor Functions *****/

        /***** Alpha Monitor Configuration *****/
        private static FSWMonitor? _alphaMonitor;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitAlphaMonitorCommand), nameof(DestroyAlphaMonitorCommand))]
        public bool _alphaMonitorPathEnabled = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitAlphaMonitorCommand), nameof(StartAlphaMonitorCommand), nameof(StopAlphaMonitorCommand), nameof(DestroyAlphaMonitorCommand))]
        public string _alphaMonitorPath = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitAlphaMonitorCommand), nameof(StartAlphaMonitorCommand), nameof(StopAlphaMonitorCommand), nameof(DestroyAlphaMonitorCommand))]
        public bool _alphaMonitorInitialized = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitAlphaMonitorCommand), nameof(StartAlphaMonitorCommand), nameof(StopAlphaMonitorCommand), nameof(DestroyAlphaMonitorCommand))]
        public bool _alphaMonitorStarted = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitAlphaMonitorCommand), nameof(StartAlphaMonitorCommand), nameof(StopAlphaMonitorCommand), nameof(DestroyAlphaMonitorCommand))]
        public bool _alphaMonitorStopped = true;

        /// <summary>
        /// Reset FileSystemWatcher instance to clear monitoring configuration and free memory.
        /// </summary>
        /// <param name="monitor"></param>
        public void ResetMonitor(FSWMonitor? monitor)
        {
            if (monitor is not null)
            {

                switch (monitor.GetName())
                {
                    case AlphaMonitorName:
                        {
                            monitor.EnableRaisingEvents = false;
                            monitor.Dispose();
                            _logger.LogInformation("ResetMonitor: An existing Alpha Monitor was disposed.");
                            AlphaMonitorPathEnabled = true;
                            AlphaMonitorInitialized = false;
                            AlphaStatusMessage = "This monitor has been reset.";
                            break;
                        }
                    case BravoMonitorName:
                        {
                            monitor.EnableRaisingEvents = false;
                            monitor.Dispose();
                            _logger.LogInformation("ResetMonitor: An existing Bravo Monitor was disposed.");
                            BravoMonitorPathEnabled = true;
                            BravoMonitorInitialized = false;
                            BravoStatusMessage = "This monitor has been reset.";
                            break;
                        }
                    case CharlieMonitorName:
                        {
                            //monitor.EnableRaisingEvents = false;
                            //monitor.Dispose();
                            //_logger.LogInformation("ResetMonitor: An existing Charlie Monitor was disposed.");
                            //CharlieMonitorPathEnabled = true;
                            //CharlieMonitorInitialized = false;
                            //AlphaStatusMessage = "This monitor has been reset.";
                            break;
                        }
                    default:
                        {
                            _logger.LogWarning("ResetMonitor: Monitor name not recognized: {monitorName}", monitor.GetName());
                            break;
                        }
                }
            }
            else
            {
                _logger.LogInformation("ResetMonitor: No existing Monitor to dispose. All monitors states unchanged.");
            }
        }

        [RelayCommand(CanExecute = nameof(CanInitAlphaMonitor))]
        public void InitAlphaMonitor()
        {
            _logger.LogInformation("InitAlphaMonitor button pressed.");

            if (_alphaMonitor is not null)
            {
                _logger.LogInformation("InitAlphaMonitor: Resetting existing Alpha Monitor.");
                ResetMonitor(_alphaMonitor);
            }

            try
            {
                _alphaMonitor = FSWatcherFactory.Create(HandleFileCreatedAsync, HandleError, AlphaMonitorPath!, AlphaMonitorName);
                AlphaMonitorPathEnabled = false;
                AlphaMonitorInitialized = _alphaMonitor!.IsInitialized;
                string isOrNotInitialized = AlphaMonitorInitialized ? "successfully" : "not";
                AlphaStatusMessage = "Monitor initialized.";
                _logger.LogInformation("Alpha Monitor {isOrNotInit} initialized for path: {monitorPath}", isOrNotInitialized, AlphaMonitorPath);
            }
            catch (Exception ex)
            {
                AlphaStatusMessage = "Unable to initialize! Try the Destroy button, then input the path again.";
                _logger.LogInformation("Alpha Monitor unable to initialize for {monitorPath}, exception msg: {exceptionMsg}",
                                      AlphaMonitorPath, ex.Message);
                AlphaMonitorPathEnabled = true;
            }
        }

        public bool CanInitAlphaMonitor()
        {
            if (string.IsNullOrWhiteSpace(AlphaMonitorPath))
            {
                _logger.LogInformation("CanInitAlphaMonitor: Alpha Monitor path is null or empty, cannot initialize.");
                return false;
            }
            else
            {
                _logger.LogInformation("CanInitAlphaMonitor: Returning true for path {alphaMonPath}", AlphaMonitorPath);
                return true;
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartAlphaMonitor))]
        public void StartAlphaMonitor()
        {
            try
            {
                _alphaMonitor!.EnableRaisingEvents = true;
                AlphaMonitorStarted = _alphaMonitor!.IsStarted;
                string startOrNot = AlphaMonitorStarted ? "successfully" : "not";
                AlphaStatusMessage = $"Monitor {startOrNot} started for path.";
                _logger.LogInformation("StartAlphaMonitor: Monitor {startOrNot} started for path {monitorPath}", startOrNot, AlphaMonitorPath);
            }
            catch (Exception ex)
            {
                AlphaStatusMessage = "Unable to start monitor! Check the logs and re-try initializing";
                _logger.LogInformation("StartAlphaMonitor: Unable to enable raising events for {monitorPath}, exception msg: {exceptionMsg}",
                                 AlphaMonitorPath, ex.Message);
            }
        }

        public bool CanStartAlphaMonitor()
        {
            if (_alphaMonitor is null)
            {
                _logger.LogInformation("CanStartAlphaMonitor: Instance in null, return false.");
                return false;
            }

            if (_alphaMonitor.EnableRaisingEvents == true)
            {
                _logger.LogInformation("CanStartAlphaMonitor: Already enabled to raise events, return false.");
                return false;
            }

            if (_alphaMonitor.MonitoredPath != AlphaMonitorPath)
            {
                _logger.LogInformation("CanStartAlphaMonitor: Monitor Path {monActualPath} neq {monConfiguredPath}. Return false.",
                    _alphaMonitor.MonitoredPath,
                    AlphaMonitorPath);
                return false;
            }

            _logger.LogInformation("CanStartAlphaMonitor: Instance not null, not already set to raise events, and path matches. Returning true.");
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanStopAlphaMonitor))]
        public void StopAlphaMonitor()
        {
            try
            {
                _alphaMonitor!.EnableRaisingEvents = false;
                AlphaMonitorStarted = _alphaMonitor!.IsStarted;
                string stopOrNot = AlphaMonitorStarted ? "not" : "successfully";
                AlphaStatusMessage = "Monitor stopped successfully.";
                _logger.LogInformation("StopAlphaMonitor: Monitor {stopOrNot} stopped for path {monitorPath}", stopOrNot, AlphaMonitorPath);
            }
            catch (Exception ex)
            {
                AlphaStatusMessage = "Something bad happened, check the logs to diagnose.";
                _logger.LogInformation("StopAlphaMonitor: Unable to disable raising events for {monitorPath}, exception msg: {exceptionMsg}",
                                      AlphaMonitorPath,
                                      ex.Message);
            }
        }

        public bool CanStopAlphaMonitor()
        {
            if (_alphaMonitor is null)
            {
                _logger.LogInformation("CanStopAlphaMonitor: Instance is null, return false.");
                return false;
            }

            if (_alphaMonitor.EnableRaisingEvents == false)
            {
                _logger.LogInformation("CanStopAlphaMonitor: Already disabled to raise events, returning true.");
                return false;
            }

            if (_alphaMonitor.MonitoredPath != AlphaMonitorPath)
            {
                _logger.LogInformation("CanStopAlphaMonitor: Monitor Path {monActualPath} neq {monConfiguredPath}. Return false.",
                    _alphaMonitor.MonitoredPath,
                    AlphaMonitorPath);
                return false;
            }

            _logger.LogInformation("CanStopAlphaMonitor: Instance not null, is raising events, and path matches. Return true.");
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyAlphaMonitor))]
        public void DestroyAlphaMonitor()
        {
            _logger.LogInformation("DestroyAlphaMonitor: Button pressed.");
            ResetMonitor(_alphaMonitor);
            AlphaStatusMessage = "Monitor has been reset.";
            AlphaMonitorPathEnabled = true;
        }

        public bool CanDestroyAlphaMonitor()
        {
            if (_alphaMonitor is null)
            {
                _logger.LogInformation("CanDestroyAlphaMonitor: Alpha Monitor already destroyed. Try to initialize new, instead.");
                return false;
            }

            if (_alphaMonitor.MonitoredPath != AlphaMonitorPath)
            {
                _logger.LogInformation("CanDestroyAlphaMonitor: Monitor Path {monActualPath} neq {monConfiguredPath} but will return true anyway.",
                                      _alphaMonitor.MonitoredPath,
                                      AlphaMonitorPath);
            }

            if (_alphaMonitor.EnableRaisingEvents == false)
            {
                _logger.LogInformation("CanDestroyAlphaMonitor: Monitor is not raising events but will return true anyway.");
            }

            return true;
        }

        /***** End Alpha Monitor Configuration *****/

        // todo: consider how to multiplex the monitor configuration and commands for all 3 monitors.

        /***** Bravo Monitor Configuration *****/
        private static FSWMonitor? _bravoMonitor;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitBravoMonitorCommand), nameof(DestroyBravoMonitorCommand))]
        public bool _bravoMonitorPathEnabled = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitBravoMonitorCommand), nameof(StartBravoMonitorCommand), nameof(StopBravoMonitorCommand), nameof(DestroyBravoMonitorCommand))]
        public string? _bravoMonitorPath = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitBravoMonitorCommand), nameof(StartBravoMonitorCommand), nameof(StopBravoMonitorCommand), nameof(DestroyBravoMonitorCommand))]
        public bool _bravoMonitorInitialized = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitBravoMonitorCommand), nameof(StartBravoMonitorCommand), nameof(StopBravoMonitorCommand), nameof(DestroyBravoMonitorCommand))]
        public bool _bravoMonitorStarted = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitBravoMonitorCommand), nameof(StartBravoMonitorCommand), nameof(StopBravoMonitorCommand), nameof(DestroyBravoMonitorCommand))]
        public bool _bravoMonitorStopped = true;

        [RelayCommand(CanExecute = nameof(CanInitBravoMonitor))]
        public void InitBravoMonitor()
        {
            _logger.LogInformation("InitBravoMonitor button pressed.");

            if (_bravoMonitor is not null)
            {
                _logger.LogInformation("InitBravoMonitor: Resetting existing Bravo Monitor.");
                ResetMonitor(_bravoMonitor);
            }

            try
            {
                _bravoMonitor = FSWatcherFactory.Create(HandleFileCreatedAsync, HandleError, BravoMonitorPath!, BravoMonitorName);
                BravoMonitorPathEnabled = false;
                BravoMonitorInitialized = _bravoMonitor!.IsInitialized;
                string isOrNotInitialized = BravoMonitorInitialized ? "successfully" : "not";
                BravoStatusMessage = "Monitor initialized.";
                _logger.LogInformation("Bravo Monitor {isOrNotInit} initialized for path: {monitorPath}", isOrNotInitialized, BravoMonitorPath);
            }
            catch (Exception ex)
            {
                BravoStatusMessage = "Unable to initialize! Try the Destroy button, then input the path again.";
                _logger.LogInformation("Bravo Monitor unable to initialize for {monitorPath}, exception msg: {exceptionMsg}",
                                      BravoMonitorPath, ex.Message);
                BravoMonitorPathEnabled = true;
            }
        }

        public bool CanInitBravoMonitor()
        {
            if (string.IsNullOrWhiteSpace(BravoMonitorPath))
            {
                _logger.LogInformation("CanInitBravoMonitor: Bravo Monitor path is null or empty, cannot initialize.");
                return false;
            }
            else
            {
                _logger.LogInformation("CanInitBravoMonitor: Returning true for path {BravoMonPath}", BravoMonitorPath);
                return true;
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartBravoMonitor))]
        public void StartBravoMonitor()
        {
            if (_bravoMonitor is not null && _bravoMonitor.CanStart())
            {
                _bravoMonitor.EnableRaisingEvents = true;
                BravoMonitorStarted = _bravoMonitor.IsStarted;
                string isOrNot = _bravoMonitor.IsStarted ? "successfully" : "not";
                BravoStatusMessage = $"Monitor {isOrNot} started for path.";
                _logger.LogInformation("StartBravoMonitor: Monitor {isOrNot} started for path {monitorPath}", isOrNot, BravoMonitorPath);
            }
            else
            {
                BravoMonitorStarted = false;
                BravoStatusMessage = "Unable to start monitor! Check logs then try Destroy or Initialize instead.";
                _logger.LogWarning("StartBravoMonitor: Unable to enable raising events for {monitorpath}!", BravoMonitorPath);
            }
        }

        public bool CanStartBravoMonitor()
        {
            if (_bravoMonitor is not null && _bravoMonitor.CanStart())
            {
                _logger.LogInformation("CanStartBravoMonitor: BravoMonitor is not null, is not enabled to raise events, and path matches. Returning true.");
                return true;
            }
            else
            {
                bool isNull = _bravoMonitor is null;
                bool isEnabled = _bravoMonitor?.EnableRaisingEvents == true;
                bool pathMatches = _bravoMonitor?.MonitoredPath == BravoMonitorPath;
                _logger.LogWarning("CanStartBravoMonitor: Monitor is null? {isnull}; isEnabled? {isenabled}; pathMatches? {pathmatches}.",
                    isNull, isEnabled, pathMatches);
            }

            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopBravoMonitor))]
        public void StopBravoMonitor()
        {
            if (_bravoMonitor is not null && _bravoMonitor.IsStarted)
            {
                _bravoMonitor.EnableRaisingEvents = false;
                BravoMonitorStarted = _bravoMonitor.IsStarted;
                string isOrNot = _bravoMonitor.IsStarted ? "not" : "successfully";
                BravoStatusMessage = "Monitor stopped successfully.";
                _logger.LogInformation("StopBravoMonitor: Monitor {isOrNot} stopped for path {monitorPath}", isOrNot, AlphaMonitorPath);
            }
            else
            {
                BravoMonitorStarted = _bravoMonitor?.IsStarted ?? false;
                BravoStatusMessage = "Unable to stop monitor! Check logs then try Destroy or Initialize instead.";
                _logger.LogWarning("StopBravoMonitor: Unable to disable raising events for {monitorpath}!", BravoMonitorPath);
            }
        }

        public bool CanStopBravoMonitor()
        {
            if (_bravoMonitor is not null && _bravoMonitor.IsStarted)
            {
                _logger.LogInformation("CanStopBravoMonitor: BravoMonitor is not null, is enabled to raise events, and path matches. Returning true.");
                return true;
            }
            else
            {
                bool isNull = _bravoMonitor is null;
                bool isEnabled = _bravoMonitor?.EnableRaisingEvents == true;
                bool pathMatches = _bravoMonitor?.MonitoredPath == BravoMonitorPath;
                _logger.LogWarning("CanStopBravoMonitor: Monitor is null? {isnull}; isEnabled? {isenabled}; pathMatches? {pathmatches}.",
                                       isNull, isEnabled, pathMatches);
            }

            return false;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyBravoMonitor))]
        public void DestroyBravoMonitor()
        {
            _logger.LogInformation("DestroyBravoMonitor: Button pressed.");
            ResetMonitor(_bravoMonitor);
            BravoStatusMessage = "Monitor has been reset.";
            BravoMonitorPathEnabled = true;
        }

        public bool CanDestroyBravoMonitor()
        {
            if (_bravoMonitor is null)
            {
                _logger.LogInformation("CanDestroyBravoMonitor: Bravo Monitor already destroyed. Try to initialize new, instead.");
                return false;
            }

            if (_bravoMonitor.MonitoredPath != BravoMonitorPath)
            {
                _logger.LogInformation("CanDestroyBravoMonitor: Monitor Path {monActualPath} neq {monConfiguredPath} but will return true anyway.",
                                      _bravoMonitor.MonitoredPath,
                                      BravoMonitorPath);
            }

            if (_bravoMonitor.EnableRaisingEvents == false)
            {
                _logger.LogInformation("CanDestroyBravoMonitor: Monitor is not raising events but will return true anyway.");
            }

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
