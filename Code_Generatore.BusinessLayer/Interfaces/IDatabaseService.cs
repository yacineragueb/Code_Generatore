
namespace Code_Generatore.BusinessLayer.Interfaces
{
    public interface IDatabaseService
    {
        Task<ConnectionSession> LoginAsync(string username, string password);
        List<string> GetAllDatabases(ConnectionSession session);
    }
}
