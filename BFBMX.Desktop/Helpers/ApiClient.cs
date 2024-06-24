using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text;

namespace BFBMX.Desktop.Helpers
{
    public class ApiClient : HttpClient, IApiClient
    {
        private HttpClient _apiClient;
        private readonly ApiClientSettings _apiClientSettings;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(ILogger<ApiClient> logger, ApiClientSettings apiClientSettings)
        {
            _logger = logger;
            _apiClient = new HttpClient();
            _apiClientSettings = apiClientSettings;
        }

        public async Task<bool> PostWinlinkMessageAsync(string jsonPayload)
        {
            bool result = false;
            if (string.IsNullOrWhiteSpace(jsonPayload))
            {
                _logger.LogWarning("PostWinlinkMessageAsync: The JSON payload is empty or null. NOT calling server.");
                return result;
            }
            else
            {
                StringContent postPayload = new(jsonPayload, Encoding.UTF8, _apiClientSettings.DefaultMediaType);

                try
                {
                    string uriPath = System.IO.Path.Combine(_apiClientSettings.BaseUri, _apiClientSettings.PostWinlinkMsgUri);
                    _logger.LogInformation("Sending message to server at {uriPath}. Will timeout in {apiClientTimeout} (HH:MM:SS)", uriPath, _apiClientSettings.DefaultTimeout);

                    // httpclient will dispose automatically to conserve resources
                    using (HttpResponseMessage response = await _apiClient.PostAsync(uriPath, postPayload))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            result = true;
                            _logger.LogInformation("Response from Server was a success status code.");
                        }
                        else
                        {
                            _logger.LogWarning("Response from Server was NOT a success status code.");
                            result = false;
                        }
                    }
                }
                catch (UriFormatException uriFormEx)
                {
                    _logger.LogError("Forming Uri failed\nStacktrace: {uriFormatExceptionTrace}\nMessage: {uriFormatExceptionMessage}", uriFormEx.StackTrace, uriFormEx.Message);
                }
                catch (FormatException frmtEx)
                {
                    _logger.LogError("Forming a valid URI failed!\nStacktrace: {formatExceptionTrace}\nMessage: {formatExceptionMessage}", frmtEx.StackTrace, frmtEx.Message);
                    result = false;
                }
                catch (ArgumentNullException ArgNullEx)
                {
                    _logger.LogError("PostWinlinkMessageAsync: Argument Null Exception Stacktrace: {argumentNullExceptionTrace}. Message: {argumentNullExceptionMessage}", ArgNullEx.StackTrace, ArgNullEx.Message);
                    if (ArgNullEx.InnerException != null)
                    {
                        _logger.LogError("PostWinlinkMessageAsync: Arugment Null Inner Exception {innerExceptionMessage}", ArgNullEx.InnerException.Message);
                    }
                    result = false;
                }
                catch (HttpRequestException HReqEx)
                {
                    _logger.LogError("PostWinlinkMessageAsync: HTTP Request Exception Stacktrace: {httpRequestExceptionTrace}. Message: {httpRequestExceptionMessage}", HReqEx.StackTrace, HReqEx.Message);
                    if (HReqEx.InnerException != null)
                    {
                        _logger.LogError("PostWinlinkMessageAsync: HTTP Request Inner Exception {innerExceptionMessage}", HReqEx.InnerException.Message);
                    }
                    result = false;
                }
                catch (Exception ex)
                {
                    _logger.LogError("PostWinlinkMessageAsync: Exception Stacktrace: {exceptionTrace}. Message: {exceptionMessage}", ex.StackTrace, ex.Message);
                    if (ex.InnerException != null)
                    {
                        _logger.LogError("PostWinlinkMessageAsync: Exception Inner Exception {innerExceptionMessage}", ex.InnerException.Message);
                    }
                    result = false;
                }
            }
            return result;
        }
    }
}
