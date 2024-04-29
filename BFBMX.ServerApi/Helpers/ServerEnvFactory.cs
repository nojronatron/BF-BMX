using System.Net;

namespace BFBMX.ServerApi.Helpers
{
    public class ServerEnvFactory : IServerEnvFactory
    {
        public string GetUserProfilePath()
        {
            string? userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            string upPath = string.IsNullOrWhiteSpace(userProfilePath) ? @"C:\" : userProfilePath;
            return upPath;
        }

        public string GetServerFolderName()
        {
            string? logDirectory = Environment.GetEnvironmentVariable("BFBMX_SERVER_LOG_DIR");
            string sfName = string.IsNullOrWhiteSpace(logDirectory) ? "BFBMX" : logDirectory;
            return sfName;
        }

        public string GetServerLogPath()
        {
            string userProfilePath = GetUserProfilePath();
            string logDirectory = GetServerFolderName();
            string serverLogPath = System.IO.Path.Combine(userProfilePath, "Documents", logDirectory);
            return serverLogPath;
        }

        public string GetServerPort()
        {
            return Environment.GetEnvironmentVariable("BFBMX_SERVER_PORT") ?? "5150";
        }

        public IPHostEntry GetServerHostname()
        {
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntryName = Dns.GetHostEntry(serverName);
            return hostEntryName;
        }
    }
}
