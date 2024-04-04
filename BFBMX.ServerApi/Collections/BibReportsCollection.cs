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
            _logger.LogWarning("BibReportsCollection: Unable to add null entity to collection");
            return false;
        }

        string wlMsgId = string.IsNullOrWhiteSpace(wlMessagePayload.WinlinkMessageId) ? "ERROR!" : wlMessagePayload.WinlinkMessageId;

        // todo: find out what stakeholders want to do if duplicate because collection items are written to file, db is not
        if (this.Contains(wlMessagePayload))
        {
            _logger.LogWarning("Winlink Message ID {wlmsgid} already exists in memory!", wlMsgId);
        }

        lock (LockObject)
        {
            Add(wlMessagePayload);
        }

        _logger.LogInformation("Saved Winlink Message ID {msgid} and its Bib Records to memory.", wlMsgId);

        try
        {
            // see https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory-eg-for-blazor
            using (var bibMessageContext = _dbContextFactory.CreateDbContext())
            {
                if (bibMessageContext.WinlinkMessageModels.Any(wl => wl.WinlinkMessageId == wlMessagePayload.WinlinkMessageId))
                {
                    // todo: find out what stakeholders want to do if the entity already exists in the DB or it that matters
                    _logger.LogWarning("Winlink Message ID {wlmsgid} already exists in the server DB!", wlMsgId);
                }
                else
                {
                    bibMessageContext.Add(wlMessagePayload);
                    savedEntityCount = bibMessageContext.SaveChanges();
                    int bibRecordCount = savedEntityCount - 1; // Entity contains 1 WL Message and a collection of N BibRecords
                    _logger.LogInformation("Stored Winlink Message ID {wlmsgid} with {bibcount} bib records to server DB.", wlMsgId, bibRecordCount);
                }
            }
        }
        catch (DbUpdateConcurrencyException dbuce)
        {
            _logger.LogError("Error adding concurrent entity to server DB: {msg}", dbuce.Message);
        }
        catch (DbUpdateException dbue)
        {
            _logger.LogError("Error adding entity to server DB: {msg}", dbue.Message);
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
        List<WinlinkMessageModel> winlinkMessages = _dataExImService.ImportFileData();

        if (winlinkMessages.Count > 0)
        {
            lock(LockObject)
            {
                Clear();
            }

            foreach (WinlinkMessageModel message in winlinkMessages)
            {
                if (AddEntityToCollection(message))
                {
                    saveCount++;
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
