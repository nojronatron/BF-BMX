using BFBMX.Desktop.Helpers;
using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.Diagnostics;
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


        public MainWindowViewModel(ILogger<MainWindowViewModel> logger,
            IFileProcessor fileProcessor,
            IApiClient apiClient)
        {
            _logger = logger;
            _fileProcessor = fileProcessor;
            _apiClient = apiClient;
            MostRecentItems = new();
            LogfilePath = DesktopEnvFactory.GetBfBmxLogPath();
            ServerNamePort = DesktopEnvFactory.GetServerHostnameAndPort();
        }

        /***** Global UI Properties *****/

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
        public ObservableCollection<DiscoveredFileModel> _mostRecentItems;

        /// <summary>
        /// Launches the log file path in Windows Explorer.
        /// </summary>
        /// <remarks>
        /// This method opens the log file path in Windows Explorer using the default file explorer application.
        /// </remarks>
        /// <seealso cref="CanLaunchLogfilePath"/>
        [RelayCommand(CanExecute = nameof(CanLaunchLogfilePath))]
        public void LaunchLogfilePath()
        {
#pragma warning disable CS8604 // Possible null reference argument.
            Process.Start("explorer.exe", LogfilePath);
#pragma warning restore CS8604 // Possible null reference argument.
        }


        /// <summary>
        /// Determines whether the log file path is valid and can be launched.
        /// </summary>
        /// <returns>True if the log file path is valid; otherwise, false.</returns>
        public bool CanLaunchLogfilePath()
        {
            return IsGoodPath(LogfilePath);
        }

        /// <summary>
        /// Event Handler for all three FileSystemWatcher calls when a file created event occurs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void HandleFileCreatedAsync(object sender, FileSystemEventArgs e)
        {
            string? discoveredFilepath = e.FullPath ?? "unknown - check logs!";
            DiscoveredFileModel newFile = new(discoveredFilepath);
            DateTime fileTimeStamp = newFile.FileTimeStamp;
            _logger.LogInformation("Discovered file path {filepath} creation stamp {filedatetime} for processing.", discoveredFilepath, fileTimeStamp);

            // insert the new item into the collection on the UI Thread (WPF requirement)
            App.Current.Dispatcher.Invoke(() =>
            {
                MostRecentItems.Insert(0, newFile);
                // update the UI by adding an item to the observable collection and maintaining a max of 12 items
                int mostRecentItemsCount = MostRecentItems.Count;

                if (mostRecentItemsCount > 12)
                {
                    MostRecentItems.RemoveAt(mostRecentItemsCount - 1);
                }
            });

            _logger.LogInformation("Path {discoveredFilepath} sent to screen for display.", discoveredFilepath);

            // get machine name for File Processor
            string? hostname = Environment.MachineName;
            string machineName = string.IsNullOrWhiteSpace(hostname) ? "Unknown" : hostname;

            // process the file for bib records
            await Task.Delay(1000);
            _logger.LogInformation("Sending file {newFile} created at {fileTimeStamp} to file processor.", newFile.FullFilePath, newFile.FileTimeStamp);
            WinlinkMessageModel winlinkMessage = _fileProcessor.ProcessWinlinkMessageFile(newFile.FileTimeStamp, machineName, newFile.FullFilePath);

            // No bib reports found, log and return
            if (winlinkMessage is null || winlinkMessage.BibRecords.Count <= 0)
            {
                _logger.LogInformation("No bibrecords found in winlinkMessage {msgName}.", newFile.FullFilePath);
                return;
            }

            // send bib reports to API and log to file
            string logPathAndFilename = Path.Combine(DesktopEnvFactory.GetBfBmxLogPath(), DesktopEnvFactory.GetBibRecordsLogFileName());
            _logger.LogInformation("Sending {wlMsgId} bib data to logfile and API.", winlinkMessage.WinlinkMessageId);
            bool wroteToFile = _fileProcessor.WriteWinlinkMessageToFile(winlinkMessage, logPathAndFilename);
            bool postedToApi = await _apiClient.PostWinlinkMessageAsync(winlinkMessage.ToJsonString());
            _logger.LogInformation("Message ID: {wlMsgId} => Wrote to file? {wroteToFile}. Posted to API? {postedToApi}. Items stored in memory: {collectionCount}.", winlinkMessage.WinlinkMessageId, wroteToFile, postedToApi, winlinkMessage.BibRecords.Count);
        }

        /// <summary>
        /// The first File Watcher will call this Event Handler if there was an error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleErrorAlpha(object sender, ErrorEventArgs e)
        {
            string errMsg = e.GetException().Message;
            SetStatusMessage(AlphaMonitorName, $"Error handling file: {errMsg}");
            _logger.LogInformation("HandleError called by Monitor #1: {errmsg}", errMsg);
        }

        /// <summary>
        /// The second File Watcher will call this Event Handler if there was an error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleErrorBravo(object sender, ErrorEventArgs e)
        {
            string errMsg = e.GetException().Message;
            SetStatusMessage(BravoMonitorName, $"Error handling file: {errMsg}");
            _logger.LogInformation("HandleError called by Monitor #2: {errmsg}", errMsg);
        }

        /// <summary>
        /// The third File Watcher will call this Event Handler if there was an error.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void HandleErrorCharlie(object sender, ErrorEventArgs e)
        {
            string errMsg = e.GetException().Message;
            SetStatusMessage(CharlieMonitorName, $"Error handling file: {errMsg}");
            _logger.LogInformation("HandleError called by Monitor #3: {errmsg}", errMsg);
        }

        /// <summary>
        /// Tests for null or whitespace directory path, then checks that the path exists and returns true only if both are true.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <returns></returns>
        private static bool IsGoodPath(string? directoryPath)
        {
            return !string.IsNullOrWhiteSpace(directoryPath) && Directory.Exists(directoryPath);
        }

        /// <summary>
        /// Handle helper method to set the status message for the appropriate monitor.
        /// </summary>
        /// <param name="monitorName"></param>
        /// <param name="message"></param>
        private void SetStatusMessage(string? monitorName, string? message)
        {
            DateTime dateTimeStamp = DateTime.Now;
            string statusMessage = $"{dateTimeStamp:M} at {dateTimeStamp:HH:mm:ss} - {message}";

            switch (monitorName)
            {
                case AlphaMonitorName:
                    {
                        AlphaStatusMessage = statusMessage;
                        break;
                    }
                case BravoMonitorName:
                    {
                        BravoStatusMessage = statusMessage;
                        break;
                    }
                case CharlieMonitorName:
                    {
                        CharlieStatusMessage = statusMessage;
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
                SetStatusMessage(AlphaMonitorName, "Monitor initialized. Click Start to begin monitoring.");
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

        /// <summary>
        /// Determines if the Alpha Monitor can be initialized based on the path being set.
        /// </summary>
        /// <returns>True if the path is set and exists.</returns>
        public bool CanInitAlphaMonitor()
        {
            if (IsGoodPath(AlphaMonitorPath))
            {
                if (AlphaMonitorPath!.Equals(BravoMonitorPath)
                    || AlphaMonitorPath.Equals(CharlieMonitorPath))
                {
                    SetStatusMessage(AlphaMonitorName, "Path already on another Monitor. Choose another path.");
                    _logger.LogWarning("CanInitAlphaMonitor: Path not unique. Returning false;");
                    return false;
                }

                if (_alphaMonitor is null)
                {
                    _logger.LogInformation("CanInitAlphaMonitor: Monitor is null and path exists. Returning true.");
                    SetStatusMessage(AlphaMonitorName, "Path exists and monitor can be initialized.");
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

        /// <summary>
        /// Determines if the Alpha Monitor can be started based on its current state.
        /// </summary>
        /// <returns>True if not null, is not already running, and the path matches.</returns>
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

        /// <summary>
        /// Determines if Alpha Monitor can be stopped based on its current state.
        /// </summary>
        /// <returns>True if not null, is already set to raise events, and the path matches.</returns>
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

        /// <summary>
        /// Determines if Alpha Monitor can be destroyed based on its current state.
        /// This is more lenient than the other Can methods so the operator can recover from a bad or misbehaving Monitor.
        /// </summary>
        /// <returns>True in nearly any case except for null.</returns>
        public bool CanDestroyAlphaMonitor()
        {
            if (_alphaMonitor is null && string.IsNullOrWhiteSpace(AlphaMonitorPath))
            {
                _logger.LogInformation("CanDestroyAlphaMonitor: Monitor is null and path is empty. Returning false.");
                return false;
            }

            if (_alphaMonitor is null && !string.IsNullOrWhiteSpace(AlphaMonitorPath))
            {
                _logger.LogInformation("CanDestroyAlphaMonitor: Monitor is null and path has been entered (not validated). Returning false.");
                return false;
            }

            _logger.LogInformation("CanDestroyAlphaMonitor: Monitor exists and is initialized. Returning true.");
            return true;
        }

        /***** End Alpha Monitor Configuration *****/

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
                SetStatusMessage(BravoMonitorName, "Monitor initialized. Click Start to begin monitoring.");
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
                    SetStatusMessage(BravoMonitorName, "Path already on another Monitor. Choose another path.");
                    _logger.LogWarning("CanInitBravoMonitor: Path not unique. Returning false.");
                    return false;
                }

                if (_bravoMonitor is null)
                {
                    _logger.LogInformation("CanInitBravoMonitor: Monitor is null and path exists. Returning true.");
                    SetStatusMessage(BravoMonitorName, "Path exists and monitor can be initialized.");
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
            if (_bravoMonitor is not null)
            {
                _bravoMonitor.EnableRaisingEvents = false;
                _bravoMonitor.Dispose();
                _bravoMonitor = null;
            }

            BravoMonitorPathEnabled = true;
            BravoMonitorInitialized = false;
            BravoMonitorStarted = false;
            BravoMonitorStopped = false;
            SetStatusMessage(BravoMonitorName, "Monitor reset and must be reconfigured.");
            _logger.LogInformation("DestroyBravoMonitor button: Monitor destroyed.");
        }

        /// <summary>
        /// Determines if Bravo Monitor can be destroyed based on its current state.
        /// This is more lenient than the other Can methods so the operator can recover from a bad or misbehaving Monitor.
        /// </summary>
        /// <returns>True in nearly any case exept for null.</returns>
        public bool CanDestroyBravoMonitor()
        {
            if (_bravoMonitor is null && string.IsNullOrWhiteSpace(BravoMonitorPath))
            {
                _logger.LogInformation("CanDestroyBravoMonitor: Monitor is null and path is empty. Returning false.");
                return false;
            }

            if (_bravoMonitor is null && !string.IsNullOrWhiteSpace(BravoMonitorPath))
            {
                _logger.LogInformation("CanDestroyBravoMonitor: Monitor is null and path has been entered (not validated). Returning false.");
                return false;
            }

            _logger.LogInformation("CanDestroyBravoMonitor: Monitor exists and is initialized. Returning true.");
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
                SetStatusMessage(CharlieMonitorName, "Monitor initialized. Click Start to begin monitoring.");
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
                    SetStatusMessage(CharlieMonitorName, "Path already on another Monitor. Choose another path.");
                    _logger.LogWarning("CanInitCharlieMonitor: Path not unique. Returning false.");
                    return false;
                }

                if (_charlieMonitor is null)
                {
                    _logger.LogInformation("CanInitCharlieMonitor: Monitor is null and path exists. Returning true.");
                    SetStatusMessage(CharlieMonitorName, "Path exists and monitor can be initialized.");
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
                _charlieMonitor.Dispose();
                _charlieMonitor = null;
            }

            CharlieMonitorPathEnabled = true;
            CharlieMonitorInitialized = false;
            CharlieMonitorStarted = false;
            CharlieMonitorStopped = false;
            SetStatusMessage(CharlieMonitorName, "Monitor reset and must be reconfigured.");
            _logger.LogInformation("DestroyCharlieMonitor button: Monitor destroyed.");
        }

        /// <summary>
        /// Determines whether Charlie Monitor can be destroyed based on its current state.
        /// </summary>
        /// <returns>True in nearly every case except if null.</returns>
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
