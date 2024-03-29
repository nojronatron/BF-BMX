namespace BFBMX.Desktop.Helpers
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
            string? logDirectory = Environment.GetEnvironmentVariable("BFBMX_FOLDER_NAME");
            string bffName = string.IsNullOrEmpty(logDirectory) ? "BFBMX" : logDirectory;
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
            string logFilename = "bfbmx-desktop.log";
            return logFilename;
        }

        public static string GetBibRecordsLogFileName()
        {
            string logFilename = "BibRecordsLog.txt";
            return logFilename;
        }

        public static string GetServerHostnameAndPort()
        {
            const string PROTOCOL = @"http://";
            const string DEFAULTSERVER = "localhost";
            const int DEFAULTPORT = 5150;

            string serverName = Environment.GetEnvironmentVariable("BFBMX_SERVERNAME") ?? DEFAULTSERVER;
            string envVarPort = Environment.GetEnvironmentVariable("BFBMX_SERVERPORT") ?? string.Empty;
            int tempPort;

            // if parse fails tempPort will be set to DEFAULTPORT
            if (int.TryParse(envVarPort, out tempPort))
            {
                // tempPort is set correctly when TryParse succeeds
            }
            else
            { 
                tempPort = DEFAULTPORT;
            }

            return $"{PROTOCOL}{serverName}:{tempPort.ToString()}/";
        }
    }
}
