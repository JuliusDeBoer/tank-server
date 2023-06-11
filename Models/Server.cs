namespace Models
{
    // This serves no purpose. Its fun though
    public class ServerInfo
    {
        private static ServerInfo? _serverInfo;
        public string Version { get; } = "1.0";

        public static ServerInfo GetInfo()
        {
            _serverInfo = new ServerInfo();
            return (ServerInfo)_serverInfo;
        }
    }
}
