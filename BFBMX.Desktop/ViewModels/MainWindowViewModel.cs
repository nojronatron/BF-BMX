﻿using BFBMX.Desktop.Collections;
using BFBMX.Desktop.Helpers;
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

        private static readonly object _lock = new();

        private readonly ILogger<MainWindowViewModel> _logger;

        public readonly IDiscoveredFilesCollection _discoveredFiles;

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger,
            IDiscoveredFilesCollection discoveredFilesCollection,
            IMostRecentFilesCollection mostRecentFilesCollection)
        {
            _logger = logger;
            _discoveredFiles = discoveredFilesCollection;
            MostRecentFilesCollection = mostRecentFilesCollection;
            MostRecentItems = new();
            LogfilePath = DesktopEnvFactory.GetBfBmxLogPath();
            ServerNamePort = DesktopEnvFactory.GetServerHostnameAndPort();
        }

        [ObservableProperty]
        public string? _logfilePath;
        [ObservableProperty]
        public string? _serverNamePort;

        [ObservableProperty]
        public string? _alphaStatusMessage;
        [ObservableProperty]
        public string? _bravoStatusMessage;
        [ObservableProperty]
        public string? _charlieStatusMessage;

        [ObservableProperty]
        public IMostRecentFilesCollection _mostRecentFilesCollection;

        [ObservableProperty]
        public List<DiscoveredFileModel> _mostRecentItems;

        /***** Global Monitor Functions *****/
        public async void HandleFileCreatedAsync(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation("File creation detected, waiting 1 second before reading contents.");
            await Task.Delay(1000);
            string? discoveredFilepath = e.FullPath ?? "unknown - check logs!";
            _logger.LogInformation("Discovered file path is {filepath}", discoveredFilepath);
            DiscoveredFileModel newFile = new(discoveredFilepath);
            await _discoveredFiles.EnqueueAsync(newFile);
            lock (_lock)
            {
                MostRecentFilesCollection.AddFirst(newFile);
                MostRecentItems.Clear();
                MostRecentItems = MostRecentFilesCollection.GetList();
            }
            _logger.LogInformation("Enqueued path {discoveredFilepath}", discoveredFilepath);
        }

        public void HandleErrorAlpha(object sender, ErrorEventArgs e)
        {
            string errMsg = e.GetException().Message;
            AlphaStatusMessage = $"Error handling file: {errMsg}";
            _logger.LogInformation("HandleError called: {errmsg}", errMsg);
        }

        public void HandleErrorBravo(object sender, ErrorEventArgs e)
        {
            string errMsg = e.GetException().Message;
            AlphaStatusMessage = $"Error handling file: {errMsg}";
            _logger.LogInformation("HandleError called: {errmsg}", errMsg);
        }

        public void HandleErrorCharlie(object sender, ErrorEventArgs e)
        {
            string errMsg = e.GetException().Message;
            AlphaStatusMessage = $"Error handling file: {errMsg}";
            _logger.LogInformation("HandleError called: {errmsg}", errMsg);
        }

        private bool IsGoodPath(string? directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return false;
            }

            if (Directory.Exists(directoryPath))
            {
                return true;
            }

            return false;
        }

        private void SetStatusMessage(string? monitorName, string? message)
        {
            switch (monitorName)
            {
                case AlphaMonitorName:
                    {
                        AlphaStatusMessage = message;
                        break;
                    }
                case BravoMonitorName:
                    {
                        BravoStatusMessage = message;
                        break;
                    }
                case CharlieMonitorName:
                    {
                        CharlieStatusMessage = message;
                        break;
                    }
                default:
                    {
                        _logger.LogWarning("SetStatusMessage: Monitor name not recognized: {monitorName}", monitorName);
                        break;
                    }
            }
        }

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
                            _logger.LogInformation("ResetMonitor: Alpha Monitor disposed.");
                            AlphaMonitorPathEnabled = true;
                            AlphaMonitorInitialized = false;
                            SetStatusMessage(AlphaMonitorName, "This monitor has been reset.");
                            monitor = null;
                            break;
                        }
                    case BravoMonitorName:
                        {
                            monitor.EnableRaisingEvents = false;
                            monitor.Dispose();
                            _logger.LogInformation("ResetMonitor: Bravo Monitor disposed.");
                            BravoMonitorPathEnabled = true;
                            BravoMonitorInitialized = false;
                            SetStatusMessage(BravoMonitorName, "This monitor has been reset.");
                            monitor = null;
                            break;
                        }
                    case CharlieMonitorName:
                        {
                            monitor.EnableRaisingEvents = false;
                            monitor.Dispose();
                            _logger.LogInformation("ResetMonitor: Charlie Monitor disposed.");
                            //CharlieMonitorPathEnabled = true;
                            CharlieMonitorInitialized = false;
                            SetStatusMessage(CharlieMonitorName, "This monitor has been reset.");
                            monitor = null;
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
                _logger.LogInformation("ResetMonitor: No existing Monitor to dispose, no state(s) were changed.");
            }
        }

        /***** End Global Monitor Functions *****/

        /***** Alpha Monitor Configuration *****/
        private static FSWMonitor? _alphaMonitor;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitAlphaMonitorCommand), nameof(StartAlphaMonitorCommand), nameof(StopAlphaMonitorCommand), nameof(DestroyAlphaMonitorCommand))]
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

        [RelayCommand(CanExecute = nameof(CanInitAlphaMonitor))]
        public void InitAlphaMonitor()
        {
            _logger.LogInformation("InitAlphaMonitor button pressed.");

            if (_alphaMonitor is null)
            {
                _alphaMonitor = FSWatcherFactory.Create(
                    HandleFileCreatedAsync,
                    HandleErrorAlpha,
                    AlphaMonitorPath!,
                    AlphaMonitorName);
            }
            else
            {
                _alphaMonitor.EnableRaisingEvents = false;
                _alphaMonitor.MonitoredPath = AlphaMonitorPath!;
            } 

            try
            {
                AlphaMonitorInitialized = _alphaMonitor!.IsInitialized;
                AlphaMonitorStarted = _alphaMonitor.IsStarted;
                AlphaMonitorStopped = _alphaMonitor.IsStopped;
                AlphaMonitorPathEnabled = false;
                string isOrNotInitialized = AlphaMonitorInitialized ? "successfully" : "not";
                _logger.LogInformation("Alpha Monitor {isOrNotInit} initialized for path: {monitorPath}", 
                    isOrNotInitialized, 
                    AlphaMonitorPath);
            }
            catch (Exception ex)
            {
                SetStatusMessage(AlphaMonitorName, "Unable to initialize! Click Destroy, add the path, then click Initialize.");
                _logger.LogInformation("Alpha Monitor unable to initialize for {monitorPath}, exception msg: {exceptionMsg}",
                                      AlphaMonitorPath, 
                                      ex.Message);
                AlphaMonitorPathEnabled = true;

                if (_alphaMonitor is not null)
                {
                    _alphaMonitor.EnableRaisingEvents = false;
                    _alphaMonitor.Dispose();
                }
            }
        }

        public bool CanInitAlphaMonitor()
        {
            if (IsGoodPath(AlphaMonitorPath))
            {
                if (AlphaMonitorPath!.Equals(BravoMonitorPath)
                    || AlphaMonitorPath.Equals(CharlieMonitorPath))
                {
                    SetStatusMessage(AlphaMonitorName, "Path already set elsewhere. Choose another path.");
                    _logger.LogWarning("CanInitAlphaMonitor: Path not unique. Returning false;");
                    return false;
                }

                if (_alphaMonitor is null)
                {
                    _logger.LogInformation("CanInitAlphaMonitor: Monitor is null and path exists. Returning true.");
                    SetStatusMessage(AlphaMonitorPath, string.Empty);
                    return true;
                }

                _logger.LogInformation("CanInitAlphaMonitor: Monitor is not null but a path exists. Returning false.");
                return false;
            }
            else
            {
                _logger.LogInformation("CanInitAlphaMonitor: Path is null or empty or does not exist. Returning false.");
                return false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartAlphaMonitor))]
        public void StartAlphaMonitor()
        {
            _logger.LogInformation("StartAlphaMonitor: Button presed.");
            _alphaMonitor!.EnableRaisingEvents = true;
            AlphaMonitorPathEnabled = false;
            AlphaMonitorInitialized = _alphaMonitor.IsInitialized;
            AlphaMonitorStarted = _alphaMonitor.IsStarted;
            AlphaMonitorStopped = _alphaMonitor.IsStopped;
            SetStatusMessage(AlphaMonitorName, "Monitor is watching for new files at path.");
            _logger.LogInformation("StartAlphaMonitor: Monitor started for path {monitorPath}", AlphaMonitorPath);
        }

        public bool CanStartAlphaMonitor()
        {
            if (_alphaMonitor is null)
            {
                _logger.LogInformation("CanStartAlphaMonitor: Monitor is null. Returning false.");
                return false;
            }

            if (_alphaMonitor.CanStart())
            {
                _logger.LogInformation("CanStartAlphaMonitor: Monitor is not null and can start. Returning true.");
                return true;
            }

            _logger.LogInformation("CanStartAlphaMonitor: Monitor is not null and any other state. Returning false.");
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopAlphaMonitor))]
        public void StopAlphaMonitor()
        {
            _logger.LogInformation("StopAlphaMonitor button pressed.");
            _alphaMonitor!.EnableRaisingEvents = false;
            AlphaMonitorPathEnabled = false;
            AlphaMonitorInitialized = _alphaMonitor.IsInitialized;
            AlphaMonitorStarted = _alphaMonitor.IsStarted;
            AlphaMonitorStopped = _alphaMonitor.IsStopped;
            SetStatusMessage(AlphaMonitorName, "Monitor no longer watching for new files.");
            _logger.LogInformation("StopAlphaMonitor button: Monitor stopped for path {monitorPath}", AlphaMonitorPath);
        }

        public bool CanStopAlphaMonitor()
        {
            if (_alphaMonitor is null)
            {
                _logger.LogInformation("CanStopAlphaMonitor: Instance is null, return false.");
                return false;
            }

            if (_alphaMonitor.IsStarted)
            {
                _logger.LogInformation("CanStopAlphaMonitor: Monitor is not null and is started. Returning true.");
                return true;
            }

            _logger.LogInformation("CanStopAlphaMonitor: Is in any other state. Returning false.");
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyAlphaMonitor))]
        public void DestroyAlphaMonitor()
        {
            _logger.LogInformation("DestroyAlphaMonitor: Button pressed.");
            if (_alphaMonitor is not null)
            {
                _alphaMonitor.EnableRaisingEvents = false;
                _alphaMonitor.Dispose();
                _alphaMonitor = null;
            }

            AlphaMonitorPathEnabled = true;
            AlphaMonitorInitialized = false;
            AlphaMonitorStarted = false;
            AlphaMonitorStopped = false;
            SetStatusMessage(AlphaMonitorName, "Monitor reset and must be reconfigured.");
            _logger.LogInformation("DestroyAlphaMonitor button: Monitor destroyed.");
        }

        public bool CanDestroyAlphaMonitor()
        {
            if (_alphaMonitor is null)
            {
                _logger.LogInformation("CanDestroyAlphaMonitor: Monitor is null and path is empty. Returning false.");
                return false;
            }

            if (_alphaMonitor is null && !string.IsNullOrWhiteSpace(AlphaMonitorPath))
            {
                _logger.LogInformation("CanDestroyAlphaMonitor: Monitor is null and path has be entered (not validated). Returning false.");
                return false;
            }

            _logger.LogInformation("CanDestroyAlphaMonitor: Monitor exists and is initialized. Returning true.");
            return true;
        }

        /***** End Alpha Monitor Configuration *****/

        // todo: consider how to multiplex the monitor configuration and commands for all 3 monitors.

        /***** Bravo Monitor Configuration *****/
        private static FSWMonitor? _bravoMonitor;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitBravoMonitorCommand), nameof(StartBravoMonitorCommand), nameof(StopBravoMonitorCommand), nameof(DestroyBravoMonitorCommand))]
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

            if (_bravoMonitor is null)
            {
                _bravoMonitor = FSWatcherFactory.Create(
                    HandleFileCreatedAsync,
                    HandleErrorBravo,
                    BravoMonitorPath!,
                    BravoMonitorName);
            }
            else
            {
                _bravoMonitor.EnableRaisingEvents = false;
                _bravoMonitor.MonitoredPath = BravoMonitorPath!;
            }

            try
            {
                BravoMonitorInitialized = _bravoMonitor!.IsInitialized;
                BravoMonitorStarted = _bravoMonitor.IsStarted;
                BravoMonitorStopped = _bravoMonitor.IsStopped;
                BravoMonitorPathEnabled = false;
                string isOrNotInitialized = BravoMonitorInitialized ? "successfully" : "not";
                SetStatusMessage(BravoMonitorName, "Monitor initialized. Click Launch to start monitoring.");
                _logger.LogInformation("Bravo Monitor {isOrNotInit} initialized for path: {monitorPath}",
                    isOrNotInitialized,
                    BravoMonitorPath);
            }
            catch (Exception ex)
            {
                SetStatusMessage(BravoMonitorName, "Unable to initialize! Click Destroy, add the path, then click initialize.");
                _logger.LogInformation("Bravo Monitor unable to initialize for {monitorPath}, exception msg: {exceptionMsg}",
                                       BravoMonitorPath, ex.Message);
                BravoMonitorPathEnabled = true;

                if (_bravoMonitor is not null)
                {
                    _bravoMonitor.EnableRaisingEvents = false;
                    _bravoMonitor.Dispose();
                }
            }
        }

        public bool CanInitBravoMonitor()
        {
            if (IsGoodPath(BravoMonitorPath))
            {
                if (BravoMonitorPath!.Equals(AlphaMonitorPath)
                    || BravoMonitorPath.Equals(CharlieMonitorPath))
                {
                    SetStatusMessage(BravoMonitorName, "Path is already being monitored. Choose another path.");
                    _logger.LogWarning("CanInitBravoMonitor: Path not unique. Returning false.");
                    return false;
                }

                if (_charlieMonitor is null)
                {
                    _logger.LogInformation("CanInitBravoMonitor: Monitor is null and path exists. Returning true.");
                    SetStatusMessage(BravoMonitorName, string.Empty);
                    return true;
                }

                _logger.LogInformation("CanInitBravoMonitor: Monitor is not null but a path exists. Returning false.");
                return false;
            }
            else
            {
                _logger.LogInformation("CanInitBravoMonitor: Path is null or empty or does not exist. Returning false.");
                return false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartBravoMonitor))]
        public void StartBravoMonitor()
        {
            _logger.LogInformation("StartBravoMonitor button pressed.");
            _bravoMonitor!.EnableRaisingEvents = true;
            BravoMonitorPathEnabled = false;
            BravoMonitorInitialized = _bravoMonitor.IsInitialized;
            BravoMonitorStarted = _bravoMonitor.IsStarted;
            BravoMonitorStopped = _bravoMonitor.IsStopped;
            SetStatusMessage(BravoMonitorName, "Monitor is watching for new files at path.");
            _logger.LogInformation("StartBravoMonitor button: Monitor started for path {monitorPath}", BravoMonitorPath);
        }

        public bool CanStartBravoMonitor()
        {
            if (_bravoMonitor is null)
            {
                _logger.LogInformation("CanStartBravoMonitor: Monitor is null. Returning false.");
                return false;
            }

            if (_bravoMonitor.CanStart())
            {
                _logger.LogInformation("CanStartBravoMonitor: Monitor is not null and CanStart(). Returning true.");
                return true;
            }

            _logger.LogInformation("CanStartBravoMonitor: Monitor is not null and any other state. Returning false.");
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopBravoMonitor))]
        public void StopBravoMonitor()
        {
            _logger.LogInformation("StopBravoMonitor button pressed.");
            _bravoMonitor!.EnableRaisingEvents = false;
            BravoMonitorPathEnabled = false;
            BravoMonitorInitialized = _bravoMonitor.IsInitialized;
            BravoMonitorStarted = _bravoMonitor.IsStarted;
            BravoMonitorStopped = _bravoMonitor.IsStopped;
            SetStatusMessage(BravoMonitorName, "Monitor no longer watching for new files.");
            _logger.LogInformation("StopBravoMonitor button: Monitor stopped for path {monitorPath}", BravoMonitorPath);
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
            SetStatusMessage(BravoMonitorName, "Monitor has been reset.");
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

        /***** Charlie Monitor Configuration *****/
        private static FSWMonitor? _charlieMonitor;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitCharlieMonitorCommand), nameof(StartCharlieMonitorCommand), nameof(StopCharlieMonitorCommand), nameof(DestroyCharlieMonitorCommand))]
        public bool _charlieMonitorPathEnabled = true;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitCharlieMonitorCommand), nameof(StartCharlieMonitorCommand), nameof(StopCharlieMonitorCommand), nameof(DestroyCharlieMonitorCommand))]
        public string? _charlieMonitorPath = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitCharlieMonitorCommand), nameof(StartCharlieMonitorCommand), nameof(StopCharlieMonitorCommand), nameof(DestroyCharlieMonitorCommand))]
        public bool _charlieMonitorInitialized = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitCharlieMonitorCommand), nameof(StartCharlieMonitorCommand), nameof(StopCharlieMonitorCommand), nameof(DestroyCharlieMonitorCommand))]
        public bool _charlieMonitorStarted = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(InitCharlieMonitorCommand), nameof(StartCharlieMonitorCommand), nameof(StopCharlieMonitorCommand), nameof(DestroyCharlieMonitorCommand))]
        public bool _charlieMonitorStopped = true;

        /***** End Charlie Monitor Configuration *****/

        /***** Charlie Monitor Commands *****/

        [RelayCommand(CanExecute = nameof(CanInitCharlieMonitor))]
        public void InitCharlieMonitor()
        {
            _logger.LogInformation("InitCharlieMonitor button pressed.");

            if (_charlieMonitor is null)
            {
                _charlieMonitor = FSWatcherFactory.Create(
                    HandleFileCreatedAsync,
                    HandleErrorCharlie,
                    CharlieMonitorPath!,
                    CharlieMonitorName);
            }
            else
            {
                _charlieMonitor.EnableRaisingEvents = false;
                _charlieMonitor.MonitoredPath = CharlieMonitorPath!;
            }

            try
            {
                CharlieMonitorInitialized = _charlieMonitor!.IsInitialized;
                CharlieMonitorStarted = _charlieMonitor.IsStarted;
                CharlieMonitorStopped = _charlieMonitor.IsStopped;
                CharlieMonitorPathEnabled = false;
                string isOrNotInitialized = CharlieMonitorInitialized ? "successfully" : "not";
                SetStatusMessage(CharlieMonitorName, "Monitor initialized. Click Launch to start monitoring.");
                _logger.LogInformation("Charlie Monitor {isOrNotInit} initialized for path: {monitorPath}",
                    isOrNotInitialized,
                    CharlieMonitorPath);
            }
            catch (Exception ex)
            {
                SetStatusMessage(CharlieMonitorName, "Unable to initialize! Click Destroy, add the path, then click initialize.");
                _logger.LogInformation("Charlie Monitor unable to initialize for {monitorPath}, exception msg: {exceptionMsg}",
                                       CharlieMonitorPath, ex.Message);
                CharlieMonitorPathEnabled = true;

                if (_charlieMonitor is not null)
                {
                    _charlieMonitor.EnableRaisingEvents = false;
                    _charlieMonitor.Dispose();
                }
            }
        }

        public bool CanInitCharlieMonitor()
        {
            if (IsGoodPath(CharlieMonitorPath))
            {
                if (CharlieMonitorPath!.Equals(AlphaMonitorPath)
                    || CharlieMonitorPath.Equals(BravoMonitorPath))
                {
                    SetStatusMessage(CharlieMonitorName, "Path already set elsewhere. Choose another path.");
                    _logger.LogWarning("CanInitCharlieMonitor: Path not unique. Returning false.");
                    return false;
                }

                if (_charlieMonitor is null)
                {
                    _logger.LogInformation("CanInitCharlieMonitor: Monitor is null and path exists. Returning true.");
                    SetStatusMessage(CharlieMonitorName, string.Empty);
                    return true;
                }

                _logger.LogInformation("CanInitCharlieMonitor: Monitor is not null but a path exists. Returning false.");
                return false;
            }
            else
            {
                _logger.LogInformation("CanInitCharlieMonitor: Path is null or empty or does not exist. Returning false.");
                return false;
            }
        }

        [RelayCommand(CanExecute = nameof(CanStartCharlieMonitor))]
        public void StartCharlieMonitor()
        {
            _logger.LogInformation("StartCharlieMonitor button pressed.");
            _charlieMonitor!.EnableRaisingEvents = true;
            CharlieMonitorPathEnabled = false;
            CharlieMonitorInitialized = _charlieMonitor.IsInitialized;
            CharlieMonitorStarted = _charlieMonitor.IsStarted;
            CharlieMonitorStopped = _charlieMonitor.IsStopped;
            SetStatusMessage(CharlieMonitorName, "Monitor is watching for new files at path.");
            _logger.LogInformation("StartCharlieMonitor button: Monitor started for path {monitorPath}", CharlieMonitorPath);
        }

        public bool CanStartCharlieMonitor()
        {
            if (_charlieMonitor is null)
            {
                _logger.LogInformation("CanStartCharlieMonitor: Monitor is null. Returning false.");
                return false;
            }

            if (_charlieMonitor.CanStart())
            {
                _logger.LogInformation("CanStartCharlieMonitor: Monitor is not null and CanStart(). Returning true.");
                return true;
            }

            _logger.LogInformation("CanStartCharlieMonitor: Monitor is not null and any other state. Returning false.");
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanStopCharlieMonitor))]
        public void StopCharlieMonitor()
        {
            _logger.LogInformation("StopCharlieMonitor button pressed.");
            _charlieMonitor!.EnableRaisingEvents = false;
            CharlieMonitorPathEnabled = false;
            CharlieMonitorInitialized = _charlieMonitor.IsInitialized;
            CharlieMonitorStarted = _charlieMonitor.IsStarted;
            CharlieMonitorStopped = _charlieMonitor.IsStopped;
            SetStatusMessage(CharlieMonitorName, "Monitor no longer watching for new files.");
            _logger.LogInformation("StopCharlieMonitor button: Monitor stopped for path {monitorPath}", CharlieMonitorPath);
        }

        public bool CanStopCharlieMonitor()
        {
            if (_charlieMonitor is null)
            {
                _logger.LogInformation("CanStopCharlieMonitor: Monitor is null. Returning false");
                return false;
            }

            if (_charlieMonitor.IsStarted)
            {
                _logger.LogInformation("CanStopCharlieMonitor: Monitor is not null and IsStarted. Returning true.");
                return true;
            }

            _logger.LogInformation("CanStopCharlieMonitor: Is in any other state. Returning false.");
            return false;
        }

        [RelayCommand(CanExecute = nameof(CanDestroyCharlieMonitor))]
        public void DestroyCharlieMonitor()
        {
            _logger.LogInformation("DestroyCharlieMonitor button pressed.");

            if (_charlieMonitor is not null)
            {
                _charlieMonitor.EnableRaisingEvents = false;
                // No need to reset MonitoredPath since this instance will be disposed
                _charlieMonitor.Dispose();
                _charlieMonitor = null;
            }

            CharlieMonitorPathEnabled = true;
            //CharlieMonitorPath = string.Empty;
            CharlieMonitorInitialized = false;
            CharlieMonitorStarted = false;
            CharlieMonitorStopped = false;
            SetStatusMessage(CharlieMonitorName, "Monitor reset and must be reconfigured.");
            _logger.LogInformation("DestroyCharlieMonitor button: Monitor destroyed.");
        }

        public bool CanDestroyCharlieMonitor()
        {
            if (_charlieMonitor is null && string.IsNullOrWhiteSpace(CharlieMonitorPath))
            {
                _logger.LogInformation("CanDestroyCharlieMonitor: Monitor is null and path is empty. Returning false.");
                return false;
            }

            if (_charlieMonitor is null && !string.IsNullOrWhiteSpace(CharlieMonitorPath))
            {
                _logger.LogInformation("CanDestroyCharlieMonitor: Monitor is null and path has been entered (not validated). Returning false.");
                return false;
            }

            _logger.LogInformation("CanDestroyCharlieMonitor: Monitor exists and is initialized. Returning true.");
            return true;
        }

        /***** End Charlie Monitor Commands *****/

    }
}
