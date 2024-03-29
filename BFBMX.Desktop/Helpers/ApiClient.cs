using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace BFBMX.Desktop.Helpers
{
    public class ApiClient : HttpClient, IApiClient
    {
        private HttpClient _apiClient;
        private readonly ApiClientSettings _apiClientSettings;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(ILogger<ApiClient> logger, ApiClientSettings apiConfig)
        {
            _logger = logger;
            _apiClient = new HttpClient();
            _apiClientSettings = apiConfig;
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<bool> PostWinlinkMessageAsync(string jsonPayload)
        {
            string postWinlinkMsgUri = "WinlinkMessage"; // post Winlink Message service endpoint
            bool result = false;
            if (string.IsNullOrWhiteSpace(jsonPayload))
            {
                _logger.LogWarning("PostWinlinkMessageAsync: The JSON payload is empty or null. NOT calling server.");
                return result;
            }
            else
            {
                StringContent postPayload = new(jsonPayload, Encoding.UTF8, "application/json");
                try
                {
                    string uriPath = System.IO.Path.Combine(_apiClientSettings.BaseUri, postWinlinkMsgUri);
                    _logger.LogInformation("ApiClient set uriPath to {uriPath}.", uriPath);

                    // httpclient will dispose automatically to conserve resources
                    using (HttpResponseMessage response = await _apiClient.PostAsync(uriPath, postPayload))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            result = true;
                            _logger.LogInformation("PostWinlinkMessageAsync: Response was success status code.");
                        }
                        else
                        {
                            _logger.LogWarning("PostWinlinkMessageAsync: Response was not a success status code.");
                            result = false;
                        }
                    }
                }
                catch (UriFormatException uriFormEx)
                {
                    _logger.LogError("Forming Uri failed\nStacktrace: {stacktrace}\nMessage: {msg}", uriFormEx.StackTrace, uriFormEx.Message);
                }
                catch (FormatException frmtEx)
                {
                    _logger.LogError("Forming a valid URI failed!\nStacktrace: {stacktrace}\nMessage: {msg}", frmtEx.StackTrace, frmtEx.Message);
                    result = false;
                }
                catch (ArgumentNullException ArgNullEx)
                {
                    _logger.LogError("PostWinlinkMessageAsync: Argument Null Exception Stacktrace: {argnullst}. Message: {argnullmsg}", ArgNullEx.StackTrace, ArgNullEx.Message);
                    if (ArgNullEx.InnerException != null)
                    {
                        _logger.LogError("PostWinlinkMessageAsync: Arugment Null Inner Exception {argnullexinnermsg}", ArgNullEx.InnerException.Message);
                    }
                    result = false;
                }
                catch (HttpRequestException HReqEx)
                {
                    _logger.LogError("PostWinlinkMessageAsync: HTTP Request Exception Stacktrace: {argnullst}. Message: {argnullmsg}", HReqEx.StackTrace, HReqEx.Message);
                    if (HReqEx.InnerException != null)
                    {
                        _logger.LogError("PostWinlinkMessageAsync: HTTP Request Inner Exception {argnullexinnermsg}", HReqEx.InnerException.Message);
                    }
                    result = false;
                }
                catch (Exception ex)
                {
                    _logger.LogError("PostWinlinkMessageAsync: Exception Stacktrace: {argnullst}. Message: {argnullmsg}", ex.StackTrace, ex.Message);
                    if (ex.InnerException != null)
                    {
                        _logger.LogError("PostWinlinkMessageAsync: Exception Inner Exception {argnullexinnermsg}", ex.InnerException.Message);
                    }
                    result = false;
                }
            }
            return result;
        }
    }
}
