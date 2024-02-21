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
            Assert.Null(sut.@Action);
            Assert.Null(sut.Location);
            Assert.True(sut.DataWarning);
        }

        [Fact]
        public void BibRecordModel_ToJson()
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
    }
}
