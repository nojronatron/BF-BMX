using BFBMX.Service.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;

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
        private readonly ILogger<DiscoveredFilesCollection> _logger;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        public DiscoveredFilesCollection(
            ILogger<DiscoveredFilesCollection> logger)
        {
            _logger = logger;
        }

        public async Task EnqueueAsync(DiscoveredFileModel discoveredFile)
        {
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

            // add the discovered file to the collection and notify subscribers
            Enqueue(discoveredFile);
        }
    }
}
