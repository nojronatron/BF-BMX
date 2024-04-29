using System.Net;

namespace BFBMX.ServerApi.Helpers
{
    public interface IServerEnvFactory
    {
        string GetServerFolderName();
        string GetServerLogPath();
        string GetUserProfilePath();
        string GetServerPort();
        IPHostEntry GetServerHostname();
    }
}