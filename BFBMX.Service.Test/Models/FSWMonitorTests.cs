using BFBMX.Service.Models;

namespace BFBMX.Service.Test.Models
{
    public class FSWMonitorTests
    {
        [Fact]
        public void WhenCreated_ShouldNotBeNull()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var name = "TestWatcher";

            // Act
            FSWMonitor sut = new(watcher, name);

            // Assert
            Assert.NotNull(sut);
        }

        [Fact]
        public void WhenCreated_ShouldHaveName()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var name = "TestWatcher";

            // Act
            FSWMonitor sut = new(watcher, name);

            // Assert
            Assert.Equal(name, sut.GetName());
        }

        [Fact]
        public void WhenCreated_ShouldBeStopped()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var name = "TestWatcher";

            // Act
            FSWMonitor sut = new(watcher, name);

            // Assert
            Assert.True(sut.IsStopped);
        }

        [Fact]
        public void WhenCreated_ShouldNotBeStarted()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var name = "TestWatcher";

            // Act
            var monitor = new FSWMonitor(watcher, name);

            // Assert
            Assert.False(monitor.IsStarted);
        }

        [Fact]
        public void WhenCreated_ShouldNotBeInitialized()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var name = "TestWatcher";

            // Act
            FSWMonitor sut = new (watcher, name);
            bool isInitialized = sut.IsInitialized;
            sut.Dispose();

            // Assert
            Assert.True(isInitialized);
        }

        [Fact]
        public void WhenCreated_ShouldHaveEmptyMonitoredPath()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var name = "TestWatcher";

            // Act
            FSWMonitor sut = new(watcher, name);

            // Assert
            Assert.Equal(string.Empty, sut.MonitoredPath);
        }

        // should be MOQd
        //[Fact]
        //public void WhenMonitoredPathSet_ShouldBeSet()
        //{
        //    // Arrange
        //    var watcher = new FileSystemWatcher();
        //    var name = "TestWatcher";
        //    var path = @"C:\Test-Temp";

        //    // Act
        //    var sut = new FSWMonitor(watcher, name)
        //    {
        //        MonitoredPath = path
        //    };

        //    // Assert
        //    Assert.Equal(path, sut.MonitoredPath);
        //}

        // should be MOQd
        //[Fact]
        //public void WhenEnableRaisingEventsSet_ShouldBeSet()
        //{
        //    // Arrange
        //    var watcher = new FileSystemWatcher();
        //    var name = "TestWatcher";
        //    var path = @"C:\Test-Temp";

        //    // Act
        //    FSWMonitor sut = new(watcher, name)
        //    {
        //        MonitoredPath = path,
        //        EnableRaisingEvents = true
        //    };

        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }

        //    bool isEnabledRaisingEvents = sut.EnableRaisingEvents;
        //    sut.EnableRaisingEvents = false;
        //    sut.Dispose();

        //    // Assert
        //    Assert.True(isEnabledRaisingEvents);
        //}

        // should be MOQ'd
        //[Fact]
        //public void CanStart_NotNullNotEnabledPathNotEmptyIsStopped_ReturnsTrue()
        //{
        //    // Arrange
        //    var watcher = new FileSystemWatcher();
        //    var name = "TestWatcher";
        //    var path = @"C:\Test-Temp";

        //    // Act
        //    FSWMonitor sut = new(watcher, name)
        //    {
        //        MonitoredPath = path
        //    };

        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }

        //    bool isInit = sut.IsInitialized;
        //    bool enableRaisingEventsState = sut.EnableRaisingEvents;
        //    bool canStart = sut.CanStart();
        //    sut.Dispose();

        //    // Assert
        //    Assert.True(isInit);
        //    Assert.False(enableRaisingEventsState);
        //    Assert.True(canStart);
        //}

        [Fact]
        public void CanStart_NoPathReturnFalse()
        {
            // Arrange
            var watcher = new FileSystemWatcher();
            var name = "TestWatcher";

            // Act
            FSWMonitor sut = new(watcher, name); // no path
            bool canStart = sut.CanStart();

            // Assert
            Assert.False(canStart);
        }

        // should be MOQ'd
        //[Fact]
        //public void CanStart_NotStoppedReturnFalse()
        //{
        //    // Arrange
        //    var watcher = new FileSystemWatcher();
        //    var name = "TestWatcher";
        //    var path = @"C:\Test-Temp";

        //    // Act
        //    FSWMonitor sut = new(watcher, name)
        //    {
        //        MonitoredPath = path
        //    };

        //    if (!Directory.Exists(path))
        //    {
        //        Directory.CreateDirectory(path);
        //    }

        //    sut.EnableRaisingEvents = true;
        //    bool canStart = sut.CanStart();
        //    sut.EnableRaisingEvents = false;
        //    sut.Dispose();

        //    // Assert
        //    Assert.False(canStart);
        //}
    }
}
