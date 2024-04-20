using BFBMX.ServerApi.Helpers;

namespace BFBMX.Service.Test.Helpers
{
    public class ServerEnvFactoryTests
    {
        private readonly ServerEnvFactory _serverEnvFactory;

        public ServerEnvFactoryTests()
        {
            _serverEnvFactory = new ServerEnvFactory();
        }

        [Fact]
        public void GetUserProfilePath_ShouldReturnValidPath()
        {
            string? result = _serverEnvFactory.GetUserProfilePath();
            Assert.False(string.IsNullOrWhiteSpace(result));
            Assert.True(Directory.Exists(result));
        }

        [Fact]
        public void GetUserProfilePath_SpecificValueReturned()
        {
            // 2 possible scenarios:
            // envvar NOT set => default value will be returned: C:\
            // envvar IS set => value will be returned that matches envvar
            string? currentEnvVar = Environment.GetEnvironmentVariable("USERPROFILE");

            if (string.IsNullOrWhiteSpace(currentEnvVar))
            {
                // NOT set
                string expectedValue = @"C:\";
                string actualValue = _serverEnvFactory.GetUserProfilePath();
                Assert.Equal(expectedValue, actualValue);
            }
            else
            {
                // IS set
                string result = _serverEnvFactory.GetUserProfilePath();
                Assert.Equal(currentEnvVar, result);
            }
        }

        [Fact]
        public void GetServerFolderName_ShouldReturnValidName()
        {
            string? result = _serverEnvFactory.GetServerFolderName();
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void GetServerFoldername_SpecificValueReturned()
        {
            // 2 possible scenarios:
            // envvar NOT set => default value will be returned: BFBMX
            // envvar IS set => value will be returned that matches envvar
            string? currentEnvVar = Environment.GetEnvironmentVariable("BFBMX_SERVER_FOLDER_NAME");

            if (string.IsNullOrWhiteSpace(currentEnvVar))
            {
                // NOT set
                string expectedValue = "BFBMX";
                string actualValue = _serverEnvFactory.GetServerFolderName();
                Assert.Equal(expectedValue, actualValue);
            }
            else
            {
                // IS set
                string result = _serverEnvFactory.GetServerFolderName();
                Assert.Equal(currentEnvVar, result);
            }
        }

        [Fact]
        public void GetServerBackupFilename_ShouldReturnValidName()
        {
            var result = _serverEnvFactory.GetServerBackupFilename();
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void GetServerBackupFilename_SpecificValueReturned()
        {
            // 2 possible scenarios:
            // envvar is NOT set => default value will be returned: BFBMX-LocalDb-Backup.txt
            // envvar IS Set => value will be retruned that matches envvar
            string? currentEnvVar = Environment.GetEnvironmentVariable("BFBMX_BACKUP_FILE_NAME");

            if (string.IsNullOrWhiteSpace(currentEnvVar))
            {
                // NOT set
                string expectedValue = "BFBMX-LocalDb-Backup.txt";
                string actualValue = _serverEnvFactory.GetServerBackupFilename();
                Assert.Equal(expectedValue, actualValue);
            }
            else
            {
                // IS set
                string result = _serverEnvFactory.GetServerBackupFilename();
                Assert.Equal(currentEnvVar, result);
            }
        }

        [Fact]
        public void GetServerLogPath_ShouldReturnNonNullString()
        {
            var result = _serverEnvFactory.GetServerLogPath();
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void GetServerLogPath_ContainsDefaultKeyword()
        {
            var result = _serverEnvFactory.GetServerLogPath();
            Assert.Contains("Documents", result);
        }

        [Fact]
        public void GetServerBackupFileNameAndPath_ShouldReturnValidPath()
        {
            var result = _serverEnvFactory.GetServerBackupFileNameAndPath();
            Assert.False(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void GetServerBackupFileNameAndPath_ContainsDefaultKeyword()
        {
            var result = _serverEnvFactory.GetServerBackupFileNameAndPath();
            Assert.Contains("Documents", result);
        }
    }
}
