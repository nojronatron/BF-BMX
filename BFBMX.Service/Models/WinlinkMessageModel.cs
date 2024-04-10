using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace BFBMX.Service.Models;

public class WinlinkMessageModel
{
    [Key]
    public string? WinlinkMessageId { get; set; } // max len appears to be 12
    public DateTime MessageDateStamp { get; set; } // datetime from Winlink message
    public string? ClientHostname { get; set; } // env:COMPUTERNAME (might not be necessary)
    public DateTime FileCreatedTimeStamp { get; set; } // the time the file was created on the Desktop App
    public List<FlaggedBibRecordModel> BibRecords { get; set; } = new List<FlaggedBibRecordModel>();

    public static WinlinkMessageModel GetWinlinkMessageInstance(string? winlinkMessageId,
                                                                DateTime messageDateTime,
                                                                string? clientHostname,
                                                                DateTime fileCreatedDateTime,
                                                                List<FlaggedBibRecordModel> bibRecords)
    {
        return new WinlinkMessageModel
        {
            WinlinkMessageId = winlinkMessageId,
            MessageDateStamp = messageDateTime,
            ClientHostname = clientHostname,
            FileCreatedTimeStamp = fileCreatedDateTime,
            BibRecords = bibRecords,
        };
    }

    public bool HasDataWarning()
    {
        return BibRecords.Any(x => x.DataWarning);
    }

    public string PrintableMsgDateTime(DateTime dateTimeEntry)
    {
        return dateTimeEntry.ToString("yyyy-MM-ddTHH-mm-ss");
    }

    public string ToFilename()
    {
        // learn.microsoft.com: 2009-06-15T13:45:30 (DateTimeKind.Local) -> 2009-06-15T13:45:30
        //string customFormatPattern = PrintableMsgDateTime(MessageDateStamp);
        string customFormatPattern = PrintableMsgDateTime(FileCreatedTimeStamp);
        return $"{WinlinkMessageId}-{customFormatPattern}.txt";
    }

    public string ToJsonString()
    {
        // for sending data over the wire
        return JsonSerializer.Serialize<WinlinkMessageModel>(this);
    }

    public string ToAccessDatabaseTabbedString()
    {
        string recordPrefix = $"{WinlinkMessageId}\t{PrintableMsgDateTime(MessageDateStamp)}\t";
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
                && MessageDateStamp == other.MessageDateStamp
                && ClientHostname == other.ClientHostname
                && FileCreatedTimeStamp == other.FileCreatedTimeStamp
                && (BibRecords?.Count == other.BibRecords?.Count);
        }

        return false;
    }

    // for sake of comparing entities
    public override int GetHashCode()
    {
        int hash = 17;
        hash = hash * 23 + (WinlinkMessageId?.GetHashCode() ?? 0);
        hash = hash * 23 + MessageDateStamp.GetHashCode();
        hash = hash * 23 + (ClientHostname?.GetHashCode() ?? 0);
        hash = hash * 23 + FileCreatedTimeStamp.GetHashCode();
        hash = hash * 23 + (BibRecords?.Count.GetHashCode() ?? 0);
        return hash;
    }
}
