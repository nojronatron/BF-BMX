using BFBMX.Service.Models;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BFBMX.Service.Helpers
{
    public interface IFileReader
    {
        bool Exists(string path);
        string[] ReadAllLines(string path);
    }

    public class FileReader : IFileReader
    {
        public bool Exists(string path) => File.Exists(path);
        public string[] ReadAllLines(string path) => File.ReadAllLines(path);
    }

    public class FileProcessor
    {
        private static readonly string messageIdPattern = @"\bMessage-ID\S\s?(?'msgid'.{12})\b";
        private static readonly string strictBibPattern = @"\b\d{1,3}\t(OUT|IN|DROP)\t\d{4}\t\d{1,2}\t\w{2}\b";
        private static readonly string sloppyBibPattern = @"\b\w{1,15}\t\w{1,5}\t\w{1,5}\t\w{1,3}\t\w{1,26}\b";
        
        /// <summary>
        /// Resuable wrapper method to get file data without throwing an IO and other Exceptions.
        /// </summary>
        /// <param name="fullFilePath"></param>
        /// <returns></returns>
        public static string[] GetFileData(string fullFilePath)
        {
            if (!string.IsNullOrWhiteSpace(fullFilePath) && File.Exists(fullFilePath))
            {

                try
                {
                    string[] lines = File.ReadAllLines(fullFilePath);
                    return lines;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Unable to read data from {fullFilePath}: {ex.Message}");
                }
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Finds the first instance of a Winlink message Message-ID.
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns>Message-ID value or an empty string if none found.</returns>
        public static string GetMessageId(string fileData)
        {
            if (string.IsNullOrWhiteSpace(fileData))
            {
                return string.Empty;
            }

            Regex match = new(messageIdPattern, RegexOptions.IgnoreCase, new TimeSpan(0, 0,  2));
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
        public static bool ProcessBibs(List<FlaggedBibRecordModel> bibRecords, string[] lines)
        {
            if (lines is null || lines.Length < 1)
            {
                Debug.WriteLine($"Input {nameof(lines)}. Returning and empty list.");
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
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to find potential matches in bib data. Returns a list of FlaggedBibRecordModel with DataWarnings set as appropriate.
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        public static List<FlaggedBibRecordModel> GetSloppyMatches(string[] lines)
        {
            var sloppyBibRecords = new List<FlaggedBibRecordModel>();

            if (lines is null || lines.Length < 1)
            {
                Debug.WriteLine($"GetSloppyMatches input {nameof(lines)} was empty. Returning an empty list.");
                return sloppyBibRecords;
            }

            bool result = GetBibMatches(sloppyBibRecords, lines, sloppyBibPattern);
            string didOrNotFind = result ? "found" : "did not find";
            Debug.WriteLine($"GetSloppyMatches {didOrNotFind} bib data.");
            return sloppyBibRecords;
        }

        public static List<FlaggedBibRecordModel> GetStrictMatches(string[] lines)
        {
            var strictBibRecords = new List<FlaggedBibRecordModel>();

            if (lines is null || lines.Length < 1)
            {
                Debug.WriteLine($"GetStrictMatches input {nameof(lines)} was empty. Returning and empty list.");
                return strictBibRecords;
            }

            bool result = GetBibMatches(strictBibRecords, lines, strictBibPattern);
            string didOrNotFind = result ? "found" : "did not find";
            Debug.WriteLine($"GetStrictMatches {didOrNotFind} bib data.");
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
        public static bool GetBibMatches(List<FlaggedBibRecordModel> emptyBibList,
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
                        Debug.WriteLine($"GetBibMatches RegEx operation could not match \"{pattern}\" to \"{line}\"!" +
                            $"Data should be audited carefully! Exception message: {ex.Message}." +
                            $"FileProcessor operations will continue.");
                    }
                }

                return emptyBibList.Count > 0;
            }
        }
    }
}
