using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using BFBMX.Service.Test.TestData;

namespace BFBMX.Service.Test;

public class FileProcessorTests
{
    [Fact]
    public void FileDoesNotExist_ReturnsZeroCountList()
    {
        var expectedCount = 0;
        var actualResult = FileProcessor.GetFileData("nonexistentfile.txt");
        var actualCount = actualResult.Length;
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public void CaptureThreeBibRecordsStrict()
    {
        int expectedCount = 3;
        var bibList = new string[] {
      "115	OUT	2009	11	WR",
      "195	OUT	2009	11	WR",
      "196	OUT	2009	11	WR"
     };

        List<BibRecordModel> actualResult = new();
        bool strictMatches = FileProcessor.ProcessBibs(actualResult, bibList);
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

        List<BibRecordModel> actualResult = new();
        bool strictMatches = FileProcessor.ProcessBibs(actualResult, bibList);
        Assert.False(strictMatches);
        Assert.NotNull(actualResult);
        var actualCount = actualResult.Count;
        Assert.Equal(expectedCount, actualCount);
    }

    [Fact]
    public void DetectsCorrectMessageId()
    {
        string sampleMsg = SampleMessages.AlphaMsg;
        string expectedResult = "0K3K2DET73LU";
        var actualResult = FileProcessor.GetMessageId(sampleMsg);
        Assert.Equal(expectedResult, actualResult);
    }
}
