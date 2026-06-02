using Code_Generatore.BusinessLayer;
using Code_Generatore.ViewModels.Commands;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace Code_Generatore.ViewModels
{
    public class CodeGeneratorViewModel : INotifyPropertyChanged
    {
        private readonly ConnectionSession _session;
        private DatabaseService _databaseService;
        private string _selectedDatabase = string.Empty;
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
                OnPropertyChanged(nameof(SelectedDatabaseDisplay));
                OnPropertyChanged(nameof(CanSelectAllTables));
            }
        }

        public string SelectedDatabaseDisplay =>
           string.IsNullOrWhiteSpace(SelectedDatabase) ? "Not Selected Yet" : SelectedDatabase;

        public string OutputFolder
        {
            get => _outputFolder;
            set { 
                if (_outputFolder == value)
                    return;

                _outputFolder = value;
                OnPropertyChanged(nameof(OutputFolder)); 
            }
        }

        public ICommand BrowseCommand { get; }

        public string ProjectName
        {
            get => _projectName;
            set { 
                if(_projectName == value)
                    return;

                _projectName = value; 
                OnPropertyChanged(nameof(ProjectName)); 
            }
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

        public bool CanSelectAllTables => !string.IsNullOrWhiteSpace(SelectedDatabase);

        public CodeGeneratorViewModel(ConnectionSession session)
        {
            _session = session;
            _databaseService = new DatabaseService();
            DatabasesList = _databaseService.GetAllDatabases(_session);
            BrowseCommand = new RelayCommand(Browse);
        }

        private void Browse(object? paramter)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog
            {
                Title = "Select a folder",
                Multiselect = false,
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                OutputFolder = openFolderDialog.FolderName;
            }
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
