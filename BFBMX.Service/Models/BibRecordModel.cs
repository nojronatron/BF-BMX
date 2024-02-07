using System.Text.Json;

namespace BFBMX.Service.Models;

public class BibRecordModel
{
  public int BibNumber { get; set; }
  public string? @Action { get; set; }
  public string? BibTimeOfDay { get; set; }
  public int DayOfMonth { get; set; }
  public string? Location { get; set; }
  public bool DataWarning { get; set; } = true;
  public string ToJson()
  {
    return JsonSerializer.Serialize<BibRecordModel>(this);
  }
  public override string ToString()
  {
    return $"{BibNumber}\t{Action}\t{BibTimeOfDay}\t{DayOfMonth}\t{Location}";
  }
}
