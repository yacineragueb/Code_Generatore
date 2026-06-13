using Code_Generatore.Lib;

namespace Code_Generatore.BusinessLayer
{
    public class ProjectGeneratore
    {
        private const string SUFFIX_OF_SOLUTION = ".Solution";
        private const string SUFFIX_OF_BUSINESS_LOGIC_LAYER = ".BusinessLogicLayer";
        private const string SUFFIX_OF_DATA_ACCESS_LAYER = ".DataAccessLayer";

        public enum enProjectType
        {
            WINDOWS_PRESENTATION_FOUNDATION = 0,
            WINDOWS_FORMS = 1,
        }

        public static async Task<GeneratedProjectInfo?> GenerateProjectAsync(string projectName, string projectFolderPath, enProjectType projectType)
        {
            string template = projectType switch
            {
                enProjectType.WINDOWS_PRESENTATION_FOUNDATION => "wpf",
                enProjectType.WINDOWS_FORMS => "winforms",
                _ => throw new ArgumentOutOfRangeException(nameof(projectType))
            };

            return await GenerateFromTemplateAsync(projectName, projectFolderPath, template);
        }

        private static async Task<GeneratedProjectInfo?> GenerateFromTemplateAsync(string projectName, string projectFolderPath, string template)
        {
            try
            {
                if (!Utility.CreateFolderIfDoesNotExist(projectFolderPath))
                    return null;

                string solutionName = projectName + SUFFIX_OF_SOLUTION;

                string presentationProjectName = projectName;
                string BLLProjectName = projectName + SUFFIX_OF_BUSINESS_LOGIC_LAYER;
                string DALProjectName = projectName + SUFFIX_OF_DATA_ACCESS_LAYER;

                // 1. Create solution
                await Utility.RunCommandAsync("dotnet", $"new sln -n {solutionName}", projectFolderPath);

                // 2. Create Presentation + BLL + DAL
                await Task.WhenAll(
                    // 2.1 Create Presentation project
                    Utility.RunCommandAsync("dotnet", $"new {template} -n {projectName}", projectFolderPath),

                    //2.2 Create a Class Library project for Bussiness Logic Layer
                    Utility.RunCommandAsync("dotnet", $"new classlib -n {BLLProjectName}", projectFolderPath),

                    //2.3 Create a Class Library project for Data Access Layer
                    Utility.RunCommandAsync("dotnet", $"new classlib -n {DALProjectName}", projectFolderPath)
                );

                // 3. Add Presentation project to solution
                await Utility.RunCommandAsync("dotnet", $"sln {solutionName}.sln add \"{projectName}\\{projectName}.csproj\"", projectFolderPath);

                //4. Add DAL & BLL projects to solution
                await Utility.RunCommandAsync("dotnet", $"sln {solutionName}.sln add \"{BLLProjectName}\\{BLLProjectName}.csproj\"", projectFolderPath);
                await Utility.RunCommandAsync("dotnet", $"sln {solutionName}.sln add \"{DALProjectName}\\{DALProjectName}.csproj\"", projectFolderPath);

                // 5. Add BLL reference to Presentation project
                await Utility.RunCommandAsync("dotnet", $"add \"{presentationProjectName}\\{presentationProjectName}.csproj\" reference \"{BLLProjectName}\\{BLLProjectName}.csproj\"", projectFolderPath);

                // 6. Add DAL reference to BLL project
                await Utility.RunCommandAsync("dotnet", $"add \"{BLLProjectName}\\{BLLProjectName}.csproj\" reference \"{DALProjectName}\\{DALProjectName}.csproj\"", projectFolderPath);

                // TODO: Install required NuGet packages in the DAL project (Microsoft.Data.SqlClient, Microsoft.Extensions.Configuration.Json)
                // These operations are slow (+5min each) because they hit NuGet servers and restore dependencies.
                // They should NOT run on the UI thread — move them to a background thread to keep the UI responsive.
                //
                // Implementation plan:
                //   1. Make GenerateFromTemplate async (Task<GeneratedProjectInfo?>)
                //   2. Run the two package installs via Task.Run() or move them to a separate method InstallDALPackagesAsync()
                //   3. Before starting: show a progress indicator (spinner or marquee ProgressBar) with a status label e.g. "Installing NuGet packages..."
                //   4. After both finish: hide the indicator and continue
                //
                // Commands to run (working directory = projectFolderPath):
                //   dotnet add "{DALProjectName}/{DALProjectName}.csproj" package Microsoft.Data.SqlClient
                //   dotnet add "{DALProjectName}/{DALProjectName}.csproj" package Microsoft.Extensions.Configuration.Json
                //
                // Note: Utility.RunCommand may need an async overload (RunCommandAsync) that returns Task
                //       so it can be properly awaited without blocking.

                string presentationPath = Path.Combine(projectFolderPath, presentationProjectName);
                string bllPath = Path.Combine(projectFolderPath, BLLProjectName);
                string dalPath = Path.Combine(projectFolderPath, DALProjectName);

                return new GeneratedProjectInfo(presentationPath, bllPath, dalPath); ;
            }
            catch
            {
                return null;
            }
        }
    }
}