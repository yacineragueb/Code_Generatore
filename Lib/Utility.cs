using CredentialManagement;

namespace Code_Generatore.Lib
{
    public static class Utility
    {
        private const string CredentialTarget = "Code_Generatore_App";

        public static bool AreCredentialsProvided(string username, string password)
        {
            return !string.IsNullOrWhiteSpace(username)
                && !string.IsNullOrWhiteSpace(password);
        }

        public static void SaveCredentials(string username, string password)
        {
            using Credential credential = new Credential
            {
                Target = CredentialTarget,
                Username = username,
                Password = password,
                Type = CredentialType.Generic,
                PersistanceType = PersistanceType.LocalComputer,
            };

            credential.Save();
        }

        public static (string Username, string Password)? LoadCredentials()
        {
            using Credential credential = new Credential 
            { 
                Target = CredentialTarget,
                Type = CredentialType.Generic
            };

            if (!credential.Load())
                return null;

            return (credential.Username, credential.Password);
        }

        public static void ClearCredentials()
        {
            using Credential credential = new Credential
            {
                Target = CredentialTarget,
                Type = CredentialType.Generic
            };

            credential.Delete();
        }
    }
}
