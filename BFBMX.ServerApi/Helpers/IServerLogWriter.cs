
namespace BFBMX.ServerApi.Helpers
{
    public interface IServerLogWriter
    {
        Task WriteActivityToLogAsync(string message);
    }
}