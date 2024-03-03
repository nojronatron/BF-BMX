using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Helpers
{
    public interface IBibRecordLogger
    {
        bool ValidateVariables(out string? bfBmxLogPath);
        bool LogWinlinkMessagePayloadToJsonAuditFile(WinlinkMessageModel wlMessagePayload);
        bool LogFlaggedRecordsTabDelimited(WinlinkMessageModel wlMessagePayload);
        Task<bool> LogFlaggedRecordsTabDelimitedAsync(WinlinkMessageModel wlMessagePayload);
    }
}