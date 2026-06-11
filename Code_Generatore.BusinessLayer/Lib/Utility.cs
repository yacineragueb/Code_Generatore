using CredentialManagement;
using System.Diagnostics;

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

        /// <summary>
        /// Checks whether a string is null, empty, or whitespace.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>
        /// <see langword="true"/> if the string is null, empty, or whitespace; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsEmpty(string? value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Creates a directory at the specified path if it does not already exist.
        /// </summary>
        /// <param name="folderPath">The full path of the folder to create.</param>
        /// <returns>
        /// Returns <see langword="true"/> if the folder already exists or was successfully created;
        /// otherwise, returns <see langword="false"/> if an exception occurred.
        /// </returns>
        /// <remarks>
        /// This method safely checks for the existence of the directory before attempting to create it,
        /// preventing unnecessary exceptions from duplicate creation attempts.
        /// </remarks>
        public static bool CreateFolderIfDoesNotExist(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                return true;
            } catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously writes the specified code content to a file.
        /// Creates the file if it does not exist; otherwise, overwrites the existing content.
        /// </summary>
        /// <param name="code">
        /// The source code or text content to write.
        /// </param>
        /// <param name="filePath">
        /// The path of the destination file.
        /// </param>
        /// <returns>
        /// A task that returns <see langword="true"/> if the operation completes successfully;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static async Task<bool> WriteCodeToFileAsync(string code, string filePath)
        {
            try
            {
                await File.WriteAllTextAsync(filePath, code);
                return true;
            }
            catch (Exception) 
            {
                return false;
            }
        }

        /// <summary>
        /// Executes an external command-line process and waits for it to complete.
        /// </summary>
        /// <param name="fileName">
        /// The executable or command to run (e.g., "dotnet", "cmd.exe").
        /// </param>
        /// <param name="arguments">
        /// The command-line arguments passed to the executable.
        /// </param>
        /// <param name="workingDirectory">
        /// The directory in which the process will be executed.
        /// </param>
        /// <remarks>
        /// The process is executed without creating a console window. Standard output
        /// and standard error streams are redirected. This method blocks the calling
        /// thread until the process exits.
        /// </remarks>
        /// <exception cref="InvalidOperationException" />
        public static void RunCommand(string fileName, string arguments, string workingDirectory)
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            try
            {
                process.Start();
                process.WaitForExit();
            } catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to execute command '{fileName} {arguments}'.", ex);
            }
        }
    }
}
