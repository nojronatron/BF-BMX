using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BFBMX.Service.Models
{
    public class FlaggedBibRecordModel
    {
        [JsonIgnore]
        [Key]
        public int? Id { get; set; }
        public int BibNumber { get; set; } = -1; // max/min int value is [-] 2_147_483_647
        public string? @Action { get; set; } = "MISSING"; // IN, OUT, DROP
        public string? BibTimeOfDay { get; set; } // format: HHMM
        public int DayOfMonth { get; set; } = -1; // digit count range(1-31)
        public string? Location { get; set; } = "MISSING"; // max len 26 (as defined by select element in form)
        public bool DataWarning { get; set; } = true;

        /// <summary>
        /// Generate a new instance based on typed inputs. Caller will need to set or unset DataWarning as needed.
        /// </summary>
        /// <param name="bibNumber"></param>
        /// <param name="action"></param>
        /// <param name="bibTimeOfDay"></param>
        /// <param name="dayOfMonth"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        public static FlaggedBibRecordModel GetBibRecordInstance(int bibNumber,
                                                                 string? action,
                                                                 string? bibTimeOfDay,
                                                                 int dayOfMonth,
                                                                 string? location)
        {
            return new FlaggedBibRecordModel
            {
                BibNumber = bibNumber,
                Action = action,
                BibTimeOfDay = bibTimeOfDay,
                DayOfMonth = dayOfMonth,
                Location = location,
            };
        }

        /// <summary>
        /// Generate a new instance based on collection of strings and actively sets DataWarning bit if any field is not parsable.
        /// </summary>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static FlaggedBibRecordModel GetBibRecordInstance(string[] fields)
        {
            if (fields.Length < 5 )
            {
                // fewer than 5 fields probably means poorly validated data, return new new instance with DataWarning set.
                return new FlaggedBibRecordModel();
            }

            int bibNum = int.TryParse(fields[0], out bibNum) ? bibNum : -1;
            string action = string.IsNullOrWhiteSpace(fields[1]) ? "MISSING" : fields[1].Trim();
            string bibTimeOfDay = string.IsNullOrWhiteSpace(fields[2]) ? "MISSING" : fields[2].Trim();
            int dayOfMonth = int.TryParse(fields[3], out dayOfMonth) ? dayOfMonth : -1;
            string location = string.IsNullOrWhiteSpace(fields[4]) ? "MISSING" : fields[4].Trim();


            var result = FlaggedBibRecordModel.GetBibRecordInstance(bibNumber: bibNum,
                                                                    action: action,
                                                                    bibTimeOfDay: bibTimeOfDay,
                                                                    dayOfMonth: dayOfMonth,
                                                                    location: location);
            // actively set Data Warning if any of the fields are missing or could not be parsed
            result.DataWarning = action.Equals("MISSING") 
                                || bibTimeOfDay.Equals("MISSING") 
                                || dayOfMonth < 1 
                                || dayOfMonth > 31 
                                || location.Equals("MISSING");
            return result;
        }

        public string ToTabbedString()
        {
            // Set DataWarning as a string: ALERT if true, NOMINAL if false
            string dwText = DataWarning ? "ALERT" : "NOMINAL";
            return $"{dwText}\t{BibNumber}\t{Action}\t{BibTimeOfDay}\t{DayOfMonth}\t{Location}";
        }

        public string ToJsonString()
        {
            // serialize to json
            return JsonSerializer.Serialize<FlaggedBibRecordModel>(this);
        }

        /// <summary>
        /// Custom hash code implementation to support equality comparison.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + BibNumber.GetHashCode();
            hash = hash * 23 + (Action != null ? Action.GetHashCode() : 0);
            hash = hash * 23 + (BibTimeOfDay != null ? BibTimeOfDay.GetHashCode() : 0);
            hash = hash * 23 + DayOfMonth.GetHashCode();
            hash = hash * 23 + (Location != null ? Location.GetHashCode() : 0);
            return hash;
        }

        /// <summary>
        /// Custom equality comparison implementation to be used by HashSet and others.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj is FlaggedBibRecordModel other)
            {
                return BibNumber == other.BibNumber
                    && Action == other.Action
                    && BibTimeOfDay == other.BibTimeOfDay
                    && DayOfMonth == other.DayOfMonth
                    && Location == other.Location;
            }

            return false;
        }
    }
}
