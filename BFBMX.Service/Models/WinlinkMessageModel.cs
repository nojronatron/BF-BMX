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

    /// <summary>
    /// Guaranteed returns a fully hydrated Winlink Message entity with the provided parameters.
    /// </summary>
    /// <param name="winlinkMessageId"></param>
    /// <param name="messageDateTime"></param>
    /// <param name="clientHostname"></param>
    /// <param name="fileCreatedDateTime"></param>
    /// <param name="bibRecords"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Tests if this Winlink Message entity has any Bib Records with Data Warnings.
    /// </summary>
    /// <returns></returns>
    public bool HasDataWarning()
    {
        return BibRecords.Any(x => x.DataWarning);
    }

    /// <summary>
    /// Generates a printable string representation of the DateTime parameter provided in yyyy-MM-ddTHH-mm-ss format.
    /// </summary>
    /// <param name="dateTimeEntry"></param>
    /// <returns></returns>
    public string PrintableMsgDateTime(DateTime dateTimeEntry)
    {
        return dateTimeEntry.ToString("yyyy-MM-ddTHH-mm-ss");
    }

    /// <summary>
    /// Converts WinlinkID to a filename for storing a Winlink Message entity to disk.
    /// </summary>
    /// <returns>String concatenation of this.WinlinkMessageId with file extension ".txt"</returns>
    public string ToFilename()
    {
        // learn.microsoft.com: 2009-06-15T13:45:30 (DateTimeKind.Local) -> 2009-06-15T13:45:30
        //string customFormatPattern = PrintableMsgDateTime(MessageDateStamp);
        //string customFormatPattern = PrintableMsgDateTime(FileCreatedTimeStamp);
        return $"{WinlinkMessageId}.txt";
    }

    /// <summary>
    /// Returns a Json serialized representation of this Winlink Message entity, used for web POSTing and auditing.
    /// </summary>
    /// <returns></returns>
    public string ToJsonString()
    {
        return JsonSerializer.Serialize<WinlinkMessageModel>(this);
    }


    /// <summary>
    /// Generate a plain text, tab delimited representation of Bib Records within this Winlink Message entity for consumption by MS Access project.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Equals method override used for comparing Winlink Message Entities for equality.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
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

    /// <summary>
    /// GetHashCode override used for comparing Winlink Message Entities for equality.
    /// </summary>
    /// <returns></returns>
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
