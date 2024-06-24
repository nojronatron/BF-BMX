
using System.Net.Http.Headers;

namespace BFBMX.Desktop.Helpers
{
    public interface IApiClientSettings
    {
        string BaseUri { get; }
        TimeSpan DefaultTimeout { get; }
        string PostWinlinkMsgUri { get; }
        MediaTypeHeaderValue DefaultMTQHJson { get; }
        string DefaultMediaType { get; }
    }
}