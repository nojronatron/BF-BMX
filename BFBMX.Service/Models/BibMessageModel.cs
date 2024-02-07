using System.Text.Json;

namespace BFBMX.Service.Models;

public class BibMessageModel
{
  public string? WinlinkMessageId { get; set; } // max len appears to be 12
  public DateTime ClientDateTime { get; set; } // datetime file date was scraped
  public string? ClientHostname { get; set; } // env:COMPUTERNAME
  public int BibNumber { get; set; } = -1; // max len 15
  public string? @Action { get; set; } // IN, OUT, DROP
  public string? BibTimeOfDay { get; set; } // format: HHMM
  public int DayOfMonth { get; set; } = -1; // digit count range(1-31)
  public string? Location { get; set; } // max len 26 (as defined by select element in form)
  public bool DataWarning { get; set; } = true;
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
    return JsonSerializer.Serialize<BibMessageModel>(this);
  }

  public string BibDataToString()
  {
    // for UI display
    return $"{BibNumber}\t{Action}\t{BibTimeOfDay}\t{DayOfMonth}\t{Location}";
  }
  public override string ToString()
  {
    // for logging
    var dataWarningText = DataWarning ? "Data Warning" : "OK";
    return $"{WinlinkMessageId}\t{ToFilename()}\t{ClientHostname}\t{BibDataToString()}\t{dataWarningText}";
  }
}
