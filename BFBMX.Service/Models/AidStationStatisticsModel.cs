namespace BFBMX.Service.Models
{
    public class AidStationStatisticsModel
    {
        public string AidStationName { get; set; } = "Unknown Aid Station";
        public List<WinlinkMessageModel> WinlinkMessages { get; set; } = new();
    }
}
