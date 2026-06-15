
using Code_Generatore.AccessDataLayer;

namespace Code_Generatore.BusinessLayer.Interfaces
{
    public interface IDatabaseService
    {
        Task<ConnectionSession> LoginAsync(string username, string password);
        Task<List<string>> GetAllDatabasesAsync(ConnectionSession session);
        Task<List<string>> GetAllTablesAsync(ConnectionSession session, string databaseName, CancellationToken ct);
        List<ColumnInfo> GetTableColumns(ConnectionSession session, string databaseName, string tableName);
    }
}
