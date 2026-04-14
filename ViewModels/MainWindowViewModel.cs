using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZootekniPro.App.Services;
using ZootekniPro.App.ViewModels;

namespace ZootekniPro.App.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly FeedDataService _feedDataService;
    private readonly OptimizationService _optimizationService;
    private readonly NutritionCalculatorService _nutritionService;
    private readonly ReportService _reportService;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private string _currentPageTitle = "Gösterge Paneli";

    [ObservableProperty]
    private int _selectedMenuIndex;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _username = "Admin";

    // ViewModels
    public LoginViewModel LoginViewModel { get; }
    public DashboardViewModel DashboardViewModel { get; }
    public FeedLibraryViewModel FeedLibraryViewModel { get; }
    public RationCalculatorViewModel RationCalculatorViewModel { get; }

    public MainWindowViewModel()
    {
        _dbService = new DatabaseService();
        _feedDataService = new FeedDataService(_dbService);
        _optimizationService = new OptimizationService();
        _nutritionService = new NutritionCalculatorService();
        _reportService = new ReportService();

        // Initialize ViewModels
        LoginViewModel = new LoginViewModel(_dbService);
        DashboardViewModel = new DashboardViewModel(_dbService, _feedDataService);
        FeedLibraryViewModel = new FeedLibraryViewModel(_dbService);
        RationCalculatorViewModel = new RationCalculatorViewModel(
            _dbService, _optimizationService, _nutritionService, _reportService);

        // Set login handler
        LoginViewModel.LoginSuccessful += OnLoginSuccessful;

        // Start with login view
        CurrentViewModel = LoginViewModel;
    }

    private void OnLoginSuccessful()
    {
        IsLoggedIn = true;
        NavigateToDashboard();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        CurrentViewModel = DashboardViewModel;
        CurrentPageTitle = "Gösterge Paneli";
        SelectedMenuIndex = 0;
        _ = DashboardViewModel.LoadDashboardDataCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private void NavigateToFeedLibrary()
    {
        CurrentViewModel = FeedLibraryViewModel;
        CurrentPageTitle = "Yem Kütüphanesi";
        SelectedMenuIndex = 1;
        FeedLibraryViewModel.LoadFeedsCommand.Execute(null);
    }

    [RelayCommand]
    private void NavigateToRationCalculator()
    {
        CurrentViewModel = RationCalculatorViewModel;
        CurrentPageTitle = "Rasyon Hesapla";
        SelectedMenuIndex = 2;
    }

    [RelayCommand]
    private void NavigateToReports()
    {
        CurrentViewModel = RationCalculatorViewModel;
        CurrentPageTitle = "Raporlar";
        SelectedMenuIndex = 3;
    }

    [RelayCommand]
    private void NavigateToSettings()
    {
        CurrentPageTitle = "Ayarlar";
        SelectedMenuIndex = 4;
    }

    [RelayCommand]
    private void Logout()
    {
        IsLoggedIn = false;
        CurrentViewModel = LoginViewModel;
        CurrentPageTitle = "Giriş";
    }

    partial void OnSelectedMenuIndexChanged(int value)
    {
        switch (value)
        {
            case 0:
                NavigateToDashboard();
                break;
            case 1:
                NavigateToFeedLibrary();
                break;
            case 2:
                NavigateToRationCalculator();
                break;
            case 3:
                NavigateToReports();
                break;
            case 4:
                NavigateToSettings();
                break;
        }
    }
}
