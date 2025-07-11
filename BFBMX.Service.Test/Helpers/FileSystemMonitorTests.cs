﻿using BFBMX.Service.Helpers;
using System.Diagnostics;

namespace BFBMX.Service.Test.Helpers
{
    public class FileSystemMonitorTests
    {
        private readonly string? tempPath;
        private readonly string tempFilename;
        private readonly string AlphaMonitorName = "AlphaMonitor";
        private readonly string BravoMonitorName = "BravoMonitor";
        //private readonly string CharlieMonitorName = "CharlieMonitor";

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
        public async Task StartStopDestroyFileSystemWatcher()
        {
            Debug.WriteLine($"Temp file found? {FwmTempfileFound}");
            Debug.WriteLine($"Error reported? {FwmErrorCallbackMessage}");
            Assert.NotNull(tempPath);
            var expectedPath = tempPath;

            // capture the watcher object and ensure automatic disposal if a test fails
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
                using StreamWriter sw = File.CreateText(testFile);
                sw.WriteLine(fileContent);
                Console.WriteLine("Writing file...");
            }, cancelToken);
            var delayTask = Task.Delay(250, cancelToken);
            await Task.WhenAll(swTask, delayTask);

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
