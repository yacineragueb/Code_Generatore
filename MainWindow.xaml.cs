using System.Windows;
using System.Windows.Controls;
using Code_Generatore.Lib;
using Microsoft.Data.SqlClient;
using Code_Generatore.BusinessLayer;

namespace Code_Generatore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            try
            {
                ConnectionSession session = DatabaseService.Login(Username, Password);

                if (isRememberMeChecked)
                {
                    Utility.SaveCredentials(Username, Password);
                }
                else
                {
                    Utility.ClearCredentials();
                }

                CodeGeneratoreWindow windCode_Gen = new CodeGeneratoreWindow(session);
                this.Hide();
                windCode_Gen.ShowDialog();

                ShowPasswordCheckbox.IsChecked = false;

                if (!isRememberMeChecked)
                {
                    UsernameTextBox.Clear();
                    PasswordBox.Clear();
                }

                this.Show();
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