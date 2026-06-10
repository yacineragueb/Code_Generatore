namespace Code_Generatore.BusinessLayer
{
    public class ConnectionSession
    {
        public string ServerName { get; set; }

        public string UserName { get; set; }

        public bool IsConnected { get; set; }

        public string ConnectionString { get; set; }

        public string ConnectionStatus => IsConnected ? "Connected" : "Disconnected";

        public ConnectionSession(string serverName, string userName, string connectionString, bool isConnected)
        {
            ServerName = serverName;
            UserName = userName;
            ConnectionString = connectionString;
            IsConnected = isConnected;
        }
    }
}
