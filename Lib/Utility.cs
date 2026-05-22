using CredentialManagement;

namespace Code_Generatore.Lib
{
    public static class Utility
    {
        private const string CredentialTarget = "Code_Generatore_App";

        /// <summary>
        /// Determines whether both username and password are provided.
        /// </summary>
        /// <param name="username">
        /// The username entered by user.
        /// </param>
        /// <param name="password">
        /// The password entered by user.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if both username and password contain values;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool AreCredentialsProvided(string username, string password)
        {
            return !string.IsNullOrWhiteSpace(username)
                && !string.IsNullOrWhiteSpace(password);
        }

        /// <summary>
        /// Saves the user's credentials to the Windows Credential Manager.
        /// </summary>
        /// <param name="username">
        /// The username provided by the user.
        /// </param>
        /// <param name="password">
        /// The password provided by the user.
        /// </param>
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

        /// <summary>
        /// Retrieves the user's credentials from the Windows Credential Manager.
        /// </summary>
        /// <returns>
        /// A tuple containing the username and password if the credentials exist;
        /// otherwise, <see langword="null"/>.
        /// </returns>
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

        /// <summary>
        /// Removes the user's credentials from the Windows Credential Manager.
        /// </summary>
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
