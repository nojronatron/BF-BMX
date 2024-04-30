using BFBMX.ServerApi.EF;
using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BFBMX.ServerApi.Collections;

public class BibReportsCollection : ObservableCollection<WinlinkMessageModel>, IBibReportsCollection
{
    private static Object LockObject = new();
    private readonly IDbContextFactory<BibMessageContext> _dbContextFactory;
    private readonly ILogger<BibReportsCollection> _logger;

    public BibReportsCollection(
        IDbContextFactory<BibMessageContext> dbContextFactory,
        ILogger<BibReportsCollection> logger
        )
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
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
}
