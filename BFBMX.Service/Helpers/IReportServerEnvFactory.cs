namespace BFBMX.Service.Helpers
{
    public interface IReportServerEnvFactory
    {
        string GetApiServerHostname();
        string GetApiServerPort();
        int[] GetBigfootBibRange();
        int[] GetLittlefoot20BibRange();
        int[] GetLittlefoot40BibRange();
    }
}