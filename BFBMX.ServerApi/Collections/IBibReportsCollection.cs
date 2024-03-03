using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Collections
{
    public interface IBibReportsCollection
    {
        bool AddEntityToCollection(WinlinkMessageModel message);
        Task<bool> AddEntityToCollectionAsync(WinlinkMessageModel message);
        int AddEntitiesToCollection(List<WinlinkMessageModel> wlMessages);
        Task<int> AddEntitiesToCollectionAsync(List<WinlinkMessageModel> wlMessages);
        bool BackupCollection();
        Task<bool> BackupCollectionAsync();
        bool RestoreFromBackupFile();
        Task<bool> RestoreFromBackupFileAsync();
    }
}