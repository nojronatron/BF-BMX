using BFBMX.Service.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BFBMX.ServerApi.Helpers
{
    public class BibRecordLogger : IBibRecordLogger
    {
        private static readonly object LockObject = new object(); // lock object to ensure only single thread can access the file at a time
        private readonly ILogger<BibRecordLogger> _logger;

        private readonly string ParentDirectory = "Documents";
        private readonly string LogFilename = "BibRecordsServerLog.txt";

        public JsonNumberHandling JsonNumerHandling { get; private set; }

        public BibRecordLogger(ILogger<BibRecordLogger> logger) {
            _logger = logger;
        }

        /// <summary>
        /// Ensures that the necessary environment variables are set and that the log directory exists.
        /// </summary>
        /// <param name="bfBmxLogPath"></param>
        /// <returns></returns>
        public bool ValidateServerVariables(out string? bfBmxLogPath)
        {
            string? userProfilePath = Environment.GetEnvironmentVariable("USERPROFILE");

            if (string.IsNullOrEmpty(userProfilePath))
            {
                _logger.LogWarning("Missing user profile path!");
                bfBmxLogPath = string.Empty;
                return false;
            }

            string? logDirectory = Environment.GetEnvironmentVariable("BFBMX_SERVER_FOLDER_NAME");

            if (string.IsNullOrEmpty(logDirectory))
            {
                _logger.LogWarning("Missing log directory name!");
                bfBmxLogPath = string.Empty;
                return false;
            }

            bfBmxLogPath = Path.Combine(userProfilePath, ParentDirectory, logDirectory);

            if (!Directory.Exists(bfBmxLogPath))
            {
                _logger.LogInformation("Creating directory at {logPath}", bfBmxLogPath);
                Directory.CreateDirectory(bfBmxLogPath);
            }

            return true;
        }

        /// <summary>
        /// Writes Winlink Message payload to a JSON formatted file for auditing.
        /// </summary>
        /// <param name="wlMessagePayload"></param>
        /// <returns></returns>
        public bool LogWinlinkMessagePayloadToJsonAuditFile(WinlinkMessageModel wlMessagePayload)
        {
            try
            {
                if (ValidateServerVariables(out string? bfBmxLogPath))
                {
                    string bfBmxLogFilePath = Path.Combine(bfBmxLogPath!, wlMessagePayload.ToFilename());

                    JsonSerializerOptions options = new()
                    {
                        NumberHandling = JsonNumberHandling.AllowReadingFromString,
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };

                    lock (LockObject)
                    {
                        using (StreamWriter file = File.CreateText(bfBmxLogFilePath))
                        {
                            string? serializedPayload = JsonSerializer.Serialize(wlMessagePayload, options);
                            file.Write(serializedPayload);
                        }
                    }

                    _logger.LogInformation("Write WinlinkMessage to audit file {logFile}", bfBmxLogFilePath);
                }
                else
                {
                    _logger.LogWarning("Unable to path variables for logging payload to the Json Audit file.");
                    return false;
                }
            }
            catch (IOException ioex)
            {
                _logger.LogError("LogWinlinkMessagePayloadToJsonAuditFile: writing serialized payload to file caused exception: {ex}", ioex);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError("Unauthorized access to parent directory {pd} or log file {lfn}, causing exception msg: {exMsg}", ParentDirectory, LogFilename, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while attempting to log the WinlinkMessage to file, exception msg: {exMsg}", ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Writes a Winlink Message payload to a tab-delimited log file for AccessDB importing.
        /// </summary>
        /// <param name="wlMessagePayload"></param>
        /// <returns></returns>
        public bool LogFlaggedRecordsTabDelimited(WinlinkMessageModel wlMessagePayload)
        {
            try
            {
                if (ValidateServerVariables(out string? bfBmxLogPath))
                {
                    string bfBmxLogFilePath = Path.Combine(bfBmxLogPath!, LogFilename);

                    lock (LockObject)
                    {
                        using (StreamWriter file = File.AppendText(bfBmxLogFilePath))
                        {
                            file.Write(wlMessagePayload.ToAccessDatabaseTabbedString());
                        }
                    }

                    _logger.LogInformation("Wrote 1 Winlink Message payload to log file {logFile}", bfBmxLogFilePath);
                }
                else
                {
                    _logger.LogWarning("Unable to path variables for logging BibRecords to file.");
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError("Unauthorized access to parent directory {pd} or log file {lfn}, causing exception msg: {exMsg}", ParentDirectory, LogFilename, ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred while attempting to log the BibRecords to file, exception msg: {exMsg}", ex.Message);
                return false;
            }

            return true;
        }
    }
}
