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

        public int MaxItems { get; } = 6;

        public DiscoveredFilesCollection(ILogger<DiscoveredFilesCollection> logger,
            IApiClient apiClient)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task EnqueueAsync(DiscoveredFileModel item)
        {
            Enqueue(item);
            WinlinkMessageModel winlinkMessage = FileProcessor.ProcessWinlinkMessageFile(item.FileTimeStamp, Environment.MachineName, item.FullFilePath);
            if (winlinkMessage is not null && winlinkMessage.BibRecords.Count > 0)
            {
                // todo: address these hanging, unused, promised variables
                bool wroteToFile = await FileProcessor.WriteWinlinkMessageToFile(winlinkMessage, Path.Combine(DesktopEnvFactory.GetBfBmxLogPath(), DesktopEnvFactory.GetBibRecordsLogFileName()));
                bool postedToApi = await _apiClient.PostWinlinkMessageAsync(winlinkMessage.ToJsonString());
                _logger.LogInformation("DiscoveredFilesCollection: EnqueueAync: MaxItems: {maxitems}. Item count: {count}.", MaxItems, Count);
            }
            else
            {
                _logger.LogInformation("DiscoveredFilesCollection: EnqueueAsync: No bibrecords found in winlinkMessage.");
            }
        }
    }
}
