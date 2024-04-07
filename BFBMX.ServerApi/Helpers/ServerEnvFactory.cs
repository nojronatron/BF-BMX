namespace BFBMX.ServerApi.Helpers
{
    public static class ServerEnvFactory
    {
        public static string GetuserProfilePath()
        {
            string? userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");
            string upPath = string.IsNullOrWhiteSpace(userProfilePath) ? @"C:\" : userProfilePath;
            return upPath;
        }

        public static string GetServerFolderName()
        {
            string? logDirectory = Environment.GetEnvironmentVariable("BFBMX_SERVER_FOLDER_NAME");
            string sfName = string.IsNullOrWhiteSpace(logDirectory) ? "BFBMX" : logDirectory;
            return sfName;
        }

        public static string GetServerBackupFilename()
        {
            string? bbBackupFilename = Environment.GetEnvironmentVariable("BFBMX_BACKUP_FILE_NAME");
            string backupFileName = string.IsNullOrWhiteSpace(bbBackupFilename) ? "BFBMX-LocalDb-Backup.txt" : bbBackupFilename;
            return backupFileName;
        }

        public static string GetServerLogPath()
        {
            string userProfilePath = GetuserProfilePath();
            string logDirectory = GetServerFolderName();
            string serverLogPath = System.IO.Path.Combine(userProfilePath, "Documents", logDirectory);
            return serverLogPath;
        }

        public static string GetServerBackupFileNameAndPath()
        {
            string backupFilePathAndName = Path.Combine(
                ServerEnvFactory.GetServerLogPath(),
                ServerEnvFactory.GetServerBackupFilename()
                );
            return backupFilePathAndName;
        }
    }
}
