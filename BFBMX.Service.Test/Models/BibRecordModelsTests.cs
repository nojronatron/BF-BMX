﻿using BFBMX.Service.Models;

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
        public void InstantiateNullBibRecord()
        {
            FlaggedBibRecordModel sut = new();
            Assert.Equal("MISSING", sut.@Action);
            Assert.Equal("MISSING", sut.Location);
            Assert.True(sut.DataWarning);
        }

        [Fact]
        public void BibRecordModelToTabbedStringBothDataWarningStates()
        {
            // no data warning
            FlaggedBibRecordModel sutGoodData = new ()
            {
                BibNumber = 1,
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = 2,
                Location = "test-location",
                DataWarning = false
            };

            var expectedGoodData = $"{NoDataWarningText}\t1\tIN\t1314\t2\ttest-location";
            var actualGoodData = sutGoodData.ToTabbedString();

            Assert.Equal(expectedGoodData, actualGoodData);

            // with data warning
            FlaggedBibRecordModel sutDataWarning = new ()
            {
                BibNumber = 1,
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = 2,
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
            int negativeBib = -1;
            int zeroBib = 0;
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
            int negativeDay = -1;
            int zeroDay = 0;
            int thirtyTwoDay = 32;
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

        [Fact]
        public void ToJsonEdgeCases()
        {
            // set all fields to null or empty string
            FlaggedBibRecordModel sut = new()
            {
                Action = null,
                BibTimeOfDay = null,
                Location = null
            };

            var expectedData = "{\"BibNumber\":-1,\"Action\":null,\"BibTimeOfDay\":null,\"DayOfMonth\":-1,\"Location\":null,\"DataWarning\":true}";
            var actualData = sut.ToJsonString();

            Assert.Equal(expectedData, actualData);

            // special characters that need to be escaped
            sut.Action = "@";
            sut.BibTimeOfDay = "@";
            sut.Location = "@";

            expectedData = "{\"BibNumber\":-1,\"Action\":\"@\",\"BibTimeOfDay\":\"@\",\"DayOfMonth\":-1,\"Location\":\"@\",\"DataWarning\":true}";
            actualData = sut.ToJsonString();

            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public void BibRecordModelToJson()
        {
            // no data warning
            var sutGoodData = new FlaggedBibRecordModel
            {
                BibNumber = 1,
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = 2,
                Location = "test-location",
                DataWarning = false
            };

            var expectedGoodData = "{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":false}";
            var actualGoodData = sutGoodData.ToJsonString();

            Assert.Equal(expectedGoodData, actualGoodData);

            // with data warning
            var sutDataWarning = new FlaggedBibRecordModel
            {
                BibNumber = 1,
                @Action = "IN",
                BibTimeOfDay = "1314",
                DayOfMonth = 2,
                Location = "test-location",
                DataWarning = true
            };

            var expectedWarningData = "{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":true}";
            var actualWarningData = sutDataWarning.ToJsonString();

            Assert.Equal(expectedWarningData, actualWarningData);
        }
    }
}
