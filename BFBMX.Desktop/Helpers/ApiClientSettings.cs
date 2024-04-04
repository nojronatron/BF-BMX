namespace BFBMX.Desktop.Helpers
{
    public class ApiClientSettings : IApiClientSettings
    {
        private readonly string _baseUri;

        public string BaseUri => _baseUri;

        public ApiClientSettings(string serverNameAndPort)
        {
            // see https://stackoverflow.com/questions/70628314/injecting-primitive-type-in-constructor-of-generic-type-using-microsoft-di
            _baseUri = serverNameAndPort ?? throw new ArgumentNullException();
        }
    }
}
