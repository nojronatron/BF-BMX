using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Helpers
{
    public interface IDataExImService
    {
        string DocumentsDirectoryName { get; }

        int ExportDataToFile(List<WinlinkMessageModel> data);
        List<WinlinkMessageModel> ImportFileData();
        Task<List<WinlinkMessageModel>> ImportFileDataAsync();
    }
}