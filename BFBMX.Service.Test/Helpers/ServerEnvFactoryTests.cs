using BFBMX.ServerApi.Helpers;
using System.Net;

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
        public void GetHostnameSucceeds()
        {
            string hostname = Dns.GetHostName();
            IPHostEntry actualHostEntry = _serverEnvFactory.GetServerHostname();
            Assert.Equal(hostname, actualHostEntry.HostName);
        }

        [Fact]
        public void GetPortSucceeds()
        {
            string bfbmxPort = Environment.GetEnvironmentVariable("BFBMX_SERVER_PORT") ?? "5150";
            Assert.Equal(bfbmxPort, _serverEnvFactory.GetServerPort());
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
            string? currentEnvVar = Environment.GetEnvironmentVariable("BFBMX_SERVER_LOG_DIR");

            if (string.IsNullOrWhiteSpace(currentEnvVar))
            {
                // NOT set
                string expectedValue = "BFBMX_Server_Logs";
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
    }
}
