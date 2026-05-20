using Code_Generatore.BusinessLayer;
using Microsoft.Win32;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Code_Generatore
{
    /// <summary>
    /// Interaction logic for CodeGeneratoreWindow.xaml
    /// </summary>
    public partial class CodeGeneratoreWindow : Window, INotifyPropertyChanged
    {
        private ConnectionSession _session;
        private DispatcherTimer? _timer;

        public List<string> DatabasesList { get; set; }
        public ConnectionSession Session => _session;

        private string _selectedDatabase;

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public string SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                _selectedDatabase = value;

                // update session
                _session.DatabaseName = value;

                // force UI refresh
                OnPropertyChanged(nameof(SelectedDatabase));
                OnPropertyChanged(nameof(Session));
            }
        }

        private void StartLiveClock()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += (s, e) => UpdateClock();
            UpdateClock();
            _timer.Start();
        }

        public CodeGeneratoreWindow(ConnectionSession Session)
        {
            InitializeComponent();

            _session = Session;

            StartLiveClock();

            DatabasesList = DatabaseService.GetAllDatabases(_session);

            this.DataContext = this;
        }

        private void UpdateClock()
        {
            CurrentDateTimeTextBlock.Content = DateTime.Now.ToString("dd MMMM, yyyy - HH:mm:ss");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _timer?.Stop();
        }

        private void DisconnectBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog
            {
                Title = "Select a folder",
                Multiselect = false,
            };

            if(openFolderDialog.ShowDialog() == true)
            {
                OutputFolderTextBox.Text = openFolderDialog.FolderName;
            }
        }
    }
}
