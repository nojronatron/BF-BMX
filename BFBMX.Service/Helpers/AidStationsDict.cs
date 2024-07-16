using System.Text;

namespace BFBMX.Service.Helpers
{
    public class AidStationsDict : IAidStationsDict
    {
        private Dictionary<string, string> _aidStations = new();

        public AidStationsDict()
        {
            _aidStations.Add("MM", "Marble Mountain");
            _aidStations.Add("BL", "Blue Lake");
            _aidStations.Add("AC", "Ape Canyon");
            _aidStations.Add("WR", "Windy Ridge");
            _aidStations.Add("JR", "Johnston Ridge");
            _aidStations.Add("CW", "Coldwater Lake");
            _aidStations.Add("NP", "Norway Pass");
            _aidStations.Add("EP", "Elk Pass");
            _aidStations.Add("WM", "Wright Meadow (Rd.9327");
            _aidStations.Add("SB", "Spencer Butte");
            _aidStations.Add("LR", "Lewis River");
            _aidStations.Add("CB", "Council Bluff");
            _aidStations.Add("QR", "Quartz Ridge");
            _aidStations.Add("CH", "Chain of Lakes");
            _aidStations.Add("KT", "Klikitat");
            _aidStations.Add("TS", "Twin Sisters");
            _aidStations.Add("OC", "Owen's Creek");
            _aidStations.Add("FN", "Finish (Bigfoot Base)");
        }

        /// <summary>
        /// Accepts Aid Station abbreviation and returns the long-form name.
        /// </summary>
        /// <param name="aidStationCode"></param>
        /// <returns>Expanded Aid Station name, or "Unknown" if not in dictionary.</returns>
        public string GetAidStationName(string aidStationCode)
        {
            if (_aidStations.ContainsKey(aidStationCode))
            {
                return _aidStations[aidStationCode];
            }
            else
            {
                return "Unknown";
            }
        }

        /// <summary>
        /// Accepts Aid Station long-form name and returns the abbreviation.
        /// </summary>
        /// <param name="aidStationName"></param>
        /// <returns>Abbreviation for aidStationName, or null if not in dictionary.</returns>
        public string? GetAidStationCode(string aidStationName)
        {
            return _aidStations.FirstOrDefault(x => x.Value == aidStationName).Key;
        }

        /// <summary>
        /// Returns all Aid Station names and abbreviations.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAll()
        {
            List<string> result = new();

            foreach(var kvp in _aidStations)
            {
                result.Add($"{kvp.Key}: {kvp.Value}");
            }

            return result;
        }
    }
}
