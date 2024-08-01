using System.Net;

namespace BFBMX.Service.Helpers
{
    public class ReportServerEnvFactory : IReportServerEnvFactory
    {
        public string GetApiServerPort()
        {
            return Environment.GetEnvironmentVariable("BFBMX_SERVER_PORT") ?? "5150";
        }

        public string GetApiServerHostname()
        {
            string? bfBmxServerName = Environment.GetEnvironmentVariable("BFBMX_SERVER_NAME");
            
            if (bfBmxServerName is null)
            {
                return Dns.GetHostName();
            }

            return bfBmxServerName.Trim();
        }

        public int[] GetBigfootBibRange()
        {
            string? bfBmxBigfootBibRange = Environment.GetEnvironmentVariable("BFBMX_BIGFOOT_BIBRANGE");
            int defaultLowest = 1;
            int defaultHighest = 299;

            if (string.IsNullOrWhiteSpace(bfBmxBigfootBibRange))
            {
                bfBmxBigfootBibRange = $"{defaultLowest},{defaultHighest}";
            }

            string[] strRange = bfBmxBigfootBibRange.Split(',');
            int[] resultRange = new int[2];

            if (int.TryParse(strRange[0], out int firstNum))
            {
                resultRange[0] = firstNum >= defaultLowest && firstNum < int.MaxValue
                    ? firstNum
                    : defaultLowest;
            }
            else
            {
                resultRange[0] = 1;
            }

            if (int.TryParse(strRange[1], out int secondNum))
            {
                resultRange[1] = secondNum >= defaultLowest && secondNum > firstNum && secondNum < int.MaxValue
                    ? secondNum
                    : defaultHighest;
            }

            return resultRange;
        }

        public int[] GetLittlefoot40BibRange()
        {
            string? bfBmxBibRange = Environment.GetEnvironmentVariable("BFBMX_40M_BIBRANGE");
            int defaultLowest = 300;
            int defaultHighest = 599;

            if (string.IsNullOrWhiteSpace(bfBmxBibRange))
            {
                bfBmxBibRange = $"{defaultLowest},{defaultHighest}";
            }

            string[] strRange = bfBmxBibRange.Split(',');
            int[] resultRange = new int[2];

            if (int.TryParse(strRange[0], out int firstNum))
            {
                resultRange[0] = firstNum >= 0 && firstNum < int.MaxValue
                    ? firstNum
                    : defaultLowest;
            }
            else
            {
                resultRange[0] = defaultHighest;
            }

            if (int.TryParse(strRange[1], out int secondNum))
            {
                resultRange[1] = secondNum >= 0 && secondNum > firstNum && secondNum < int.MaxValue
                    ? secondNum
                    : defaultHighest;
            }

            return resultRange;
        }

        public int[] GetLittlefoot20BibRange()
        {
            string? bfBmxBibRange = Environment.GetEnvironmentVariable("BFBMX_20M_BIBRANGE");
            int defaultLowest = 600;
            int defaultHighest = 999;

            if (string.IsNullOrWhiteSpace(bfBmxBibRange))
            {
                bfBmxBibRange = $"{defaultLowest},{defaultHighest}";
            }

            string[] strRange = bfBmxBibRange.Split(',');
            int[] resultRange = new int[2];

            if (int.TryParse(strRange[0], out int firstNum))
            {
                resultRange[0] = firstNum >= 0 && firstNum < int.MaxValue
                    ? firstNum
                    : defaultLowest;
            }
            else
            {
                resultRange[0] = defaultHighest;
            }

            if (int.TryParse(strRange[1], out int secondNum))
            {
                resultRange[1] = secondNum >= 0 && secondNum > firstNum && secondNum < int.MaxValue
                    ? secondNum
                    : defaultHighest;
            }

            return resultRange;
        }
    }
}
