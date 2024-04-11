using BFBMX.Service.Helpers;
using BFBMX.Service.Models;
using System.Diagnostics;

namespace BFBMX.Service.Test.Helpers
{
    public class FileSystemMonitorTests
    {
        private const string defaultFilter = "*.mime";
        private readonly string? tempPath;
        private readonly string tempFilename;
        private readonly string AlphaMonitorName = "AlphaMonitor";
        private readonly string BravoMonitorName = "BravoMonitor";
        private readonly string CharlieMonitorName = "CharlieMonitor";

        private string? FwmTempfileFound { get; set; }
        private string? FwmErrorCallbackMessage { get; set; }

        public FileSystemMonitorTests()
        {
            tempPath = Environment.GetEnvironmentVariable("TEMP");
            tempFilename = "FsmFileTest.mime";
        }

        [Fact]
        public void IsStarted_Should_Return_True_When_EnableRaisingEvents_Is_True()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = true;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.IsStarted;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsStarted_Should_Return_False_When_EnableRaisingEvents_Is_False()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = false;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.IsStarted;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsStopped_Should_Return_True_When_EnableRaisingEvents_Is_False()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = false;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.IsStopped;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsStopped_Should_Return_False_When_EnableRaisingEvents_Is_True()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = true;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.IsStopped;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsInitialized_Should_Return_True_When_EnableRaisingEvents_Is_False()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = false;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.IsInitialized;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInitialized_Should_Return_False_When_EnableRaisingEvents_Is_True()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = true;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.IsInitialized;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void MonitoredPath_Should_Return_Path_When_Path_Is_Not_Null_Or_Whitespace()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.Path = tempPath!;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.MonitoredPath;

            // Assert
            Assert.Equal(tempPath, result);
        }

        [Fact]
        public void MonitoredPath_Should_Return_Empty_String_When_Path_Is_Null_Or_Whitespace()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(string.Empty);
            fileSystemWatcher.Path = string.Empty;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.MonitoredPath;

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void EnableRaisingEvents_Should_Set_EnableRaisingEvents_Property()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            fswMonitor.EnableRaisingEvents = true;

            // Assert
            Assert.True(fileSystemWatcher.EnableRaisingEvents);
        }

        [Fact]
        public void CanStart_Should_Return_True_When_All_Conditions_Are_Met()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = false;
            fileSystemWatcher.Path = tempPath!;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.CanStart();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void CanStart_Should_Return_False_When_EnableRaisingEvents_Is_True()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = true;
            fileSystemWatcher.Path = tempPath!;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.CanStart();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanStart_Should_Return_False_When_Path_Is_Null_Or_Whitespace()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = false;
            fileSystemWatcher.Path = string.Empty;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.CanStart();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void CanStart_Should_Return_False_When_IsNotStopped_Is_False()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            fileSystemWatcher.EnableRaisingEvents = false;
            fileSystemWatcher.Path = tempPath!;
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.CanStart();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Dispose_Should_Call_Dispose_Method_Of_FileSystemWatcher()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            fswMonitor.Dispose();

            // Assert
            Assert.True(fileSystemWatcher.EnableRaisingEvents);
        }

        [Fact]
        public void GetName_Should_Return_Name_When_Name_Is_Not_Null()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            var fswMonitor = new FSWMonitor(fileSystemWatcher, "Test");

            // Act
            var result = fswMonitor.GetName();

            // Assert
            Assert.Equal("Test", result);
        }

        [Fact]
        public void GetName_Should_Return_Empty_String_When_Name_Is_Null()
        {
            // Arrange
            var fileSystemWatcher = new FileSystemWatcher(tempPath!);
            var fswMonitor = new FSWMonitor(fileSystemWatcher, string.Empty);

            // Act
            var result = fswMonitor.GetName();

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void InitializeFileSystemWatcher()
        {
            Assert.NotNull(tempPath);
            var expectedPath = tempPath;
            var expectedFilter = defaultFilter;
            var expectedIncludeSubdirs = false;
            var expectedNotifyFilter = NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.CreationTime;
            var expectedWatcher = new FileSystemWatcher(expectedPath!)
            {
                NotifyFilter = expectedNotifyFilter,
                Filter = expectedFilter,
                IncludeSubdirectories = expectedIncludeSubdirs
            };

            var actualWatcher = FSWatcherFactory.Create((s, e) => { }, (s, e) => { }, expectedPath!, AlphaMonitorName);

            Assert.Equal(expectedPath, actualWatcher.MonitoredPath);
            FileSystemWatcher? FSWInstance = actualWatcher.GetFileSystemWatcher;
            Assert.NotNull(FSWInstance);
            Assert.Equal(expectedFilter, FSWInstance!.Filter);
            Assert.Equal(expectedIncludeSubdirs, FSWInstance.IncludeSubdirectories);
            Assert.Equal(expectedNotifyFilter, FSWInstance.NotifyFilter);
            expectedWatcher.Dispose();
            actualWatcher.Dispose();
        }

        [Fact]
        public void StartStopDestroyFileSystemWatcher()
        {
            Debug.WriteLine($"Temp file found? {FwmTempfileFound}");
            Debug.WriteLine($"Error reported? {FwmErrorCallbackMessage}");
            Assert.NotNull(tempPath);
            var expectedPath = tempPath;

            // capture the watcher object and ensure automatic disposal if a test fails
            //using var actualWatcher =
            //    FSWatcherFactory.Create(HandleFileCreated, HandleFileWatherError, expectedPath!);
            var actualWatcher = FSWatcherFactory.Create(HandleFileCreated, HandleFileWatherError, expectedPath!, BravoMonitorName);

            // verify starting conditions
            FileSystemWatcher? FSWInstance = actualWatcher.GetFileSystemWatcher; 
            Assert.NotNull(FSWInstance);
            Assert.Equal(expectedPath, actualWatcher.MonitoredPath);
            Assert.True(string.IsNullOrEmpty(FwmTempfileFound));
            Assert.True(string.IsNullOrEmpty(FwmErrorCallbackMessage));

            // enable the watcher
            Assert.True(actualWatcher.EnableRaisingEvents = true);

            // create a file to test if the watcher is working
            var testFile = Path.Combine(tempPath!, tempFilename);
            string fileContent = "test file content";
            CancellationToken cancelToken = new CancellationTokenSource().Token;

            // if file exists already, delete it otherwise create callback will not fire
            if (File.Exists(testFile))
            {
                File.Delete(testFile);
            }

            var swTask = Task.Run(() =>
            {
                using (StreamWriter sw = File.CreateText(testFile))
                {
                    sw.WriteLine(fileContent);
                    Console.WriteLine("Writing file...");
                }
            }, cancelToken);
            var delayTask = Task.Delay(250, cancelToken);
            Task.WaitAll(swTask, delayTask);

            Debug.WriteLine($"Temp file found? {FwmTempfileFound}");
            Debug.WriteLine($"Error reported? {FwmErrorCallbackMessage}");

            // FwmTempfileFound should be set to the newly created file name
            Assert.True(File.Exists(testFile));
            Assert.False(string.IsNullOrWhiteSpace(FwmTempfileFound));
            Assert.True(string.IsNullOrEmpty(FwmErrorCallbackMessage));

            // reset temp file found
            FwmTempfileFound = string.Empty;
            FwmErrorCallbackMessage = string.Empty;

            // turn-off file watching and ensure no further events are raised
            Assert.False(actualWatcher.EnableRaisingEvents = false);

            // delete the file
            File.Delete(testFile);
            Assert.False(File.Exists(testFile));

            // add a test file while the watcher is off
            var testFile2 = Path.Combine(tempPath!, "FsmFileTest2.mime");

            using (StreamWriter sw = File.CreateText(testFile2))
            {
                sw.WriteLine(fileContent);
            }

            Assert.True(File.Exists(testFile2));

            Debug.WriteLine($"Temp file found? {FwmTempfileFound}");
            Debug.WriteLine($"Error reported? {FwmErrorCallbackMessage}");

            // test if Created Event was NOT raised
            Assert.True(string.IsNullOrWhiteSpace(FwmTempfileFound));

            // remove the created file
            File.Delete(testFile2);
            // dispose the watcher instance
            actualWatcher.Dispose();
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void HandleFileCreated(object sender, FileSystemEventArgs e)
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            FwmTempfileFound = e.Name;
        }

#pragma warning disable xUnit1013 // Public method should be marked as test
        public void HandleFileWatherError(object sender, ErrorEventArgs e)
#pragma warning restore xUnit1013 // Public method should be marked as test
        {
            FwmErrorCallbackMessage = e.GetException().Message;
        }
    }
}
