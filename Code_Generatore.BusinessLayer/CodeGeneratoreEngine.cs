using Code_Generatore.AccessDataLayer;
using Code_Generatore.Lib;

namespace Code_Generatore.BusinessLayer
{
    public class CodeGeneratoreEngine
    {
        private readonly ConnectionSession _session;

        public string DatabaseName { get; set; }
        public string ProjectName { get; set; }
        public string ProjectFolderPath { get; set; }
        public List<TableItem> SelectedTables { get; set; }
        public ProjectGeneratore.enProjectType ProjectType { get; set; }
        public GenerationOptions GenerationOptions { get; set; }

        public CodeGeneratoreEngine(ConnectionSession session, string databaseName, string projectName, string projectFolderPath, ProjectGeneratore.enProjectType projectType, List<TableItem> selectedTables, GenerationOptions generationOptions)
        {
            _session = session;
            DatabaseName = databaseName;
            ProjectName = projectName;
            ProjectFolderPath = projectFolderPath;
            ProjectType = projectType;
            SelectedTables = selectedTables;
            GenerationOptions = generationOptions;
        }

        public async Task<bool> GenerateAsync()
        {
            GeneratedProjectInfo projectInfo = await ProjectGeneratore.GenerateProjectAsync(ProjectName, ProjectFolderPath, ProjectType, GenerationOptions);

            if (projectInfo == null)
            {
                return false;
            }

            await GenerateBLLAndDALFilesAsync(projectInfo);
            return true;
        }

        private async Task GenerateBLLAndDALFilesAsync(GeneratedProjectInfo projectInfo)
        {
            var dbService = new DatabaseService();

            // Fetch all columns in parallel
            var tasks = SelectedTables.Select(table => Task.Run(() => new
            {
                table.TableName,
                Columns = dbService.GetTableColumns(_session, DatabaseName, table.TableName)
            }));

            var results = await Task.WhenAll(tasks);

            var writeTask = results.Select(result =>
            {
                var tasks = new List<Task>();

                if (GenerationOptions.Layers.HasFlag(GenerationOptions.enGeneratedLayers.BLL))
                {
                    tasks.Add(GenerateBLLFileForTableAsync(result.TableName, result.Columns, projectInfo));
                }

                if(GenerationOptions.Layers.HasFlag(GenerationOptions.enGeneratedLayers.DAL))
                {
                    tasks.Add(GenerateDALFileForTableAsync(result.TableName, result.Columns, projectInfo));
                }

                return Task.WhenAll(tasks);
            });

            await Task.WhenAll(writeTask);
        }

        private async Task GenerateBLLFileForTableAsync(string tableName, List<ColumnInfo> tableColumns, GeneratedProjectInfo projectInfo)
        {
            var generatore = new BLLGenerator(tableName, tableColumns);

            string generatedCode = generatore.Generate(GenerationOptions);

            string filePath = Path.Combine(projectInfo.BusinessLogicLayerPath, $"{tableName}.cs");

            await Utility.WriteCodeToFileAsync(generatedCode, filePath);
        }

        private async Task GenerateDALFileForTableAsync(string tableName, List<ColumnInfo> tableColumns, GeneratedProjectInfo projectInfo)
        {
            var generatore = new DALGenerator(tableName, tableColumns);

            string generatedCode = generatore.Generate(GenerationOptions);

            string filePath = Path.Combine(projectInfo.DataAccessLayerPath, $"{tableName}Data.cs");

            await Utility.WriteCodeToFileAsync(generatedCode, filePath);
        }
    }
}
