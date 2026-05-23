using Code_Generatore.BusinessLayer;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        public ConnectionSession Session => _session;
        public List<string> DatabasesList { get; }

        public string SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                _selectedDatabase = value;

                // force UI refresh
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

        public CodeGeneratorViewModel(ConnectionSession session)
        {
            _session = session;
            _databaseService = new DatabaseService();
            DatabasesList = _databaseService.GetAllDatabases(_session);
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
