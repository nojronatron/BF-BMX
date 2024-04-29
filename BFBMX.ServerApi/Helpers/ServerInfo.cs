using System.Net.Sockets;
using System.Net;

namespace BFBMX.ServerApi.Helpers
{
    public class ServerInfo : IServerInfo
    {
        private readonly ILogger<ServerInfo> _logger;
        private readonly IServerEnvFactory _serverEnvFactory;
        private DateTime _executionTime;
        private TimeSpan _minWaitTime = new(0, 15, 0); // hh, mm, ss

        public ServerInfo(ILogger<ServerInfo> logger, IServerEnvFactory serverEnvFactory)
        {
            _logger = logger;
            _serverEnvFactory = serverEnvFactory;
            _executionTime = DateTime.Now;
        }

        /// <summary>
        /// Returns true if the minimum wait time has passed since the last execution.
        /// </summary>
        /// <returns>True if minimum wait time has passed since last exectuion.</returns>
        public bool CanStart()
        {
            return DateTime.Now - _executionTime > _minWaitTime;
        }

        /// <summary>
        /// Logs configured Hostname, IP address(es), and HTTP listen port to the console.
        /// </summary>
        public void StartHostInfo()
        {
            _executionTime = DateTime.Now;
            string serverPort = _serverEnvFactory.GetServerPort();
            IPHostEntry hostEntry = _serverEnvFactory.GetServerHostname();
            string machineName = hostEntry.HostName;
            string ipV4Addresses = string.Empty;

            foreach (IPAddress ipAddr in hostEntry.AddressList)
            {
                if (!ipAddr.AddressFamily.ToString().Equals(ProtocolFamily.InterNetworkV6.ToString()))
                {
                    if (ipV4Addresses.Length > 0)
                    {
                        ipV4Addresses += ", ";
                    }

                    ipV4Addresses += ipAddr.ToString();
                }
            }

            _logger.LogInformation("Server name {machinename} at address(es) {ipv4addresses} listening on HTTP port {serverport}", machineName, ipV4Addresses, serverPort);
        }

        /// <summary>
        /// Logs configured location of server log files to the console.
        /// </summary>
        public void StartLogfileInfo()
        {
            _executionTime = DateTime.Now;
            string configuredLogFilepath = _serverEnvFactory.GetServerLogPath();
            _logger.LogInformation("Logfiles are at {configuredlogfilepath}", configuredLogFilepath);
        }
    }
}
