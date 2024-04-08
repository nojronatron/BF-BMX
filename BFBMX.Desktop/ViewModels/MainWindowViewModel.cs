using BFBMX.Desktop.Collections;
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
        private readonly IFileProcessor _fileProcessor;
        private readonly IApiClient _apiClient;

        public readonly IDiscoveredFilesCollection _discoveredFiles;

        public MainWindowViewModel(ILogger<MainWindowViewModel> logger,
            IFileProcessor fileProcessor,
            IApiClient apiClient,
            IDiscoveredFilesCollection discoveredFilesCollection,
            IMostRecentFilesCollection mostRecentFilesCollection)
        {
            _logger = logger;
            _fileProcessor = fileProcessor;
            _apiClient = apiClient;
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

            // moved from DiscoveredFilesCollection

            // get machine name for File Processor
            string? hostname = Environment.MachineName;
            string machineName = string.IsNullOrWhiteSpace(hostname) ? "Unknown" : hostname;

            // process the file for bib records
            WinlinkMessageModel winlinkMessage = _fileProcessor.ProcessWinlinkMessageFile(newFile.FileTimeStamp, machineName, newFile.FullFilePath);

            // write the non-empty Winlink Message to a file and post it to the API, or do nothing if no bib records were found
            // todo: consider moving the following code to FileProcessor
            if (winlinkMessage is null || winlinkMessage.BibRecords.Count <= 0)
            {
                _logger.LogInformation("No bibrecords found in winlinkMessage.");
                return;
            }

            string logPathAndFilename = Path.Combine(DesktopEnvFactory.GetBfBmxLogPath(), DesktopEnvFactory.GetBibRecordsLogFileName());
            bool wroteToFile = _fileProcessor.WriteWinlinkMessageToFile(winlinkMessage, logPathAndFilename);
            bool postedToApi = await _apiClient.PostWinlinkMessageAsync(winlinkMessage.ToJsonString());
            _logger.LogInformation("Wrote to file? {wroteToFile}. Posted to API? {postedToApi}. Items stored in memory: {collectionCount}.", wroteToFile, postedToApi, winlinkMessage.BibRecords.Count);
            // end code move
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

        private static bool IsGoodPath(string? directoryPath)
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
                _alphaMonitor = FSWatcherFactory.Create(HandleFileCreatedAsync, HandleErrorAlpha, AlphaMonitorPath!, AlphaMonitorName);
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
                _bravoMonitor = FSWatcherFactory.Create(HandleFileCreatedAsync, HandleErrorBravo, BravoMonitorPath!, BravoMonitorName);
                BravoMonitorPathEnabled = false;
                BravoMonitorInitialized = _bravoMonitor!.IsInitialized;
                string isOrNotInitialized = BravoMonitorInitialized ? "successfully" : "not";
                SetStatusMessage(BravoMonitorName, "Monitor initialized.");
                _logger.LogInformation("Bravo Monitor {isOrNotInit} initialized for path: {monitorPath}", isOrNotInitialized, BravoMonitorPath);
            }
            catch (Exception ex)
            {
                SetStatusMessage(BravoMonitorName, "Unable to initialize! Try the Destroy button, then input the path again.");
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
                SetStatusMessage(BravoMonitorName, $"Monitor {isOrNot} started for path.");
                _logger.LogInformation("StartBravoMonitor: Monitor {isOrNot} started for path {monitorPath}", isOrNot, BravoMonitorPath);
            }
            else
            {
                BravoMonitorStarted = false;
                SetStatusMessage(BravoMonitorName, "Unable to start monitor! Check logs then try Destroy or Initialize instead.");
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
                    SetStatusMessage(CharlieMonitorName, "Path is already being monitored. Choose another path.");
                    _logger.LogWarning("CanInitCharlieMonitor: Path not unique. Returning false.");
                    return false;
                }

                if (_charlieMonitor is null)
                {
                    _logger.LogInformation("CanInitCharlieMonitor: Monitor is null and path exists. Returning true.");
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
