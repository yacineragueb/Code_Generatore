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

        public static async Task<GeneratedProjectInfo?> GenerateProjectAsync(string projectName, string projectFolderPath, enProjectType projectType, GenerationOptions options)
        {
            string template = projectType switch
            {
                enProjectType.WINDOWS_PRESENTATION_FOUNDATION => "wpf",
                enProjectType.WINDOWS_FORMS => "winforms",
                _ => throw new ArgumentOutOfRangeException(nameof(projectType))
            };

            return await GenerateFromTemplateAsync(projectName, projectFolderPath, template, options);
        }

        private static async Task<GeneratedProjectInfo?> GenerateFromTemplateAsync(string projectName, string projectFolderPath, string template, GenerationOptions options)
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

                // 2. Create Presentation project
                await Utility.RunCommandAsync("dotnet", $"new {template} -n {presentationProjectName}", projectFolderPath);

                // 3. Add Presentation project to solution
                await Utility.RunCommandAsync("dotnet", $"sln {solutionName}.sln add \"{presentationProjectName}\\{presentationProjectName}.csproj\"", projectFolderPath);

                bool HasBLLFlag = options.Layers.HasFlag(GenerationOptions.enGeneratedLayers.BLL);

                // 4. Create BLL project
                if (HasBLLFlag)
                {
                    // 4.1 Create a Class Library project for Bussiness Logic Layer if selected
                    await Utility.RunCommandAsync("dotnet", $"new classlib -n {BLLProjectName}", projectFolderPath);

                    // 4.2 Add it to solution
                    await Utility.RunCommandAsync("dotnet", $"sln {solutionName}.sln add \"{BLLProjectName}\\{BLLProjectName}.csproj\"", projectFolderPath);

                    // 4.3 Add BLL reference to Presentation project
                    await Utility.RunCommandAsync("dotnet", $"add \"{presentationProjectName}\\{presentationProjectName}.csproj\" reference \"{BLLProjectName}\\{BLLProjectName}.csproj\"", projectFolderPath);
                }

                // 5. Create DAL project
                if (options.Layers.HasFlag(GenerationOptions.enGeneratedLayers.DAL))
                {
                    // 5.1 Create a Class Library project for Data Access Layer if selected
                    await Utility.RunCommandAsync("dotnet", $"new classlib -n {DALProjectName}", projectFolderPath);

                    // 5.2 Add it to solution
                    await Utility.RunCommandAsync("dotnet", $"sln {solutionName}.sln add \"{DALProjectName}\\{DALProjectName}.csproj\"", projectFolderPath);

                    // 5.3 Add DAL reference to BLL project if and only if BLL project exist
                    if(HasBLLFlag)
                        await Utility.RunCommandAsync("dotnet", $"add \"{BLLProjectName}\\{BLLProjectName}.csproj\" reference \"{DALProjectName}\\{DALProjectName}.csproj\"", projectFolderPath);

                    // 5.4 Install required NuGet packages in the DAL project
                    await Utility.RunCommandAsync("dotnet", $"add \"{DALProjectName}/{DALProjectName}.csproj\" package Microsoft.Data.SqlClient", projectFolderPath);
                    await Utility.RunCommandAsync("dotnet", $"add \"{DALProjectName}/{DALProjectName}.csproj\" package Microsoft.Extensions.Configuration.Json", projectFolderPath);
                }

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