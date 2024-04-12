using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.IO;
using Xunit;

namespace BFBMX.Service.Test.Helpers
{
    public class BibRecordLoggerTests
    {
        private static readonly object LockObject = new object();
        private readonly NullLogger<BibRecordLogger> _moqLogster;
        private readonly Mock<IServerEnvFactory> _moqServerEnvFactory;
        private readonly BibRecordLogger _bibRecordLogger;

        public BibRecordLoggerTests()
        {
            _moqLogster = new NullLogger<BibRecordLogger>();
            _moqServerEnvFactory = new Mock<IServerEnvFactory>();
            _bibRecordLogger = new BibRecordLogger(_moqLogster, _moqServerEnvFactory.Object);
        }

        [Fact]
        public void LogWinlinkMessagePayloadToTabDelimitedFile_ValidPayload_ReturnsTrue()
        {
            // Arrange
            WinlinkMessageModel winlinkMessagePayload = new()
            {
                WinlinkMessageId = "123456",
                MessageDateStamp = DateTime.Now,
                ClientHostname = "Test-Client",
                FileCreatedTimeStamp = DateTime.Now,
                BibRecords = new List<FlaggedBibRecordModel>()
            };

            // Set up the mock object behavior
            _moqServerEnvFactory.Setup(f => f.GetUserProfilePath()).Returns("C://Test-Temp");
            _moqServerEnvFactory.Setup(g => g.GetServerLogPath()).Returns("Test-Log-Path");
            _moqServerEnvFactory.Setup(h => h.GetServerFolderName()).Returns("Test-Folder-Name");

            // clean-up any previous run
            string expectedFilename = winlinkMessagePayload.ToAccessDatabaseTabbedString();
            string expectedPath = Path.Combine(
                _moqServerEnvFactory.Object.GetUserProfilePath(),
                _moqServerEnvFactory.Object.GetServerLogPath(),
                _moqServerEnvFactory.Object.GetServerFolderName()
                );

            if (File.Exists(Path.Combine(expectedPath, expectedFilename)))
            {
                lock (LockObject)
                {
                    File.Delete(Path.Combine(expectedPath, expectedFilename));
                }
            }

            // Act
            var result = _bibRecordLogger.LogWinlinkMessagePayloadToTabDelimitedFile(winlinkMessagePayload);

            // Assert
            Assert.True(result);
        }
    }
}
