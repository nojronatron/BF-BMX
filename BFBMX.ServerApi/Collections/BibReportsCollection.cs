using BFBMX.ServerApi.EF;
using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BFBMX.ServerApi.Collections;

public class BibReportsCollection : ObservableCollection<WinlinkMessageModel>, IBibReportsCollection
{
    private readonly IDbContextFactory<BibMessageContext> _dbContextFactory;
    private readonly ILogger<BibReportsCollection> _logger;

    public BibReportsCollection(
        IDbContextFactory<BibMessageContext> dbContextFactory,
        ILogger<BibReportsCollection> logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public bool AddEntityToCollection(WinlinkMessageModel wlMessagePayload)
    {
        int saveCount = 0;

        if (wlMessagePayload is null)
        {
            _logger.LogWarning("Unable to add null entity to collection");
            return false;
        }
        else
        {
            try
            {
                // see https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory-eg-for-blazor
                using (var bibMessageContext = _dbContextFactory.CreateDbContext())
                {
                    bibMessageContext.Add(wlMessagePayload);
                    saveCount = bibMessageContext.SaveChanges();
                    Add(wlMessagePayload);
                    _logger.LogInformation("Stored {num} messages to the internal DB.", saveCount);
                }
            }
            catch (DbUpdateConcurrencyException dbuce)
            {
                _logger.LogError("Error adding concurrent entity to collection: {msg}", dbuce.Message);
            }
            catch (DbUpdateException dbue)
            {
                _logger.LogError("Error adding entity to collection: {msg}", dbue.Message);
            }
        }

        return saveCount > 0;
    }

    public async Task<bool> AddEntityToCollectionAsync(WinlinkMessageModel wlMessagePayload)
    {
        return await Task.Run(() =>
        {
            return AddEntityToCollection(wlMessagePayload);
        });
    }

    /// <summary>
    /// Add a list of entities to this collection and to the local DB.
    /// </summary>
    /// <param name="winlinkMessageList"></param>
    /// <returns></returns>
    public int AddEntitiesToCollection(List<WinlinkMessageModel> winlinkMessageList)
    {
        int saveCount = 0;

        if (winlinkMessageList is not null && winlinkMessageList.Count > 0)
        {
            foreach (var message in winlinkMessageList)
            {
                AddEntityToCollection(message);
            }
        }

        _logger.LogInformation("Added {num} message entries to the internal DB.", saveCount);
        return saveCount;
    }

    public async Task<int> AddEntitiesToCollectionAsync(List<WinlinkMessageModel> winlinkMessageList)
    {
        return await Task.Run(() =>
        {
            return AddEntitiesToCollection(winlinkMessageList);
        });
    }

    public bool BackupCollection()
    {
        // todo: return a count of items backed up
        if (this.Count < 1)
        {
            _logger.LogInformation("There are no items in memory to back up.");
            return false;
        }
        else
        {
            var items = this.ToList<WinlinkMessageModel>();
            _logger.LogInformation("Going to store {num} items to a file.", items.Count);
            return DataExImService.ExportDataToFile(items);
        }
    }

    public async Task<bool> BackupCollectionAsync()
    {
        // todo: return a count of items backed up
        if (this.Count < 1)
        {
            return false;
        }
        else
        {
            var items = this.ToList<WinlinkMessageModel>();
            _logger.LogInformation("Going to store {num} items to a file.", items.Count);
            return await DataExImService.ExportDataToFileAsync(items);
        }
    }

    public bool RestoreFromBackupFile()
    {
        var newItems = DataExImService.ImportFileData();

        if (newItems.Count > 0)
        {
            Clear();
            AddEntitiesToCollection(newItems);
            _logger.LogInformation("Restoring {num} items from backup file.", newItems.Count);
            return true;
        }
        else
        {
            _logger.LogInformation("No data to restore from backup file.");
            return false;
        }
    }

    public async Task<bool> RestoreFromBackupFileAsync()
    {
        return await Task.Run(() =>
        {
            return RestoreFromBackupFile();
        });
    }
}
