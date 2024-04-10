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
        private static readonly object _lock = new();
        private readonly ILogger<FileProcessor> _logger;

        private readonly string messageIdPattern = @"\bMessage-ID\S\s?(?'msgid'.{12})\b";
        private readonly string dateTimeStampPattern = @"\d{1,2}\s\w{3}\s\d{4}\s\d{1,2}:\d{1,2}:\d{1,2}\s\+\d{4}";
        private readonly string strictBibPatternTabDelim = @"\b\d{1,3}\t(OUT|IN|DROP)\t\d{1,4}\t\d{1,2}\t\w{2}\b";
        private readonly string sloppyBibPatternTabDelim = @"\b\w{1,15}\t\w{1,5}\t\w{1,5}\t\w{1,3}\t\w{1,26}\b";

        private static JsonSerializerOptions LocalJsonSerializerOptions => new()
        {
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            PropertyNameCaseInsensitive = true
        };

        private static RegexOptions LocalRegexOptions => RegexOptions.IgnoreCase;
        private static TimeSpan LocalRegexTimeout => new(0, 0, 1);

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
                string json = JsonSerializer.Serialize<WinlinkMessageModel>(msg, LocalJsonSerializerOptions);
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
            string[] fileData = GetFileData(filePath);

            if (fileData.Length < 1)
            {
                // could not read or understand file data
                return new WinlinkMessageModel();
            }

            string recordArray = RecordsArrayToString(fileData);
            string winlinkMessageId = GetMessageId(recordArray);
            DateTime winlinkDateTimeStamp = GetWinlinkMessageDateTimeStamp(recordArray);
            List<FlaggedBibRecordModel> bibRecords = ProcessBibs(fileData, winlinkMessageId);

            if (bibRecords.Count > 0)
            {
                // bib data was found so return a concrete object
                return WinlinkMessageModel.GetWinlinkMessageInstance(winlinkMessageId,
                                                                     winlinkDateTimeStamp,
                                                                     machineName,
                                                                     winlinkDateTimeStamp,
                                                                     bibRecords);
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
                    try
                    {
                        lock (_lock)
                        {
                            string[] lines = File.ReadAllLines(fullFilePath);
                            _logger.LogInformation("GetFileData: Successfully read {dataLines} lines of data from {fullFilePath}.", lines.Length, fullFilePath);
                            return lines;
                        }
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
                _logger.LogWarning("FileProcessor: GetMessageId: Somehow an empty string was received. Returning an empty MessageID.");
            }
            else
            {
                Regex match = new(messageIdPattern, LocalRegexOptions, LocalRegexTimeout);
                MatchCollection matches = match.Matches(fileData);

                if (matches.Count < 1)
                {
                    _logger.LogInformation("FileProcessor: GetMessageId: No Message-ID found in the data. Returning an empty MessageID.");
                }

                return matches[0].Groups["msgid"].Value;
            }

            return string.Empty;
        }

        /// <summary>
        /// Finds the DateTime stamp in a Winlink message data string and returns the DateTime value.
        /// </summary>
        /// <param name="winlinkMessageData"></param>
        /// <returns>Parsed DateTime value or if input was not parseable, Jan 1 0001.</returns>
        public DateTime GetWinlinkMessageDateTimeStamp(string winlinkMessageData)
        {
            if (string.IsNullOrWhiteSpace(winlinkMessageData))
            {
                _logger.LogWarning("FileProcessor: GetWinlinkMessageDataTimeStamp: Somehow an empty string was received. Returning DateTime.MinValue!");
            }
            else
            {
                Regex match = new(dateTimeStampPattern, LocalRegexOptions, LocalRegexTimeout);
                MatchCollection matches = match.Matches(winlinkMessageData);

                if (matches.Count < 1)
                {
                    return DateTime.MinValue;
                }

                string dateStamp = matches[0].Value;

                try
                {
                    DateTime parsedValue = DateTime.Parse(dateStamp);
                    return parsedValue.ToUniversalTime();
                }
                catch (FormatException fmtEx)
                {
                    _logger.LogWarning(
                        "FileProcessor: GetWinlinkMessageDateTimeStamp: A format exception was thrown ### msg {fmtEx} ### for the following message {msg}.", 
                        fmtEx.Message, 
                        winlinkMessageData);
                }
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Matches strictBibPattern and sloppyBibPattern and adds matches to a list of BibRecordModel.
        /// </summary>
        /// <param name="lines">Array of string data to check for bib records</param>
        /// <param name="messageId">The Winlink MessageID that is being processed</param>
        /// <returns>True if strict and sloppy match counts are same, otherwise False.</returns>
        public List<FlaggedBibRecordModel> ProcessBibs(string[] lines, string messageId)
        {
            List<FlaggedBibRecordModel> bibRecords = new();

            if (lines is null || lines.Length < 1)
            {
                _logger.LogWarning("Input {linesProperty} is null in Message ID {msgId}. Returning and empty list.", nameof(lines), messageId);
                return bibRecords;
            }

            HashSet<FlaggedBibRecordModel> strictMatches = GetStrictMatches(lines);
            HashSet<FlaggedBibRecordModel> sloppyMatches = GetSloppyMatches(lines);

            int strictCount = strictMatches.Count;
            int sloppyCount = sloppyMatches.Count;

            switch(strictCount, sloppyCount)
            {
                case (0, 0):
                    _logger.LogInformation("ProcessBibs: Neither strict nor relaxed matches found in Message ID {msgId}, returning empty list.", messageId);
                    break;
                case (0, > 0):
                    bibRecords = sloppyMatches.ToList();
                    _logger.LogWarning("ProcessBibs: No strict matches and {sloppyCount} relaxed matches found in Message ID {msgId}.", sloppyCount, messageId);
                    break;
                case (> 0, 0):
                    bibRecords = strictMatches.ToList();
                    _logger.LogWarning("ProcessBibs: {strictCount} strict matches and no relaxed matches found in Message ID {msgId}.", strictCount, messageId);
                    break;
                case (> 0, > 0):
                    strictMatches.UnionWith(sloppyMatches);
                    bibRecords = strictMatches.ToList();
                    _logger.LogWarning("ProcessBibs: Will union {strictCount} strict matches with {sloppyCount} relaxed matches, favoring found strict matches in Message ID {msgId}.", strictCount, sloppyCount, messageId);
                    break;
                default:
                    _logger.LogError("ProcessBibs: An unexpected combination of strict and relaxed records matching occurred when processing bib records in Message ID {msgId}. Returning an empty list.", messageId);
                    break;
            }

            return bibRecords;
        }

        /// <summary>
        /// Attempts to find potential matches in bib data. Returns a list of FlaggedBibRecordModel with DataWarnings set as appropriate.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>List of FlaggedBibRecordModel instances</returns>
        public HashSet<FlaggedBibRecordModel> GetSloppyMatches(string[] lines)
        {
            if (lines is null || lines.Length < 1)
            {
                return new HashSet<FlaggedBibRecordModel>();
            }

            HashSet<FlaggedBibRecordModel> foundBibRecords = GetBibMatches(lines, sloppyBibPatternTabDelim);

            foreach(FlaggedBibRecordModel bibRecord in foundBibRecords)
            {
                bibRecord.DataWarning = true;
            }

            return foundBibRecords;
        }

        /// <summary>
        /// Attempts to find strict matches in bib data. Returns a list of FlaggedBibRecordModel with DataWarnings set as appropriate.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>List of FlaggedBibRecordModel instances</returns>
        public HashSet<FlaggedBibRecordModel> GetStrictMatches(string[] lines)
        {
            if (lines is null || lines.Length < 1)
            {
                return new HashSet<FlaggedBibRecordModel>();
            }

            return GetBibMatches(lines, strictBibPatternTabDelim);
        }

        /// <summary>
        /// Identifies data matches in array of potential bib records usinga RegEx pattern.
        /// Data Warning flags could be set on any FlaggedBibRecordModel that cannot be fully parsed.
        /// </summary>
        /// <param name="emptyBibList"></param>
        /// <param name="fileDataLines"></param>
        /// <param name="pattern"></param>
        /// <returns>true if any matches are found, false in any other case.</returns>
        public HashSet<FlaggedBibRecordModel> GetBibMatches(string[] fileDataLines, string pattern)
        {
            HashSet<FlaggedBibRecordModel> emptyBibList = new();

            if (
                fileDataLines is null || fileDataLines.Length < 1
                || string.IsNullOrWhiteSpace(pattern)
                )
            {
                return emptyBibList;
            }
            else
            {
                foreach (var line in fileDataLines)
                {
                    try
                    {
                        if (Regex.IsMatch(line, pattern, LocalRegexOptions, LocalRegexTimeout))
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
                        _logger.LogError("FileProcessor GetBibMatches: RegEx operation could not match {pattern} to {line}!", pattern, line);
                        _logger.LogError("FileProcessor GetBibMatches: Exception message is {exMessage}.", ex.Message);
                        _logger.LogError("FileProcessor GetBibMatches: Operations will continue but an audit should be performed.");
                    }
                }

                return emptyBibList;
            }
        }
    }
}
