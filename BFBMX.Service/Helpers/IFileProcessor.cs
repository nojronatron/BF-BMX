using BFBMX.Service.Models;
using System.Text.RegularExpressions;

namespace BFBMX.Service.Helpers
{
    public interface IFileProcessor
    {
        HashSet<FlaggedBibRecordModel> GetBibMatches(string[] fileDataLines, Regex regexInstance, bool setWarning = false);
        string[] GetFileData(string fullFilePath);
        string GetMessageId(string fileData);
        HashSet<FlaggedBibRecordModel> GetSloppyMatches(string[] lines);
        HashSet<FlaggedBibRecordModel> GetStrictMatches(string[] lines);
        DateTime GetWinlinkMessageDateTimeStamp(string winlinkMessageData);
        List<FlaggedBibRecordModel> ProcessBibs(string[] lines, string messageId);
        WinlinkMessageModel ProcessWinlinkMessageFile(DateTime timestamp, string machineName, string filePath);
        string RecordsArrayToString(string[] fileData);
        bool WriteWinlinkMessageToFile(WinlinkMessageModel msg, string filepath);
    }
}