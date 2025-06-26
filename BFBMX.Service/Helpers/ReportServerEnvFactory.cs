using System.Net;

namespace BFBMX.Service.Helpers
{
    public class ReportServerEnvFactory : IReportServerEnvFactory
    {
        private readonly string _defaultServerApiPort = "5150";

        private readonly string _serverApiPortEnvVar = "BFBMX_SERVER_PORT";
        private readonly string _serverApiNameEnvVar = "BFBMX_SERVER_NAME";
        private readonly string _bigfootBibRangeEnvVar = "BFBMX_BIGFOOT_BIBRANGE";
        private readonly string _littlefoot40BibRangeEnvVar = "BFBMX_40M_BIBRANGE";
        private readonly string _littlefoot20BibRangeEnvVar = "BFBMX_20M_BIBRANGE";

        private readonly char _defaultDelimiterChar = ',';

        private readonly int _bigfootDefaultLowest = 1;
        private readonly int _bigfootDefaultHighest = 299;
        private readonly int _littleFootDefaultMin = 300;
        private readonly int _littleFootDefaultMax = 599;
        private readonly int _littleFootSecondaryDefaultMin = 600;
        private readonly int _littleFootSecondaryDefaultMax = 999;

        public string GetApiServerPort()
        {
            return Environment.GetEnvironmentVariable(_serverApiPortEnvVar) ?? _defaultServerApiPort;
        }

        public string GetApiServerHostname()
        {
            string? bfBmxServerName = Environment.GetEnvironmentVariable(_serverApiNameEnvVar);

            if (bfBmxServerName is null)
            {
                return Dns.GetHostName();
            }

            return bfBmxServerName.Trim();
        }

        public int[] GetBigfootBibRange()
        {
            string? bfBmxBigfootBibRange = Environment.GetEnvironmentVariable(_bigfootBibRangeEnvVar);

            if (string.IsNullOrWhiteSpace(bfBmxBigfootBibRange))
            {
                bfBmxBigfootBibRange = $"{_bigfootDefaultLowest},{_bigfootDefaultHighest}";
            }

            string[] strRange = bfBmxBigfootBibRange.Split(_defaultDelimiterChar);
            int[] resultRange = new int[2];

            if (int.TryParse(strRange[0], out int firstNum))
            {
                resultRange[0] 
                    = firstNum >= _bigfootDefaultLowest 
                        && firstNum < int.MaxValue
                    ? firstNum
                    : _bigfootDefaultLowest;
            }
            else
            {
                resultRange[0] = _bigfootDefaultLowest;
            }

            if (int.TryParse(strRange[1], out int secondNum))
            {
                resultRange[1]
                    = secondNum >= _bigfootDefaultLowest 
                        && secondNum > firstNum 
                        && secondNum < int.MaxValue
                    ? secondNum
                    : _bigfootDefaultHighest;
            }

            return resultRange;
        }

        public int[] GetLittlefoot40BibRange()
        {
            string? bfBmxBibRange = Environment.GetEnvironmentVariable(_littlefoot40BibRangeEnvVar);

            if (string.IsNullOrWhiteSpace(bfBmxBibRange))
            {
                bfBmxBibRange = $"{_littleFootDefaultMin},{_littleFootDefaultMax}";
            }

            string[] strRange = bfBmxBibRange.Split(',');
            int[] resultRange = new int[2];

            if (int.TryParse(strRange[0], out int firstNum))
            {
                resultRange[0] = firstNum >= 0 && firstNum < int.MaxValue
                    ? firstNum
                    : _littleFootDefaultMin;
            }
            else
            {
                resultRange[0] = _littleFootDefaultMax;
            }

            if (int.TryParse(strRange[1], out int secondNum))
            {
                resultRange[1] 
                    = secondNum >= 0 
                        && secondNum > firstNum 
                        && secondNum < int.MaxValue
                    ? secondNum
                    : _littleFootDefaultMax;
            }

            return resultRange;
        }

        public int[] GetLittlefoot20BibRange()
        {
            string? bfBmxBibRange = Environment.GetEnvironmentVariable(_littlefoot20BibRangeEnvVar);

            if (string.IsNullOrWhiteSpace(bfBmxBibRange))
            {
                bfBmxBibRange = $"{_littleFootSecondaryDefaultMin},{_littleFootSecondaryDefaultMax}";
            }

            string[] strRange = bfBmxBibRange.Split(_defaultDelimiterChar);
            int[] resultRange = new int[2];

            if (int.TryParse(strRange[0], out int firstNum))
            {
                resultRange[0] 
                    = firstNum >= 0 
                        && firstNum < int.MaxValue
                    ? firstNum
                    : _littleFootSecondaryDefaultMin;
            }
            else
            {
                resultRange[0] = _littleFootSecondaryDefaultMax;
            }

            if (int.TryParse(strRange[1], out int secondNum))
            {
                resultRange[1] 
                    = secondNum >= 0 
                        && secondNum > firstNum 
                        && secondNum < int.MaxValue
                    ? secondNum
                    : _littleFootSecondaryDefaultMax;
            }

            return resultRange;
        }
    }
}
