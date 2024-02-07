namespace BFBMX.Service.Models;

public class MessageModel
{
  public DateTime ClientDateTime { get; set; }
  public string? Hostname { get; set; }
  public string? IPAddress { get; set; }
  public bool DataWarning { get; set; } = false;
  public List<BibRecordModel> BibRecords { get; set; } = new();
  public string ToFilename()
  {
    string customFormatS = ClientDateTime.ToString("s");
    return $"{customFormatS}.txt";
  }
  public bool AddBibRecord(BibRecordModel bibRecord)
  {
    if (bibRecord is null)
    {
      return false;
    }
    if (bibRecord.DataWarning)
    {
      DataWarning = true;
    }
    BibRecords.Add(bibRecord);
    return true;
  }
  public bool AddBibRecords(List<BibRecordModel> bibRecords)
  {
    if (bibRecords is null || bibRecords.Count == 0)
    {
      return false;
    }
    foreach (var bibRecord in bibRecords)
    {
      AddBibRecord(bibRecord);
    }
    return true;
  }
}
