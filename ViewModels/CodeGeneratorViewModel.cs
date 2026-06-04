using Code_Generatore.BusinessLayer;
using Code_Generatore.ViewModels.Commands;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
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
        private string _previewCode = string.Empty;
        private bool _hasSelectedTable = false;
        private bool _isResetting = false;
        private CancellationTokenSource? _previewCts;

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

                ResetCRUDOperations();

                LoadTables();

                OnPropertyChanged(nameof(SelectedDatabase));
                OnPropertyChanged(nameof(SelectedDatabaseDisplay));
                OnPropertyChanged(nameof(CanSelectAllTables));
                OnPropertyChanged(nameof(CanGenerate));
                OnPropertyChanged(nameof(AreOperationsEnabled));
                OnPropertyChanged(nameof(AreAllTablesSelected));
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
        public ICommand CopyPreviewCommand { get; }

        public bool HasPreviewCode => !string.IsNullOrWhiteSpace(PreviewCode);

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
            get => Tables.Count > 0 && Tables.All(table => table.IsSelected);
            set
            {
                foreach (var table in Tables)
                {
                    table.IsSelected = value;
                }

                OnPropertyChanged(nameof(AreAllTablesSelected));
                OnPropertyChanged(nameof(AreOperationsEnabled));
            }
        }

        public bool CanSelectAllTables => !string.IsNullOrWhiteSpace(SelectedDatabase) && Tables.Count > 0;

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
                OnPropertyChanged(nameof(HasPreviewCode));
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
                OnPropertyChanged(nameof(GetAllSelected));

                RefreshPreview();
            }
        }

        public bool AreOperationsEnabled => !string.IsNullOrWhiteSpace(SelectedDatabase) && HasSelectedTable;

        public bool HasSelectedTable
        {
            get => _hasSelectedTable;
            set
            {
                if (_hasSelectedTable == value) return;

                _hasSelectedTable = value;

                OnPropertyChanged(nameof(HasSelectedTable));
                OnPropertyChanged(nameof(AreOperationsEnabled));
            }
        }

        public CodeGeneratorViewModel(ConnectionSession session)
        {
            _session = session;
            _databaseService = new DatabaseService();
            DatabasesList = _databaseService.GetAllDatabases(_session);
            BrowseCommand = new RelayCommand(Browse);
            CopyPreviewCommand = new RelayCommand(_ => Clipboard.SetText(PreviewCode));
            GenerateCommand = new RelayCommand(GenerateCode);
        }

        private void RefreshPreview()
        {
            if (_isResetting) return; // skip during batch reset

            var selectedTable = Tables.FirstOrDefault(t => t.IsSelected);
            if (selectedTable == null)
            {
                PreviewCode = "// ⚠ Select a table to preview code.";
                return;
            }

            if (!InsertSelected && !UpdateSelected && !DeleteSelected && !GetByIdSelected && !GetAllSelected)
            {
                PreviewCode = "// ⚠ Select at least one CRUD operation to preview code.";
                return;
            }

            _previewCts?.Cancel();
            _previewCts = new CancellationTokenSource();

            _ = RefreshPreviewAsync(selectedTable.TableName, _previewCts.Token);
        }

        private async Task RefreshPreviewAsync(string tableName, CancellationToken ct)
        {
            var columns = await Task.Run(() => 
                _databaseService.GetTableColumns(Session, SelectedDatabase, tableName));

            if (ct.IsCancellationRequested) return; // stale, discard

            var generator = new PreviewGenerator(tableName, columns);

            var preview = new StringBuilder();

            preview.AppendLine("// ==============================");
            preview.AppendLine("//   BUSINESS LAYER PREVIEW");
            preview.AppendLine("// ==============================");
            preview.AppendLine();

            preview.Append(generator.Generate(
                insert: InsertSelected,
                update: UpdateSelected,
                delete: DeleteSelected,
                getById: GetByIdSelected,
                getAll: GetAllSelected
            ));

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
                var table = new TableItem(tableName);

                table.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == nameof(TableItem.IsSelected))
                    {
                        HasSelectedTable = Tables.Any(t => t.IsSelected);

                        if (!HasSelectedTable)
                        {
                            ResetCRUDOperations();
                        }

                        OnPropertyChanged(nameof(AreAllTablesSelected));
                        RefreshPreview();
                    }
                };

                Tables.Add(table);
            }
        }

        private void ResetCRUDOperations()
        {
            _isResetting = true;

            _previewCts?.Cancel(); // ← kill any in-flight preview immediately
            _previewCts = null;

            _insertSelected = false;
            _updateSelected = false;
            _deleteSelected = false;
            _getByIdSelected = false;
            _getAllSelected = false;

            OnPropertyChanged(nameof(InsertSelected));
            OnPropertyChanged(nameof(UpdateSelected));
            OnPropertyChanged(nameof(DeleteSelected));
            OnPropertyChanged(nameof(GetByIdSelected));
            OnPropertyChanged(nameof(GetAllSelected));

            PreviewCode = string.Empty;

            _isResetting = false;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
