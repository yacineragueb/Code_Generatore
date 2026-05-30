using Code_Generatore.BusinessLayer;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Code_Generatore.ViewModels
{
    public class CodeGeneratorViewModel : INotifyPropertyChanged
    {
        private readonly ConnectionSession _session;
        private DatabaseService _databaseService;
        private string _selectedDatabase = "Not Selected Yet";
        private string _outputFolder = string.Empty;
        private string _projectName = string.Empty;
        private bool _areAllTablesSelected = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ConnectionSession Session => _session;
        public List<string> DatabasesList { get; }
        public ObservableCollection<TableItem> Tables { get; } = [];

        public string SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                if (_selectedDatabase == value)
                    return;

                _selectedDatabase = value;

                LoadTables();

                OnPropertyChanged(nameof(SelectedDatabase));
            }
        }

        public string OutputFolder
        {
            get => _outputFolder;
            set { _outputFolder = value; OnPropertyChanged(nameof(OutputFolder)); }
        }

        public string ProjectName
        {
            get => _projectName;
            set { _projectName = value; OnPropertyChanged(nameof(ProjectName)); }
        }

        public bool AreAllTablesSelected
        {
            get => _areAllTablesSelected;
            set
            {
                if (_areAllTablesSelected == value)
                    return;

                _areAllTablesSelected = value;

                foreach (var table in Tables)
                {
                    table.IsSelected = value;
                }

                OnPropertyChanged(nameof(AreAllTablesSelected));
            }
        }

        public CodeGeneratorViewModel(ConnectionSession session)
        {
            _session = session;
            _databaseService = new DatabaseService();
            DatabasesList = _databaseService.GetAllDatabases(_session);
        }

        private void LoadTables()
        {
            Tables.Clear();

            if (string.IsNullOrWhiteSpace(SelectedDatabase))
                return;

            List<string> tableNames =
                _databaseService.GetAllTables(_session, SelectedDatabase);

            foreach (string tableName in tableNames)
            {
                Tables.Add(new TableItem(tableName));
            }
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
