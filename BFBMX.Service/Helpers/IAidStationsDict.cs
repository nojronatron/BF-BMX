namespace BFBMX.Service.Helpers
{
    public interface IAidStationsDict
    {
        string GetAidStationCode(string aidStationName);
        string GetAidStationName(string aidStationCode);
        List<string> GetAll();
    }
}