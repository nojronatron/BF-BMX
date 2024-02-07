using BFBMX.Service.Models;

namespace BFBMX.Service.Test;

public class ModelsTest
{
  [Fact]
  public void InstantiateWithAllFields()
  {
    var sut = new BibMessageModel
    {
      WinlinkMessageId = "ABCDEFGHIJKL",
      ClientDateTime = new DateTime(2024, 01, 02, 12, 11, 10),
      ClientHostname = "test-hostname",
      BibNumber = 1,
      @Action = "IN",
      BibTimeOfDay = "1314",
      DayOfMonth = 2,
      Location = "test-location",
      DataWarning = false,
    };
    Assert.NotNull(sut);
  }
  [Fact]
  public void InstantiateNull()
  {
    var sut = new BibMessageModel();

    // test nullable fields
    Assert.Null(sut.WinlinkMessageId);
    // If client datetime is null there is nothing this code could do about it
    Assert.Null(sut.ClientHostname);
    Assert.Null(sut.Action);
    Assert.Null(sut.BibTimeOfDay);
    Assert.Null(sut.Location);

    // tests NON-nullable fields
    Assert.Equal(-1, sut.BibNumber);
    Assert.Equal(-1, sut.DayOfMonth);
    Assert.True(sut.DataWarning);
  }

  [Fact]
  public void InstantiateWithNulls()
  {
    var bibNum = 0;
    var dayOfMonth = 0;

    var sut = new BibMessageModel
    {
      WinlinkMessageId = null,
      ClientDateTime = new DateTime(),
      ClientHostname = null,
      BibNumber = bibNum,
      @Action = null,
      BibTimeOfDay = null,
      DayOfMonth = dayOfMonth,
      Location = null,
      DataWarning = false,
    };

    // test nullable fields
    Assert.Null(sut.WinlinkMessageId);
    // If client datetime is null there is nothing this code could do about it
    Assert.Null(sut.ClientHostname);
    Assert.Null(sut.Action);
    Assert.Null(sut.BibTimeOfDay);
    Assert.Null(sut.Location);

    // tests NON-nullable fields
    Assert.Equal(bibNum, sut.BibNumber);
    Assert.Equal(dayOfMonth, sut.DayOfMonth);
    Assert.False(sut.DataWarning);
  }
  [Fact]
  public void ToFilename_Hostname_ClientDateTimeCustomFormatting()
  {
    var sut = new BibMessageModel
    {
      WinlinkMessageId = "ABCDEFGHIJKL",
      ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
      ClientHostname = "test-hostname",
      BibNumber = 1,
      @Action = "IN",
      BibTimeOfDay = "1314",
      DayOfMonth = 2,
      Location = "test-location",
      DataWarning = false,
    };
    var expected = "test-hostname-2024-01-02T13-12-11.txt";
    var actual = sut.ToFilename();
    Assert.Equal(expected, actual);
  }
  [Fact]
  public void ToJson_EntireModelNoBlanksOrErrors()
  {
    var sut = new BibMessageModel
    {
      WinlinkMessageId = "ABCDEFGHIJKL",
      ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
      ClientHostname = "test-hostname",
      BibNumber = 1,
      @Action = "IN",
      BibTimeOfDay = "1314",
      DayOfMonth = 2,
      Location = "test-location",
      DataWarning = false,
    };
    var expected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"ClientDateTime\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":false}";
    var actual = sut.ToJson();
    Assert.Equal(expected, actual);
  }
  [Fact]
  public void ToString_BibDataOnlyInTabDelimitedFormat()
  {
    var sut = new BibMessageModel
    {
      WinlinkMessageId = "ABCDEFGHIJKL",
      ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
      ClientHostname = "test-hostname",
      BibNumber = 1,
      @Action = "IN",
      BibTimeOfDay = "1314",
      DayOfMonth = 2,
      Location = "test-location",
      DataWarning = false,
    };
    var expected = "1\tIN\t1314\t2\ttest-location";
    var actual = sut.BibDataToString();
    Assert.Equal(expected, actual);
  }
  [Fact]
  public void ToString_MessageId_ClientDT_ClientName_BibData_DataWarning()
  {
    var sut = new BibMessageModel
    {
      WinlinkMessageId = "ABCDEFGHIJKL",
      ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
      ClientHostname = "test-hostname",
      BibNumber = 1,
      @Action = "IN",
      BibTimeOfDay = "1314",
      DayOfMonth = 2,
      Location = "test-location",
      DataWarning = false,
    };
    var expected = "ABCDEFGHIJKL\ttest-hostname-2024-01-02T13-12-11.txt\ttest-hostname\t1\tIN\t1314\t2\ttest-location\tOK";
    var actual = sut.ToString();
    Assert.Equal(expected, actual);

    sut.DataWarning = true;
    expected = "ABCDEFGHIJKL\ttest-hostname-2024-01-02T13-12-11.txt\ttest-hostname\t1\tIN\t1314\t2\ttest-location\tData Warning";
    actual = sut.ToString();
    Assert.Equal(expected, actual);
  }
}
