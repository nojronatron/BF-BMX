namespace BFBMX.Service.Models
{
    public class BibRecordsStatisticsModel
    {
        public int TotalWinlinkMessagesProcessed { get; set; } = 0;
        public List<WinlinkMessageModel> AllWinlinkMessages { get; set; } = new();
    }
}
