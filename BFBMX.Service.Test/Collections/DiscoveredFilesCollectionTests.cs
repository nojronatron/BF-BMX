using BFBMX.Service.Models;

namespace BFBMX.Service.Test.Collections;

public class DiscoveredFilesCollectionTests
{
    private readonly DiscoveredFileModel testFileAlpha = new("a-test-file.mime");
    private readonly DiscoveredFileModel testFileBravo = new("b-test-file.mime");
    private readonly DiscoveredFileModel testFileCharlie = new("c-test-file.mime");
    private readonly DiscoveredFileModel testFileDelta = new("d-test-file.mime");
    private readonly DiscoveredFileModel testFileEcho = new("e-test-file.mime");
    private readonly DiscoveredFileModel testFileFoxtrot = new("f-test-file.mime");
    private readonly DiscoveredFileModel testFileGolf = new("g-test-file.mime");
    private readonly DiscoveredFileModel testFileHotel = new("h-test-file.mime");

    [Fact]
    public void MaximumItemsCountIsHonored()
    {
    //    var sut = new DiscoveredFilesCollection();

    //    var expectedMaxItems = sut.MaxItems;

    //    Assert.True(sut.IsEmpty);
    //    sut.Enqueue(testFileAlpha);
    //    Assert.True(sut.Count <= expectedMaxItems);
    //    sut.Enqueue(testFileBravo);
    //    Assert.True(sut.Count <= expectedMaxItems);
    //    sut.Enqueue(testFileCharlie);
    //    Assert.True(sut.Count <= expectedMaxItems);
    //    sut.Enqueue(testFileDelta);
    //    Assert.True(sut.Count <= expectedMaxItems);
    //    sut.Enqueue(testFileEcho);
    //    Assert.True(sut.Count <= expectedMaxItems);
    //    sut.Enqueue(testFileFoxtrot);
    //    Assert.True(sut.Count <= expectedMaxItems);
    //    sut.Enqueue(testFileGolf);
    //    Assert.True(sut.Count <= expectedMaxItems);
    //    sut.Enqueue(testFileHotel);
    //    Assert.True(sut.Count <= expectedMaxItems);
    //    sut.Clear();
    //    Assert.True(sut.Count == 0);
    //}

    //[Fact]
    //public void AddRemoveCountCollectionItems()
    //{
    //    var sut = new DiscoveredFilesCollection();
    //    Assert.True(sut.IsEmpty);
        
    //    sut.Enqueue(testFileAlpha);
    //    Assert.True(sut.Count == 1);
        
    //    Assert.True(sut.TryPeek(out DiscoveredFileModel? peekResult));
    //    Assert.Equal(testFileAlpha, peekResult);
    //    Assert.True(sut.Count == 1);

    //    sut.Enqueue(testFileBravo);
    //    Assert.True(sut.Count == 2);
        
    //    Assert.True(sut.TryDequeue(out DiscoveredFileModel? alphaResult));
    //    Assert.Equal(testFileAlpha, alphaResult);
    //    Assert.True(sut.Count == 1);
        
    //    sut.Enqueue(testFileCharlie);
    //    sut.Enqueue(testFileDelta);
    //    Assert.True(sut.Count == 3);
        
    //    Assert.True(sut.TryDequeue(out DiscoveredFileModel? bravoResult));
    //    Assert.Equal(testFileBravo, bravoResult);
    //    Assert.True(sut.Count == 2);
        
    //    Assert.True(sut.TryDequeue(out DiscoveredFileModel? charlieResult));
    //    Assert.Equal(testFileCharlie, charlieResult);
    //    Assert.True(sut.Count == 1);
        
    //    Assert.True(sut.TryDequeue(out DiscoveredFileModel? deltaResult));
    //    Assert.Equal(testFileDelta, deltaResult);
    //    Assert.True(sut.IsEmpty);
    }
}