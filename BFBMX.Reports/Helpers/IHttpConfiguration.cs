using System.Text.Json;

namespace BFBMX.Reports.Helpers
{
    public interface IHttpConfiguration
    {
        string AcceptHeader { get; }
        string AidStationEndpoint { get; }
        string AllRecordsEndpoint { get; }
        string BaseAddress { get; }
        string BibNumberEndpoint { get; }
        CancellationTokenSource Cts { get; }
        string DropReportEndpoint { get; }
        JsonSerializerOptions JsonOptions { get; }
        string PortNumber { get; }
        string ServerInfoEndpoint { get; }
        string ServerName { get; }
        string StatisticsEndpoint { get; }
        string UserAgentHeader { get; }
    }
}