using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using BFBMX.Service.Test.TestData;
using System.Diagnostics;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;

namespace BFBMX.Service.Test.Helpers;

public class FileProcessorTests
{
    private readonly IFileProcessor _fileProcessor;
    private readonly Mock<ILogger<FileProcessor>> _mockLogger;

    public static string GenericWinlinkId => "ABC123DEF456";
    public static string? GetTempDirectory => Environment.GetEnvironmentVariable("TEMP");
    public string sample_RepliedToWinlinkMessageNoBibData => SampleMessages.RepliedToWinlinkMessageNoBibData;
    public string sample_NonconformingBibReportMessage => SampleMessages.NonconformingBibReportMessage;
    public string sample_ValidMessageWithBibDataInReplyMessage => SampleMessages.ValidMessageWithBibDataInReplyMessage;
    public string sample_MessageWith26ValidBibs => SampleMessages.MessageWith26ValidBibs;


    public FileProcessorTests()
    {
        _mockLogger = new Mock<ILogger<FileProcessor>>();
        _fileProcessor = new FileProcessor(_mockLogger.Object);
    }

    [Fact]
    public void FileDoesNotExist_ReturnsZeroCountList()
    {
        var expectedCount = 0;
        var actualResult = _fileProcessor.GetFileData("nonexistentfile.txt");
        var actualCount = actualResult.Length;
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public void FileDoesExist_ReturnsPositiveCountList()
    {
        if (!string.IsNullOrWhiteSpace(GetTempDirectory))
        {
            var tempFile = Path.Combine(GetTempDirectory, "tempfiledata.txt");
            string tempfileData = "This is a test file.\nIt has four lines.\nIt is a test.\nIt is only a test.";

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
            else
            {
                File.WriteAllLines(tempFile, tempfileData.Split('\n'));
            }

            var expectedCount = 4;
            var actualResult = _fileProcessor.GetFileData(tempFile);
            var actualCount = actualResult.Length;
            Assert.Equal(expectedCount, actualCount);

            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
        else
        {
            Assert.True(false, "Environment variable TEMP not found.");
        }
    }

    [InlineData(new string[] { "115\tOUT\t2009\t11\tWR", "195\tOUT\t2009\t11\tWR", "196\tOUT\t2009\t11\tWR" }, 3)]
    [InlineData(new string[] { "236\tOUT\t2231\t14\tSB", "13\tIN\t2231\t14\tSB", "118\tIN\t2235\t14\tSB" }, 3)]
    [InlineData(new string[] { "84\tIN\t2346\t14\tSB", "31\tIN\t2348\t14\tSB", "174\tDROP\t2355\t14\tSB" }, 3)]
    [Theory]
    public void CapturesThreeTabbedRecords_WellFormattedStrictData(string[] bibArray, int expectedCount)
    {
        List<FlaggedBibRecordModel> actualResult = _fileProcessor.GetStrictMatches(bibArray).ToList();
        Assert.NotNull(actualResult);
        Assert.Equal(expectedCount, actualResult.Count);
    }

    [InlineData(new string[] { "217\tOUT\tunknow\t11\tCW", "307362\tOUT\t0948\t11\tBL", "198\tOUT\t18009\t12\tEPEP" }, 3)]
    [InlineData(new string[] { @"115	0UT	2oo9	II	WR", @"195	OUT	2009	11	WR", @"196	OUT	2009	11	WR" }, 3)]
    [Theory]
    public void CapturesThreeTabRecords_WellFormattedSloppyData(string[] tabbedBibData, int expectedCount)
    {
        List<FlaggedBibRecordModel> actualTabDelimitedResult = _fileProcessor.GetSloppyMatches(tabbedBibData).ToList();
        Assert.NotNull(actualTabDelimitedResult);
        Assert.Equal(expectedCount, actualTabDelimitedResult.Count);
    }

    [Fact]
    public void ProcessBibs_ProcessesBibsSuccessfully()
    {
        // Arrange
        int expectedCount = 5;
        string messageId = "ABC123DEF456";

        var lines = new string[] {
        "Content-Transfer-Encoding: quoted - printable",
        "",
        "-",
        "10\tOUT\t834\t13\tCH",
        "10\tIN\t748\t13\tCH",
        "34\tOUT\t449\t13\tCH",
        "34\tIN\t406\t13\tCH",
        "37\tOUT\t855\t13\tCH",
        "-----",
        "* The entries in this email are TAB delimited. This allows you to copy and =\r\npaste into a spreadsheet."
        };

        // Act
        List<FlaggedBibRecordModel> bibRecords = _fileProcessor.ProcessBibs(lines, messageId);

        // Assert
        Assert.NotEmpty(bibRecords);
        Assert.Equal(expectedCount, bibRecords.Count);
    }

    [InlineData(new string[] { "206 ,IN ,0311 ,27 ,TS", "60 ,IN ,0301 ,27 ,TS" }, 2)]
    [InlineData(new string[] { "206 , IN , 0311 , 27 , TS", "60 , IN , 0301 , 27 , TS" }, 2)]
    [Theory]
    public void CapturesCorrectNumber_PoorlyFormattedCommaSeparatedValidBibRecords(string[] commaBibRecords, int expectedCount)
    {  
        List<FlaggedBibRecordModel> actualCommaDelmitedResult = _fileProcessor.GetStrictMatches(commaBibRecords).ToList();
        Assert.NotNull(actualCommaDelmitedResult);
        Assert.Equal(expectedCount, actualCommaDelmitedResult.Count);
    }

    [InlineData(new string[] { "206,IN,0311,27,TS", "60,IN,0301,27,TS" }, 2)]
    [InlineData(new string[] { "206, IN, 0311, 27, TS", "60, IN, 0301, 27, TS" }, 2)]
    [Theory]
    public void CapturesCorrectNumber_WellFormattedCommaSeparatedValidBibRecords(string[] commaBibRecords, int expectedCount)
    {
        List<FlaggedBibRecordModel> actualCommaDelmitedResult = _fileProcessor.GetStrictMatches(commaBibRecords).ToList();
        Assert.NotNull(actualCommaDelmitedResult);
        Assert.Equal(expectedCount, actualCommaDelmitedResult.Count);
    }

    [InlineData(new string[] {
          "115 , 0UT,2oo9, II ,WR",
          "19S,OUT,2009,11,WR",
          "196, OUTIE ,2009 , 11,  Wright Meadow"
        }, 3)]
    [Theory]
    public void CapturesThreeCommaRecords_PoorFormattingAndMistypedValues(string[] commaBibRecords, int expectedCount)
    {
        List<FlaggedBibRecordModel> actualCommaDelimitedResult = _fileProcessor.GetSloppyMatches(commaBibRecords).ToList();
        Assert.NotNull(actualCommaDelimitedResult);
        Assert.Equal(expectedCount, actualCommaDelimitedResult.Count);
    }

    [Fact]
    public void ProcessBibs_ProcessesCommaDelimBibsSuccessfully()
    {
        // Arrange
        int expectedCount = 5;
        List<FlaggedBibRecordModel> bibRecords = new();
        string messageId = "ABC123DEF456";

        var lines = new string[] {
        "Content-Transfer-Encoding: quoted - printable",
        "",
        "-",
        "10,OUT,834,13,CH",
        "10,IN,748,13,CH",
        "34,OUT,449,13,CH",
        "34,IN,406,13,CH",
        "37,OUT,855,13,CH",
        "-----",
        "* The entries in this email are TAB delimited. This allows you to copy and =\r\npaste into a spreadsheet."
        };

        // Act
        bibRecords = _fileProcessor.ProcessBibs(lines, messageId);

        // Assert
        Assert.NotEmpty(bibRecords);
        Assert.Equal(expectedCount, bibRecords.Count);
    }

    [Fact]
    public void CaptureCorrectMessageId_SampleMessage()
    {
        string sampleMsg = SampleMessages.ValidSingleMesageWithSevenBibRecords;
        string expectedResult = "0K3K2DET73LU";
        var actualResult = _fileProcessor.GetMessageId(sampleMsg);
        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void CaptureCorrectMessageIds_MultipleSampleMessages()
    {
        string messageAlpha = SampleMessages.RepliedToWinlinkMessageNoBibData;
        string messageBravo = SampleMessages.NonconformingBibReportMessage;
        string messageCharlie = SampleMessages.ValidSingleMesageWithSevenBibRecords;

        string expectedResultAlpha = "2QH411ZD77CZ";
        string expectedResultBravo = "3HPR0R1L20LD";
        string expectdResultCharlie = "0K3K2DET73LU";

        string actualResultAlpha = _fileProcessor.GetMessageId(messageAlpha);
        string actualResultBravo = _fileProcessor.GetMessageId(messageBravo);
        string actualResultCharlie = _fileProcessor.GetMessageId(messageCharlie);

        Assert.Equal(expectedResultAlpha, actualResultAlpha);
        Assert.Equal(expectedResultBravo, actualResultBravo);
        Assert.Equal(expectdResultCharlie, actualResultCharlie);
    }

    [Fact]
    public void SloppyAndStrictMatchesFindCorrectNumberUsingSampleMessages()
    {
        int expectedCountAlpha = 0;
        int expectedCountBravo = 0;
        int expectedCountCharlie = 7;
        int expectedCountDelta = 26;

        string[] alphaLines = sample_RepliedToWinlinkMessageNoBibData.Split('\n');
        List< FlaggedBibRecordModel> actualStrictResultListAlpha = _fileProcessor.GetStrictMatches(alphaLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultListAlpha = _fileProcessor.GetSloppyMatches(alphaLines).ToList();

        Assert.Equal(expectedCountAlpha, actualStrictResultListAlpha.Count);
        Assert.Equal(expectedCountAlpha, actualSloppyResultListAlpha.Count);

        string[] bravoLines = sample_NonconformingBibReportMessage.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListBravo = _fileProcessor.GetStrictMatches(bravoLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultListBravo = _fileProcessor.GetSloppyMatches(bravoLines).ToList();

        Assert.Equal(expectedCountBravo, actualStrictResultListBravo.Count);
        Assert.Equal(expectedCountBravo, actualSloppyResultListBravo.Count);

        string[] charlieLines = sample_ValidMessageWithBibDataInReplyMessage.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListCharlie = _fileProcessor.GetStrictMatches(charlieLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultListCharlie = _fileProcessor.GetSloppyMatches(charlieLines).ToList();

        Assert.Equal(expectedCountCharlie, actualStrictResultListCharlie.Count);
        Assert.Equal(expectedCountCharlie, actualSloppyResultListCharlie.Count);

        string[] deltaLines = sample_MessageWith26ValidBibs.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListDelta = _fileProcessor.GetStrictMatches(deltaLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultListDelta = _fileProcessor.GetSloppyMatches(deltaLines).ToList();

        Assert.Equal(expectedCountDelta, actualStrictResultListDelta.Count);
        Assert.Equal(expectedCountDelta, actualSloppyResultListDelta.Count);
    }

    [InlineData(new string[] { "1 OUT 1 1 AB" }, 0)]
    [InlineData(new string[] { "1 IN 1 1 AB" }, 0)]
    [InlineData(new string[] { "1 IN 1 1 AB", "1 OUT 1 1 AB" }, 0)]
    [InlineData(new string[] { "1 IN 1 1 AB", "1 IN 1 1 AB" }, 0)]
    [Theory]
    public void UnableToMatch_SpaceDelimitedBibRecords(string[] lines, int expectedCount)
    {
        List<FlaggedBibRecordModel> actualResult = _fileProcessor.GetStrictMatches(lines).ToList();
        Assert.NotNull(actualResult);
        Assert.Equal(expectedCount, actualResult.Count);
    }

    [Fact]
    public void MatchersFailToMatchSpaceDelimitedBibsInValidMessage()
    {
        string spaceDelimitedMessages = SampleMessages.ValidMessageWith5SpaceDelimitedBibs;
        int expectedSpaceDelimitedBibs = 0; // there are 5 space-delimited bibs in the sample msg
        string[] spaceDelimitedLines = spaceDelimitedMessages.Split('\n');

        List<FlaggedBibRecordModel> actualStrictResultList = _fileProcessor.GetStrictMatches(spaceDelimitedLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultList = _fileProcessor.GetSloppyMatches(spaceDelimitedLines).ToList();

        Assert.Equal(expectedSpaceDelimitedBibs, actualStrictResultList.Count);
        Assert.Equal(expectedSpaceDelimitedBibs, actualSloppyResultList.Count);
    }

    [Fact]
    public void MatchersMatchCommaDelimintedBibs_ValidMessage()
    {
        string commaDelimitedMessages = SampleMessages.ValidMessageWithCommaDelimitedBibs;

        // there are 5 comma-delimited bibs in the sample msg
        int expectedCommasTabsCount = 5;
        int expectedLooseCount = 5;
        string[] commaDelimitedLines = commaDelimitedMessages.Split('\n');

        List<FlaggedBibRecordModel> actualStrictResult = _fileProcessor.GetStrictMatches(commaDelimitedLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultList = _fileProcessor.GetSloppyMatches(commaDelimitedLines).ToList();

        Assert.Equal(expectedCommasTabsCount, actualStrictResult.Count);
        Assert.Equal(expectedLooseCount, actualSloppyResultList.Count);
    }

    [Fact]
    public void WriteWinlinkMessageToFile_WritesFileSuccessfully()
    {
        // Arrange
        var targetFolder = Environment.SpecialFolder.CommonDocuments;
        string fileName = "testFilePath";
        string filePath = Path.Combine(Environment.GetFolderPath(targetFolder), fileName);
        Console.WriteLine($"Target filepath is: {filePath}");

        FlaggedBibRecordModel mockBibRecord = new()
        {
            BibNumber = "123",
            Action = "OUT",
            BibTimeOfDay = "1234",
            DayOfMonth = "12",
            Location = "KT",
            DataWarning = false
        };

        WinlinkMessageModel mockMessage = new()
        {
            WinlinkMessageId = "ABC123DEF456",
            ClientHostname = "TestMachine",
            MessageDateStamp = DateTime.Now
        };

        mockMessage.BibRecords.Add(mockBibRecord);

        // Act
        var result = _fileProcessor.WriteWinlinkMessageToFile(mockMessage, filePath);

        if (File.Exists(filePath))
        {
            Thread.Sleep(150);
            File.Delete(filePath);
        }

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void WriteWinlinkMessageToFile_WontWriteFileIfNoBibs()
    {
        // Arrange
        var targetFolder = Environment.SpecialFolder.CommonDocuments;
        string fileName = "testFilePath";
        string filePath = Path.Combine(Environment.GetFolderPath(targetFolder), fileName);
        Console.WriteLine($"Target filepath is: {filePath}");

        WinlinkMessageModel mockMessage = new()
        {
            WinlinkMessageId = "ABC123DEF456",
            ClientHostname = "TestMachine",
            MessageDateStamp = DateTime.Now
        };

        // Act
        var result = _fileProcessor.WriteWinlinkMessageToFile(mockMessage, filePath);

        if (File.Exists(filePath))
        {
            Thread.Sleep(150);
            File.Delete(filePath);
        }

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RecordsArrayToString_ConvertsArrayToString()
    {
        // Arrange
        var array = new string[] { "Hello", "World", "!" };

        // Act
        var result = _fileProcessor.RecordsArrayToString(array);

        // Assert
        Assert.Equal("Hello\nWorld\n!\n", result);
    }

    [Fact]
    public void ProcessWinlinkMessageFile_ProcessesFileSuccessfully()
    {
        // Arrange
        var timestamp = DateTime.Now;
        var machineName = "TestMachine";
        var filePath = "testFilePath.txt";

        // Create a test file
        File.WriteAllText(filePath, SampleMessages.ValidSingleMesageWithSevenBibRecords);

        // Act
        var result = _fileProcessor.ProcessWinlinkMessageFile(timestamp, machineName, filePath);

        // Assert
        Assert.NotNull(result);

        // Clean up the test file
        File.Delete(filePath);
    }

    [Fact]
    public void ProcessWinlinkMessageFile_HandleTimeStamps()
    {
        // single DateTime stamp in message header
        DateTime expectedWinlinkTimestamp = new(2023, 8, 13, 19, 54, 29, DateTimeKind.Utc);
        DateTime actualWlTimeStamp = _fileProcessor.GetWinlinkMessageDateTimeStamp(SampleMessages.ValidSingleMesageWithSevenBibRecords);
        Assert.Equal(expectedWinlinkTimestamp, actualWlTimeStamp);

        // two DateTime stamps in message (one in header, another in forwarded message)
        DateTime expectedWinlinkTimestamp2 = new(2023,8, 13, 19, 54, 29, DateTimeKind.Utc);
        DateTime actualWlTimeStamp2  = _fileProcessor.GetWinlinkMessageDateTimeStamp(SampleMessages.ValidMessageWithBibDataInReplyMessage);
        Assert.Equal(expectedWinlinkTimestamp2, actualWlTimeStamp2);
    }

    [Fact]
    public void FileProcessor_ProcessBibLikeMessageContentShouldFindNone()
    {
        int expectedCount = 0;
        string messageId = "ABC123DEF456";

        string[] lines =
        {
            "\r\n", "\r\n", "306\r\n", "317\r\n", "318\r\n", "322\r\n", "339\r\n", "343\r\n", "348\r\n", "351\r\n", "357\r\n", "359\r\n", "366\r\n", "372\r\n", "374\r\n",
            "396\r\n", "402\r\n", "403\r\n", "404\r\n", "\r\n\r\nJ"
        };

        List<FlaggedBibRecordModel> actualResult = _fileProcessor.ProcessBibs(lines, messageId);

        Assert.Empty(actualResult);
        Assert.Equal(expectedCount, actualResult.Count);
    }

    [Theory]
    [InlineData(new string[] { "115\tOUT\t2009\t11\tWR" }, 1)]
    [InlineData(new string[] { "115\tOUT\t2009\t11\tWR", "195\tOUT\t2009\t11\tWR" }, 2)]
    [InlineData(new string[] { "115,OUT,2009,11,WR" }, 1)]
    [InlineData(new string[] { "115,OUT,2009,11,WR", "195,OUT,2009,11,WR" }, 2)]
    [InlineData(new string[] { "12345678901234567890123456\t12345678901234567890123456\t12345678901234567890123456\t12345678901234567890123456\t12345678901234567890123456" }, 1)]
    [InlineData(new string[] { "123456789012345678901234567\t12345678901234567890123456\t12345678901234567890123456\t12345678901234567890123456\t12345678901234567890123456" }, 0)]
    [InlineData(new string[] { "12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456" }, 1)]
    [InlineData(new string[] { "123456789012345678901234567, 12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456" }, 0)]
    [InlineData(new string[] { "12345678901234567890123456, 123456789012345678901234567, 12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456" }, 0)]
    [InlineData(new string[] { "12345678901234567890123456, 12345678901234567890123456, 123456789012345678901234567, 12345678901234567890123456, 12345678901234567890123456" }, 0)]
    [InlineData(new string[] { "12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456, 123456789012345678901234567, 12345678901234567890123456" }, 0)]
    [InlineData(new string[] { "12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456, 12345678901234567890123456, 123456789012345678901234567" }, 0)]
    [InlineData(new string[] { " \t \t \t \t " }, 0)]
    [InlineData(new string[] { " , , , , " }, 0)]
    [InlineData(new string[] { "\t\t\t\t" }, 0)]
    [InlineData(new string[] { ",,,," }, 0)]
    public void FileProcessor_ProcessSloppyRecordsWithVaryingFieldLengths(string[] lines, int expectedCount)
    {
        HashSet<FlaggedBibRecordModel> actualResult = _fileProcessor.GetSloppyMatches(lines);
        Assert.NotNull(actualResult);
        Assert.Equal(expectedCount, actualResult.Count);
    }

    [Theory]
    [InlineData(new string[] { "115\tOUT\t2009\t11\tWR" }, 1)]
    [InlineData(new string[] { "115\tOUT\t2009\t11\tWR", "195\tOUT\t2009\t11\tWR" }, 2)]
    [InlineData(new string[] { "115,OUT,2009,11,WR" }, 1)]
    [InlineData(new string[] { "115,OUT,2009,11,WR", "195,OUT,2009,11,WR" }, 2)]
    [InlineData(new string[] { "1\tIN\t1\t1\tAB" }, 1)]
    [InlineData(new string[] { "1, IN, 1, 1, AB" }, 1)]
    [InlineData(new string[] { "12345\tIN\t1234\t12\tab" }, 0)]
    [InlineData(new string[] { "12345, IN, 1234, 12, ab" }, 0)]
    [InlineData(new string[] { "1234, IN, 12345, 12, ab" }, 0)]
    [InlineData(new string[] { "1234, IN, 1234, 123, ab" }, 0)]
    [InlineData(new string[] { "1234, IN, 1234, 12, abc" }, 0)]
    [InlineData(new string[] { " \t \t \t \t " }, 0)]
    [InlineData(new string[] { " , , , , " }, 0)]
    [InlineData(new string[] { "\t\t\t\t" }, 0)]
    [InlineData(new string[] { ",,,," }, 0)]
    public void FileProcessor_ProcessStrictRecordsWithVaryingFieldLengths(string[] lines, int expectedCount)
    {
        HashSet<FlaggedBibRecordModel> actualResult = _fileProcessor.GetStrictMatches(lines);
        Assert.NotNull(actualResult);
        Assert.Equal(expectedCount, actualResult.Count);
    }
}
