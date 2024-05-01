using BFBMX.Service.Models;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.RegularExpressions;

namespace BFBMX.Service.Helpers
{
    public class FileProcessor : IFileProcessor
    {
        private static readonly object _lock = new();
        private readonly ILogger<FileProcessor> _logger;

        private readonly string DateTimeStampPattern = @"\d{1,2}\s\w{3}\s\d{4}\s\d{1,2}:\d{1,2}:\d{1,2}\s\+\d{4}"; 
        private readonly string MessageIdPattern = @"\bMessage-ID\S\s?(?'msgid'.{12})\b";
        private readonly string CommaDelimitedPattern = @"\b\d{1,3}(?:\s?,\s?)(?:IN|OUT|DROP)(?:\s?,\s?)\d{1,4}(?:\s?,\s?)\d{1,2}(?:\s?,\s?)\w{2}\b";
        private readonly string TabDelimitedPattern = @"\b\d{1,3}(?:\s?\t\s?)(?:IN|OUT|DROP)(?:\s?\t\s?)\d{1,4}(?:\s?\t\s?)\d{1,2}(?:\s?\t\s?)\w{2}\b";
        private readonly string SloppyBibPattern = @"\b\w{1,26}(?:\s*[,|\t]\s*\w{1,26}){4}\b"; // match 5 words with length 1-26 characters, separated by commas or tabs padded with 0 or more spaces
        private static RegexOptions LocalRegexOptions => RegexOptions.IgnoreCase;
        private static TimeSpan LocalRegexTimeout => new(0, 0, 1); // after timeout Regex pattern match will stop to fend against mischeveous input

        public FileProcessor(ILogger<FileProcessor> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Writes a non-null Winlink Message instance and its Flagged Bib Records to a file at filepath.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="filepath"></param>
        /// <returns>True if succeeds writing to file, otherwise False</returns>
        public bool WriteWinlinkMessageToFile(WinlinkMessageModel msg, string filepath)
        {
            if (msg.BibRecords.Count < 1)
            {
                _logger.LogInformation("FileProcessor: WriteWinlinkMessageToFile: No bibs found in message ID {msgid}.", msg.WinlinkMessageId);
                return false;
            }
            if (string.IsNullOrWhiteSpace(filepath))
            {
                _logger.LogError("FileProcessor: WriteWinlinkMessageToFile: No filepath provided by the caller!");
                return false;
            }

            string bibData = msg.ToAccessDatabaseTabbedString();

            try
            {
                lock (_lock)
                {
#pragma warning disable IDE0063 // Use simple 'using' statement
                    using (StreamWriter file = File.AppendText(filepath))
                    {
                        file.WriteLine(bibData);
                    }
#pragma warning restore IDE0063 // Use simple 'using' statement
                }

                return true;
            }
            catch (Exception)
            {
                _logger.LogWarning("FileProcessor: WriteWinlinkMessageToFile: Could not write to {filepath}.", filepath);
            }

            return false;
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
                Regex match = new(MessageIdPattern, LocalRegexOptions, LocalRegexTimeout);
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
                Regex match = new(DateTimeStampPattern, LocalRegexOptions, LocalRegexTimeout);
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
                case (0, 0): // no matches
                    _logger.LogInformation("ProcessBibs: Neither strict nor relaxed matches found in Message ID {msgId}. Returning 0 items.", messageId);
                    break;
                case (0, > 0): // only sloppy matches
                    bibRecords = sloppyMatches.ToList();
                    _logger.LogInformation("ProcessBibs: Found no strict bib records but {sloppyCount} records matched in Message ID {msgId}. Returning {sloppyCount} items.", sloppyCount, messageId, sloppyCount);
                    break;
                case (> 0, 0): // only strict matches
                    bibRecords = strictMatches.ToList();
                    _logger.LogInformation("ProcessBibs: Found {strictCount} strict matches but no relaxed matches in Message ID {msgId}. Returning {msgCount} items.", strictCount, messageId, strictCount);
                    break;
                case (> 0, > 0): // both strict and sloppy matches, will maintain strict and add only unique sloppy matches
                    strictMatches.UnionWith(sloppyMatches);
                    bibRecords = strictMatches.ToList();
                    _logger.LogInformation("ProcessBibs: Found {strictCount} strict and {sloppyCOunt} relaxed matches in Message ID {msgId}. Returning {unionCount} items.", strictCount, sloppyCount, messageId, bibRecords.Count);
                    break;
                default:
                    _logger.LogError("ProcessBibs: An unexpected error occurred while trying to process bib records in Message ID {msgId}. Returning 0 items.", messageId);
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
            HashSet<FlaggedBibRecordModel> foundBibRecords = new();

            if (lines is null || lines.Length < 1)
            {
                return foundBibRecords;
            }

            foundBibRecords = GetBibMatches(lines, SloppyBibPattern, true);
            return foundBibRecords;
        }

        /// <summary>
        /// Attempts to find strict matches in bib data. Returns a list of FlaggedBibRecordModel with DataWarnings set as appropriate.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns>List of FlaggedBibRecordModel instances</returns>
        public HashSet<FlaggedBibRecordModel> GetStrictMatches(string[] lines)
        {
            HashSet<FlaggedBibRecordModel> foundBibRecords = new();

            if (lines is null || lines.Length < 1)
            {
                return foundBibRecords;
            }

            //foundBibRecords = GetBibMatches(lines, StrictBibPattern, false);
            HashSet<FlaggedBibRecordModel> csvMatches = GetBibMatches(lines, CommaDelimitedPattern, false);
            foundBibRecords = GetBibMatches(lines, TabDelimitedPattern, false);
            foundBibRecords.UnionWith(csvMatches);
            return foundBibRecords;
        }

        /// <summary>
        /// Identifies data matches in an array of potential bib records using a 
        /// strict RegEx pattern that follows the BibFoot Bib Report Form format.
        /// </summary>
        /// <param name="fileDataLines"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public HashSet<FlaggedBibRecordModel> GetBibMatches(string[] fileDataLines,
                                                               string pattern,
                                                               bool setWarning = false)
        {
            HashSet<FlaggedBibRecordModel> resultBibList = new();

            if (fileDataLines is null || fileDataLines.Length < 1 || string.IsNullOrWhiteSpace(pattern))
            {
                return resultBibList;
            }
            else
            {
                Regex regex = new(pattern, LocalRegexOptions, LocalRegexTimeout);

                foreach (var line in fileDataLines)
                {
                    var fields = line.Split('\t', ',');

                    if (fields.Length == 5)
                    {
                        try
                        {
                            if (regex.IsMatch(line))

                            {
                                FlaggedBibRecordModel bibRecord = FlaggedBibRecordModel.GetBibRecordInstance(fields);

                                if (bibRecord is not null)
                                {
                                    if (setWarning)
                                    {
                                        bibRecord.DataWarning = true;
                                    }

                                    resultBibList.Add(bibRecord);
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
                }

                return resultBibList;
            }
        }
    }
}
