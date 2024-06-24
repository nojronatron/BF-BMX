using System.Net.Http.Headers;

namespace BFBMX.Desktop.Helpers
{
    public class ApiClientSettings : IApiClientSettings
    {
        private readonly string _baseUri;
        private readonly string _winlinkMessageEndpoint = "WinlinkMessage";
        private readonly string _defaultMediaType = "application/json";
        private readonly MediaTypeWithQualityHeaderValue _defaultMTQHJson = new("application/json");
        private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(20);

        public string BaseUri => _baseUri;
        public string PostWinlinkMsgUri => _winlinkMessageEndpoint;
        public string DefaultMediaType => _defaultMediaType;
        public MediaTypeHeaderValue DefaultMTQHJson => _defaultMTQHJson;
        public TimeSpan DefaultTimeout => _defaultTimeout;

        public ApiClientSettings(string serverNameAndPort)
        {
            // see https://stackoverflow.com/questions/70628314/injecting-primitive-type-in-constructor-of-generic-type-using-microsoft-di
            _baseUri = serverNameAndPort ?? throw new ArgumentNullException();
        }
    }
}
