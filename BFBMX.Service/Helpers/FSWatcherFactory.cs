namespace BFBMX.Service.Helpers
{
    public static class FSWatcherFactory
    {
        private static string DefaultFileType = "*.mime";

        public static FileSystemWatcher Create(FileSystemEventHandler asyncCreatedCallback,
                                               ErrorEventHandler asyncErrorCallback,
                                               string fullpath)
        {
            var watcher = new FileSystemWatcher(fullpath);

            watcher.NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.CreationTime;

            watcher.Created += asyncCreatedCallback;
            watcher.Error += asyncErrorCallback;

            watcher.Filter = DefaultFileType;
            watcher.IncludeSubdirectories = false;
            return watcher;
        }
    }
}
