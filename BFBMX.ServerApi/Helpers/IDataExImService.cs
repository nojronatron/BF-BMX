using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Helpers
{
    public interface IDataExImService
    {
        int ExportDataToFile(List<WinlinkMessageModel> data);
        List<WinlinkMessageModel> ImportFileData();
    }
}