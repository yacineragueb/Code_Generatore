using Code_Generatore.AccessDataLayer;
using Code_Generatore.BusinessLayer.Exceptions;
using Code_Generatore.BusinessLayer.Interfaces;
using Microsoft.Data.SqlClient;

namespace Code_Generatore.BusinessLayer
{
    public class DatabaseService : IDatabaseService
    {
        private const string _server = "localhost";

        private static string BuildConnectionString(string server, string username, string password, string database = "master")
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = server,
                UserID = username,
                Password = password,
                InitialCatalog = database,
                TrustServerCertificate = true
            };

            return builder.ConnectionString;
        }

        public async Task<ConnectionSession> LoginAsync(string username, string password)
        {
            string connectionString = BuildConnectionString(_server, username, password);

            bool ok = await DatabaseServiceData.TestConnectionAsync(connectionString);

            if (!ok)
                throw new DatabaseConnectionException($"Could not connect to '{_server}'. Check credentials and server availability.");

            return new ConnectionSession(_server, username, connectionString, isConnected: true);
        }

        public async Task<List<string>> GetAllDatabasesAsync(ConnectionSession session)
        {
            return await DatabaseServiceData.GetDatabasesAsync(session.ConnectionString);
        }
    
        public async Task<List<string>> GetAllTablesAsync(ConnectionSession session, string databaseName, CancellationToken ct)
        {
            return await DatabaseServiceData.GetAllTablesAsync(session.ConnectionString, databaseName, ct);
        }
    
        public List<ColumnInfo> GetTableColumns(ConnectionSession session, string databaseName, string tableName)
        {
            return DatabaseServiceData.GetTableColumns(session.ConnectionString, databaseName, tableName);
        }
    }
}
