using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFBMX.Service.Models
{
    public class FSWMonitor
    {
        private readonly FileSystemWatcher? fileSystemWatcher;
        public FileSystemWatcher? GetFileSystemWatcher => fileSystemWatcher;

        public FSWMonitor(FileSystemWatcher fileSystemWatcher)
        {
            this.fileSystemWatcher = fileSystemWatcher;
        }

        public bool IsStarted
        {
            get
            {
                return fileSystemWatcher is not null && fileSystemWatcher.EnableRaisingEvents;
            }
        }

        public bool IsStopped
        {
            get
            {
                return fileSystemWatcher is not null && !fileSystemWatcher.EnableRaisingEvents;
            }
        }

        public bool IsInitialized
        {
            get
            {
                return fileSystemWatcher is not null && !fileSystemWatcher.EnableRaisingEvents;
            }
        }

        public string MonitoredPath
        {
            get
            {
                return (fileSystemWatcher is not null && !string.IsNullOrWhiteSpace(fileSystemWatcher.Path))
                       ? fileSystemWatcher.Path
                       : string.Empty;
            }
            set
            {
                if (fileSystemWatcher is not null)
                {
                    fileSystemWatcher.Path = value;
                }
            }
        }

        public bool EnableRaisingEvents
        {
            get
            {
                return fileSystemWatcher is not null && fileSystemWatcher.EnableRaisingEvents;
            }
            set
            {
                if (fileSystemWatcher is not null)
                {
                    fileSystemWatcher.EnableRaisingEvents = value;
                }
            }
        }

        public bool CanInitialize()
        {
            return fileSystemWatcher is not null 
                   && !string.IsNullOrWhiteSpace(fileSystemWatcher.Path) 
                   && IsStopped;
        }

        public bool CanStart()
        {
            return fileSystemWatcher is not null 
                   && fileSystemWatcher.EnableRaisingEvents == false 
                   && !string.IsNullOrWhiteSpace(fileSystemWatcher.Path);
        }

        public void Dispose()
        {
            fileSystemWatcher?.Dispose();
        }
    }
}
