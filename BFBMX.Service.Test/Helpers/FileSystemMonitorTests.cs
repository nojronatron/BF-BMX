using BFBMX.Service.Helpers;

namespace BFBMX.Service.Test.Helpers
{
    public class FileSystemMonitorTests
    {
        private readonly string? tempPath;
        private readonly string tempFilename;

        private string? FwmTempfileFound { get; set; }
        private string? FwmErrorCallbackMessage { get; set; }

        public FileSystemMonitorTests()
        {
            tempPath = Environment.GetEnvironmentVariable("TEMP");
            tempFilename = "FsmFileTest.mime";
        }

        [Fact]
        public void InitializeFileSystemWatcher()
        {
            Assert.NotNull(tempPath);
            var expectedPath = tempPath;
            var expectedFilter = "*.mime";
            var expectedIncludeSubdirs = false;
            var expectedNotifyFilter = NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName
                                     | NotifyFilters.CreationTime;
            var expectedWatcher = new FileSystemWatcher(expectedPath!);
            expectedWatcher.NotifyFilter = expectedNotifyFilter;
            expectedWatcher.Filter = expectedFilter;
            expectedWatcher.IncludeSubdirectories = expectedIncludeSubdirs;

            var actualWatcher = FSWatcherFactory.Create((s, e) => { }, (s, e) => { }, expectedPath!);

            Assert.Equal(expectedPath, actualWatcher.Path);
            Assert.Equal(expectedFilter, actualWatcher.Filter);
            Assert.Equal(expectedIncludeSubdirs, actualWatcher.IncludeSubdirectories);
            Assert.Equal(expectedNotifyFilter, actualWatcher.NotifyFilter);
            expectedWatcher.Dispose();
            actualWatcher.Dispose();
        }

        [Fact]
        public void StartStopDestroyFileSystemWatcher()
        {
            Assert.NotNull(tempPath);
            var expectedPath = tempPath;

            // capture the watcher object and ensure automatic disposal if a test fails
            using var actualWatcher =
                FSWatcherFactory.Create(HandleFileCreated, HandleFileWatherError, expectedPath!);

            // verify starting conditions
            Assert.Equal(expectedPath, actualWatcher.Path);
            Assert.True(string.IsNullOrEmpty(FwmTempfileFound));
            Assert.True(string.IsNullOrEmpty(FwmErrorCallbackMessage));

            // enable the watcher
            Assert.True(actualWatcher.EnableRaisingEvents = true);

            // create a file to test if the watcher is working
            var testFile = Path.Combine(tempPath!, tempFilename);
            string fileContent = "test file content";
            CancellationToken cancelToken = new CancellationTokenSource().Token;

            var swTask = Task.Run(() =>
            {
                using (StreamWriter sw = File.CreateText(testFile))
                {
                    sw.WriteLine(fileContent);
                    Console.WriteLine("Writing file...");
                }
            }, cancelToken);
            swTask.Wait();

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
