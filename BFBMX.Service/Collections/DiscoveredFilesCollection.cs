using BFBMX.Service.Models;
using System.Collections.Concurrent;

namespace BFBMX.Service.Collections
{
    /// <summary>
    /// Concurrent Queue datastructure with a maximum item limit.
    /// </summary>
    public class DiscoveredFilesCollection : ConcurrentQueue<DiscoveredFileModel>
    {
        public int MaxItems { get; } = 6;

        public DiscoveredFilesCollection()
        {
        }

        public new void Enqueue(DiscoveredFileModel item)
        {
            base.Enqueue(item);

            while (base.Count > this.MaxItems)
            {
                base.TryDequeue(out DiscoveredFileModel? _);
            }
        }

        public DiscoveredFilesCollection(IEnumerable<DiscoveredFileModel> collection) : base(collection)
        {
        }
    }
}
