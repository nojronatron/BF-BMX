using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BFBMX.Desktop.Helpers
{
    public partial class DesktopLogger
    {
        private readonly static string DesktopLogFilename = "bfbmx-desktop.log";
        private ILogger<DesktopLogger> _logger;

        public DesktopLogger(ILogger<DesktopLogger> logger)
        {
            _logger = logger;
        }
        public string HandleRequest()
        {
            LogHandleRequest(_logger);
            return "Handled request";
        }

        [LoggerMessage(LogLevel.Information, "DesktopLogger.HandleRequest was called")]
        public static partial void LogHandleRequest(ILogger logger);
    }
}
