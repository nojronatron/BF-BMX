namespace BFBMX.Service.Models
{
    public class BibRecordsStatisticsModel
    {
        public int TotalWinlinkMessagesProcessed { get; set; } = 0;
        public int BigFootBibRecordsProcessed { get; set; } = 0;
        public int LittleFootBibRecordsProcessed { get; set; } = 0;
        public List<int> LittleFeetBibNumbersSeen { get; set; } = new();
        public List<int> BigFootBibNumbersSeen { get; set; } = new();
        public List<FlaggedBibRecordModel> UnknownBibDataItems { get; set; } = new();
    }
}
