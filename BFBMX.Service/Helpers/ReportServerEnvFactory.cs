using System.Net;

namespace BFBMX.Service.Helpers
{
    public class ReportServerEnvFactory : IReportServerEnvFactory
    {
        public string GetApiServerPort()
        {
            return Environment.GetEnvironmentVariable("BFBMX_SERVER_PORT") ?? "5150";
        }

        public string GetApiServerHostname()
        {
            string? bfBmxServerName = Environment.GetEnvironmentVariable("BFBMX_SERVER_NAME");
            
            if (bfBmxServerName is null)
            {
                return Dns.GetHostName();
            }

            return bfBmxServerName.Trim();
        }
    }
}
