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
        public string? BibNumber { get; set; }
        public string? @Action { get; set; } // IN, OUT, DROP
        public string? BibTimeOfDay { get; set; } // format: HHMM
        public string? DayOfMonth { get; set; } // digit count range(1-31)
        public string? Location { get; set; } // max len 26 (as defined by select element in form)
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
        public static FlaggedBibRecordModel GetBibRecordInstance(string? bibNumber,
                                                                 string? action,
                                                                 string? bibTimeOfDay,
                                                                 string? dayOfMonth,
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
        /// <returns>New FlaggedBibRecordModel instance</returns>
        public static FlaggedBibRecordModel GetBibRecordInstance(string[] fields)
        {
            if (fields.Length < 5 )
            {
                // fewer than 5 fields probably means poorly validated data, return new new instance with DataWarning set.
                return new FlaggedBibRecordModel();
            }

            string bibNum = string.IsNullOrWhiteSpace(fields[0]) ? string.Empty : fields[0].Trim();
            string action = string.IsNullOrWhiteSpace(fields[1]) ? string.Empty : fields[1].Trim();
            string bibTimeOfDay = string.IsNullOrWhiteSpace(fields[2]) ? string.Empty : fields[2].Trim();
            string dayOfMonth = string.IsNullOrWhiteSpace(fields[3]) ? string.Empty : fields[3].Trim();
            string location = string.IsNullOrWhiteSpace(fields[4]) ? string.Empty : fields[4].Trim();


            var result = FlaggedBibRecordModel.GetBibRecordInstance(bibNumber: bibNum,
                                                                    action: action,
                                                                    bibTimeOfDay: bibTimeOfDay,
                                                                    dayOfMonth: dayOfMonth,
                                                                    location: location);

            // actively set Data Warning if any of the fields are missing or can not be parsed
            result.DataWarning = string.IsNullOrWhiteSpace(bibNum) 
                                || string.IsNullOrWhiteSpace(action) 
                                || string.IsNullOrWhiteSpace(bibTimeOfDay) 
                                || string.IsNullOrWhiteSpace(dayOfMonth) 
                                || string.IsNullOrWhiteSpace(location);

            if (int.TryParse(bibNum, out int parsedBibNum))
            {
                result.DataWarning = parsedBibNum < 1 || parsedBibNum > int.MaxValue;
            }
            else
            {
                result.DataWarning = true;
            }

            if (int.TryParse(dayOfMonth, out int parsedDayOfMonth))
            {
                result.DataWarning = parsedDayOfMonth < 1 || parsedDayOfMonth > 31;
            }
            else
            {
                result.DataWarning = true;
            }
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
            hash = hash * 23 + (BibNumber is not null ? BibNumber.GetHashCode() : 0);
            hash = hash * 23 + (Action != null ? Action.GetHashCode() : 0);
            hash = hash * 23 + (BibTimeOfDay != null ? BibTimeOfDay.GetHashCode() : 0);
            hash = hash * 23 + (DayOfMonth is not null ? DayOfMonth.GetHashCode() : 0);
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
