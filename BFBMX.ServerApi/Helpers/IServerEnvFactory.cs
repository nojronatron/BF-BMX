using System.Net;

namespace BFBMX.ServerApi.Helpers
{
    public interface IServerEnvFactory
    {
        string GetServerBackupFilename();
        string GetServerBackupFileNameAndPath();
        string GetServerFolderName();
        string GetServerLogPath();
        string GetUserProfilePath();
        string GetServerPort();
        IPHostEntry GetServerHostname();
    }
}