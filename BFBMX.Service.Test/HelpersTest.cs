using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using Moq;

namespace BFBMX.Service.Test;

public class HelpersTest
{
  private Mock<IFileReader> _fileReaderMock;
  private FileProcessor _fileProcessor;

  public HelpersTest()
  {
    _fileReaderMock = new Mock<IFileReader>();
    _fileProcessor = new FileProcessor(_fileReaderMock.Object);
  }

  [Fact]
  public void ProcessFile_FileDoesNotExist_ReturnsZeroCountList()
  {
    var expectedCount = 0;
    _fileReaderMock.Setup(fr => fr.Exists(It.IsAny<string>())).Returns(false);
    var actualResult = _fileProcessor.ProcessFile("nonExistantFile.txt");
    var actualCount = actualResult.Count;
    Assert.Equal(expectedCount, actualCount);
  }

  [Fact]
  public void ProcessFile_FileExists_ReturnsExpectedRecords()
  {
    int expectedCount = 3;
    _fileReaderMock.Setup(fr => fr.Exists(It.IsAny<string>())).Returns(true);
    _fileReaderMock.Setup(fr => fr.ReadAllLines(It.IsAny<string>())).Returns(new string[] {
      "115	OUT	2009	11	WR",
      "195	OUT	2009	11	WR",
      "196	OUT	2009	11	WR"
     });

    List<BibMessageModel> actualResult = new();
    try
    {
      string? bfBmxTestFilesPath = Environment.GetEnvironmentVariable("BFBMX_TEST_FILES_PATH");
      Assert.True(!string.IsNullOrWhiteSpace(bfBmxTestFilesPath));
      actualResult = _fileProcessor.ProcessFile(Path.Combine(bfBmxTestFilesPath!, "threeBibEntries.txt"));
    }
    catch (FileNotFoundException fnfex)
    {
      Console.WriteLine($"File Not Found exception: {fnfex.Message}");
    }

    Assert.NotNull(actualResult);
    var actualCount = actualResult.Count;
    Assert.Equal(expectedCount, actualCount);
  }
}
