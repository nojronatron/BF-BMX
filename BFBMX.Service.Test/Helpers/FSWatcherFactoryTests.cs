using BFBMX.Service.Helpers;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using Xunit;

namespace BFBMX.Service.Tests.Helpers
{
    public class FSWatcherFactoryTests
    {
        private static string? TestPath;
        private static string DefaultFilter = "*.mime";

        public FSWatcherFactoryTests()
        {
            TestPath = Environment.GetEnvironmentVariable("TEMP");
        }

        [Fact]
        public void Create_ReturnsValidFSWMonitorInstance()
        {
            // Arrange
            string name = "TestWatcher";
            FileSystemEventHandler createdCallback = (sender, e) => { };
            ErrorEventHandler errorCallback = (sender, e) => { };

            // Act
            var result = FSWatcherFactory.Create(createdCallback, errorCallback, TestPath!, name);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(TestPath, result.MonitoredPath);
            Assert.Equal(name, result.GetName());
            Assert.Equal(DefaultFilter, result.GetFileSystemWatcher!.Filter);
            Assert.False(result.GetFileSystemWatcher!.IncludeSubdirectories);
        }
    }
}
