﻿namespace BFBMX.Service.Helpers
{
    public static class FSWatcherFactory
    {
        private static string DefaultFileType = "*.mime";

        /// <summary>
        /// Safely generate a new, valid FileSystemWatcher instance.
        /// </summary>
        /// <param name="asyncCreatedCallback">A method/delegate handler for when a created file is detected.</param>
        /// <param name="asyncErrorCallback">A method/delegate handler for when an error occurs.</param>
        /// <param name="fullpath">The path to watch for files with .mime extension.</param>
        /// <returns>A new FileSystemWatcher instance.</returns>
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
