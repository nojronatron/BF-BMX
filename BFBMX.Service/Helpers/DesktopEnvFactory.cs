namespace BFBMX.Service.Helpers
{
    public static class DesktopEnvFactory
    {
        public static string GetUserProfilePath()
        {
            string? userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            string upPath = string.IsNullOrEmpty(userProfilePath) ? @"C:\" : userProfilePath;
            return upPath;
        }

        public static string GetBfBmxFolderName()
        {
            string? logDirectory = Environment.GetEnvironmentVariable("BFBMX_DESKTOP_LOG_DIR");
            string bffName = string.IsNullOrEmpty(logDirectory) ? "BFBMX_Desktop_Logs" : logDirectory;
            return bffName;
        }

        public static string GetBfBmxLogPath()
        {
            string userProfilePath = GetUserProfilePath();
            string logDirectory = GetBfBmxFolderName();
            string upnlDirectory = System.IO.Path.Combine(userProfilePath, "Documents", logDirectory);
            return upnlDirectory;
        }

        /// <summary>
        /// Return the preferred log file name for the BFBMX Desktop application.
        /// </summary>
        /// <returns>Filename for storing plain text running log data.</returns>
        public static string GetBfBmxLogFileName()
        {
            string logFilename = "bfbmx-desktop-app-log.txt";
            return logFilename;
        }

        public static string GetBibRecordsLogFileName()
        {
            string logFilename = "captured-bib-records.txt";
            return logFilename;
        }

        public static string GetServerHostnameAndPort()
        {
            const string PROTOCOL = @"http://";
            const string DEFAULTSERVER = "localhost";
            const string DEFAULTPORT = "5150";

            string serverName = Environment.GetEnvironmentVariable("BFBMX_SERVER_NAME") ?? DEFAULTSERVER;
            string serverPort = Environment.GetEnvironmentVariable("BFBMX_SERVER_PORT") ?? DEFAULTPORT;

            return $"{PROTOCOL}{serverName}:{serverPort}/";
        }
    }
}
