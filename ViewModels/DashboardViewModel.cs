using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZootekniPro.App.Services;
using ZootekniPro.App.Models;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ZootekniPro.App.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly FeedDataService _feedDataService;

    [ObservableProperty]
    private int _totalFeeds;

    [ObservableProperty]
    private int _totalRations;

    [ObservableProperty]
    private int _activeAnimalGroups;

    [ObservableProperty]
    private double _iofcToday;

    [ObservableProperty]
    private string _lastRationName = "Henüz rasyon yok";

    [ObservableProperty]
    private string _lastRationDate = "-";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "Hazır";

    public ObservableCollection<Ration> RecentRations { get; } = new();

    public DashboardViewModel(DatabaseService dbService, FeedDataService feedDataService)
    {
        _dbService = dbService;
        _feedDataService = feedDataService;
    }

    [RelayCommand]
    private async Task LoadDashboardData()
    {
        IsLoading = true;
        StatusMessage = "Veriler yükleniyor...";

        try
        {
            // Load summary statistics
            var feeds = _dbService.GetAllFeeds();
            TotalFeeds = feeds.Count;

            var rations = _dbService.GetAllRations();
            TotalRations = rations.Count;
            
            if (rations.Count > 0)
            {
                var lastRation = rations[0];
                LastRationName = lastRation.Name;
                LastRationDate = lastRation.CreatedDate.ToString("dd.MM.yyyy");
                IofcToday = lastRation.Result?.IOFC ?? 0;
            }

            var groups = _dbService.GetAllAnimalGroups();
            ActiveAnimalGroups = groups.Count;

            // Load recent rations
            RecentRations.Clear();
            foreach (var ration in rations.Take(5))
            {
                RecentRations.Add(ration);
            }

            StatusMessage = "Hazır";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hata: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImportFeeds()
    {
        IsLoading = true;
        StatusMessage = "Yem verileri import ediliyor...";

        try
        {
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "yemler.csv");
            
            if (File.Exists(csvPath))
            {
                await _feedDataService.ImportFeedsFromCsvAsync(csvPath);
                var feeds = _dbService.GetAllFeeds();
                TotalFeeds = feeds.Count;
                StatusMessage = $"{feeds.Count} yem başarıyla yüklendi!";
            }
            else
            {
                StatusMessage = "Yem veri dosyası bulunamadı!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Import hatası: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}