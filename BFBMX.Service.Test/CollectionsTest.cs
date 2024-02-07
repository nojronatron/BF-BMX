using BFBMX.Service.Collections;
using BFBMX.Service.Models;

namespace BFBMX.Service.Test;

public class CollectionsTest
{
  [Fact]
  public void AddRemoveCountCollectionItems()
  {
    // Arrange
    var bibMessage = new BibMessageModel
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

    var msgCollection = new MessageModelCollection();

    // Act
    msgCollection.Add(bibMessage);

    // Assert
    Assert.Single(msgCollection);
    msgCollection.Clear();
    Assert.Empty(msgCollection);
  }
}