using BFBMX.Service.Models;

namespace BFBMX.ServerApi.Helpers
{
    public class BibRecordLogger : IBibRecordLogger
    {
        private static readonly object LockObject = new object(); // lock object to ensure only single thread can access the file at a time
        private readonly IServerEnvFactory _serverEnvFactory;
        private readonly ILogger<BibRecordLogger> _logger;

        public BibRecordLogger(ILogger<BibRecordLogger> logger, IServerEnvFactory serverEnvFactory)
        {
            _logger = logger;
            _serverEnvFactory = serverEnvFactory;
        }

        /// <summary>
        /// Write a Winlink Message payload to a tab-delimited file named after the Winlink ID, for AccessDB importing.
        /// </summary>
        /// <param name="wlMessagePayload"></param>
        /// <returns>True if able to open a new file and write contents, otherwise false.</returns>
        public bool LogWinlinkMessagePayloadToTabDelimitedFile(WinlinkMessageModel wlMessagePayload)
        {
            string bfBmxLogPath = _serverEnvFactory.GetServerLogPath();

            string bfBmxLogFilePath = Path.Combine(bfBmxLogPath!, wlMessagePayload.ToFilename()); // ABC123CDE456.txt

            try
            {
                lock (LockObject)
                {
                    using (StreamWriter file = File.AppendText(bfBmxLogFilePath))
                    {
                        file.Write(wlMessagePayload.ToAccessDatabaseTabbedString());
                    }
                }

                _logger.LogInformation("Wrote 1 Winlink Message payload to Access DB file {logFile}", bfBmxLogFilePath);
                return true;

            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("An unexpected error occurred while attempting to log the Winlink Message payload to the Access DB file: {ex}", ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning("An agument was incorrect or null while attempting to log the Winlink Message payload to the Access DB file: {ex}", ex.Message);
            }
            catch (PathTooLongException ex)
            {
                _logger.LogWarning("Path {bfbmxpath} is too long! Set the Environment Variable and restart the server in order to log Winlink Message payloads!", bfBmxLogPath);
            }
            catch (DirectoryNotFoundException ex)
            {
                _logger.LogWarning("Path {bfbmxpath} was not found! Set the Environment Variable to a directory that exists and restart the server in order to log Winlink Message payloads!", bfBmxLogPath);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning("Unable to read, seek, or write to the Access DB file: {bfBmxLogPath}. Set the Environment Variable to a directory that you have read and write access to and restart the server to try again.", bfBmxLogPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("An unexpected error occurred while attempting to log the Winlink Message payload to the Access DB file: {ex}", ex.Message);
            }

            return false;
        }
    }
}
