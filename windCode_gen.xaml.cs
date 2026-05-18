using System.Windows;
using System.Windows.Threading;

namespace Code_Generatore
{
    /// <summary>
    /// Interaction logic for windCode_gen.xaml
    /// </summary>
    public partial class windCode_gen : Window
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

        public windCode_gen(MainWindow loginWindow)
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
    }
}
