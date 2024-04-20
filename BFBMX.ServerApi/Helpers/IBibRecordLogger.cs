using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Helpers
{
    public interface IBibRecordLogger
    {
        bool LogWinlinkMessagePayloadToTabDelimitedFile(WinlinkMessageModel wlMessagePayload);
    }
}