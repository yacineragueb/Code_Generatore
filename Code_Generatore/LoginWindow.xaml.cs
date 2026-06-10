using Code_Generatore.BusinessLayer;
using Code_Generatore.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Code_Generatore
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;
        public LoginWindow()
        {
            InitializeComponent();

            _viewModel = new LoginViewModel();

            _viewModel.LoginSucceeded += OnLoginSucceeded;

            this.DataContext = _viewModel;

            PasswordBox.Password = _viewModel.Password;
        }

        private void ShowPasswordCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordUnmaskTextBox.Text = PasswordBox.Password;
            PasswordUnmaskTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Collapsed;

            _viewModel.Password = PasswordUnmaskTextBox.Text;
        }

        private void ShowPasswordCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordUnmaskTextBox.Text;
            PasswordUnmaskTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;

            _viewModel.Password = PasswordBox.Password;
        }

        private void OnLoginSucceeded(ConnectionSession session)
        {
            CodeGeneratoreWindow window =
                new CodeGeneratoreWindow(session);

            this.Hide();

            window.ShowDialog();

            ShowPasswordCheckbox.IsChecked = false;

            if (!_viewModel.isRememberMeChecked())
            {
                UsernameTextBox.Clear();
                PasswordBox.Clear();
            }

            this.Show();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _viewModel.Password = PasswordBox.Password;
        }

        private void PasswordUnmaskTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.Password = PasswordUnmaskTextBox.Text;
        }
    }
}