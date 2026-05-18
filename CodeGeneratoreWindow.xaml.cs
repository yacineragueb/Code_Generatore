using Microsoft.Win32;
using System.Windows;
using System.Windows.Threading;

namespace Code_Generatore
{
    /// <summary>
    /// Interaction logic for CodeGeneratoreWindow.xaml
    /// </summary>
    public partial class CodeGeneratoreWindow : Window
    {
        private readonly MainWindow _loginWindow;
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

        public CodeGeneratoreWindow(MainWindow loginWindow)
        {
            InitializeComponent();
            _loginWindow = loginWindow;

            StartLiveClock();
        }

        private void UpdateClock()
        {
            CurrentDateTimeTextBlock.Content = DateTime.Now.ToString("dd MMMM, yyyy - HH:mm:ss");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _timer?.Stop();
            _loginWindow.Show();
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
