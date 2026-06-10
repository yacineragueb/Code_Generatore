namespace Code_Generatore.BusinessLayer.Exceptions
{
    public class DatabaseConnectionException : Exception
    {
        public DatabaseConnectionException(string message, Exception? inner = null) : base(message, inner)
        {

        }
    }
}
