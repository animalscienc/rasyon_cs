using Avalonia.Controls;
using ZootekniPro.App.ViewModels;

namespace ZootekniPro.App.Views;

public partial class LoginView : UserControl
{
    public LoginView()
    {
        this.InitializeComponent();

        // Handle Enter key for login
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key == Avalonia.Input.Key.Enter && DataContext is LoginViewModel vm)
        {
            vm.LoginCommand.Execute(null);
        }
    }
}