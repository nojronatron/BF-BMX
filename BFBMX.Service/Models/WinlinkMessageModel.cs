using System.Text;
using System.Text.Json;

namespace BFBMX.Service.Models;

public class WinlinkMessageModel
{
    public string? WinlinkMessageId { get; set; } // max len appears to be 12
    public DateTime ClientDateTime { get; set; } // datetime file date was scraped
    public string? ClientHostname { get; set; } // env:COMPUTERNAME
    public List<BibRecordModel> BibRecords { get; set; } = new List<BibRecordModel>();

    public bool HasDataWarning()
    {
        return BibRecords.Any(x => x.DataWarning);
    }

    public string ToFilename()
    {
        // learn.microsoft.com: 2009-06-15T13:45:30 (DateTimeKind.Local) -> 2009-06-15T13:45:30
        // string sortableFormatPattern = ClientDateTime.ToString("s");
        string customFormatPattern = ClientDateTime.ToString("yyyy-MM-ddTHH-mm-ss");
        return $"{ClientHostname}-{customFormatPattern}.txt";
    }

    public string ToJson()
    {
        // for sending data over the wire
        return JsonSerializer.Serialize<WinlinkMessageModel>(this);
    }

    public override string ToString()
    {
        // for logging
        StringBuilder sb = new StringBuilder();
        sb.Append(WinlinkMessageId).Append('\t');
        sb.Append(ToFilename()).Append('\t');
        sb.Append("Bibs: [ ");

        foreach (var record in BibRecords)
        {
            sb.Append(record.BibDataToString()).Append('\t');
        }

        sb.Append(" ]").Append('\t');
        return sb.ToString();
    }
}
