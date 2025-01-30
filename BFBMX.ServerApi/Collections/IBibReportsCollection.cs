using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Collections
{
    public interface IBibReportsCollection
    {
        bool AddEntityToCollection(WinlinkMessageModel message);
        IEnumerable<WinlinkMessageModel> GetAllEntities();
        IEnumerable<WinlinkMessageModel> GetDroppedReports();
        IEnumerable<WinlinkMessageModel> GetBibReport(string bibNumber);
        BibRecordsStatisticsModel GetStatistics();
        AidStationStatisticsModel GetAidStationReport(string aidStationId);
    }
}