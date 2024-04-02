using BFBMX.Service.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BFBMX.Service.Helpers
{
    public class FileProcessor : IFileProcessor
    {
        private static readonly object _lock = new object();
        private readonly ILogger<FileProcessor> _logger;

        private readonly string messageIdPattern = @"\bMessage-ID\S\s?(?'msgid'.{12})\b";
        private readonly string strictBibPattern = @"\b\d{1,3}\t(OUT|IN|DROP)\t\d{4}\t\d{1,2}\t\w{2}\b";
        private readonly string sloppyBibPattern = @"\b\w{1,15}\t\w{1,5}\t\w{1,5}\t\w{1,3}\t\w{1,26}\b";

        public FileProcessor(ILogger<FileProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Writes a non-null WinilnkMessageModel instance to a file at filepath.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="filepath"></param>
        /// <returns>True if succeeds writing to file, otherwise False</returns>
        public bool WriteWinlinkMessageToFile(WinlinkMessageModel msg, string filepath)
        {
            if (msg is null || msg.BibRecords.Count < 1 || string.IsNullOrWhiteSpace(filepath))
            {
                return false;
            }
            else
            {
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true,
                    NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                    PropertyNameCaseInsensitive = true
                };

                string json = JsonSerializer.Serialize<WinlinkMessageModel>(msg, options);
                string prefixText = File.Exists(filepath) ? "," : string.Empty;

                for (int tries = 3; tries > 0; tries--)
                {
                    try
                    {
                        lock (_lock)
                        {
#pragma warning disable IDE0063 // Use simple 'using' statement
                            using (StreamWriter file = File.AppendText(filepath))
                            {
                                file.WriteLine(prefixText);
                                file.WriteLine(json);
                            }
#pragma warning restore IDE0063 // Use simple 'using' statement
                        }

                        return true;
                    }
                    catch (Exception)
                    {
                        _logger.LogWarning("WriteWinlinkMessageToFile: Attempt number {tries} - Could not write to {filepath}.", tries, filepath);
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Converts an array of strings to a single string with each element separated by a newline character.
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns>string representing elements, each separated by a newline character.</returns>
        public string RecordsArrayToString(string[] fileData)
        {
            StringBuilder sb = new();
            foreach (var fd in fileData)
            {
                sb.Append(fd).Append('\n');
            }
            return sb.ToString();
        }

        /// <summary>
        /// Processes a Winlink message file information and returns a WinlinkMessageModel instance.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="machineName"></param>
        /// <param name="filePath"></param>
        /// <returns>WinlinkMessageModel instance</returns>
        public WinlinkMessageModel ProcessWinlinkMessageFile(DateTime timestamp, string machineName, string filePath)
        {
            var fileData = GetFileData(filePath);

            if (fileData.Length < 1)
            {
                // could not read or understand file data
                return new WinlinkMessageModel();
            }

            string winlinkMessageId = GetMessageId(RecordsArrayToString(fileData));
            List<FlaggedBibRecordModel> bibRecords = new();

            if (ProcessBibs(bibRecords, fileData))
            {
                // no errors processing file
                if (bibRecords.Count > 0)
                {
                    // bib data was found so return a concrete object
                    return WinlinkMessageModel.GetWinlinkMessageInstance(winlinkMessageId, timestamp, machineName, bibRecords);
                }
            }

            return new WinlinkMessageModel();
        }

        /// <summary>
        /// Resuable wrapper method to get file data without throwing an IO and other Exceptions.
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns>Array of strings representing lines of data in the file at fullFilePath</returns>
        public string[] GetFileData(string fullFilePath)
        {
            if (!string.IsNullOrWhiteSpace(fullFilePath) && File.Exists(fullFilePath))
            {
                for (int tries = 1; tries < 4; tries++)
                {
                    Thread.Sleep(100);

                    try
                    {
                        string[] lines = File.ReadAllLines(fullFilePath);
                        _logger.LogWarning("GetFileData: Successfully read data from {fullFilePath}.", fullFilePath);
                        return lines;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("GetFileData: Attempt number {tries} - Unable to read data from {fullFilePath}: {exMessage}", tries, fullFilePath, ex.Message);
                    }
                }
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Finds the first instance of a Winlink message Message-ID.
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns>Message-ID value or an empty string if none found.</returns>
        public string GetMessageId(string fileData)
        {
            if (string.IsNullOrWhiteSpace(fileData))
            {
                return string.Empty;
            }

            Regex match = new(messageIdPattern, RegexOptions.IgnoreCase, new TimeSpan(0, 0, 2));
            MatchCollection matches = match.Matches(fileData);

            if (matches.Count > 0)
            {
                return matches[0].Groups["msgid"].Value;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Matches strictBibPattern and sloppyBibPattern and adds matches to a list of BibRecordModel.
        /// </summary>
        /// <param name="bibRecords">Empty List of type BibRecordModel</param>
        /// <param name="lines">Array of string data to check for bib records</param>
        /// <returns>True if strict and sloppy match counts are same, otherwise False.</returns>
        public bool ProcessBibs(List<FlaggedBibRecordModel> bibRecords, string[] lines)
        {
            if (lines is null || lines.Length < 1)
            {
                _logger.LogWarning("Input {linesProperty} is null, returning and empty list.", nameof(lines));
            }
            else
            {
                List<FlaggedBibRecordModel> strictMatches = GetStrictMatches(lines);
                List<FlaggedBibRecordModel> sloppyMatches = GetSloppyMatches(lines);

                if (strictMatches.Count == sloppyMatches.Count)
                {
                    bibRecords.AddRange(strictMatches);
                    return true;
                }

                if (strictMatches.Count < sloppyMatches.Count)
                {
                    bibRecords.AddRange(sloppyMatches);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to find potential matches in bib data. Returns a list of FlaggedBibRecordModel with DataWarnings set as appropriate.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>List of FlaggedBibRecordModel instances</returns>
        public List<FlaggedBibRecordModel> GetSloppyMatches(string[] lines)
        {
            var sloppyBibRecords = new List<FlaggedBibRecordModel>();

            if (lines is null || lines.Length < 1)
            {
                _logger.LogWarning("GetSloppyMatches input {linesProperty} was empty, returning an empty list.", nameof(lines));
                return sloppyBibRecords;
            }

            bool result = GetBibMatches(sloppyBibRecords, lines, sloppyBibPattern);
            string didOrNotFind = result ? "found" : "did not find";
            _logger.LogWarning("GetSloppyMatches {didOrNotFind} bib data.", didOrNotFind);
            return sloppyBibRecords;
        }

        /// <summary>
        /// Attempts to find strict matches in bib data. Returns a list of FlaggedBibRecordModel with DataWarnings set as appropriate.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>List of FlaggedBibRecordModel instances</returns>
        public List<FlaggedBibRecordModel> GetStrictMatches(string[] lines)
        {
            var strictBibRecords = new List<FlaggedBibRecordModel>();

            if (lines is null || lines.Length < 1)
            {
                _logger.LogWarning("GetStrictMatches input {linesProperty} was empty, returning and empty list.", nameof(lines));
                return strictBibRecords;
            }

            bool result = GetBibMatches(strictBibRecords, lines, strictBibPattern);
            string didOrNotFind = result ? "found" : "did not find";
            _logger.LogWarning("GetStrictMatches {didOrNotFind} bib data.", didOrNotFind);
            return strictBibRecords;
        }

        /// <summary>
        /// Identifies data matches in array of potential bib records usinga RegEx pattern.
        /// Data Warning flags could be set on any FlaggedBibRecordModel that cannot be fully parsed.
        /// </summary>
        /// <param name="emptyBibList"></param>
        /// <param name="fileDataLines"></param>
        /// <param name="pattern"></param>
        /// <returns>true if any matches are found, false in any other case.</returns>
        public bool GetBibMatches(List<FlaggedBibRecordModel> emptyBibList,
                                         string[] fileDataLines,
                                         string pattern)
        {
            if (
                emptyBibList is null || emptyBibList.Count > 0
                || fileDataLines is null || fileDataLines.Length < 1
                || string.IsNullOrWhiteSpace(pattern)
                )
            {
                return false;
            }
            else
            {
                foreach (var line in fileDataLines)
                {
                    try
                    {
                        if (Regex.IsMatch(line, pattern, RegexOptions.IgnoreCase, new TimeSpan(0, 0, 2)))
                        {
                            var fields = line.Split('\t');
                            FlaggedBibRecordModel bibRecord = FlaggedBibRecordModel.GetBibRecordInstance(fields);

                            if (bibRecord is not null)
                            {
                                emptyBibList.Add(bibRecord);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("FileProcessor GetBibMatches: RegEx operation could not match {pattern} to {line}!", pattern, line);
                        _logger.LogWarning("FileProcessor GetBibMatches: Exception message is {exMessage}.", ex.Message);
                        _logger.LogWarning("FileProcessor GetBibMatches: Operations will continue but an audit should be performed.");
                    }
                }

                return emptyBibList.Count > 0;
            }
        }
    }
}
