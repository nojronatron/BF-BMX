using BFBMX.Desktop.Helpers;
using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;

namespace BFBMX.Desktop.Collections
{
    public interface IDiscoveredFilesCollection
    {
        int MaxItems { get; }

        Task EnqueueAsync(DiscoveredFileModel item);
    }

    /// <summary>
    /// Concurrent Queue datastructure with a maximum item limit.
    /// </summary>
    public class DiscoveredFilesCollection : ConcurrentQueue<DiscoveredFileModel>, IDiscoveredFilesCollection
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger<DiscoveredFilesCollection> _logger;
        private readonly IFileProcessor _fileProcessor;

        public int MaxItems { get; } = 6;

        public DiscoveredFilesCollection(
            ILogger<DiscoveredFilesCollection> logger,
            IApiClient apiClient,
            IFileProcessor fileProcessor)
        {
            _apiClient = apiClient;
            _logger = logger;
            _fileProcessor = fileProcessor;
        }

        public async Task EnqueueAsync(DiscoveredFileModel item)
        {
            if( this.Any(x => x.FullFilePath == item.FullFilePath))
            {
                _logger.LogWarning("DiscoveredFilesCollection EnqueueAsync: File {filepath} is a DUPLICATE. Storing in memory but contents might not get added to database.", item.FullFilePath);
            }

            Enqueue(item);
            WinlinkMessageModel winlinkMessage = _fileProcessor.ProcessWinlinkMessageFile(item.FileTimeStamp, Environment.MachineName, item.FullFilePath);

            if (winlinkMessage is not null && winlinkMessage.BibRecords.Count > 0)
            {
                bool wroteToFile = _fileProcessor.WriteWinlinkMessageToFile(winlinkMessage, Path.Combine(DesktopEnvFactory.GetBfBmxLogPath(), DesktopEnvFactory.GetBibRecordsLogFileName()));
                bool postedToApi = await _apiClient.PostWinlinkMessageAsync(winlinkMessage.ToJsonString());
                _logger.LogInformation("DiscoveredFilesCollection EnqueueAync: Wrote to file? {wroteToFile}. Posted to API? {postedToApi}. Items in collection: {collectionCount}.", wroteToFile, postedToApi, Count);
            }
            else
            {
                _logger.LogInformation("DiscoveredFilesCollection EnqueueAsync: No bibrecords found in winlinkMessage.");
            }
        }
    }
}
