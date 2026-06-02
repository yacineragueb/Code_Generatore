using Code_Generatore.BusinessLayer;
using Code_Generatore.ViewModels.Commands;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
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
        private string _previewCode = string.Empty;

        // Checkboxes ( CRUD Operations )
        private bool _insertSelected;
        private bool _updateSelected;
        private bool _deleteSelected;
        private bool _getByIdSelected;
        private bool _getAllSelected;

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
                OnPropertyChanged(nameof(CanGenerate));
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
                OnPropertyChanged(nameof(CanGenerate));
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
                OnPropertyChanged(nameof(CanGenerate));
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

        public ICommand GenerateCommand { get; }

        public bool CanGenerate => !string.IsNullOrWhiteSpace(SelectedDatabase) && !string.IsNullOrWhiteSpace(ProjectName) && !string.IsNullOrWhiteSpace(OutputFolder);

        public string PreviewCode
        {
            get => _previewCode;
            set { 
                if (_previewCode == value)
                    return;

                _previewCode = value; 
                OnPropertyChanged(nameof(PreviewCode));
            }
        }

        public bool InsertSelected
        {
            get => _insertSelected;
            set { 
                _insertSelected = value;
                OnPropertyChanged(nameof(InsertSelected)); 

                RefreshPreview(); 
            }
        }
        public bool UpdateSelected
        {
            get => _updateSelected;
            set { 
                _updateSelected = value;
                OnPropertyChanged(nameof(UpdateSelected));

                RefreshPreview(); 
            }
        }
        public bool DeleteSelected
        {
            get => _deleteSelected;
            set { 
                _deleteSelected = value;
                OnPropertyChanged(nameof(DeleteSelected));

                RefreshPreview(); 
            }
        }
        public bool GetByIdSelected
        {
            get => _getByIdSelected;
            set { 
                _getByIdSelected = value;
                OnPropertyChanged(nameof(GetByIdSelected)); 

                RefreshPreview(); 
            }
        }
        public bool GetAllSelected
        {
            get => _getAllSelected;
            set
            {
                _getAllSelected = value;
                OnPropertyChanged(nameof(_getAllSelected));

                RefreshPreview();
            }
        }

        public CodeGeneratorViewModel(ConnectionSession session)
        {
            _session = session;
            _databaseService = new DatabaseService();
            DatabasesList = _databaseService.GetAllDatabases(_session);
            BrowseCommand = new RelayCommand(Browse);
            GenerateCommand = new RelayCommand(GenerateCode);
        }

        private void RefreshPreview()
        {
            StringBuilder preview = new StringBuilder();

            preview.AppendLine("// ==============================");
            preview.AppendLine("//   BUSINESS LAYER PREVIEW");
            preview.AppendLine("// ==============================");
            preview.AppendLine();

            // Simulate columns fetched from DB
            var columns = new List<ColumnInfo>
            {
                new ColumnInfo { ColumnName = "ID",          CSharpType = "int",      IsPrimaryKey = true  },
                new ColumnInfo { ColumnName = "FirstName",   CSharpType = "string",   IsPrimaryKey = false },
                new ColumnInfo { ColumnName = "LastName",    CSharpType = "string",   IsPrimaryKey = false },
                new ColumnInfo { ColumnName = "DateOfBirth", CSharpType = "DateTime", IsPrimaryKey = false },
                new ColumnInfo { ColumnName = "Phone",       CSharpType = "string",   IsPrimaryKey = false },
            };

            var generator = new PreviewGenerator("Person", columns);

             preview.Append(generator.Generate(
                insert: InsertSelected,
                update: UpdateSelected,
                delete: DeleteSelected,
                getById: GetByIdSelected,
                getAll: GetAllSelected
            ));

            if (!InsertSelected && !UpdateSelected && !DeleteSelected && !GetByIdSelected && !GetAllSelected)
            {
                preview.Clear();
                preview.AppendLine("// ⚠ Select at least one CRUD operation to preview code.");
            }

            PreviewCode = preview.ToString();
        }

        private void GenerateCode(object? paramater)
        {
            throw new NotImplementedException();
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
