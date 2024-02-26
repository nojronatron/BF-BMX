using System.Text;
using System.Text.Json;

namespace BFBMX.Service.Models;

public class WinlinkMessageModel
{
    public string? WinlinkMessageId { get; set; } // max len appears to be 12
    public DateTime MessageDateTime { get; set; } // datetime from Winlink message
    public string? ClientHostname { get; set; } // env:COMPUTERNAME (might not be necessary)
    public List<FlaggedBibRecordModel> BibRecords { get; set; } = new List<FlaggedBibRecordModel>();

    public bool HasDataWarning()
    {
        return BibRecords.Any(x => x.DataWarning);
    }

    public string ToFilename()
    {
        // learn.microsoft.com: 2009-06-15T13:45:30 (DateTimeKind.Local) -> 2009-06-15T13:45:30
        // string sortableFormatPattern = ClientDateTime.ToString("s");
        string customFormatPattern = MessageDateTime.ToString("yyyy-MM-ddTHH-mm-ss");
        return $"{ClientHostname}-{customFormatPattern}.txt";
    }

    public string ToJsonString()
    {
        // for sending data over the wire
        return JsonSerializer.Serialize<WinlinkMessageModel>(this);
    }

    public override string ToString()
    {
        // override ToString to get data and collection out of this instance
        StringBuilder sb = new StringBuilder();
        sb.Append("Message-ID: ").Append(WinlinkMessageId).Append(" in ");
        sb.Append(ToFilename()).AppendLine(" contains bib records: [");

        foreach (var record in BibRecords)
        {
            sb.AppendLine(record.ToTabbedString());
        }

        sb.AppendLine("]");
        return sb.ToString();
    }
}
