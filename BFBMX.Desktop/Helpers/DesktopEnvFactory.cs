namespace BFBMX.Desktop.Helpers
{
    // todo: make DesktopEnvFactory class static
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
    }
}
