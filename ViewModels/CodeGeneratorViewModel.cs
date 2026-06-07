using Code_Generatore.BusinessLayer;
using Code_Generatore.ViewModels.Commands;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using static Code_Generatore.Lib.Utility;

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
        private ProjectGeneratore.enProjectType _selectedProjectType;

        private bool _insertSelected;
        private bool _updateSelected;
        private bool _deleteSelected;
        private bool _getByIdSelected;
        private bool _getAllSelected;

        private const string INSERT_FN_NAME = "Insert";
        private const string UPDATE_FN_NAME = "Update";
        private const string DELETE_FN_NAME = "Delete";
        private const string GET_BY_ID_FN_NAME = "GetById";
        private const string GET_ALL_FN_NAME = "GetAll";

        private string _insertFunctionName = INSERT_FN_NAME;
        private string _updateFunctionName = UPDATE_FN_NAME;
        private string _deleteFunctionName = DELETE_FN_NAME;
        private string _getByIdFunctionName = GET_BY_ID_FN_NAME;
        private string _getAllFunctionName = GET_ALL_FN_NAME;

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

                ResetCRUDOperationsCheckbox();
                ResetCRUDOperationsfunctionNames();

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
           IsEmpty(SelectedDatabase) ? "Not Selected Yet" : SelectedDatabase;

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

        public bool HasPreviewCode => !IsEmpty(PreviewCode);

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

        public bool CanSelectAllTables => !IsEmpty(SelectedDatabase) && Tables.Count > 0;

        public ICommand GenerateCommand { get; }

        public bool CanGenerate => !IsEmpty(SelectedDatabase) && !IsEmpty(ProjectName) && !IsEmpty(OutputFolder) && HasSelectedTable && HasSelectedAtLeastOneOperation;

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
                if (_insertSelected == value) return;

                _insertSelected = value;
                OnPropertyChanged(nameof(InsertSelected));
                OnPropertyChanged(nameof(HasSelectedAtLeastOneOperation));
                OnPropertyChanged(nameof(CanGenerate));

                RefreshPreview(); 
            }
        }
        public string InsertFunctionName
        {
            get => _insertFunctionName;
            set
            {
                if (_insertFunctionName == value) return;

                _insertFunctionName = value;
                OnPropertyChanged(nameof(InsertFunctionName));
                RefreshPreview();
            }
        }

        public bool UpdateSelected
        {
            get => _updateSelected;
            set { 
                if (_updateSelected == value) return;

                _updateSelected = value;
                OnPropertyChanged(nameof(UpdateSelected));
                OnPropertyChanged(nameof(HasSelectedAtLeastOneOperation));
                OnPropertyChanged(nameof(CanGenerate));

                RefreshPreview(); 
            }
        }
        public string UpdateFunctionName
        {
            get => _updateFunctionName;
            set
            {
                if (_updateFunctionName == value) return;

                _updateFunctionName = value;
                OnPropertyChanged(nameof(UpdateFunctionName));
                RefreshPreview();
            }
        }

        public bool DeleteSelected
        {
            get => _deleteSelected;
            set {
                if (_deleteSelected == value) return;

                _deleteSelected = value;
                OnPropertyChanged(nameof(DeleteSelected));
                OnPropertyChanged(nameof(HasSelectedAtLeastOneOperation));
                OnPropertyChanged(nameof(CanGenerate));

                RefreshPreview(); 
            }
        }
        public string DeleteFunctionName
        {
            get => _deleteFunctionName;
            set
            {
                if (_deleteFunctionName == value) return;

                _deleteFunctionName = value;
                OnPropertyChanged(nameof(DeleteFunctionName));
                RefreshPreview();
            }
        }

        public bool GetByIdSelected
        {
            get => _getByIdSelected;
            set {
                if (_getByIdSelected == value) return;

                _getByIdSelected = value;
                OnPropertyChanged(nameof(GetByIdSelected));
                OnPropertyChanged(nameof(HasSelectedAtLeastOneOperation));
                OnPropertyChanged(nameof(CanGenerate));

                RefreshPreview();
            }
        }
        public string GetByIdFunctionName
        {
            get => _getByIdFunctionName;
            set
            {
                if (_getByIdFunctionName == value) return;

                _getByIdFunctionName = value;
                OnPropertyChanged(nameof(GetByIdFunctionName));
                RefreshPreview();
            }
        }

        public bool GetAllSelected
        {
            get => _getAllSelected;
            set
            {
                if (_getAllSelected == value) return;

                _getAllSelected = value;
                OnPropertyChanged(nameof(GetAllSelected));
                OnPropertyChanged(nameof(HasSelectedAtLeastOneOperation));
                OnPropertyChanged(nameof(CanGenerate));

                RefreshPreview();
            }
        }
        public string GetAllFunctionName
        {
            get => _getAllFunctionName;
            set
            {
                if (_getAllFunctionName == value) return;

                _getAllFunctionName = value;
                OnPropertyChanged(nameof(GetAllFunctionName));
                RefreshPreview();
            }
        }

        public bool AreOperationsEnabled => !IsEmpty(SelectedDatabase) && HasSelectedTable;

        public bool HasSelectedTable
        {
            get => _hasSelectedTable;
            set
            {
                if (_hasSelectedTable == value) return;

                _hasSelectedTable = value;

                OnPropertyChanged(nameof(HasSelectedTable));
                OnPropertyChanged(nameof(AreOperationsEnabled));
                OnPropertyChanged(nameof(CanGenerate));
            }
        }

        public bool HasSelectedAtLeastOneOperation =>
            InsertSelected || UpdateSelected || DeleteSelected || GetByIdSelected || GetAllSelected;

        public ProjectGeneratore.enProjectType SelectedProjectType
        {
            get => _selectedProjectType;
            set
            {
                if(value == _selectedProjectType) return;

                _selectedProjectType = value;
                OnPropertyChanged(nameof(SelectedProjectType));
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
                _databaseService.GetTableColumns(Session, SelectedDatabase, tableName), ct);

            if (ct.IsCancellationRequested) return; // stale, discard

            var generator = new PreviewGenerator(tableName, columns);

            var preview = new StringBuilder();

            preview.AppendLine("// ==============================");
            preview.AppendLine("//   BUSINESS LAYER PREVIEW");
            preview.AppendLine("// ==============================");
            preview.AppendLine();

            GenerationOptions options = BuildGenerationOptions();

            preview.Append(generator.Generate(options));

            PreviewCode = preview.ToString();
        }

        private void GenerateCode(object? paramater)
        {
            string selectedProjectName = SelectedProjectType == ProjectGeneratore.enProjectType.WINDOWS_FORMS 
                ? "WinForms" 
                : "WPF";

            List<TableItem> selectedTables = Tables.Where(table => table.IsSelected).ToList();

            GenerationOptions options = BuildGenerationOptions();

            CodeGeneratoreEngine generatoreEngine = new CodeGeneratoreEngine(Session, SelectedDatabase, ProjectName, OutputFolder, SelectedProjectType, selectedTables, options);

            if(generatoreEngine.Generate())
            {
                MessageBox.Show($"{selectedProjectName} Project has generated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            } else
            {
                MessageBox.Show($"Failed to generate {selectedProjectName} Project.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            if (IsEmpty(SelectedDatabase))
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
                            ResetCRUDOperationsCheckbox();
                        }

                        ResetCRUDOperationsfunctionNames();

                        OnPropertyChanged(nameof(AreAllTablesSelected));
                        RefreshPreview();
                    }
                };

                Tables.Add(table);
            }
        }

        private void ResetCRUDOperationsCheckbox()
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

        private void ResetCRUDOperationsfunctionNames()
        {
            _insertFunctionName = INSERT_FN_NAME;
            _updateFunctionName = UPDATE_FN_NAME;
            _deleteFunctionName = DELETE_FN_NAME;
            _getByIdFunctionName = GET_BY_ID_FN_NAME;
            _getAllFunctionName = GET_ALL_FN_NAME;

            OnPropertyChanged(nameof(InsertFunctionName));
            OnPropertyChanged(nameof(UpdateFunctionName));
            OnPropertyChanged(nameof(DeleteFunctionName));
            OnPropertyChanged(nameof(GetByIdFunctionName));
            OnPropertyChanged(nameof(GetAllFunctionName));
        }

        private FunctionNames BuildFunctionNames() => new FunctionNames(
            IsEmpty(InsertFunctionName) ? INSERT_FN_NAME : InsertFunctionName,
            IsEmpty(UpdateFunctionName) ? UPDATE_FN_NAME : UpdateFunctionName,
            IsEmpty(DeleteFunctionName) ? DELETE_FN_NAME : DeleteFunctionName,
            IsEmpty(GetByIdFunctionName) ? GET_BY_ID_FN_NAME : GetByIdFunctionName,
            IsEmpty(GetAllFunctionName) ? GET_ALL_FN_NAME : GetAllFunctionName
        );

        private GenerationOptions BuildGenerationOptions() => new GenerationOptions
        {
            Insert = InsertSelected,
            Update = UpdateSelected,
            Delete = DeleteSelected,
            GetById = GetByIdSelected,
            GetAll = GetAllSelected,
            FunctionNames = BuildFunctionNames(),
        };

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
