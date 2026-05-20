using Microsoft.Win32;
using System.Windows;
using System.Windows.Threading;
using Code_Generatore.BusinessLayer;

namespace Code_Generatore
{
    /// <summary>
    /// Interaction logic for CodeGeneratoreWindow.xaml
    /// </summary>
    public partial class CodeGeneratoreWindow : Window
    {
        private ConnectionSession _session;
        private DispatcherTimer? _timer;

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
            this.DataContext = _session;

            StartLiveClock();
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
