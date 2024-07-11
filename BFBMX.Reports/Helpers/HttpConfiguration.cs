using System.Text.Json;

namespace BFBMX.Reports.Helpers
{
    public class HttpConfiguration
    {
        public string BaseAddress => "http://localhost:5150/api/v1/";
        public string ServerInfoEndpoint => string.Concat(BaseAddress, "ServerInfo");
        public string AllRecordsEndpoint => string.Concat(BaseAddress, "AllRecords");
        public string DropReportEndpoint => string.Concat(BaseAddress, "DropReport");
        public string BibNumberEndpoint => string.Concat(BaseAddress, "BibNumberReport");
        public string AidStationEndpoint => string.Concat(BaseAddress, "AidStationReport");
        public string StatisticsEndpoint => string.Concat(BaseAddress, "Statistics");
        public string UserAgentHeader = "BFBMX.Reports/v1.6.0alpha";
        public string AcceptHeader = "application/json";
        public TimeSpan Timeout = new(0, 0, 10);
        public CancellationTokenSource Cts => new CancellationTokenSource(Timeout);

        public JsonSerializerOptions JsonOptions => new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
