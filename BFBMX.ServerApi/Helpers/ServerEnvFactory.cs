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
            string? logDirectory = Environment.GetEnvironmentVariable("BFBMX_SERVER_FOLDER_NAME");
            string sfName = string.IsNullOrWhiteSpace(logDirectory) ? "BFBMX" : logDirectory;
            return sfName;
        }

        public string GetServerBackupFilename()
        {
            string? bbBackupFilename = Environment.GetEnvironmentVariable("BFBMX_BACKUP_FILE_NAME");
            string backupFileName = string.IsNullOrWhiteSpace(bbBackupFilename) ? "BFBMX-LocalDb-Backup.txt" : bbBackupFilename;
            return backupFileName;
        }

        public string GetServerLogPath()
        {
            string userProfilePath = GetUserProfilePath();
            string logDirectory = GetServerFolderName();
            string serverLogPath = System.IO.Path.Combine(userProfilePath, "Documents", logDirectory);
            return serverLogPath;
        }

        public string GetServerBackupFileNameAndPath()
        {
            string backupFilePathAndName = Path.Combine(
                GetServerLogPath(),
                GetServerBackupFilename()
                );
            return backupFilePathAndName;
        }
    }
}
