using BFBMX.Service.Models;
using System.Collections.Specialized;
using System.ComponentModel;

namespace BFBMX.Desktop.Collections
{
    public interface IDiscoveredFilesCollection
    {
        //event NotifyCollectionChangedEventHandler? CollectionChanged;
        //event PropertyChangedEventHandler? PropertyChanged;

        Task EnqueueAsync(DiscoveredFileModel item);
        //void OnCollectionChanged(NotifyCollectionChangedAction action, DiscoveredFileModel item);
        //void OnPropertyChanged(string propertyName);
    }
}