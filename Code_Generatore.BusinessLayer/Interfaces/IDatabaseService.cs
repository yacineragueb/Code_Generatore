using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code_Generatore.BusinessLayer.Interfaces
{
    public interface IDatabaseService
    {
        ConnectionSession Login(string username, string password);
        List<string> GetAllDatabases(ConnectionSession session);
    }
}
