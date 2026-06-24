

using Code_Generatore.ViewModels;

namespace CodeGeneratore.Tests;

public class LoginViewModelTests
{
    private readonly LoginViewModel _loginViewModel;
    public LoginViewModelTests()
    {
        _loginViewModel = new LoginViewModel();
    }

    [Fact]
    public void Username_ShouldRaisePropertyChanged()
    {
        string? raisedProperty = null;

        _loginViewModel.PropertyChanged += (_, e) =>
        {
            raisedProperty = e.PropertyName;
        };

        _loginViewModel.Username = "yacine";

        Assert.Equal("Username", raisedProperty);
    }

    [Fact]
    public void Username_ShouldNotRaisedPropertyChanged_WhenValueIsSame()
    {
        int count = 0;

        _loginViewModel.Username = "yacine";

        _loginViewModel.PropertyChanged += (_, _) => count++;

        _loginViewModel.Username = "yacine";

        Assert.Equal(0, count);
    }
}
