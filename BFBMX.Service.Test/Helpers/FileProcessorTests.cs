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
        string? tempDir = Environment.GetEnvironmentVariable("TEMP");

        if (!string.IsNullOrWhiteSpace(tempDir))
        {
            var tempFile = Path.Combine(tempDir, "tempfiledata.txt");
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
    public void CaptureThreeBibRecordsStrict()
    {
        int expectedCount = 3;
        var bibList = new string[] {
          "115\tOUT\t2009\t11\tWR",
          "195\tOUT\t2009\t11\tWR",
          "196\tOUT\t2009\t11\tWR"
        };

        List<FlaggedBibRecordModel> actualResult = new();
        bool strictMatches = _fileProcessor.ProcessBibs(actualResult, bibList, GenericWinlinkId);
        Assert.True(strictMatches);
        Assert.NotNull(actualResult);
        var actualCount = actualResult.Count;
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public void CaptureThreeBibRecordsNotStrict()
    {
        int expectedCount = 3;
        var bibList = new string[] {
          "115	0UT	2oo9	II	WR",
          "195	OUT	2009	11	WR",
          "196	OUT	2009	11	WR"
        };

        List<FlaggedBibRecordModel> actualResult = new();
        bool strictMatches = _fileProcessor.ProcessBibs(actualResult, bibList, GenericWinlinkId);
        Assert.True(strictMatches);
        Assert.NotNull(actualResult);
        var actualCount = actualResult.Count;
        Assert.Equal(expectedCount, actualCount);
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
        List< FlaggedBibRecordModel> actualStrictResultListAlpha = _fileProcessor.GetStrictMatches(alphaLines);
        List<FlaggedBibRecordModel> actualSloppyResultListAlpha = _fileProcessor.GetSloppyMatches(alphaLines);

        Assert.Equal(expectedCountAlpha, actualStrictResultListAlpha.Count);
        Assert.Equal(expectedCountAlpha, actualSloppyResultListAlpha.Count);

        string[] bravoLines = messageBravo.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListBravo = _fileProcessor.GetStrictMatches(bravoLines);
        List<FlaggedBibRecordModel> actualSloppyResultListBravo = _fileProcessor.GetSloppyMatches(bravoLines);

        Assert.Equal(expectedCountBravo, actualStrictResultListBravo.Count);
        Assert.Equal(expectedCountBravo, actualSloppyResultListBravo.Count);

        string[] charlieLines = messageCharlie.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListCharlie = _fileProcessor.GetStrictMatches(charlieLines);
        List<FlaggedBibRecordModel> actualSloppyResultListCharlie = _fileProcessor.GetSloppyMatches(charlieLines);

        Assert.Equal(expectedCountCharlie, actualStrictResultListCharlie.Count);
        Assert.Equal(expectedCountCharlie, actualSloppyResultListCharlie.Count);

        string[] deltaLines = messageDelta.Split('\n');
        List<FlaggedBibRecordModel> actualStrictResultListDelta = _fileProcessor.GetStrictMatches(deltaLines);
        List<FlaggedBibRecordModel> actualSloppyResultListDelta = _fileProcessor.GetSloppyMatches(deltaLines);

        Assert.Equal(expectedCountDelta, actualStrictResultListDelta.Count);
        Assert.Equal(expectedCountDelta, actualSloppyResultListDelta.Count);
    }

    [Fact]
    public void MatchersFailToMatchSpaceDelimitedBibsInValidMessage()
    {
        string spaceDelimitedMessages = SampleMessages.ValidMessageWith5SpaceDelimitedBibs;
        int expectedSpaceDelimitedBibs = 0; // there are 5 space-delimited bibs in the sample msg
        string[] spaceDelimitedLines = spaceDelimitedMessages.Split('\n');

        List<FlaggedBibRecordModel> actualStrictResultList = _fileProcessor.GetStrictMatches(spaceDelimitedLines);
        List<FlaggedBibRecordModel> actualSloppyResultList = _fileProcessor.GetSloppyMatches(spaceDelimitedLines);

        Assert.Equal(expectedSpaceDelimitedBibs, actualStrictResultList.Count);
        Assert.Equal(expectedSpaceDelimitedBibs, actualSloppyResultList.Count);
    }

    [Fact]
    public void MatchersFailToMatchCommaDelimintedBibsInValidMessage()
    {
        string commaDelimitedMessages = SampleMessages.ValidMessageWithCommaDelimintedBibs;
        int expectedCommaDelimitedBibs = 0; // there are 5 comma-delimited bibs in the sample msg
        string[] commaDelimitedLines = commaDelimitedMessages.Split('\n');

        List<FlaggedBibRecordModel> actualStrictResult = _fileProcessor.GetStrictMatches(commaDelimitedLines);
        List<FlaggedBibRecordModel> actualSloppyResultList = _fileProcessor.GetSloppyMatches(commaDelimitedLines);

        Assert.Equal(expectedCommaDelimitedBibs, actualStrictResult.Count);
        Assert.Equal(expectedCommaDelimitedBibs, actualSloppyResultList.Count);
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

        List<FlaggedBibRecordModel> actualResult = _fileProcessor.GetSloppyMatches(bibList);
        Assert.Equal(expectedCount, actualResult.Count);

        for (int idx = 0; idx < expectedResult.Length; idx++)
        {
            Debug.WriteLine($"Expected: {expectedResult[idx].ToString()}");
            Debug.WriteLine($"Actual: {actualResult[idx].ToTabbedString()}");
        }
    }
}
