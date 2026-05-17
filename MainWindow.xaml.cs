using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Code_Generatore.Lib;
using Microsoft.Data.SqlClient;

namespace Code_Generatore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _server = "localhost";
        private string _connectionString;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowPasswordCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordUnmaskTextBox.Text = PasswordBox.Password;
            PasswordUnmaskTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;
        }

        private void ShowPasswordCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordUnmaskTextBox.Text;
            PasswordUnmaskTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
        }

        private string GetPassword()
        {
            return PasswordUnmaskTextBox.Visibility == Visibility.Visible 
                ? PasswordUnmaskTextBox.Text 
                : PasswordBox.Password;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorText.Text = string.Empty;
            ErrorText.Visibility = Visibility.Collapsed;
        }

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            string Username = UsernameTextBox.Text;
            string Password = GetPassword();
            bool isRememberMeChecked = RememberMeCheckbox.IsChecked == true;

            if(!Utility.AreCredentialsProvided(Username, Password))
            {
                string errorMessage = "Username and Password are required.";
                ShowError(errorMessage);
                return;
            }

            HideError();

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = _server,
                UserID = Username,
                Password = Password,
                TrustServerCertificate = true
            };

            _connectionString = builder.ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    if (isRememberMeChecked)
                    {
                        Utility.SaveCredentials(Username, Password);
                    }
                    else
                    {
                        Utility.ClearCredentials();
                    }

                    windCode_gen windCode_Gen = new windCode_gen(this);
                    this.Hide();
                    windCode_Gen.ShowDialog();

                    ShowPasswordCheckbox.IsChecked = false;

                    if (!isRememberMeChecked)
                    {
                        UsernameTextBox.Clear();
                        PasswordBox.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Connection failed:\n" + ex.Message,
                    "SQL Server Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var credentials = Utility.LoadCredentials();

            if(credentials != null)
            {
                UsernameTextBox.Text = credentials.Value.Username;
                PasswordBox.Password = credentials.Value.Password;

                RememberMeCheckbox.IsChecked = true;
            }
        }
    }
}