using BFBMX.Service.Models;

namespace BFBMX.Service.Test.Models
{
    public class BibRecordModelsTests
    {
        [Fact]
        public void InstantiateBibRecordModelAllFields()
        {
            var sut = new BibRecordModel
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
            var sut = new BibRecordModel();
            Assert.Equal("MISSING", sut.@Action);
            Assert.Equal("MISSING", sut.Location);
            Assert.True(sut.DataWarning);
        }

        [Fact]
        public void BibRecordModelToString()
        {
            // no data warning
            var sutGoodData = new BibRecordModel();
            sutGoodData.BibNumber = 1;
            sutGoodData.@Action = "IN";
            sutGoodData.BibTimeOfDay = "1314";
            sutGoodData.DayOfMonth = 2;
            sutGoodData.Location = "test-location";
            sutGoodData.DataWarning = false;

            var expectedGoodData = "1\tIN\t1314\t2\ttest-location";
            var actualGoodData = sutGoodData.BibDataToString();

            Assert.Equal(expectedGoodData, actualGoodData);

            // with data warning
            var sutDataWarning = new BibRecordModel();
            sutDataWarning.BibNumber = 1;
            sutDataWarning.@Action = "IN";
            sutDataWarning.BibTimeOfDay = "1314";
            sutDataWarning.DayOfMonth = 2;
            sutDataWarning.Location = "test-location";
            sutDataWarning.DataWarning = true;

            var expectedDataWarning = "1\tIN\t1314\t2\ttest-location\tWarning!";
            var actualDataWarning = sutDataWarning.BibDataToString();

            Assert.Equal(expectedDataWarning, actualDataWarning);
        }

        [Fact]
        public void BibNumberEdgeCases()
        {
            // set bibnumber to negative values or zero
            int negativeBib = -1;
            int zeroBib = 0;
            BibRecordModel sut = new();
            sut.BibNumber = negativeBib;
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
            BibRecordModel sut = new();
            sut.DayOfMonth = negativeDay;
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
            BibRecordModel sut = new();
            sut.Action = null;
            Assert.Null(sut.Action);
            sut.Action = string.Empty;
            Assert.Empty(sut.Action);
        }

        [Fact]
        public void LocationEdgeCases()
        {
            // set location to null or empty string
            BibRecordModel sut = new();
            sut.Location = null;
            Assert.Null(sut.Location);
            sut.Location = string.Empty;
            Assert.Empty(sut.Location);
        }

        [Fact]
        public void ToJsonEdgeCases()
        {
            // set all fields to null or empty string
            BibRecordModel sut = new();
            sut.Action = null;
            sut.BibTimeOfDay = null;
            sut.Location = null;

            var expectedData = "{\"BibNumber\":-1,\"Action\":null,\"BibTimeOfDay\":null,\"DayOfMonth\":-1,\"Location\":null,\"DataWarning\":true}";
            var actualData = sut.BibDataToJson();

            Assert.Equal(expectedData, actualData);

            // special characters that need to be escaped
            sut.Action = "@";
            sut.BibTimeOfDay = "@";
            sut.Location = "@";

            expectedData = "{\"BibNumber\":-1,\"Action\":\"@\",\"BibTimeOfDay\":\"@\",\"DayOfMonth\":-1,\"Location\":\"@\",\"DataWarning\":true}";
            actualData = sut.BibDataToJson();

            Assert.Equal(expectedData, actualData);
        }

        [Fact]
        public void BibRecordModelToJson()
        {
            // no data warning
            var sutGoodData = new BibRecordModel();
            sutGoodData.BibNumber = 1;
            sutGoodData.@Action = "IN";
            sutGoodData.BibTimeOfDay = "1314";
            sutGoodData.DayOfMonth = 2;
            sutGoodData.Location = "test-location";
            sutGoodData.DataWarning = false;

            var expectedGoodData = "{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":false}";
            var actualGoodData = sutGoodData.BibDataToJson();

            Assert.Equal(expectedGoodData, actualGoodData);

            // with data warning
            var sutDataWarning = new BibRecordModel();
            sutDataWarning.BibNumber = 1;
            sutDataWarning.@Action = "IN";
            sutDataWarning.BibTimeOfDay = "1314";
            sutDataWarning.DayOfMonth = 2;
            sutDataWarning.Location = "test-location";
            sutDataWarning.DataWarning = true;

            var expectedWarningData = "{\"BibNumber\":1,\"Action\":\"IN\",\"BibTimeOfDay\":\"1314\",\"DayOfMonth\":2,\"Location\":\"test-location\",\"DataWarning\":true}";
            var actualWarningData = sutDataWarning.BibDataToJson();

            Assert.Equal(expectedWarningData, actualWarningData);
        }
    }
}
