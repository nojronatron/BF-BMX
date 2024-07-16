namespace BFBMX.Service.Helpers
{
    public interface IReportServerEnvFactory
    {
        string GetApiServerHostname();
        string GetApiServerPort();
    }
}