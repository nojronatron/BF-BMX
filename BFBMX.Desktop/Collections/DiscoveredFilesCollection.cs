using BFBMX.Desktop.Helpers;
using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;

namespace BFBMX.Desktop.Collections
{
    /// <summary>
    /// Concurrent Queue datastructure with a maximum item limit.
    /// </summary>
    public class DiscoveredFilesCollection :
        ConcurrentQueue<DiscoveredFileModel>,
        INotifyCollectionChanged,
        INotifyPropertyChanged,
        IDiscoveredFilesCollection
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<DiscoveredFilesCollection> _logger;
        private readonly IFileProcessor _fileProcessor;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public DiscoveredFilesCollection(
            ILogger<DiscoveredFilesCollection> logger,
            IApiClient apiClient,
            IFileProcessor fileProcessor)
        {
            _apiClient = apiClient;
            _logger = logger;
            _fileProcessor = fileProcessor;
        }

        public async Task EnqueueAsync(DiscoveredFileModel discoveredFile)
        {
            // todo: rewrite EnqueueAsync so it does not handle any non-collection activities (due to async complexity)
            if (discoveredFile is null)
            {
                _logger.LogWarning("Received a null Discovered File. Ignoring.");
                return;
            }

            if (this.Contains(discoveredFile))
            {
                _logger.LogWarning("Ignoring duplicate Discovered File {discoveredFile}.", discoveredFile.FullFilePath);
                return;
            }

            // get machine name for File Processor
            string? hostname = Environment.MachineName;
            string machineName = string.IsNullOrWhiteSpace(hostname) ? "Unknown" : hostname;

            // add the discovered file to the collection and notify subscribers
            Enqueue(discoveredFile);

            // process the file for bib records
            WinlinkMessageModel winlinkMessage = _fileProcessor.ProcessWinlinkMessageFile(discoveredFile.FileTimeStamp, machineName, discoveredFile.FullFilePath);

            // write the non-empty Winlink Message to a file and post it to the API, or do nothing if no bib records were found
            // todo: consider moving the following code to FileProcessor
            if (winlinkMessage is not null && winlinkMessage.BibRecords.Count > 0)
            {
                string logPathAndFilename = Path.Combine(DesktopEnvFactory.GetBfBmxLogPath(), DesktopEnvFactory.GetBibRecordsLogFileName());
                bool wroteToFile = _fileProcessor.WriteWinlinkMessageToFile(winlinkMessage, logPathAndFilename);
                bool postedToApi = await _apiClient.PostWinlinkMessageAsync(winlinkMessage.ToJsonString());
                _logger.LogInformation("Wrote to file? {wroteToFile}. Posted to API? {postedToApi}. Items stored in memory: {collectionCount}.", wroteToFile, postedToApi, Count);
            }
            else
            {
                _logger.LogInformation("No bibrecords found in winlinkMessage.");
            }
        }
    }
}
