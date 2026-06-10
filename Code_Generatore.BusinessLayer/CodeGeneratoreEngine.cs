using Code_Generatore.Lib;

namespace Code_Generatore.BusinessLayer
{
    public class CodeGeneratoreEngine
    {
        private DatabaseService _dbService;
        private ConnectionSession _session;

        public string DatabaseName;
        public string ProjectName { get; set; }
        public string ProjectFolderPath { get; set; }
        public List<TableItem> SelectedTables { get; set; }
        public ProjectGeneratore.enProjectType ProjectType { get; set; }
        public GenerationOptions GenerationOptions { get; set; }

        private GeneratedProjectInfo _projectInfo;

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
            _projectInfo = ProjectGeneratore.GenerateProject(ProjectName, ProjectFolderPath, ProjectType);

            if (_projectInfo == null)
            {
                return false;
            }

            GenerateBLLFiles();
            GenerateDALFiles();

            return true;
        }

        private void GenerateBLLFiles()
        {
            foreach (TableItem table in SelectedTables)
            {
                GenerateBLLFileForTable(table.TableName);
            }
        }

        private void GenerateDALFiles()
        {
            foreach (TableItem table in SelectedTables)
            {
                GenerateDALFileForTable(table.TableName);
            }
        }

        private void GenerateBLLFileForTable(string tableName)
        {
            var columns = _dbService.GetTableColumns(_session, DatabaseName, tableName);

            var generatore = new BLLGenerator(tableName, columns);

            string code = generatore.Generate(GenerationOptions);

            string filePath = Path.Combine(_projectInfo.BusinessLogicLayerPath, $"{tableName}.cs");

            Utility.WriteCodeToFile(code, filePath);
        }

        private void GenerateDALFileForTable(string tableName)
        {
            var columns = _dbService.GetTableColumns(_session, DatabaseName, tableName);

            var generatore = new DALGenerator(tableName, columns);

            string code = generatore.Generate(GenerationOptions);

            string filePath = Path.Combine(_projectInfo.DataAccessLayerPath, $"{tableName + "Data"}.cs");

            Utility.WriteCodeToFile(code, filePath);
        }
    }
}
