using BFBMX.Service.Collections;
using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BFBMX.Desktop.ViewModels
{
    public partial class MainWindowViewModel : ObservableValidator
    {
        private readonly ILogger<MainWindowViewModel> _logger;

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
                    // get data from file
                    var fileData = FileProcessor.GetFileData(discoveredFilepath);

                    // get winlink id from file
                    StringBuilder sb = new();
                    foreach(var fd in fileData)
                    {
                        sb.Append(fd).Append('\n');
                    }

                    // todo: fix FileProcessor so ViewModel doesnt have to do it
                    var wlID = FileProcessor.GetMessageId(sb.ToString());

                    // process bib data
                    List<FlaggedBibRecordModel> processedBibs = new();
                    bool resultSucceeded = FileProcessor.ProcessBibs(processedBibs, fileData);

                    // write winilnk message to logfile
                    if (resultSucceeded && processedBibs.Count > 0)
                    {
                        WinlinkMessageModel wlRecord = WinlinkMessageModel.GetWinlinkMessageInstance(wlID, DateTime.Now,
                                                                                                     Environment.MachineName,
                                                                                                     processedBibs);

                        var options = new JsonSerializerOptions()
                        {
                            NumberHandling = JsonNumberHandling.AllowReadingFromString,
                            PropertyNameCaseInsensitive = true,
                            WriteIndented = true
                        };

                        var wlrJsonPretty = JsonSerializer.Serialize < WinlinkMessageModel > (wlRecord, options);
                        _logger.LogInformation("***** Winlink Message *****\n{msgData}\n***** End Winlink Message *****", wlrJsonPretty);
                    }
                    else
                    {
                        _logger.LogInformation("No bib records found in message Winlink ID {wlId}.", wlID);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogInformation("Error enqueuing path {discoveredPath}", discoveredFilepath);
                    _logger.LogInformation("Error enqueuing path continued: {exceptionMsg}", ex.Message);
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


        /// <summary>
        /// Reset FileSystemWatcher instance to clear monitoring configuration and free memory.
        /// </summary>
        /// <param name="monitor"></param>
        public void ResetMonitor(FSWMonitor? monitor)
        {
            if (monitor is not null)
            {
                monitor.EnableRaisingEvents = false;
                monitor.Dispose();
                _logger.LogInformation("ResetMonitor: An existing Alpha Monitor was disposed.");
            }
            else
            {
                _logger.LogInformation("ResetMonitor: No existing Alpha Monitor to dispose.");
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
                _alphaMonitor = FSWatcherFactory.Create(HandleFileCreatedAsync, HandleError, AlphaMonitorPath!);
                AlphaMonitorPathEnabled = false;
                AlphaMonitorInitialized = _alphaMonitor!.IsInitialized;
                string isOrNotInitialized = AlphaMonitorInitialized ? "successfully" : "not";
                _logger.LogInformation("Alpha Monitor {isOrNotInit} initialized, for path: {monitorPath}", isOrNotInitialized, AlphaMonitorPath);
            }
            catch (Exception ex)
            {
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
                _logger.LogInformation("StartAlphaMonitor: Monitor {startOrNot} started for path {monitorPath}", startOrNot, AlphaMonitorPath);
            }
            catch (Exception ex)
            {
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
                _logger.LogInformation("StartAlphaMonitor: Monitor {stopOrNot} started for path {monitorPath}", stopOrNot, AlphaMonitorPath);
            }
            catch (Exception ex)
            {
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
