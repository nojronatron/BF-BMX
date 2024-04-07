using BFBMX.ServerApi.Collections;
using BFBMX.ServerApi.EF;
using BFBMX.ServerApi.Helpers;
using BFBMX.Service.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BFBMX.Service.Test.Collections
{
    public class BibReportsCollectionTests
    {
        [Fact]
        public void AddEntityToCollection_ShouldAddEntity_WhenEntityIsNotNull()
        {
        //    // Arrange
        //    var options = new DbContextOptionsBuilder<BibMessageContext>()
        //        .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
        //        .Options;

        //    var mockLogger = new Mock<ILogger<BibReportsCollection>>();
        //    var mockDataExImService = new Mock<DataExImService>();

        //    using (var dbContext = new BibMessageContext(options))
        //    {
        //        var collection = new BibReportsCollection(dbContext, mockLogger.Object, mockDataExImService);
        //        var bibRecord = new FlaggedBibRecordModel
        //        {
        //            BibNumber = 123,
        //            Action = "IN",
        //            BibTimeOfDay = "1234",
        //            DayOfMonth = 1,
        //            Location = "TL",
        //            DataWarning = false
        //        };
        //        var entity = new WinlinkMessageModel()
        //        {
        //            WinlinkMessageId = "ABCDEFGHIJKL",
        //            MessageDateTime = DateTime.Now,
        //            ClientHostname = "test-client",
        //            BibRecords = new List<FlaggedBibRecordModel> { bibRecord }
        //        };

        //        // Act
        //        var result = collection.AddEntityToCollection(entity);

        //        // Assert
        //        Assert.True(result);
        //        Assert.Equal(1, dbContext.WinlinkMessageModels.Count());
        //    }
        //}

        //[Fact]
        //public void AddMultipleEntities()
        //{
        //    var options = new DbContextOptionsBuilder<BibMessageContext>()
        //        .UseInMemoryDatabase(databaseName: "Add_writes_to_database")
        //        .Options;

        //    var mockLogger = new Mock<ILogger<BibReportsCollection>>();

        //    using (var dbContext = new BibMessageContext(options))
        //    {
        //        var collection = new BibReportsCollection(dbContext, mockLogger.Object);
        //        var bibRecordOne = new FlaggedBibRecordModel
        //        {
        //            BibNumber = 234,
        //            Action = "IN",
        //            BibTimeOfDay = "1345",
        //            DayOfMonth = 1,
        //            Location = "TL",
        //            DataWarning = false
        //        };
        //        var bibRecordTwo = new FlaggedBibRecordModel
        //        {
        //            BibNumber = 234,
        //            Action = "OUT",
        //            BibTimeOfDay = "1456",
        //            DayOfMonth = 1,
        //            Location = "TL",
        //            DataWarning = false
        //        };
        //        var entity = new WinlinkMessageModel()
        //        {
        //            WinlinkMessageId = "BCDEFGHIJKLM",
        //            MessageDateTime = DateTime.Now,
        //            ClientHostname = "test-client",
        //            BibRecords = new List<FlaggedBibRecordModel> { bibRecordOne, bibRecordTwo }
        //        };

        //        var result = collection.AddEntityToCollection(entity);

        //        Assert.True(result);
        //        Assert.Equal(2, dbContext.WinlinkMessageModels.Count());
            //}
        }
    }
}
