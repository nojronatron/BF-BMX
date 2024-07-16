using BFBMX.ServerApi.EF;
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

    public AidStationStatisticsModel GetAidStationReport(string aidStationId)
    {
        AidStationStatisticsModel result = new();

        if (string.IsNullOrWhiteSpace(aidStationId) == false)
        {
            string aidId = aidStationId.ToLower().Trim();

            try
            {
                var messagesWithAidStation = this.Where(wlm => wlm.BibRecords.Any(bib => bib.Location!.ToLower() == aidId));
                result.AidStationName = aidStationId;
                result.WinlinkMessages = messagesWithAidStation.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning("GetAidStationReport: Error getting Aid Station Data: {ermsg}", ex.Message);
            }
        }

        return result;
    }

    public BibRecordsStatisticsModel GetStatistics()
    {
        BibRecordsStatisticsModel result = new();
        result.TotalWinlinkMessagesProcessed = this.Count();
        result.AllWinlinkMessages = this.ToList();
        return result;
    }

    public IEnumerable<WinlinkMessageModel> GetBibReport(string bibNumber)
    {
       var messagesWithBib = this.Where(wlm => wlm.BibRecords.Any(bib => bib.BibNumber == bibNumber));
        List<WinlinkMessageModel> resultCollection = new();

        foreach (var message in messagesWithBib)
        {
            List<FlaggedBibRecordModel> bibReports = message.BibRecords.Where(bib => bib.BibNumber == bibNumber).ToList();
            WinlinkMessageModel wlm = WinlinkMessageModel.GetWinlinkMessageInstance(
                message.WinlinkMessageId,
                message.MessageDateStamp,
                message.ClientHostname,
                message.FileCreatedTimeStamp,
                bibReports);
            resultCollection.Add(wlm);
        }

        return resultCollection;
    }

    public IEnumerable<WinlinkMessageModel> GetDroppedReports()
    {
        var messagesWithDrops = this.Where(wlm => wlm.BibRecords.Any(bib => bib.@Action == "DROP"));
        List<WinlinkMessageModel> resultCollection = new();

        foreach(var message in messagesWithDrops)
        {
            List<FlaggedBibRecordModel> droppedBibsReports = message.BibRecords.Where(
                bib => bib.@Action!.ToLower() != "in" && bib.@Action.ToLower() != "out").ToList();
            WinlinkMessageModel wlm = WinlinkMessageModel.GetWinlinkMessageInstance(
                message.WinlinkMessageId,
                message.MessageDateStamp,
                message.ClientHostname,
                message.FileCreatedTimeStamp,
                droppedBibsReports);
            resultCollection.Add(wlm);
        }

        return resultCollection;
    }

    public IEnumerable<WinlinkMessageModel> GetAllEntities()
    {
        return this.Select(wlm => wlm);
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
