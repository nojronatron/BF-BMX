using BFBMX.Service.Models;
using Microsoft.Extensions.Logging;

namespace BFBMX.Desktop.Collections
{
    public interface IMostRecentFilesCollection
    {
        int Count { get; }

        void AddFirst(DiscoveredFileModel discoveredFile);
        List<DiscoveredFileModel> GetList();
        bool IsEmpty();
        DiscoveredFileModel RemoveLast();
    }

    public class MostRecentFilesCollection : IMostRecentFilesCollection
    {
        private readonly ILogger<MostRecentFilesCollection> _logger;

        private const int MAXCOUNT = 12;
        private DiscoveredFileLinkedListNode? Head { get; set; } = null;

        public int Count { get; private set; } = 0;

        public MostRecentFilesCollection(ILogger<MostRecentFilesCollection> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Returns true if this collection is empty.
        /// </summary>
        /// <returns>true if empty, false if not</returns>
        public bool IsEmpty()
        {
            return Head is null;
        }

        /// <summary>
        /// Enqueue a new item to the front of this collection, removing any items beyond MAXCOUNT.
        /// </summary>
        /// <param name="discoveredFile"></param>
        public void AddFirst(DiscoveredFileModel discoveredFile)
        {
            DiscoveredFileLinkedListNode newNode = new();
            newNode.Value = discoveredFile;
            newNode.Next = Head;
            Head = newNode;
            Count++;

            while (Count > MAXCOUNT)
            {
                _logger.LogInformation("Reducing Most Recent Files count from {currCount} to {maxCount}.", Count, MAXCOUNT);
                _ = RemoveLast();
            }
        }

        /// <summary>
        /// Removes the last FIFO item from the collection.
        /// </summary>
        /// <returns>DiscoveredFileModel</returns>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public DiscoveredFileModel RemoveLast()
        {
            if (IsEmpty())
            {
                _logger.LogError("Remove Last on an empty collection!");
                throw new NullReferenceException("There are no items in this queue.");
            }
            else
            {
                DiscoveredFileLinkedListNode? currentNode = Head;
                DiscoveredFileLinkedListNode? previousNode = null;

                while (currentNode?.Next is not null)
                {
                    previousNode = currentNode;
                    currentNode = currentNode.Next;
                }

                if (previousNode is not null)
                {
                    previousNode.Next = null;
                }

                Count--;
                _logger.LogInformation("Found the last item in the queue: {item}.", currentNode!.Value!.FullFilePath);
                return currentNode.Value!;
            }
        }

        /// <summary>
        /// Get the list of items currently in this collection, in FIFO order.
        /// </summary>
        /// <returns>List of DiscoveredFileModels</returns>
        public List<DiscoveredFileModel> GetList()
        {
            List<DiscoveredFileModel> result = new();
            DiscoveredFileLinkedListNode? currentNode = Head;

            while (currentNode is not null)
            {
                result.Add(currentNode!.Value!);
                currentNode = currentNode.Next;
            }

            _logger.LogInformation("Returning list of {count} items from the queue.", result.Count);
            return result;
        }

        private class DiscoveredFileLinkedListNode
        {
            public DiscoveredFileModel? Value { get; set; }
            public DiscoveredFileLinkedListNode? Next { get; set; }
        }
    }
}