using Code_Generatore.BusinessLayer;
using Code_Generatore.BusinessLayer.Exceptions;
using Code_Generatore.ViewModels.Commands;
using Code_Generatore.Lib;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Code_Generatore.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe;
        private string _errorMessage = string.Empty;
        private bool _hasError;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public bool RememberMe
        {
            get => _rememberMe;
            set
            {
                _rememberMe = value;
                OnPropertyChanged(nameof(RememberMe));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                _hasError = value;
                OnPropertyChanged(nameof(HasError));
            }
        }

        public ICommand LoginCommand { get; }

        public event Action<ConnectionSession>? LoginSucceeded;

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(Login);

            LoadCredentials();
        }

        private void Login(object? paramter)
        {
            if (!Utility.AreCredentialsProvided(Username, Password))
            {
                ShowError("Username and Password are required.");
                return;
            }

            HideError();

            try
            {
                DatabaseService dbService = new DatabaseService();

                ConnectionSession session = dbService.Login(Username, Password);

                if (RememberMe)
                {
                    Utility.SaveCredentials(Username, Password);
                }
                else
                {
                    Utility.ClearCredentials();
                }

                LoginSucceeded?.Invoke(session);
            }
            catch (DatabaseConnectionException ex)
            {
                ShowError(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred.\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadCredentials()
        {
            var credentials = Utility.LoadCredentials();

            if (credentials != null)
            {
                Username = credentials.Value.Username;
                Password = credentials.Value.Password;
                RememberMe = true;
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }

        private void HideError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }

        public bool isRememberMeChecked () => RememberMe;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
