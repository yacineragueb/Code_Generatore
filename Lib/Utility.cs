using System.IO;
using System.IO.IsolatedStorage;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Code_Generatore.Lib
{
    public static class Utility
    {
        private static readonly string credentialsFile = "credentials.dat";

        public static void SaveCredentials(string username, string password)
        {
            // Encrypt password before saving
            byte[] encrypted = ProtectedData.Protect(Encoding.UTF8.GetBytes(password), null, DataProtectionScope.CurrentUser);

            using var store = IsolatedStorageFile.GetUserStoreForApplication();
            using var stream = new IsolatedStorageFileStream(credentialsFile, FileMode.Create, store);

            using var writer = new BinaryWriter(stream);

            writer.Write(username);
            writer.Write(encrypted.Length);
            writer.Write(encrypted);
        }

        public static (string Username, string Password)? LoadCredentials()
        {
            try
            {
                using var store = IsolatedStorageFile.GetUserStoreForApplication();

                if(!store.FileExists(credentialsFile)) 
                    return null;

                using var stream = new IsolatedStorageFileStream(credentialsFile, FileMode.Open, store);
                using var reader = new BinaryReader(stream);
                
                string username = reader.ReadString();
                int length = reader.ReadInt32();
                byte[] encrypted = reader.ReadBytes(length);

                // Decrypt password
                byte[] decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);

                return (username, Encoding.UTF8.GetString(decrypted));

            } catch 
            {
                return null;
            }
        }

        public static void ClearCredentials()
        {
            using var store = IsolatedStorageFile.GetUserStoreForApplication();

            if(store.FileExists(credentialsFile))
            {
                store.DeleteFile(credentialsFile);
            }
        }
    }
}
