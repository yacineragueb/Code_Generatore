using Code_Generatore.AccessDataLayer;
using Code_Generatore.Lib;

namespace Code_Generatore.BusinessLayer
{
    public class CodeGeneratoreEngine
    {
        private DatabaseService _dbService;
        private ConnectionSession _session;

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

            _dbService = new DatabaseService();
        }

        public bool Generate()
        {
            GeneratedProjectInfo projectInfo = ProjectGeneratore.GenerateProject(ProjectName, ProjectFolderPath, ProjectType);

            if (projectInfo == null)
            {
                return false;
            }

            return GenerateBLLAndDALFiles(projectInfo);
        }

        private bool GenerateBLLAndDALFiles(GeneratedProjectInfo projectInfo)
        {
            foreach (TableItem table in SelectedTables)
            {
                List<ColumnInfo> columns = _dbService.GetTableColumns(_session, DatabaseName, table.TableName);

                if(columns == null || columns.Count == 0)
                {
                    return false;
                }

                GenerateBLLFileForTable(table.TableName, columns, projectInfo);
                GenerateDALFileForTable(table.TableName, columns, projectInfo);
            }

            return true;
        }

        private void GenerateBLLFileForTable(string tableName, List<ColumnInfo> tableColumns, GeneratedProjectInfo projectInfo)
        {
            var generatore = new BLLGenerator(tableName, tableColumns);

            string generatedCode = generatore.Generate(GenerationOptions);

            string filePath = Path.Combine(projectInfo.BusinessLogicLayerPath, $"{tableName}.cs");

            Utility.WriteCodeToFile(generatedCode, filePath);
        }

        private void GenerateDALFileForTable(string tableName, List<ColumnInfo> tableColumns, GeneratedProjectInfo projectInfo)
        {
            var generatore = new DALGenerator(tableName, tableColumns);

            string generatedCode = generatore.Generate(GenerationOptions);

            string filePath = Path.Combine(projectInfo.DataAccessLayerPath, $"{tableName + "Data"}.cs");

            Utility.WriteCodeToFile(generatedCode, filePath);
        }
    }
}
