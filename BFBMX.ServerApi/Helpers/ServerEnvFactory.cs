using System.Net;

namespace BFBMX.ServerApi.Helpers
{
    public class ServerEnvFactory : IServerEnvFactory
    {
        private static string _userProfile = "USERPROFILE";
        private static string _cColonSlash = @"C:\";
        private static string _defaultServerLogDir = "BFBMX_Server_Logs";
        private static string _serverPortEnv = "BFBMX_SERVER_PORT";
        private static string _serverLogDirEnv = "BFBMX_SERVER_LOG_DIR";
        private static string _defaultServerPort = "5150";
        private static string _serverActivityLogFilename = "server_activity.txt";
        private static string _documents = "Documents";

        public string GetUserProfilePath()
        {
            string? userProfilePath = Environment.GetEnvironmentVariable(_userProfile);
            string upPath = string.IsNullOrWhiteSpace(userProfilePath) ? @_cColonSlash : userProfilePath;
            return upPath;
        }

        public string GetServerFolderName()
        {
            string? logDirectory = Environment.GetEnvironmentVariable(_serverLogDirEnv);
            string sfName = string.IsNullOrWhiteSpace(logDirectory) ? _defaultServerLogDir : logDirectory;
            return sfName;
        }

        public string GetServerLogPath()
        {
            string userProfilePath = GetUserProfilePath();
            string? bfBmxServerLogDir = Environment.GetEnvironmentVariable(_serverLogDirEnv);
            string serverLogPath = string.IsNullOrWhiteSpace(bfBmxServerLogDir) 
                ? Path.Combine(userProfilePath, _documents) 
                : bfBmxServerLogDir;
            return serverLogPath;
        }

        public string GetServerPort()
        {
            return Environment.GetEnvironmentVariable(_serverPortEnv) ?? _defaultServerPort;
        }

        public IPHostEntry GetServerHostname()
        {
            string serverName = Dns.GetHostName();
            IPHostEntry hostEntryName = Dns.GetHostEntry(serverName);
            return hostEntryName;
        }

        public string GetServerActivityLogFilename()
        {
            return _serverActivityLogFilename;
        }
    }
}
