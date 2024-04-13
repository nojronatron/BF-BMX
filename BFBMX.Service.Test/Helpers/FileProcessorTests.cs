using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using BFBMX.Service.Test.TestData;
using System.Diagnostics;
using Moq;
using Microsoft.Extensions.Logging;

namespace BFBMX.Service.Test.Helpers;

public class FileProcessorTests
{
    private readonly IFileProcessor _fileProcessor;
    private readonly Mock<ILogger<FileProcessor>> _mockLogger;

    public static string GenericWinlinkId { get => "ABC123DEF456"; }
    public static string? GetTempDirectory => Environment.GetEnvironmentVariable("TEMP");

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

    [Fact]
    public void CaptureThreeBibRecordsTabsStrict()
    {
        int expectedCount = 3;
        var bibListTabs = new string[] {
          "115\tOUT\t2009\t11\tWR",
          "195\tOUT\t2009\t11\tWR",
          "196\tOUT\t2009\t11\tWR"
        };

        List<FlaggedBibRecordModel> actualResult = _fileProcessor.GetStrictMatches(bibListTabs).ToList();
        Assert.NotNull(actualResult);
        Assert.Equal(expectedCount, actualResult.Count);
    }

    [Fact]
    public void CaptureThreeBibRecordsCommasStrict()
    {  
        int expectedCount = 3;
        var bibListCommas = new string[] {
          "115,OUT,2009,11,WR",
          "195,OUT,2009,11,WR",
          "196,OUT,2009,11,WR"
        };

        List<FlaggedBibRecordModel> actualCommaDelmitedResult = _fileProcessor.GetStrictMatches(bibListCommas).ToList();
        Assert.NotNull(actualCommaDelmitedResult);
        Assert.Equal(expectedCount, actualCommaDelmitedResult.Count);
    }

    [Fact]
    public void CaptureThreeBibRecordsTabsSloppy()
    {
        int expectedCount = 3;
        var bibListTabs = new string[] {
          "115	0UT	2oo9	II	WR",
          "195	OUT	2009	11	WR",
          "196	OUT	2009	11	WR"
        };

        List<FlaggedBibRecordModel> actualTabDelimitedResult = _fileProcessor.GetSloppyMatches(bibListTabs).ToList();
        Assert.NotNull(actualTabDelimitedResult);
        Assert.Equal(expectedCount, actualTabDelimitedResult.Count);
    }

    [Fact]
    public void CaptureThreeBibRecordsCommasSloppy()
    {
        int expectedCount = 3;
        var bibListCommas = new string[] {
          "115 , 0UT,2oo9, II ,WR",
          "195,OUT,2009,11,WR",
          "196, OUT ,2009 , 11,  WR"
        };

        List<FlaggedBibRecordModel> actualCommaDelimitedResult = _fileProcessor.GetSloppyMatches(bibListCommas).ToList();
        Assert.NotNull(actualCommaDelimitedResult);
        Assert.Equal(expectedCount, actualCommaDelimitedResult.Count);
    }

    [Fact]
    public void DetectsCorrectMessageIdFromSampleMessage()
    {
        string sampleMsg = SampleMessages.ValidSingleMesageWithSevenBibRecords;
        string expectedResult = "0K3K2DET73LU";
        var actualResult = _fileProcessor.GetMessageId(sampleMsg);
        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void DetectsCorrectMessageIdsInMultipleInputs()
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
    public void ComparesStrictAndSloppyMatchesSeparately()
    {
        string bibAlpha = "115	OUT	2009	11	WR";
        string bibBravo = "195	OUT	2009	11	WR";
        string bibCharlie = "196	OUT	2009	11	WR";
        string badRecordAlpha = "115   0UT    2oo9    II    WR";
        string badRecordBravo = "195   OUT    2009    11    wr";
        string badRecordCharlie = "123456789012	DROPP	09	33	WR";
        string[] bibInput = { bibAlpha, bibBravo, bibCharlie, badRecordAlpha, badRecordBravo, badRecordCharlie };

        var strictResult = _fileProcessor.GetStrictMatches(bibInput);
        var sloppyResult = _fileProcessor.GetSloppyMatches(bibInput);

        Assert.True(strictResult.Count == 3);
        foreach(var result in strictResult)
        {
            Debug.WriteLine($"Strict: {result.ToTabbedString()}");
            Assert.False(result.DataWarning);
        }

        Assert.True(sloppyResult.Count == 4);
        foreach(var result in sloppyResult)
        {
            // Note: Parse results *could* return a good bib without a data warning!
            Debug.WriteLine($"Sloppy: {result.ToTabbedString()}");
        }
    }

    [Fact]
    public void SloppyAndStrictMatchesFindCorrectNumberUsingSampleMessages()
    {
        string messageAlpha = SampleMessages.RepliedToWinlinkMessageNoBibData;
        string messageBravo = SampleMessages.NonconformingBibReportMessage;
        string messageCharlie = SampleMessages.ValidMessageWithBibDataInReplyMessage;
        string messageDelta = SampleMessages.MessageWith26ValidBibs;

        int expectedCountAlpha = 0;
        int expectedCountBravo = 0;
        int expectedCountCharlie = 7;
        int expectedCountDelta = 26;

        string[] alphaLines = messageAlpha.Split('\n');
        List< FlaggedBibRecordModel> actualStrictResultListAlpha = _fileProcessor.GetStrictMatches(alphaLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultListAlpha = _fileProcessor.GetSloppyMatches(alphaLines).ToList();

        Assert.Equal(expectedCountAlpha, actualStrictResultListAlpha.Count);
        Assert.Equal(expectedCountAlpha, actualSloppyResultListAlpha.Count);

        string[] bravoLines = messageBravo.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListBravo = _fileProcessor.GetStrictMatches(bravoLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultListBravo = _fileProcessor.GetSloppyMatches(bravoLines).ToList();

        Assert.Equal(expectedCountBravo, actualStrictResultListBravo.Count);
        Assert.Equal(expectedCountBravo, actualSloppyResultListBravo.Count);

        string[] charlieLines = messageCharlie.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListCharlie = _fileProcessor.GetStrictMatches(charlieLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultListCharlie = _fileProcessor.GetSloppyMatches(charlieLines).ToList();

        Assert.Equal(expectedCountCharlie, actualStrictResultListCharlie.Count);
        Assert.Equal(expectedCountCharlie, actualSloppyResultListCharlie.Count);

        string[] deltaLines = messageDelta.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListDelta = _fileProcessor.GetStrictMatches(deltaLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultListDelta = _fileProcessor.GetSloppyMatches(deltaLines).ToList();

        Assert.Equal(expectedCountDelta, actualStrictResultListDelta.Count);
        Assert.Equal(expectedCountDelta, actualSloppyResultListDelta.Count);
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
        int expectedStrictCount = 1;
        int expectedLooseCount = 5;
        string[] commaDelimitedLines = commaDelimitedMessages.Split('\n');

        List<FlaggedBibRecordModel> actualStrictResult = _fileProcessor.GetStrictMatches(commaDelimitedLines).ToList();
        List<FlaggedBibRecordModel> actualSloppyResultList = _fileProcessor.GetSloppyMatches(commaDelimitedLines).ToList();

        Assert.Equal(expectedStrictCount, actualStrictResult.Count);
        Assert.Equal(expectedLooseCount, actualSloppyResultList.Count);
    }

    [Fact]
    public void SloppyMatchesFindsFourBibRecords()
    {
        int expectedCount = 4;
        string[] expectedResult = {"115\t0UT\t2oo9\t-1\tWR\tWarning",
                                   "195\tOUT\t2009\t11\tWR",
                                   "-1\tDROPP\t09\t33\tWR\tWarning",
                                   "-1\tDROP\t1234\t23\tWR\tWarning"};

        string[] bibList = {"115\t0UT\t2oo9\tII\tWR",
                            "195\tOUT\t2009\t11\twr",
                            "123456789012\tDROPP\t09\t33\tWR",
                            "one\tDROP\t1234\t23\tWR"};

        List<FlaggedBibRecordModel> actualResult = _fileProcessor.GetSloppyMatches(bibList).ToList();
        Assert.Equal(expectedCount, actualResult.Count);

        for (int idx = 0; idx < expectedResult.Length; idx++)
        {
            Debug.WriteLine($"Expected: {expectedResult[idx]}");
            Debug.WriteLine($"Actual: {actualResult[idx].ToTabbedString()}");
        }
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
}
