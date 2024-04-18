using BFBMX.Service.Models;

namespace BFBMX.Service.Test.Models
{
    public class BibRecordModelsTests
    {
        public static string DataWarningText { get => "ALERT"; }
        public static string NoDataWarningText { get => "NOMINAL";}

        [Fact]
        public void InstantiateBibRecordModelAllFields()
        {
            FlaggedBibRecordModel sut = new()
            {
                BibNumber = "1",
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = "2",
                Location = "test-location",
                DataWarning = false,
            };
            Assert.NotNull(sut);
        }

        [Fact]
        public void InstantiateNullBibRecord()
        {
            FlaggedBibRecordModel sut = new();
            Assert.Null(sut.@Action);
            Assert.Null(sut.Location);
            Assert.True(sut.DataWarning);
        }

        [Fact]
        public void BibRecordModelToTabbedStringNominalState()
        {
            // no data warning
            FlaggedBibRecordModel sutGoodData = new()
            {
                BibNumber = "1",
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = "2",
                Location = "test-location",
                DataWarning = false
            };

            var expectedGoodData = $"{NoDataWarningText}\t1\tIN\t1314\t2\ttest-location";
            var actualGoodData = sutGoodData.ToTabbedString();

            Assert.Equal(expectedGoodData, actualGoodData);
        }

        [Fact]
        public void BibRecordModelToTabbedStringWarningState()
        {
            // with data warning
            FlaggedBibRecordModel sutDataWarning = new ()
            {
                BibNumber = "1",
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = "2",
                Location = "test-location",
                DataWarning = true
            };

            var expectedDataWarning = $"{DataWarningText}\t1\tIN\t1314\t2\ttest-location";
            var actualDataWarning = sutDataWarning.ToTabbedString();

            Assert.Equal(expectedDataWarning, actualDataWarning);
        }

        [Fact]
        public void BibNumberEdgeCases()
        {
            // set bibnumber to negative values or zero
            string negativeBib = "-1";
            string zeroBib = "-1";
            FlaggedBibRecordModel sut = new()
            {
                BibNumber = negativeBib
            };
            Assert.Equal(negativeBib, sut.BibNumber);
            sut.BibNumber = zeroBib;
            Assert.Equal(zeroBib, sut.BibNumber);
        }

        [Fact]
        public void DayOfMonthEdgeCases()
        {
            // set day of month to negative values or zero
            string negativeDay = "-1";
            string zeroDay = "0";
            string thirtyTwoDay = "32";

            FlaggedBibRecordModel sut = new()
            {
                DayOfMonth = negativeDay
            };
            Assert.Equal(negativeDay, sut.DayOfMonth);

            sut.DayOfMonth = zeroDay;
            Assert.Equal(zeroDay, sut.DayOfMonth);
            
            sut.DayOfMonth = thirtyTwoDay;
            Assert.Equal(thirtyTwoDay, sut.DayOfMonth);
        }

        [Fact]
        public void ActionEdgeCases()
        {
            // set action to null or empty string
            FlaggedBibRecordModel sut = new()
            {
                Action = null
            };
            Assert.Null(sut.Action);
            sut.Action = string.Empty;
            Assert.Empty(sut.Action);
        }

        [Fact]
        public void LocationEdgeCases()
        {
            // set location to null or empty string
            FlaggedBibRecordModel sut = new()
            {
                Location = null
            };
            Assert.Null(sut.Location);
            sut.Location = string.Empty;
            Assert.Empty(sut.Location);
        }

        [Theory]
        [InlineData("0000", "0000")]
        [InlineData("0", "0")]
        [InlineData("", "")]
        [InlineData("5", "5")]
        [InlineData("010", "010")]
        [InlineData("100", "100")]
        [InlineData("105", "105")]
        [InlineData("0105", "0105")]
        [InlineData("110", "110")]
        [InlineData("1000", "1000")]
        [InlineData("55", "55")]
        [InlineData("555", "555")]
        [InlineData("1555", "1555")]
        [InlineData("2359", "2359")]
        public void TimeOfDayIsImmutableOnceSet(string inputTime, string expectedTime)
        {
            string bibNum = "1";
            string @action = "IN";
            string dayOfMonth = "1";
            string location = "TL";

            FlaggedBibRecordModel sut = FlaggedBibRecordModel.GetBibRecordInstance(
                bibNum,
                @action,
                inputTime,
                dayOfMonth,
                location);
            Assert.Equal(expectedTime, sut.BibTimeOfDay);
        }

        [Theory]
        [InlineData("", "NOMINAL\t1\tIN\t0000\t1\tTL")]
        [InlineData("0", "NOMINAL\t1\tIN\t0000\t1\tTL")]
        [InlineData("00", "NOMINAL\t1\tIN\t0000\t1\tTL")]
        [InlineData("000", "NOMINAL\t1\tIN\t0000\t1\tTL")]
        [InlineData("5", "NOMINAL\t1\tIN\t0005\t1\tTL")]
        [InlineData("010", "NOMINAL\t1\tIN\t0010\t1\tTL")]
        [InlineData("100", "NOMINAL\t1\tIN\t0100\t1\tTL")]
        [InlineData("0105", "NOMINAL\t1\tIN\t0105\t1\tTL")]
        [InlineData("110", "NOMINAL\t1\tIN\t0110\t1\tTL")]
        [InlineData("1000", "NOMINAL\t1\tIN\t1000\t1\tTL")]
        [InlineData("55", "NOMINAL\t1\tIN\t0055\t1\tTL")]
        [InlineData("555", "NOMINAL\t1\tIN\t0555\t1\tTL")]
        [InlineData("961", "NOMINAL\t1\tIN\t0961\t1\tTL")]
        [InlineData("999", "NOMINAL\t1\tIN\t0999\t1\tTL")]
        public void ToTabbedString_TimeOfDayIsPaddedWithLeadingZeros(string inputTime, string expectedPrintableText)
        {
            string bibNum = "1";
            string @action = "IN";
            string dayOfMonth = "1";
            string location = "TL";

            FlaggedBibRecordModel sut = FlaggedBibRecordModel.GetBibRecordInstance(
                bibNum,
                @action,
                inputTime,
                dayOfMonth,
                location);
            Assert.Equal(expectedPrintableText, sut.ToTabbedString());
        }

        [Fact]
        public void ToJsonNullsOrWhiteSpaces()
        {
            // set all fields to null or empty string
            FlaggedBibRecordModel sut = new()
            {
                Action = null,
                BibTimeOfDay = null,
                Location = null
            };

            string expectedData = "{\"BibNumber\":null,\"Action\":null,\"BibTimeOfDay\":null,\"DayOfMonth\":null,\"Location\":null,\"DataWarning\":true}";
            string actualData = sut.ToJsonString();

            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public void ToJsonUnexpectedCharacters()
        {
            // unexpected characters
            FlaggedBibRecordModel sut = new()
            {
                BibNumber = "$",
                Action = "@",
                BibTimeOfDay = "*",
                DayOfMonth = "~",
                Location = "|"
            };

            // characters that return charCodes when ToString() is called: < \u003c, > \u000??, & \u0026, 
            string expectedData = "{\"BibNumber\":\"$\",\"Action\":\"@\",\"BibTimeOfDay\":\"*\",\"DayOfMonth\":\"~\",\"Location\":\"|\",\"DataWarning\":true}";
            string actualData = sut.ToJsonString();

            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public void BibRecordModelToJson()
        {
            // no data warning
            var sutGoodData = new FlaggedBibRecordModel
            {
                BibNumber = "1",
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = "2",
                Location = "test-location",
                DataWarning = false
            };

            var expectedGoodData = "{\"BibNumber\":\"1\",\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":\"2\",\"Location\":\"test-location\",\"DataWarning\":false}";
            var actualGoodData = sutGoodData.ToJsonString();

            Assert.Equal(expectedGoodData, actualGoodData);

            // with data warning
            var sutDataWarning = new FlaggedBibRecordModel
            {
                BibNumber = "1",
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = "2",
                Location = "test-location",
                DataWarning = true
            };

            var expectedWarningData = "{\"BibNumber\":\"1\",\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":\"2\",\"Location\":\"test-location\",\"DataWarning\":true}";
            var actualWarningData = sutDataWarning.ToJsonString();

            Assert.Equal(expectedWarningData, actualWarningData);
        }

        [Fact]
        public void ZeroHundredHourBibTimeIncludesLeadingZeros()
        {
            // arrange
            string expectedResult = "NOMINAL\t1\tIN\t0001\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "0001",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = false
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void ZeroHundredTenHourBibTimeIncludesLeadingZeros()
        {
            // arrange
            string expectedResult = "NOMINAL\t1\tIN\t0010\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "0010",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = false
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void SloppyBibTimeIncludesLeadingZeros()
        {
            // arrange
            string expectedResult = "ALERT\t1\tIN\t0E01\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "0E01",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = true
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void BibTimeFieldSingleCharacterLeftPadsWith3Zeros()
        {
            // arrange
            string expectedResult = "NOMINAL\t1\tIN\t0001\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "1",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = false
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void BibTimeFieldTenMintuesCharactersLeftPadsWith2Zeros()
        {
            // arrange
            string expectedResult = "NOMINAL\t1\tIN\t0010\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "10",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = false
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void BibTimeFieldSingleHoursCharacterLeftPadsWith1Zero()
        {
            // arrange
            string expectedResult = "NOMINAL\t1\tIN\t0100\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "100",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = false
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void BibTimeFieldTensHoursCharacterNotPadded()
        {
            // arrange
            string expectedResult = "NOMINAL\t1\tIN\t1000\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "1000",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = false
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void BibTimeFieldFiveCharacterTimeNotPadded()
        {
            // arrange
            string expectedResult = "ALERT\t1\tIN\t11000\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "11000",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = true
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void SloppyBibTime27Characters()
        {
            // note: the Matcher methods have the character limit restriction
            // arrange
            string expectedResult = "ALERT\t1\tIN\t0123456789ABCDEFGHIJ123456\t2\tTL";

            FlaggedBibRecordModel bibEntry = new()
            {
                BibNumber = "1",
                Action = "IN",
                BibTimeOfDay = "0123456789ABCDEFGHIJ123456",
                DayOfMonth = "2",
                Location = "TL",
                DataWarning = true
            };

            // act
            string actualResult = bibEntry.ToTabbedString();

            // assert
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void GetBibRecordInstanceAllNullsReturnsNullInstance()
        {
            FlaggedBibRecordModel sut = FlaggedBibRecordModel.GetBibRecordInstance(null, null, null, null, null);
            Assert.Null(sut.BibNumber);
        }

        [Fact]
        public void GetBibRecordInstanceAllFieldsReturnsValidInstance()
        {
            string bibNum = "1";
            string @action = "IN";
            string bibTOD = "1234";
            string dayOfMonth = "2";
            string location = "TL";

            FlaggedBibRecordModel sut = FlaggedBibRecordModel.GetBibRecordInstance(bibNum,
                                                                                   @action,
                                                                                   bibTOD,
                                                                                   dayOfMonth,
                                                                                   location);

            Assert.True(sut.BibNumber == bibNum);
            Assert.True(sut.Action == @action);
            Assert.True(sut.BibTimeOfDay == bibTOD);
            Assert.True(sut.DayOfMonth == dayOfMonth);
            Assert.True(sut.Location == location);
        }

        [Fact]
        public void GetBibRecordInstanceArrayDataReturnsValidInstance()
        {
            string bibNum = "1";
            string @action = "IN";
            string bibTOD = "1234";
            string dayOfMonth = "2";
            string location = "TL";
            string[] bibDataArr =
            {
                bibNum, @action, bibTOD, dayOfMonth, location
            };

            FlaggedBibRecordModel sut = FlaggedBibRecordModel.GetBibRecordInstance(bibDataArr);

            Assert.True(sut.BibNumber == bibNum);
            Assert.True(sut.Action == @action);
            Assert.True(sut.BibTimeOfDay == bibTOD);
            Assert.True(sut.DayOfMonth == dayOfMonth);
            Assert.True(sut.Location == location);
        }

        [Fact]
        public void GetBibRecordDoesNotAlterValidInputInstanceData()
        {
            string bibNum = "1";
            string @action = "IN";
            string bibTOD = "1234";
            string dayOfMonth = "2";
            string location = "TL";
            string[] bibDataArr =
            {
                bibNum, @action, bibTOD, dayOfMonth, location
            };

            FlaggedBibRecordModel sut = FlaggedBibRecordModel.GetBibRecordInstance(bibDataArr);

            Assert.True(sut.BibNumber == bibNum);
            Assert.True(sut.Action == @action);
            Assert.True(sut.BibTimeOfDay == bibTOD);
            Assert.True(sut.DayOfMonth == dayOfMonth);
            Assert.True(sut.Location == location);
        }

        [Fact]
        public void GetBibRecordDoesNotAlterOtherInputInstanceData()
        {
            string bibNum = "one";
            string @action = "drop";
            string bibTOD = "5";
            string dayOfMonth = "32";
            string location = "test location";
            string[] bibDataArr =
            {
                bibNum, @action, bibTOD, dayOfMonth, location
            };

            FlaggedBibRecordModel sut = FlaggedBibRecordModel.GetBibRecordInstance(bibDataArr);

            Assert.True(sut.BibNumber == bibNum);
            Assert.True(sut.Action == @action);
            Assert.True(sut.BibTimeOfDay == bibTOD);
            Assert.True(sut.DayOfMonth == dayOfMonth);
            Assert.True(sut.Location == location);
        }
    }
}
