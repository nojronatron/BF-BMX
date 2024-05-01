using BFBMX.Service.Helpers;

namespace BFBMX.Service.Test.Helpers
{
    public class DesktopEnvFactoryTests
    {
        [Fact]
        public void GetUserProfilePath_SpecificValueReturned()
        {
            // Arrange
            string expectedValue = Environment.GetEnvironmentVariable("USERPROFILE") ?? "C:\\";

            // Act
            string actualValue = DesktopEnvFactory.GetUserProfilePath();

            // Assert
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void GetBfBmxFolderName_SpecificValueReturned()
        {
            string expectedValue = Environment.GetEnvironmentVariable("BFBMX_DESKTOP_LOG_DIR") ?? "BFBMX_Desktop_Logs";

            string actualValue = DesktopEnvFactory.GetBfBmxFolderName();

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void GetBfBmxLogPath_SpecificValueReturned()
        {
            string userProfile = Environment.GetEnvironmentVariable("USERPROFILE") ?? "C:\\";
            string desktoLogDir = Environment.GetEnvironmentVariable("BFBMX_DESKTOP_LOG_DIR") ?? "BFBMX_Desktop_Logs";
            string expectedValue = System.IO.Path.Combine(userProfile, "Documents", desktoLogDir);

            string actualValue = DesktopEnvFactory.GetBfBmxLogPath();

            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void GetBfBmxLogFileName_SpecificValueReturned()
        {
            string expectedValue = "bfbmx-desktop-app-log.txt";
            string actualValue = DesktopEnvFactory.GetBfBmxLogFileName();
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void GetBibRecordsLogFileName_SpecificValueReturned()
        {
            string expectedValue = "captured-bib-records.txt";
            string actualValue = DesktopEnvFactory.GetBibRecordsLogFileName();
            Assert.Equal(expectedValue, actualValue);
        }

        [Fact]
        public void GetServerHostnameAndPort_SpecificValueReturned()
        {
            string expectedHostname = Environment.GetEnvironmentVariable("BFBMX_SERVER_NAME") ?? "localhost";
            string expectedPort = Environment.GetEnvironmentVariable("BFBMX_SERVER_PORT") ?? "5150";
            string expectedValue = $"http://{expectedHostname}:{expectedPort}/";
            string actualValue = DesktopEnvFactory.GetServerHostnameAndPort();
            Assert.Equal(expectedValue, actualValue);
        }
    }
}
