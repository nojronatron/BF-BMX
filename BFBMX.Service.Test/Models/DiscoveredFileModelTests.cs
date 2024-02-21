using BFBMX.Service.Models;

namespace BFBMX.Service.Test.Models
{
    public class DiscoveredFileModelTests
    {
        [Fact]
        public void InstantiateDiscoveredFileInstance()
        {
            // get the path to a temp directory, fail the test if unable to
            string? envarTempDir = Environment.GetEnvironmentVariable("TEMP");

            if (string.IsNullOrEmpty(envarTempDir))
            {
                envarTempDir = Environment.GetEnvironmentVariable("TMP");
            }

            if (string.IsNullOrEmpty(envarTempDir))
            {
                envarTempDir = Environment.GetEnvironmentVariable("USERPROFILE");
            }
            
            if (string.IsNullOrEmpty(envarTempDir))
            {
                Assert.True(false);
            }

            string tempDir = envarTempDir!;
            string fileName = "test-file.mime";

            // safely create a new file at tempDir and close it
            {
                using var stream = File.Create(Path.Combine(tempDir, fileName));
            }

            var sut = new DiscoveredFileModel(Path.Combine(tempDir, fileName));

            // positive tests
            Assert.NotNull(sut);
            Assert.True(File.Exists(sut.FullFilePath));
            Assert.Equal(fileName, sut.FileName);
            Assert.Equal(Path.Combine(tempDir, fileName), sut.FullFilePath);

            // cleanup
            File.Delete(Path.Combine(tempDir, fileName));
            Assert.True(!File.Exists(Path.Combine(tempDir, fileName)));

            // negative tests (file not exists)
            Assert.NotNull(sut);
            Assert.False(File.Exists(sut.FullFilePath));
        }

        [Fact]
        public void DiscoveredFileModelEquals()
        {
            var sut = new DiscoveredFileModel("test-file.mime");
            var other = new DiscoveredFileModel("test-file.mime");

            Assert.True(sut.Equals(other));
        }

        [Fact]
        public void DiscoveredFileModelThrowsIfEmptyPath()
        {
            Assert.Throws<System.ArgumentException>("path", () =>
            {
                var noPath = new DiscoveredFileModel("");
            });
        }

        [Fact]
        public void DiscoveredFileModelsSorted()
        {
            var left = new DiscoveredFileModel("a-test-file.mime");
            var middle = new DiscoveredFileModel("b-test-file.mime");
            var right = new DiscoveredFileModel("c-test-file.mime");

            DiscoveredFileModel? nonExistant = null;

            Assert.False(left.Equals(middle));
            Assert.False(middle.Equals(right));
            Assert.False(right.Equals(left));

            Assert.True(left.CompareTo(middle) < 0);
            Assert.True(middle.CompareTo(middle) == 0);
            Assert.True(right.CompareTo(left) > 0);
            Assert.True(left.CompareTo(nonExistant) == 1);

            var discoveredFileModels = new List<DiscoveredFileModel?>
            {
                middle,
                right,
                nonExistant,
                left
            };

            discoveredFileModels.Sort();

            // test collection order
            Assert.Null(discoveredFileModels[0]);
            Assert.Equal(left, discoveredFileModels[1]);
            Assert.Equal(middle, discoveredFileModels[2]);
            Assert.Equal(right, discoveredFileModels[3]);
        }

        [Fact]
        public void VerifyGetHashCodeConsistency()
        {
            // ensure that GetHashCode returns consistent results for the same
            // instance or for different instances with the same FullFilePath
            var left = new DiscoveredFileModel("left-file.txt");
            var right = new DiscoveredFileModel("right-file.txt");

            Assert.Equal(left.GetHashCode(), left.GetHashCode());
            Assert.Equal(right.GetHashCode(), right.GetHashCode());
            Assert.NotEqual(left.GetHashCode(), right.GetHashCode());
        }

        [Fact]
        public void VerifyEqeqConsistency()
        {
            // Test the == operator overload
            var left = new DiscoveredFileModel("left-file.txt");
            var right = new DiscoveredFileModel("right-file.txt");
            var center = new DiscoveredFileModel("left-file.txt");

            Assert.True(left == center);
            Assert.False(right == center);
        }

        [Fact]
        public void NeqNeqConsistency()
        {
            // Test the != operator overload
            var left = new DiscoveredFileModel("left-file.txt");
            var right = new DiscoveredFileModel("right-file.txt");
            var center = new DiscoveredFileModel("left-file.txt");

            Assert.True(left !=right);
            Assert.False(left != center);
        }

        [Fact]
        public void EqualsAndEqeqReturnFalseOnNullInstance()
        {
            // Test Equals() and == operator overload return false when compared to null
            var left = new DiscoveredFileModel("left-file.txt");

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.False(left.Equals(null));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            Assert.False(left == null);
        }

        [Fact]
        public void DiscoveredFileModelIsEqualsToItself()
        {
            // Test that an instance of DiscoveredFileModel is equal to itself
            var left = new DiscoveredFileModel("left-file.txt");

            Assert.True(left.Equals(left));
        }
    }
}
