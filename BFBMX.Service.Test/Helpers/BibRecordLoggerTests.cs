using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;
using Xunit;

namespace BFBMX.Service.Test.Helpers
{
    public class BibRecordLoggerTests
    {
        [Fact]
        public void LogWinlinkMessagePayloadToTabDelimitedFile_ValidPayload_ReturnsTrue()
        {
            // todo: validate and update this test.
            // todo: write addition test cases for false returns
            // Arrange
            var logger = new NullLogger<BibRecordLogger>();
            var bibRecordLogger = new BibRecordLogger(logger);
            var winlinkMessagePayload = new WinlinkMessageModel();

            // Act
            var result = bibRecordLogger.LogWinlinkMessagePayloadToTabDelimitedFile(winlinkMessagePayload);

            // Assert
            Assert.True(result);
        }

        //[Fact]
        //public void ValidateServerVariables_WhenUserProfilePathIsNull_ReturnsFalse()
        //{
        //    // Arrange
        //    NullLogger<BibRecordLogger> logger = new();
        //    BibRecordLogger bibRecordLogger = new(logger);
        //    string? currentUserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
        //    Environment.SetEnvironmentVariable("USERPROFILE", null);

        //    // Act
        //    bool result = bibRecordLogger.ValidateServerVariables(out string? bfBmxLogPath);

        //    // Assert
        //    if (result is true)
        //    {
        //        Environment.SetEnvironmentVariable("USERPROFILE", currentUserProfile);
        //        Assert.False(result, "ValidateServerVariables returned true when USERPROFILE was null but it should have returned false.");
        //    }

        //    Environment.SetEnvironmentVariable("USERPROFILE", currentUserProfile);
        //    Assert.Equal(string.Empty, bfBmxLogPath);
        //}

        [Fact]
        public void ValidateServerVariables_WhenLogDirectoryIsNull_ReturnsFalse()
        {
            // Arrange
            var logger = new NullLogger<BibRecordLogger>();
            var bibRecordLogger = new BibRecordLogger(logger);
            //Environment.SetEnvironmentVariable("USERPROFILE", "C:\\Users\\TestUser");
            Environment.SetEnvironmentVariable("BFBMX_SERVER_FOLDER_NAME", null);

            // Act
            var result = bibRecordLogger.ValidateServerVariables(out string? bfBmxLogPath);

            // Assert
            Assert.False(result);
            Assert.Equal(string.Empty, bfBmxLogPath);
        }

        [Fact]
        public void ValidateServerVariables_WhenLogDirectoryDoesNotExist_CreatesDirectory()
        {
            // Arrange
            var logger = new NullLogger<BibRecordLogger>();
            var bibRecordLogger = new BibRecordLogger(logger);
            //Environment.SetEnvironmentVariable("USERPROFILE", "C:\\Users\\TestUser");
            Environment.SetEnvironmentVariable("BFBMX_SERVER_FOLDER_NAME", "Logs");
            var expectedLogPath = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), "Documents", "Logs");

            // Act
            var result = bibRecordLogger.ValidateServerVariables(out string? bfBmxLogPath);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedLogPath, bfBmxLogPath);
            Assert.True(Directory.Exists(expectedLogPath));

            // cleanup
            if (File.Exists(expectedLogPath))
            {
                Directory.Delete(expectedLogPath);
            }
        }

        [Fact]
        public void LogWinlinkMessagePayloadToJsonAuditFile_WhenValidationFails_ReturnsFalse()
        {
            // Arrange
            var logger = new NullLogger<BibRecordLogger>();
            var bibRecordLogger = new BibRecordLogger(logger);
            var wlMessagePayload = new WinlinkMessageModel();

            // Act
            bool result = bibRecordLogger.LogWinlinkMessagePayloadToJsonAuditFile(wlMessagePayload);

            // Assert
            Assert.False(result);
        }

        //[Fact]
        //public void LogWinlinkMessagePayloadToJsonAuditFile_WhenValidationSucceeds_WritesToFile()
        //{
        //    // Arrange
        //    var logger = new NullLogger<BibRecordLogger>();
        //    var bibRecordLogger = new BibRecordLogger(logger);
        //    var wlMessagePayload = new WinlinkMessageModel
        //    {
        //        // Set properties of the payload
        //    };

        //    // Act
        //    var result = bibRecordLogger.LogWinlinkMessagePayloadToJsonAuditFile(wlMessagePayload);

        //    // Assert
        //    Assert.True(result);
        //    // Add assertions to verify the file content
        //}

        [Fact]
        public void LogFlaggedRecordsTabDelimited_WhenValidationFails_ReturnsFalse()
        {
            // Arrange
            var logger = new NullLogger<BibRecordLogger>();
            var bibRecordLogger = new BibRecordLogger(logger);
            var wlMessagePayload = new WinlinkMessageModel();

            // Act
            var result = bibRecordLogger.LogFlaggedRecordsTabDelimited(wlMessagePayload);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void LogFlaggedRecordsTabDelimited_WhenValidationSucceeds_WritesToFile()
        {
            // todo: complete writing this test!
            Assert.True(false);
            // Arrange
            var logger = new NullLogger<BibRecordLogger>();
            var bibRecordLogger = new BibRecordLogger(logger);
            var wlMessagePayload = new WinlinkMessageModel
            {
                // Set properties of the payload
            };

            // Act
            var result = bibRecordLogger.LogFlaggedRecordsTabDelimited(wlMessagePayload);

            // Assert
            Assert.True(result);
            // Add assertions to verify the file content
        }
    }
}
