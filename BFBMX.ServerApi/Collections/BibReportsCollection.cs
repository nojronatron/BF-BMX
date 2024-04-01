using BFBMX.ServerApi.EF;
using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BFBMX.ServerApi.Collections;

public class BibReportsCollection : ObservableCollection<WinlinkMessageModel>, IBibReportsCollection
{
    private static Object LockObject = new Object();
    private readonly IDbContextFactory<BibMessageContext> _dbContextFactory;
    private readonly ILogger<BibReportsCollection> _logger;
    private readonly IDataExImService _dataExImService;

    public BibReportsCollection(
        IDbContextFactory<BibMessageContext> dbContextFactory,
        ILogger<BibReportsCollection> logger,
        IDataExImService dataExImService
        )
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _dataExImService = dataExImService;
    }

    public bool AddEntityToCollection(WinlinkMessageModel wlMessagePayload)
    {
        int savedEntityCount = 0;

        if (wlMessagePayload is null)
        {
            _logger.LogWarning("Unable to add null entity to collection");
            return false;
        }
        else
        {
            string wlMsgId = string.IsNullOrWhiteSpace(wlMessagePayload.WinlinkMessageId) ? "ERROR!" : wlMessagePayload.WinlinkMessageId;

            lock(LockObject)
            {
                Add(wlMessagePayload);
            }

            _logger.LogInformation("Stored message ID {msgid} to the internal collection.", wlMsgId);

            try
            {
                // see https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory-eg-for-blazor
                using (var bibMessageContext = _dbContextFactory.CreateDbContext())
                {
                    bibMessageContext.Add(wlMessagePayload);
                    savedEntityCount = bibMessageContext.SaveChanges();
                    int bibCount = savedEntityCount - 1; // Entity contains 1 WL Message and a collection of N BibRecords
                    _logger.LogInformation("Stored Winlink Message ID {wlmsgid} with {bibcount} bib records to the internal DB.", wlMsgId, bibCount);
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

        return savedEntityCount > 0;
    }

    public int BackupCollection()
    {
        int itemsCount = 0;

        if (this.Count > 0)
        {
            List<WinlinkMessageModel> items = new();

            lock(LockObject)
            {
                items = this.ToList<WinlinkMessageModel>();
            }

            _logger.LogInformation("Going to store {num} captured Winlink Messages to a file.", items.Count);
            itemsCount = _dataExImService.ExportDataToFile(items);
        }
        else
        {
            _logger.LogInformation("There are no items in memory to back up.");
            return itemsCount;
        }

        return itemsCount;
    }

    public bool RestoreFromBackupFile()
    {
        int saveCount = 0;
        var winlinkMessages = _dataExImService.ImportFileData();

        if (winlinkMessages.Count > 0)
        {
            lock(LockObject)
            {
                Clear();
            }

            if (winlinkMessages is not null && winlinkMessages.Count > 0)
            {
                foreach (var message in winlinkMessages)
                {
                    if (AddEntityToCollection(message))
                    {
                        saveCount++;
                    }
                }
            }

            _logger.LogInformation("Restored {num} items from backup file.", saveCount);
            return true;
        }
        else
        {
            _logger.LogInformation("No data to restore from backup file.");
            return false;
        }
    }
}
