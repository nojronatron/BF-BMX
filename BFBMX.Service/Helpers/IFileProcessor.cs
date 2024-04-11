using BFBMX.Service.Models;

namespace BFBMX.Service.Helpers
{
    public interface IFileProcessor
    {
        HashSet<FlaggedBibRecordModel> GetBibMatches(string[] fileDataLines, string pattern);
        string[] GetFileData(string fullFilePath);
        string GetMessageId(string fileData);
        HashSet<FlaggedBibRecordModel> GetSloppyMatches(string[] lines);
        HashSet<FlaggedBibRecordModel> GetStrictMatches(string[] lines);
        List<FlaggedBibRecordModel> ProcessBibs(string[] lines, string messageId);
        WinlinkMessageModel ProcessWinlinkMessageFile(DateTime timestamp, string machineName, string filePath);
        string RecordsArrayToString(string[] fileData);
        bool WriteWinlinkMessageToFile(WinlinkMessageModel msg, string filepath);
    }
}