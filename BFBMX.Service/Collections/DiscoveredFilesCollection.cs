using BFBMX.Service.Models;
using System.Collections.Concurrent;

namespace BFBMX.Service.Collections
{
    /// <summary>
    /// Concurrent Queue datastructure with a maximum item limit.
    /// </summary>
    public class DiscoveredFilesCollection : ConcurrentQueue<DiscoveredFileModel>
    {
        private int maxItems = 6;
        public int MaxItems { get { return maxItems; } }

        public DiscoveredFilesCollection()
        {
        }

        public new void Enqueue(DiscoveredFileModel item)
        {
            base.Enqueue(item);

            while (base.Count > this.maxItems)
            {
                base.TryDequeue(out DiscoveredFileModel? _);
            }
        }

        public DiscoveredFilesCollection(IEnumerable<DiscoveredFileModel> collection) : base(collection)
        {
        }
    }
}
