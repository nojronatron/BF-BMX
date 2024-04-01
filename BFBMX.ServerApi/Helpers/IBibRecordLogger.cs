using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Helpers
{
    public interface IBibRecordLogger
    {
        bool ValidateServerVariables(out string? bfBmxLogPath);
        bool LogWinlinkMessagePayloadToJsonAuditFile(WinlinkMessageModel wlMessagePayload);
        bool LogFlaggedRecordsTabDelimited(WinlinkMessageModel wlMessagePayload);
    }
}