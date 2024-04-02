using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace BFBMX.Service.Models;

public class WinlinkMessageModel
{
    [Key]
    public string? WinlinkMessageId { get; set; } // max len appears to be 12
    public DateTime MessageDateTime { get; set; } // datetime from Winlink message
    public string? ClientHostname { get; set; } // env:COMPUTERNAME (might not be necessary)
    public List<FlaggedBibRecordModel> BibRecords { get; set; } = new List<FlaggedBibRecordModel>();

    public static WinlinkMessageModel GetWinlinkMessageInstance(string? winlinkMessageId, DateTime messageDateTime, string? clientHostname, List<FlaggedBibRecordModel> bibRecords)
    {
        return new WinlinkMessageModel
        {
            WinlinkMessageId = winlinkMessageId,
            MessageDateTime = messageDateTime,
            ClientHostname = clientHostname,
            BibRecords = bibRecords,
        };
    }

    public bool HasDataWarning()
    {
        return BibRecords.Any(x => x.DataWarning);
    }

    public string PrintableMsgDateTime()
    {
        return MessageDateTime.ToString("yyyy-MM-ddTHH-mm-ss");
    }

    public string ToFilename()
    {
        // learn.microsoft.com: 2009-06-15T13:45:30 (DateTimeKind.Local) -> 2009-06-15T13:45:30
        string customFormatPattern = MessageDateTime.ToString("yyyy-MM-ddTHH-mm-ss");
        return $"{WinlinkMessageId}-{customFormatPattern}.txt";
    }

    public string ToJsonString()
    {
        // for sending data over the wire
        return JsonSerializer.Serialize<WinlinkMessageModel>(this);
    }

    public string ToAccessDatabaseTabbedString()
    {
        string recordPrefix = $"{WinlinkMessageId}\t{PrintableMsgDateTime()}\t";
        StringBuilder sbBibData = new();

        foreach(var record in BibRecords)
        {
            sbBibData.Append(recordPrefix).AppendLine(record.ToTabbedString());
        }

        return sbBibData.ToString();
    }

    public string ToServerAuditTabbedString()
    {
        string sbHeader = $"{WinlinkMessageId}\t{ClientHostname}\t";
        StringBuilder sbBibData = new();
        
        foreach (var record in BibRecords)
        {
            sbBibData.Append(sbHeader).AppendLine(record.ToTabbedString());
        }

        return sbBibData.ToString();
    }

    // for sake of comparing entities
    public override bool Equals(object? obj)
    {
        if (obj is WinlinkMessageModel other)
        {
            return WinlinkMessageId == other.WinlinkMessageId
                && MessageDateTime == other.MessageDateTime
                && ClientHostname == other.ClientHostname
                && (BibRecords?.Count == other.BibRecords?.Count);
        }

        return false;
    }

    // for sake of comparing entities
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (WinlinkMessageId?.GetHashCode() ?? 0);
        hash = hash * 23 + MessageDateTime.GetHashCode();
        hash = hash * 23 + (ClientHostname?.GetHashCode() ?? 0);
        hash = hash * 23 + (BibRecords?.Count.GetHashCode() ?? 0);
        return hash;
    }
}
