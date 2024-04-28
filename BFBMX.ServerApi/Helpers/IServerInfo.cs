
namespace BFBMX.ServerApi.Helpers
{
    public interface IServerInfo
    {
        bool CanStart();
        void StartHostInfo();
        void StartLogfileInfo();
    }
}