using System.Net.Sockets;
using System.Net;

namespace BFBMX.ServerApi.Helpers
{
    public class ServerInfo : IServerInfo
    {
        private readonly ILogger<ServerInfo> _logger;
        private readonly IServerEnvFactory _serverEnvFactory;
        private DateTime _executionTime;
        private TimeSpan _offset = new(0, 10, 0); // 10 minutes

        public ServerInfo(ILogger<ServerInfo> logger, IServerEnvFactory serverEnvFactory)
        {
            _logger = logger;
            _serverEnvFactory = serverEnvFactory;
            _executionTime = DateTime.Now;
        }

        public bool CanStart()
        {
            return DateTime.Now - _executionTime > _offset;
        }

        public void Start()
        {
            _executionTime = DateTime.Now;

                // get current hostname, ipaddress, and configured port
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
    }
}
