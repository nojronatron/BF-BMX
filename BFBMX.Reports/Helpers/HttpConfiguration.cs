using BFBMX.Service.Helpers;
using System.Text.Json;

namespace BFBMX.Reports.Helpers
{
    public class HttpConfiguration : IHttpConfiguration
    {
        private readonly IReportServerEnvFactory rsEnvFactory;
        private readonly string rootProto = "http://";
        private readonly string rootApiPath = "api/v1";

        public string ServerName => rsEnvFactory.GetApiServerHostname();
        public string PortNumber => rsEnvFactory.GetApiServerPort();

        public string BaseAddress { get; set; } //"http://localhost:5150/api/v1/";
        public string ServerInfoEndpoint => string.Concat(BaseAddress, "ServerInfo");
        public string AllRecordsEndpoint => string.Concat(BaseAddress, "AllRecords");
        public string DropReportEndpoint => string.Concat(BaseAddress, "DropReport");
        public string BibNumberEndpoint => string.Concat(BaseAddress, "BibNumberReport");
        public string AidStationEndpoint => string.Concat(BaseAddress, "AidStationReport");
        public string StatisticsEndpoint => string.Concat(BaseAddress, "Statistics");
        public string UserAgentHeader => "BFBMX.Reports/v2.0.1";
        public string AcceptHeader => "application/json";
        public TimeSpan Timeout = new(0, 0, 10);

        public CancellationTokenSource Cts => new(Timeout);

        public JsonSerializerOptions JsonOptions => new()
        {
            PropertyNameCaseInsensitive = true
        };

        public HttpConfiguration(IReportServerEnvFactory rsEnvFactory)
        {
            this.rsEnvFactory = rsEnvFactory;
            BaseAddress = $"{rootProto}{ServerName}:{PortNumber}/{rootApiPath}/";
        }
    }
}
