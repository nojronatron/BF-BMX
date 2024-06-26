﻿using BFBMX.ServerApi.EF;
using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace BFBMX.ServerApi.Collections;

public class BibReportsCollection : ObservableCollection<WinlinkMessageModel>, IBibReportsCollection
{
    private static Object LockObject = new();
    private readonly IDbContextFactory<BibMessageContext> _dbContextFactory;
    private readonly ILogger<BibReportsCollection> _logger;
    private readonly IServerLogWriter _serverLogWriter;

    public BibReportsCollection(
        IDbContextFactory<BibMessageContext> dbContextFactory,
        ILogger<BibReportsCollection> logger,
        IServerLogWriter serverLogWriter
        )
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _serverLogWriter = serverLogWriter;
    }

    public bool AddEntityToCollection(WinlinkMessageModel wlMessagePayload)
    {
        int savedEntityCount = 0;

        if (wlMessagePayload is null)
        {
            _logger.LogWarning("BibReportsCollection: Unable to add null entity to collection.");
            _serverLogWriter.WriteActivityToLogAsync("BibReportsCollection: Unable to add null entity to collection.");
            return false;
        }

        string wlMsgId = string.IsNullOrWhiteSpace(wlMessagePayload.WinlinkMessageId) ? "ERROR!" : wlMessagePayload.WinlinkMessageId;

        if (this.Contains(wlMessagePayload))
        {
            _logger.LogWarning("Winlink Message ID {wlmsgid} already exists in collection and will not be stored again.", wlMsgId);
            _serverLogWriter.WriteActivityToLogAsync($"BibReportsCollection: Winlink Message ID {wlMsgId} already exists in collection and will not be stored again.");
        }
        else
        {
            lock (LockObject)
            {
                Add(wlMessagePayload);
            }

            _logger.LogInformation("Saved Winlink Message ID {msgid} and its Bib Records to memory.", wlMsgId);
            _serverLogWriter.WriteActivityToLogAsync($"BibReportsCollection: Saved Winlink Message ID {wlMsgId} and its Bib Records to memory.");
        }

        try
        {
            // see https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory-eg-for-blazor
#pragma warning disable IDE0063 // Use simple 'using' statement
            using (var bibMessageContext = _dbContextFactory.CreateDbContext())
            {
                if (bibMessageContext.WinlinkMessageModels.Any(wl => wl.WinlinkMessageId == wlMessagePayload.WinlinkMessageId))
                {
                    _logger.LogWarning("Winlink Message ID {wlmsgid} already exists in the server DB!", wlMsgId);
                    _serverLogWriter.WriteActivityToLogAsync($"Winlink Message ID {wlMsgId} already exists in the Server Database - it will not get stored.");
                }
                else
                {
                    bibMessageContext.Add(wlMessagePayload);
                    savedEntityCount = bibMessageContext.SaveChanges();
                    int bibRecordCount = savedEntityCount - 1; // Entity contains 1 WL Message and a collection of N BibRecords
                    _logger.LogInformation("Stored Winlink Message ID {wlmsgid} with {bibcount} bib records to server DB.", wlMsgId, bibRecordCount);
                    _serverLogWriter.WriteActivityToLogAsync($"Stored Winlink Message ID {wlMsgId} with {bibRecordCount} bib records to server DB.");
                }
            }
#pragma warning restore IDE0063 // Use simple 'using' statement
        }
        catch (DbUpdateConcurrencyException dbuce)
        {
            _logger.LogError("Error concurrently adding {wlMessagePayload} to server DB: {msg}",wlMessagePayload.WinlinkMessageId, dbuce.Message);
            _serverLogWriter.WriteActivityToLogAsync($"Error concurrently adding {wlMessagePayload.WinlinkMessageId} to server DB. Reason: {dbuce.Message}");
        }
        catch (DbUpdateException dbue)
        {
            _logger.LogError("Error adding entity to server DB: {msg}", dbue.Message);
            _serverLogWriter.WriteActivityToLogAsync($"Error adding {wlMessagePayload.WinlinkMessageId} to server DB. Reason: {dbue.Message}");
        }

        return savedEntityCount > 0;
    }
}
