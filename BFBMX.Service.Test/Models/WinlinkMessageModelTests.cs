using BFBMX.Service.Models;
using System.Diagnostics;

namespace BFBMX.Service.Test.Models;

public class WinlinkMessageModelTests
{
    [Fact]
    public void InstantiateAllFields()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateTime = new DateTime(2024, 01, 02, 12, 11, 10),
            ClientHostname = "test-hostname",
            BibRecords = new List<FlaggedBibRecordModel>
      {
          new FlaggedBibRecordModel
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
            MessageDateTime = new DateTime(),
            ClientHostname = null,
            BibRecords = new List<FlaggedBibRecordModel>
            {
                new FlaggedBibRecordModel()
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
            MessageDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<FlaggedBibRecordModel>
      {
            new FlaggedBibRecordModel
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
        var expected = "ABCDEFGHIJKL-2024-01-02T13-12-11.txt";
        var actual = sut.ToFilename();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageToJSON()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<FlaggedBibRecordModel>
            {
                new FlaggedBibRecordModel
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

        var singleBibExpected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"MessageDateTime\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"BibRecords\":[" +
            "{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":false}]}";
        var singleBibActual = sut.ToJsonString();
        
        Debug.WriteLine($"singleBibExpected:\r\n{singleBibExpected}");
        Debug.WriteLine($"singleBibActual:\r\n{singleBibActual}");
        Assert.Equal(singleBibExpected, singleBibActual);

        var newRecord = new FlaggedBibRecordModel
        {
            BibNumber = 1,
            @Action = "OUTT",
            BibTimeOfDay = "2014",
            DayOfMonth = 2,
            Location = "test-location",
            DataWarning = true
        };

        sut.BibRecords.Add(newRecord);

        var twoBibsExpected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"MessageDateTime\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"BibRecords\":[" +
            "{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":false}," +
            "{\"BibNumber\":1,\"Action\":\"OUTT\",\"BibTimeOfDay\":\"2014\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":true}]}";
        var twoBibsActual = sut.ToJsonString();

        Debug.WriteLine($"twobibsExpected:\r\n{twoBibsExpected}");
        Debug.WriteLine($"twoBibsActual:\r\n{twoBibsActual}");
        Assert.Equal(twoBibsExpected, twoBibsActual);
    }

    [Fact]
    public void MessageWithWarningToJSON()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<FlaggedBibRecordModel>
            {
                new FlaggedBibRecordModel
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

        var expected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"MessageDateTime\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"BibRecords\":[{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"13145\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":true}]}";
        var actual = sut.ToJsonString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageTabDelimitedFormat()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<FlaggedBibRecordModel>
            {
                new FlaggedBibRecordModel
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
        var actual = sut.BibRecords[0].ToTabbedString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageToServerAuditTabbedString()
    {
        var bibEntry = new FlaggedBibRecordModel
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
            MessageDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<FlaggedBibRecordModel>
            {
                bibEntry
            }
        };

        var oneLineExpected = "ABCDEFGHIJKL\ttest-hostname\t1\tIN\t1314\t2\ttest-location\r\n";
        var oneLineActual = sut.ToServerAuditTabbedString();
        Debug.WriteLine($"Expected:\r\n{oneLineExpected}\r\nActual:\r\n{oneLineActual}");
        Assert.Equal(oneLineExpected, oneLineActual);

        var newRecord = FlaggedBibRecordModel.GetBibRecordModel(1, "OUTT", "2014", 2, "test-location", true);
        sut.BibRecords.Add(newRecord);

        var twoLinesExpected = "ABCDEFGHIJKL\ttest-hostname\t1\tIN\t1314\t2\ttest-location\r\nABCDEFGHIJKL\ttest-hostname\t1\tOUTT\t2014\t2\ttest-location\tWarning\r\n";
        string twoLinesActual = sut.ToServerAuditTabbedString();
        Debug.WriteLine($"Expected:\r\n{twoLinesExpected}\r\nActual:\r\n{twoLinesActual}");
        Assert.Equal(twoLinesExpected, twoLinesActual);
    }

    [Fact]
    public void MessageToStringWithWarning()
    {
        var bibEntry = new FlaggedBibRecordModel
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
            MessageDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<FlaggedBibRecordModel>
            {
                bibEntry
            }
        };

        var expected = "ABCDEFGHIJKL\ttest-hostname\t1\tIN\t1314\t2\ttest-location\tWarning\r\n";
        var oneLIneActual = sut.ToServerAuditTabbedString();
        Debug.WriteLine($"Expected:\r\n{expected}\r\nActual:\r\n{oneLIneActual}");
        Assert.Equal(expected, oneLIneActual);
    }


    [Fact]
    public void MessageToAccessDatabaseTabbedString()
    {
        var bibEntry = new FlaggedBibRecordModel
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
            MessageDateTime = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            BibRecords = new List<FlaggedBibRecordModel>
            {
                bibEntry
            }
        };

        var oneLineExpected = "ABCDEFGHIJKL\t2024-01-02T13-12-11\t1\tIN\t1314\t2\ttest-location\r\n";
        var oneLineActual = sut.ToAccessDatabaseTabbedString();
        Debug.WriteLine($"Expected:\r\n{oneLineExpected}\r\nActual:\r\n{oneLineActual}");
        Assert.Equal(oneLineExpected, oneLineActual);

        var newRecord = new FlaggedBibRecordModel
        {
            BibNumber = 1,
            @Action = "OUTT",
            BibTimeOfDay = "2014",
            DayOfMonth = 2,
            Location = "test-location",
            DataWarning = true
        };
        sut.BibRecords.Add(newRecord);

        var twoLinesExpected = "ABCDEFGHIJKL\t2024-01-02T13-12-11\t1\tIN\t1314\t2\ttest-location\r\nABCDEFGHIJKL\t2024-01-02T13-12-11\t1\tOUTT\t2014\t2\ttest-location\tWarning\r\n";
        string twoLinesActual = sut.ToAccessDatabaseTabbedString();
        Debug.WriteLine($"Expected:\r\n{twoLinesExpected}\r\nActual:\r\n{twoLinesActual}");
        Assert.Equal(twoLinesExpected, twoLinesActual);
    }
}
