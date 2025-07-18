using BFBMX.Service.Models;
using System.Diagnostics;

namespace BFBMX.Service.Test.Models;

public class WinlinkMessageModelTests
{
    public static string NoDataWarningText => "NOMINAL";
    public static string DataWarningText => "ALERT";
    public static DateTime JanuarySecond => new(2024, 01, 02, 03, 04, 05);
    public static DateTime FebruaryThird => new(2024, 02, 03, 04, 05, 06);
    public static DateTime MarchFourth => new(2024, 03, 04, 05, 06, 07);

    [Fact]
    public void InstantiateAllFields()
    {
        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateStamp = new DateTime(2024, 01, 02, 12, 11, 10),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
            BibRecords = [
                new FlaggedBibRecordModel
                {
                    BibNumber = "1",
                    @Action = "IN",
                    BibTimeOfDay = "1314",
                    DayOfMonth = "2",
                    Location = "test-location",
                    DataWarning = false,
                }
            ]
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
        WinlinkMessageModel sut = new()
        {
            WinlinkMessageId = null,
            MessageDateStamp = new DateTime(),
            ClientHostname = null,
            FileCreatedTimeStamp = new DateTime(),
            BibRecords = [
                new FlaggedBibRecordModel()
            ]
        };

        // test nullable fields
        Assert.Null(sut.WinlinkMessageId);
        // DateTime type properties cannot be null
        Assert.Null(sut.ClientHostname);
        Assert.True(sut.BibRecords.Count > 0);
        Assert.Null(sut.BibRecords[0].Action);
        Assert.Null(sut.BibRecords[0].BibTimeOfDay);
        Assert.Null(sut.BibRecords[0].Location);
        Assert.Null(sut.BibRecords[0]!.BibNumber);
        Assert.Null(sut.BibRecords[0].DayOfMonth);

        // tests NON-nullable fields
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
            BibRecords = 
            [
                new FlaggedBibRecordModel
                {
                    BibNumber = "1",
                    @Action = "IN",
                    BibTimeOfDay = "1314",
                    DayOfMonth = "2",
                    Location = "test-location",
                    DataWarning = false,
                }
            ]
        };

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
            BibRecords = 
            [
                new FlaggedBibRecordModel
                {
                    BibNumber = "1",
                    @Action = "IN",
                    BibTimeOfDay = "1314",
                    DayOfMonth = "2",
                    Location = "test-location",
                    DataWarning = false,
                }
            ]
        };

        var singleBibExpected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"MessageDateStamp\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"FileCreatedTimeStamp\":\"2023-08-13T23:22:21\"," +
            "\"BibRecords\":[{\"BibNumber\":\"1\",\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":\"2\",\"Location\":\"test-location\",\"DataWarning\":false}]}";
        var singleBibActual = sut.ToJsonString();

        Debug.WriteLine($"singleBibExpected:\r\n{singleBibExpected}");
        Debug.WriteLine($"singleBibActual:\r\n{singleBibActual}");
        Assert.Equal(singleBibExpected, singleBibActual);

        var newRecord = new FlaggedBibRecordModel
        {
            BibNumber = "1",
            @Action = "OUTT",
            BibTimeOfDay = "2014",
            DayOfMonth = "2",
            Location = "test-location",
            DataWarning = true
        };

        sut.BibRecords.Add(newRecord);

        var twoBibsExpected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"MessageDateStamp\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"FileCreatedTimeStamp\":\"2023-08-13T23:22:21\"," +
            "\"BibRecords\":[" +
            "{\"BibNumber\":\"1\",\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":\"2\",\"Location\":\"test-location\",\"DataWarning\":false}," +
            "{\"BibNumber\":\"1\",\"Action\":\"OUTT\",\"BibTimeOfDay\":\"2014\",\"DayOfMonth\":\"2\",\"Location\":\"test-location\",\"DataWarning\":true}]}";
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
            BibRecords =
            [
                new FlaggedBibRecordModel
                {
                    BibNumber = "1",
                    @Action = "IN",
                    BibTimeOfDay = "13145",
                    DayOfMonth = "2",
                    Location = "test-location",
                    DataWarning = true,
                }
            ]
        };

        var expected = "{\"WinlinkMessageId\":\"ABCDEFGHIJKL\",\"MessageDateStamp\":\"2024-01-02T13:12:11\",\"ClientHostname\":\"test-hostname\",\"FileCreatedTimeStamp\":\"2023-08-13T23:22:21\"," +
            "\"BibRecords\":[{\"BibNumber\":\"1\",\"Action\":\"IN\",\"BibTimeOfDay\":\"13145\",\"DayOfMonth\":\"2\",\"Location\":\"test-location\",\"DataWarning\":true}]}";
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
            BibRecords =
            [
                new FlaggedBibRecordModel
                {
                    BibNumber = "1",
                    @Action = "IN",
                    BibTimeOfDay = "1314",
                    DayOfMonth = "2",
                    Location = "test-location",
                    DataWarning = false,
                }
            ]
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
            BibNumber = "1",
            @Action = "IN",
            BibTimeOfDay = "1314",
            DayOfMonth = "2",
            Location = "test-location",
            DataWarning = false,
        };

        var sut = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateStamp = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
            BibRecords =
            [
                bibEntry
            ]
        };

        var oneLineExpected = $"ABCDEFGHIJKL\t2024-01-02T13-12-11\t{NoDataWarningText}\t1\tIN\t1314\t2\ttest-location\r\n";
        var oneLineActual = sut.ToAccessDatabaseTabbedString();
        Debug.WriteLine($"Expected:\r\n{oneLineExpected}\r\nActual:\r\n{oneLineActual}");
        Assert.Equal(oneLineExpected, oneLineActual);

        var newRecord = new FlaggedBibRecordModel
        {
            BibNumber = "1",
            Action = "OUTT",
            BibTimeOfDay = "2014",
            DayOfMonth = "2",
            Location = "test-location",
            DataWarning = true
        };
        sut.BibRecords.Add(newRecord);

        var twoLinesExpected = $"ABCDEFGHIJKL\t2024-01-02T13-12-11\t{NoDataWarningText}\t1\tIN\t1314\t2\ttest-location\r\nABCDEFGHIJKL\t2024-01-02T13-12-11\t{DataWarningText}\t1\tOUTT\t2014\t2\ttest-location\r\n";
        string twoLinesActual = sut.ToAccessDatabaseTabbedString();
        Debug.WriteLine($"Expected:\r\n{twoLinesExpected}\r\nActual:\r\n{twoLinesActual}");
        Assert.Equal(twoLinesExpected, twoLinesActual);
    }

    [Theory]
    [InlineData("", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0000\t2\tTL\r\n")]
    [InlineData("0", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0000\t2\tTL\r\n")]
    [InlineData("00", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0000\t2\tTL\r\n")]
    [InlineData("000", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0000\t2\tTL\r\n")]
    [InlineData("5", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0005\t2\tTL\r\n")]
    [InlineData("010", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0010\t2\tTL\r\n")]
    [InlineData("100", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0100\t2\tTL\r\n")]
    [InlineData("0105", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0105\t2\tTL\r\n")]
    [InlineData("110", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0110\t2\tTL\r\n")]
    [InlineData("1000", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t1000\t2\tTL\r\n")]
    [InlineData("55", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0055\t2\tTL\r\n")]
    [InlineData("555", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0555\t2\tTL\r\n")]
    [InlineData("961", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0961\t2\tTL\r\n")]
    [InlineData("999", "ABCDEFGHIJKL\t2024-01-02T13-12-11\tNOMINAL\t1\tIN\t0999\t2\tTL\r\n")]
    public void MessageToAccessDatabaseString_BibRecordTimesHaveLeadingZerosIfLessThanFourCharacters(string inputTime, string expectedPrintableText)
    {
        FlaggedBibRecordModel bibRecord = FlaggedBibRecordModel.GetBibRecordInstance("1", "IN", inputTime, "2", "TL");
        
        WinlinkMessageModel sut = new()
        {
            WinlinkMessageId = "ABCDEFGHIJKL",
            MessageDateStamp = new DateTime(2024, 01, 02, 13, 12, 11),
            ClientHostname = "test-hostname",
            FileCreatedTimeStamp = new DateTime(2023, 08, 13, 23, 22, 21),
            BibRecords =
            [
                bibRecord
            ]
        };

        Assert.Equal(expectedPrintableText, sut.ToAccessDatabaseTabbedString());
    }

    [Fact]
    public void MessagesWithDifferentDataReturnDifferentHashCodes()
    {
        var winlinkMessage1 = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABC123",
            MessageDateStamp = MarchFourth,
            ClientHostname = "localhost",
            FileCreatedTimeStamp = MarchFourth.AddDays(1),
            BibRecords = []
        };

        var winlinkMessage2 = new WinlinkMessageModel
        {
            WinlinkMessageId = "XYZ789",
            MessageDateStamp = MarchFourth.AddDays(1),
            ClientHostname = "localhost",
            FileCreatedTimeStamp = MarchFourth.AddDays(1),
            BibRecords = []
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
            BibRecords = []
        };

        var winlinkMessage2 = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABC123",
            MessageDateStamp = MarchFourth.AddDays(-1),
            ClientHostname = "localhost",
            FileCreatedTimeStamp = MarchFourth,
            BibRecords = []
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
            BibRecords = []
        };

        var winlinkMessage2 = new WinlinkMessageModel
        {
            WinlinkMessageId = "ABC123DEF856",
            MessageDateStamp = MarchFourth,
            ClientHostname = "localhost",
            FileCreatedTimeStamp = JanuarySecond,
            BibRecords = []
        };

        Assert.False(winlinkMessage1.Equals(winlinkMessage2));
    }

    [Theory]
    [InlineData(2024, 01, 02, 03, 04, 05, "2024-01-02T03-04-05")]
    [InlineData(2024, 12, 31, 12, 59, 59, "2024-12-31T12-59-59")]
    [InlineData(2024, 01, 02, 00, 00, 00, "2024-01-02T00-00-00")]
    [InlineData(2024, 01, 02, 23, 59, 59, "2024-01-02T23-59-59")]
    [InlineData(2000, 06, 30, 00, 00, 00, "2000-06-30T00-00-00")]
    public void FormatDateTimeForAccessDB_ReturnsCorrectFormat(int year, int month, int day, int hour, int minute, int second, string expectedResult)
    {
        DateTime inputDateTime = new(year, month, day, hour, minute, second);
        string actualResult = WinlinkMessageModel.FormatDateTimeForAccessDb(inputDateTime);
        Debug.WriteLine($"Input: {JanuarySecond}; Expected: {expectedResult}; Actual: {actualResult}");
        Assert.Equal(expectedResult, actualResult);
    }

    [Fact]
    public void HasDataWarning()
    {
        FlaggedBibRecordModel alpha = new()
        {
            BibNumber = "1",
            @Action = "IN",
            BibTimeOfDay = "1314",
            DayOfMonth = "2",
            Location = "test-location",
            DataWarning = false,
        };

        FlaggedBibRecordModel bravo = new()
        {
            BibNumber = "2",
            @Action = "OUT",
            BibTimeOfDay = "2014",
            DayOfMonth = "2",
            Location = "test-location",
            DataWarning = true,
        };

        List<FlaggedBibRecordModel> bibs =
        [
            alpha, bravo
        ];

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
        List<FlaggedBibRecordModel> bibRecords =
        [
            new FlaggedBibRecordModel
            {
                BibNumber = "1",
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = "2",
                Location = "test-location",
                DataWarning = false,
            }
        ];

        WinlinkMessageModel sut = WinlinkMessageModel.GetWinlinkMessageInstance(winlinkMessageId, messageDateTime, clientHostname, fileCreatedDateTime, bibRecords);

        Assert.NotNull(sut);
        Assert.Equal(winlinkMessageId, sut.WinlinkMessageId);
        Assert.Equal(messageDateTime, sut.MessageDateStamp);
        Assert.Equal(clientHostname, sut.ClientHostname);
        Assert.Equal(fileCreatedDateTime, sut.FileCreatedTimeStamp);
        Assert.Single(sut.BibRecords);
        Assert.Equal("1", sut.BibRecords[0].BibNumber);
    }
}
