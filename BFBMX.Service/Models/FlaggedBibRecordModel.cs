﻿using System.Text.Json;

namespace BFBMX.Service.Models
{
    public class FlaggedBibRecordModel
    {
        public int BibNumber { get; set; } = -1; // max len 15
        public string? @Action { get; set; } = "MISSING"; // IN, OUT, DROP
        public string? BibTimeOfDay { get; set; } // format: HHMM
        public int DayOfMonth { get; set; } = -1; // digit count range(1-31)
        public string? Location { get; set; } = "MISSING"; // max len 26 (as defined by select element in form)
        public bool DataWarning { get; set; } = true;

        public string ToTabbedString()
        {
            // optionally display (tab)Warning if true, otherwise nothing
            string dwText = DataWarning ? "\tWarning" : string.Empty;
            return $"{BibNumber}\t{Action}\t{BibTimeOfDay}\t{DayOfMonth}\t{Location}{dwText}";
        }

        public string ToJsonString()
        {
            // serialize to json
            return JsonSerializer.Serialize<FlaggedBibRecordModel>(this);
        }
    }
}