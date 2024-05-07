namespace BFBMX.ServerApi.Helpers
{
    public class ServerLogWriter : IServerLogWriter
    {
        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly IServerEnvFactory _serverEnvFactory;
        private readonly ILogger<ServerLogWriter> _logger;

        public ServerLogWriter(IServerEnvFactory serverEnvFactory, ILogger<ServerLogWriter> logger)
        {
            _serverEnvFactory = serverEnvFactory;
            _logger = logger;
        }

        /// <summary>
        /// Writes a message to the server log file using asynchronous techniques.
        /// If this method is awaited, log entries might be written in an unexpected order.
        /// </summary>
        /// <param name="message"></param>
        /// <returns>An async Task that can be awaited.</returns>
        public async Task WriteActivityToLogAsync(string message)
        {
            await _semaphore.WaitAsync();
            DateTime dateTimeStamp = DateTime.Now;
            string serverLogPath = Path.Combine(_serverEnvFactory.GetServerLogPath(), _serverEnvFactory.GetServerActivityLogFilename());
            string logMessage = $"{dateTimeStamp:yyyy-MMM-dd HH:mm:ss}: {message}";

            try
            {
                if (!File.Exists(serverLogPath))
                {
                    File.Create(serverLogPath).Dispose();
                }

#pragma warning disable IDE0063 // Use simple 'using' statement
                using (StreamWriter sw = File.AppendText(serverLogPath))
                {
                    await sw.WriteLineAsync(logMessage);
                }
#pragma warning restore IDE0063 // Use simple 'using' statement
            }
            catch (Exception ex)
            {
                _logger.LogError("Error writing log to {serverLogpath}, reason: {exMessage}", serverLogPath, ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
