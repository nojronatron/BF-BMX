
namespace BFBMX.Desktop.Helpers
{
    public interface IApiClient
    {
        Task<bool> PostWinlinkMessageAsync(string jsonPayload);
    }
}