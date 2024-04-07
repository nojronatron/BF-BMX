using BFBMX.Service.Models;

namespace BFBMX.Service.Helpers
{
    public interface IFileProcessor
    {
        bool GetBibMatches(List<FlaggedBibRecordModel> emptyBibList, string[] fileDataLines, string pattern);
        string[] GetFileData(string fullFilePath);
        string GetMessageId(string fileData);
        List<FlaggedBibRecordModel> GetSloppyMatches(string[] lines);
        List<FlaggedBibRecordModel> GetStrictMatches(string[] lines);
        bool ProcessBibs(List<FlaggedBibRecordModel> bibRecords, string[] lines, string winlinkMessageId);
        WinlinkMessageModel ProcessWinlinkMessageFile(DateTime timestamp, string machineName, string filePath);
        string RecordsArrayToString(string[] fileData);
        bool WriteWinlinkMessageToFile(WinlinkMessageModel msg, string filepath);
    }
}