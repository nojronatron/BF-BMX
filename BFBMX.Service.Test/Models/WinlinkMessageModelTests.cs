using BFBMX.Service.Models;

namespace BFBMX.Service.Test.Models;

public class WinlinkMessageModelTests
{
    [Fact]
    public void InstantiateAllFields()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            ClientDateTime = new DateTime(2024, 01, 02, 12, 11, 10),
            ClientHostname = "test-hostname",
            BibRecords = new List<BibRecordModel>
      {
          new BibRecordModel
          {
          BibNumber = 1,
          @Action = "IN",
          BibTimeOfDay = "1314",
          DayOfMonth = 2,
          Location = "test-location",
          DataWarning = false,
          }
      }
        };

        Assert.NotNull(sut);
    }

    [Fact]
    public void InstantiateWithSingleNullishBibRecord()
    {
        var sut = new WinlinkMessageModel();

        // test nullable fields
        Assert.Null(sut.WinlinkMessageId);
        // If client datetime is null there is nothing this code could do about it
        Assert.Null(sut.ClientHostname);

        // tests NON-nullable fields
        Assert.False(sut.HasDataWarning());
    }

    [Fact]
    public void InstantiateWithNulls()
    {
        var bibNum = -1;
        var dayOfMonth = -1;

        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = null,
            ClientDateTime = new DateTime(),
            ClientHostname = null,
            BibRecords = new List<BibRecordModel>
      {
            new BibRecordModel()
      }
        };

        // test nullable fields
        Assert.Null(sut.WinlinkMessageId);
        // If client datetime is null there is nothing this code could do about it
        Assert.Null(sut.ClientHostname);
        Assert.True(sut.BibRecords.Count > 0);

        Assert.Equal("MISSING", sut.BibRecords[0].Action);
        Assert.Null(sut.BibRecords[0].BibTimeOfDay);
        Assert.Equal("MISSING", sut.BibRecords[0].Location);

        // tests NON-nullable fields
        Assert.Equal(bibNum, sut.BibRecords[0].BibNumber);
        Assert.Equal(dayOfMonth, sut.BibRecords[0].DayOfMonth);
        Assert.True(sut.BibRecords[0].DataWarning);
    }

    [Fact]
    public void MessageToFilename()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<BibRecordModel>
      {
            new BibRecordModel
            {
            BibNumber = 1,
            @Action = "IN",
            BibTimeOfDay = "1314",
            DayOfMonth = 2,
            Location = "test-location",
            DataWarning = false,
            }
        }
        };
        var expected = "test-hostname-2024-01-02T13-12-11.txt";
        var actual = sut.ToFilename();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageToJSON()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<BibRecordModel>
            {
                new BibRecordModel
                {
                    BibNumber = 1,
                    @Action = "IN",
                    BibTimeOfDay = "1314",
                    DayOfMonth = 2,
                    Location = "test-location",
                    DataWarning = false,
                }
            }
        };

        var expected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"ClientDateTime\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"BibRecords\":[{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":false}]}";
        var actual = sut.ToJson();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageWithWarningToJSON()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<BibRecordModel>
            {
                new BibRecordModel
                {
                    BibNumber = 1,
                    @Action = "IN",
                    BibTimeOfDay = "13145",
                    DayOfMonth = 2,
                    Location = "test-location",
                    DataWarning = true,
                }
            }
        };

        var expected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"ClientDateTime\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"BibRecords\":[{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"13145\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":true}]}";
        var actual = sut.ToJson();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageTabDelimitedFormat()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<BibRecordModel>
            {
                new BibRecordModel
                {
                    BibNumber = 1,
                    @Action = "IN",
                    BibTimeOfDay = "1314",
                    DayOfMonth = 2,
                    Location = "test-location",
                    DataWarning = false,
                }
            }
        };

        var expected = "1\tIN\t1314\t2\ttest-location";
        var actual = sut.BibRecords[0].BibDataToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageToString()
    {
        var bibEntry = new BibRecordModel
        {
            BibNumber = 1,
            @Action = "IN",
            BibTimeOfDay = "1314",
            DayOfMonth = 2,
            Location = "test-location",
            DataWarning = false,
        };

        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<BibRecordModel>
            {
                bibEntry
            }
        };

        var expected = "ABCDEFGHIJKL\ttest-hostname-2024-01-02T13-12-11.txt\tBibs: [ 1\tIN\t1314\t2\ttest-location\t ]\t";//\tOK\t";
        var actual = sut.ToString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageToStringWithWarning()
    {
        var bibEntry = new BibRecordModel
        {
            BibNumber = 1,
            @Action = "IN",
            BibTimeOfDay = "1314",
            DayOfMonth = 2,
            Location = "test-location",
            DataWarning = true,
        };

        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            ClientDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<BibRecordModel>
            {
                bibEntry
            }
        };

        var expected = "ABCDEFGHIJKL\ttest-hostname-2024-01-02T13-12-11.txt\tBibs: [ 1\tIN\t1314\t2\ttest-location\tWarning!\t ]\t";
        var actual = sut.ToString();
        Assert.Equal(expected, actual);
    }
}
