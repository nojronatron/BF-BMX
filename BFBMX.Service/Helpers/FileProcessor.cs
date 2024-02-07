using BFBMX.Service.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BFBMX.Service.Helpers
{
  public interface IFileReader
  {
    bool Exists(string path);
    string[] ReadAllLines(string path);
  }
  public class FileReader : IFileReader
  {
    public bool Exists(string path) => File.Exists(path);
    public string[] ReadAllLines(string path) => File.ReadAllLines(path);
  }
  public class FileProcessor
  {
    private readonly IFileReader _fileReader;
    private static readonly string pattern = @"\d{1,3}\t(OUT|IN|DROP)\t\d{4}\t\d{1,2}\t\w{2}";
    // todo: add singleton fields e.g. Collections, logger, etc
    // todo: add instance fields for common properties

    public FileProcessor(IFileReader fileReader)
    {
      _fileReader = fileReader;
    }

    public List<BibRecordModel> ProcessFile(string fullFileName)
    {
      // check if fullFileName exists
      if (string.IsNullOrWhiteSpace(fullFileName))
      {
        // log an informational message: fullFileName null or empty
        // throw new ArgumentException("File name cannot be null or whitespace.", nameof(fullFileName));
        return new List<BibRecordModel>();
      }
      if (!File.Exists(fullFileName))
      {
        // log an informational message: file does not exist
        //throw new FileNotFoundException("File does not exist.", fullFileName);
        return new List<BibRecordModel>();
      }
      try
      {
        // open file and read contents
        string[] lines = File.ReadAllLines(fullFileName);
        return ProcessLines(lines);
      }
      catch (Exception ex)
      {
        // log an error message: an unexpected error occurred
        Debug.WriteLine($"An unexpected error occurred in FileProcessor: {ex.Message}");
        // throw;
        return new List<BibRecordModel>();
      }
    }

    private List<BibRecordModel> ProcessLines(string[] lines)
    {
      if (lines is null)
      {
        throw new ArgumentNullException(nameof(lines));
      }

      List<BibRecordModel> bibRecords = new();

      foreach (var line in lines)
      {
        if (Regex.IsMatch(line, pattern))
        {
          var fields = line.Split('\t');

          var bibRecord = CreateBibRecord(fields);
          if (bibRecord is not null)
          {
            bibRecords.Add(bibRecord);
          }
        }
      }
      return bibRecords;
    }

    private BibRecordModel CreateBibRecord(string[] fields)
    {
      if (fields.Length < 5 || !int.TryParse(fields[0], out int bibNum) || !int.TryParse(fields[3], out int dayOfMonth))
      {
        // log this error condition and return null
        return new BibRecordModel();
      }
      return new BibRecordModel
      {
        BibNumber = bibNum,
        Action = fields[1].Trim(),
        BibTimeOfDay = fields[2].Trim(),
        DayOfMonth = dayOfMonth,
        Location = fields[4].Trim()
      };
    }
  }
}
