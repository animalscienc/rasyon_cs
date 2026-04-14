using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZootekniPro.App.Services;
using ZootekniPro.App.Models;
using System.Collections.ObjectModel;
using System.IO;
using System;

namespace ZootekniPro.App.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    [ObservableProperty]
    private string _username = "admin";

    [ObservableProperty]
    private string _password = "zootekni";

    [ObservableProperty]
    private string _errorMessage = "";

    [ObservableProperty]
    private bool _isLoading;

    public event Action? LoginSuccessful;

    public LoginViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    [RelayCommand]
    private void Login()
    {
        ErrorMessage = "";
        IsLoading = true;

        // Simple authentication check
        if (Username.Equals("admin", StringComparison.OrdinalIgnoreCase) && 
            Password.Equals("zootekni", StringComparison.OrdinalIgnoreCase) ||
            Username.Equals("demo", StringComparison.OrdinalIgnoreCase))
        {
            IsLoading = false;
            LoginSuccessful?.Invoke();
        }
        else
        {
            ErrorMessage = "Geçersiz kullanıcı adı veya şifre!";
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void DemoLogin()
    {
        ErrorMessage = "";
        IsLoading = true;
        
        // Demo mode - skip authentication
        Task.Delay(500).ContinueWith(_ =>
        {
            IsLoading = false;
            LoginSuccessful?.Invoke();
        });
    }
}