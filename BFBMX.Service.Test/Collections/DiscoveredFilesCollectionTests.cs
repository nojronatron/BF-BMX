using BFBMX.Service.Collections;
using BFBMX.Service.Models;

namespace BFBMX.Service.Test.Collections;

public class DiscoveredFilesCollectionTests
{
    [Fact]
    public void AddRemoveCountCollectionItems()
    {
        var testFileAlpha = new DiscoveredFileModel("a-test-file.mime");
        var testFileBravo = new DiscoveredFileModel("b-test-file.mime");
        var testFileCharlie = new DiscoveredFileModel("c-test-file.mime");
        var testFileDelta = new DiscoveredFileModel("d-test-file.mime");
        var sut = new DiscoveredFilesCollection();

        Assert.True(sut.IsEmpty);
        
        sut.Enqueue(testFileAlpha);
        Assert.True(sut.Count == 1);
        
        Assert.True(sut.TryPeek(out DiscoveredFileModel? peekResult));
        Assert.Equal(testFileAlpha, peekResult);
        Assert.True(sut.Count == 1);

        sut.Enqueue(testFileBravo);
        Assert.True(sut.Count == 2);
        
        Assert.True(sut.TryDequeue(out DiscoveredFileModel? alphaResult));
        Assert.Equal(testFileAlpha, alphaResult);
        Assert.True(sut.Count == 1);
        
        sut.Enqueue(testFileCharlie);
        sut.Enqueue(testFileDelta);
        Assert.True(sut.Count == 3);
        
        Assert.True(sut.TryDequeue(out DiscoveredFileModel? bravoResult));
        Assert.Equal(testFileBravo, bravoResult);
        Assert.True(sut.Count == 2);
        
        Assert.True(sut.TryDequeue(out DiscoveredFileModel? charlieResult));
        Assert.Equal(testFileCharlie, charlieResult);
        Assert.True(sut.Count == 1);
        
        Assert.True(sut.TryDequeue(out DiscoveredFileModel? deltaResult));
        Assert.Equal(testFileDelta, deltaResult);
        Assert.True(sut.IsEmpty);
    }
}