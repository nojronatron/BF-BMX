using BFBMX.Service.Models;
using System.Collections.Concurrent;

namespace BFBMX.Service.Collections
{
    public class DiscoveredFilesCollection : ConcurrentQueue<DiscoveredFileModel>
    {
        public DiscoveredFilesCollection()
        {
        }

        public DiscoveredFilesCollection(IEnumerable<DiscoveredFileModel> collection) : base(collection)
        {
        }
    }
}
