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

        private void ConnectBtn_Click(object sender, RoutedEventArgs e)
        {
            string Username = UsernameTextBox.Text;
            string Passowrd;
            bool isRememberMeChecked = RememberMeCheckbox.IsChecked == true;

            if (PasswordUnmaskTextBox.Visibility == Visibility.Visible)
            {
                Passowrd = PasswordUnmaskTextBox.Text;
            } else
            {
                Passowrd = PasswordBox.Password;
            }

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Passowrd))
            {
                ErrorText.Text = "Username and Password are required.";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            ErrorText.Visibility = Visibility.Collapsed;

            if(isRememberMeChecked)
            {
                Utility.SaveCredentials(Username, Passowrd);
            } else
            {
                Utility.ClearCredentials();
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
            {
                DataSource = _server,
                UserID = Username,
                Password = Passowrd,
                TrustServerCertificate = true
            };

            _connectionString = builder.ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    windCode_gen windCode_Gen = new windCode_gen(this);
                    this.Hide();
                    windCode_Gen.ShowDialog();

                    if(!isRememberMeChecked)
                    {
                        ShowPasswordCheckbox.IsChecked = false;
                        UsernameTextBox.Clear();
                        PasswordBox.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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