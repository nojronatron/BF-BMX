using BFBMX.Service.Models;
using System.Diagnostics;

namespace BFBMX.Service.Test.Models;

public class WinlinkMessageModelTests
{
    public string NoDataWarningText => "NOMINAL";
    public string DataWarningText => "ALERT";
    public static DateTime JanuarySecond => new DateTime(2024,01,02,03,04,05);
    public static DateTime FebruaryThird => new DateTime(2024,02,03,04,05,06);
    public static DateTime MarchFourth => new DateTime(2024, 03, 04, 05, 06, 07);

    [Fact]
    public void InstantiateAllFields()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateStamp = new DateTime(2024, 01, 02, 12, 11, 10),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
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
            MessageDateStamp = new DateTime(),
            ClientHostname = null,
            FileCreatedTimeStamp = new DateTime(),
            BibRecords = new List<FlaggedBibRecordModel>
            {
                new FlaggedBibRecordModel()
            }
        };

        // test nullable fields
        Assert.Null(sut.WinlinkMessageId);
        // DateTime type properties cannot be null
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
            MessageDateStamp = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
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

        // todo: Should it be Message date-time, or file created date-time?
        var expected = "ABCDEFGHIJKL.txt";
        var actual = sut.ToFilename();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageToJSON()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateStamp = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
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

        var singleBibExpected =
            "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\"," +
            "\"MessageDateStamp\":\"2024-01-02T13:12:11\"," +
            "\"ClientHostname\":\"test-hostname\"," +
            "\"FileCreatedTimeStamp\":\"2023-08-13T23:22:21\"," +
            "\"BibRecords\":" +
            "[{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":false}]}";
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

        var twoBibsExpected =
            "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\"," +
            "\"MessageDateStamp\":\"2024-01-02T13:12:11\"," +
            "\"ClientHostname\":\"test-hostname\"," +
            "\"FileCreatedTimeStamp\":\"2023-08-13T23:22:21\"," +
            "\"BibRecords\":[" +
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
            MessageDateStamp = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
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

        var expected =
            "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\"," +
            "\"MessageDateStamp\":\"2024-01-02T13:12:11\"," +
            "\"ClientHostname\":\"test-hostname\"," +
            "\"FileCreatedTimeStamp\":\"2023-08-13T23:22:21\"," +
            "\"BibRecords\":[{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"13145\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":true}]}";
        var actual = sut.ToJsonString();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void MessageTabDelimitedFormat()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateStamp = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
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

        var expected = $"{NoDataWarningText}\t1\tIN\t1314\t2\ttest-location";
        var actual = sut.BibRecords[0].ToTabbedString();
        Assert.Equal(expected, actual);
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
            MessageDateStamp = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
            BibRecords = new List<FlaggedBibRecordModel>
            {
                bibEntry
            }
        };

        var oneLineExpected = $"ABCDEFGHIJKL\t2024-01-02T13-12-11\t{NoDataWarningText}\t1\tIN\t1314\t2\ttest-location\r\n";
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

        var twoLinesExpected = $"ABCDEFGHIJKL\t2024-01-02T13-12-11\t{NoDataWarningText}\t1\tIN\t1314\t2\ttest-location\r\nABCDEFGHIJKL\t2024-01-02T13-12-11\t{DataWarningText}\t1\tOUTT\t2014\t2\ttest-location\r\n";
        string twoLinesActual = sut.ToAccessDatabaseTabbedString();
        Debug.WriteLine($"Expected:\r\n{twoLinesExpected}\r\nActual:\r\n{twoLinesActual}");
        Assert.Equal(twoLinesExpected, twoLinesActual);
    }

    [Fact]
    public void GetHashCodeReturnsUniqueHashCode()
    {
        var winlinkMessage1 = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABC123",
            MessageDateStamp = MarchFourth,
            ClientHostname = "localhost",
            FileCreatedTimeStamp = MarchFourth.AddDays(1),
            BibRecords = new List<FlaggedBibRecordModel>()
        };

        var winlinkMessage2 = new WinlinkMessageModel
        {
            WinlinkMessageId = "XYZ789",
            MessageDateStamp = MarchFourth.AddDays(1),
            ClientHostname = "localhost",
            FileCreatedTimeStamp = MarchFourth.AddDays(1),
            BibRecords = new List<FlaggedBibRecordModel>()
        };

        var hashCode1 = winlinkMessage1.GetHashCode();
        var hashCode2 = winlinkMessage2.GetHashCode();

        Assert.NotEqual(hashCode1, hashCode2);
    }

    [Fact]
    public void OverrideEqualsReturnsTrueForEqualObjects()
    {
        var winlinkMessage1 = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABC123",
            MessageDateStamp = MarchFourth.AddDays(-1),
            ClientHostname = "localhost",
            FileCreatedTimeStamp = MarchFourth,
            BibRecords = new List<FlaggedBibRecordModel>()
        };

        var winlinkMessage2 = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABC123",
            MessageDateStamp = MarchFourth.AddDays(-1),
            ClientHostname = "localhost",
            FileCreatedTimeStamp = MarchFourth,
            BibRecords = new List<FlaggedBibRecordModel>()
        };

        Assert.True(winlinkMessage1.Equals(winlinkMessage2));
    }

    [Fact]
    public void OverrideEqualsReturnsFalseForNotEqualObjects()
    {
        var winlinkMessage1 = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABC123DEF456",
            MessageDateStamp = JanuarySecond,
            ClientHostname = "localhost",
            FileCreatedTimeStamp = FebruaryThird,
            BibRecords = new List<FlaggedBibRecordModel>()
        };

        var winlinkMessage2 = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABC123DEF856",
            MessageDateStamp = MarchFourth,
            ClientHostname = "localhost",
            FileCreatedTimeStamp = JanuarySecond,
            BibRecords = new List<FlaggedBibRecordModel>()
        };

        Assert.False(winlinkMessage1.Equals(winlinkMessage2));
    }

    //[Fact]
    //public void PrintableDateTimeReturnsCorrectFormat()
    //{
    //    WinlinkMessageModel sut = new();
    //    string expectedResult = "2024-01-02T03-04-05";
    //    Assert.True(expectedResult.Equals(sut.PrintableMsgDateTime(JanuarySecond)));
    //}

    [Fact]
    public void HasDataWarning()
    {
        FlaggedBibRecordModel alpha = new()
        {
            BibNumber = 1,
            @Action = "IN",
            BibTimeOfDay = "1314",
            DayOfMonth = 2,
            Location = "test-location",
            DataWarning = false,
        };

        FlaggedBibRecordModel bravo = new()
        {
            BibNumber = 2,
            @Action = "OUT",
            BibTimeOfDay = "2014",
            DayOfMonth = 2,
            Location = "test-location",
            DataWarning = true,
        };

        List<FlaggedBibRecordModel> bibs = new()
        {
            alpha, bravo
        };

        WinlinkMessageModel sut = new()
        {
            WinlinkMessageId = "ABC123DEF856",
            MessageDateStamp = MarchFourth,
            ClientHostname = "localhost",
            FileCreatedTimeStamp = JanuarySecond,
            BibRecords = bibs
        };

        Assert.True(sut.HasDataWarning());
    }

    [Fact]
    public void GetWinlinkMessageInstanceReturnsWinlinkMessageModel()
    {
        string winlinkMessageId = "ABC123DEF856";
        DateTime messageDateTime = MarchFourth;
        string clientHostname = "localhost";
        DateTime fileCreatedDateTime = JanuarySecond;
        List<FlaggedBibRecordModel> bibRecords = new()
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
        };

        WinlinkMessageModel sut = WinlinkMessageModel.GetWinlinkMessageInstance(winlinkMessageId, messageDateTime, clientHostname, fileCreatedDateTime, bibRecords);

        Assert.NotNull(sut);
        Assert.Equal(winlinkMessageId, sut.WinlinkMessageId);
        Assert.Equal(messageDateTime, sut.MessageDateStamp);
        Assert.Equal(clientHostname, sut.ClientHostname);
        Assert.Equal(fileCreatedDateTime, sut.FileCreatedTimeStamp);
        Assert.Single(sut.BibRecords);
        Assert.Equal(1, sut.BibRecords[0].BibNumber);
    }
}
