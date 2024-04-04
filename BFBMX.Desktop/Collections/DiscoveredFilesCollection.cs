using BFBMX.Desktop.Helpers;
using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using System.Collections.Concurrent;
using System.IO;

namespace BFBMX.Desktop.Collections
{
    public interface IDiscoveredFilesCollection
    {
        int MaxItems { get; }

        void Enqueue(DiscoveredFileModel item);
    }

    /// <summary>
    /// Concurrent Queue datastructure with a maximum item limit.
    /// </summary>
    public class DiscoveredFilesCollection : ConcurrentQueue<DiscoveredFileModel>, IDiscoveredFilesCollection
    {
        public int MaxItems { get; } = 6;

        public DiscoveredFilesCollection()
        {
        }

        public new void Enqueue(DiscoveredFileModel item)
        {
            base.Enqueue(item);

            var winlinkMessage = FileProcessor.ProcessWinlinkMessageFile(item.FileTimeStamp, Environment.MachineName, item.FullFilePath);

            // write winilnkMessage to logfile
            Task.Run(async () => {
                return await FileProcessor.WriteWinlinkMessageToFile(winlinkMessage, Path.Combine(DesktopEnvFactory.GetBfBmxLogPath(), DesktopEnvFactory.GetBibRecordsLogFileName()));
            });

            // todo: send winlinkMessage to API Helper

            //while (base.Count > this.MaxItems)
            //{
            //    base.TryDequeue(out DiscoveredFileModel? _);
            //}
        }

        public DiscoveredFilesCollection(IEnumerable<DiscoveredFileModel> collection) : base(collection)
        {
        }
    }
}
