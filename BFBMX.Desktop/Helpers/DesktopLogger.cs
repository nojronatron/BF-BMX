using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Versioning;

namespace BFBMX.Desktop.Helpers
{
    public partial class DesktopLogger : ILogger
    {
        private readonly Func<DesktopLoggerConfiguration> getCurrentConfig;
        private readonly string? name;

        public DesktopLogger(string name, Func<DesktopLoggerConfiguration> getCurrentConfig)
        {
            this.name = name;
            this.getCurrentConfig = getCurrentConfig;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return default!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return getCurrentConfig().LogLevelToTextOutputMap.ContainsKey(logLevel);
        }

        public void Log<TState>(LogLevel logLevel,
                                EventId eventId,
                                TState state,
                                Exception? exception,
                                Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            DesktopLoggerConfiguration config = getCurrentConfig();

            if (config.EventId == 0 || config.EventId == eventId.Id)
            {
                string logLevelText = config.LogLevelToTextOutputMap[logLevel];

                if (formatter != null)
                {
                    string message = formatter(state, exception);
                    DateTime timeStamp = DateTime.Now;
                    string concatMessage = $"{timeStamp:dd-MM-yy-HH:mm:ss} {logLevelText}: {message}";
                    string filePath = config.LogfilePath!;

#pragma warning disable IDE0063 // Use simple 'using' statement
                    using (StreamWriter file = File.AppendText(filePath))
                    {
                        file.WriteLine(concatMessage);
                    }
#pragma warning restore IDE0063 // Use simple 'using' statement
                }
            }
        }
    }

    public sealed class DesktopLoggerConfiguration
    {
        public int EventId { get; set; }
        public string? LogfilePath { get; set; }

        public Dictionary<LogLevel, string> LogLevelToTextOutputMap { get; set; } = new()
        {
            [LogLevel.Information] = "INFO",
            [LogLevel.Debug] = "DEBUG",
            [LogLevel.Warning] = "WARN",
            [LogLevel.Critical] = "CRITICAL"
        };
    }

    /// <summary>
    /// An ILoggerProvider that generates a new DesktopLogger based on the current configuration.
    /// If using IConfiguration (JSON) then alias will be "DesktopLogger" to set LogLevelToTextOutputMap configuration.
    /// </summary>
    [UnsupportedOSPlatform("browser")]
    [ProviderAlias("DesktopLogger")]
    public class DesktopLoggerProvider : ILoggerProvider
    {
        private readonly IDisposable? _onChangeToken;
        private DesktopLoggerConfiguration _currentConfig;
        private readonly ConcurrentDictionary<string, DesktopLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);

        public DesktopLoggerProvider(IOptionsMonitor<DesktopLoggerConfiguration> config)
        {
            _currentConfig = config.CurrentValue;
            _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(
                categoryName,
                name => new DesktopLogger(categoryName, GetCurrentConfig));
        }

        private DesktopLoggerConfiguration GetCurrentConfig()
        {
            return _currentConfig;
        }

        public void Dispose()
        {
            _loggers.Clear();
            _onChangeToken?.Dispose();
        }
    }
}
