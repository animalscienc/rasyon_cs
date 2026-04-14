using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZootekniPro.App.Services;
using ZootekniPro.App.Models;
using System.Collections.ObjectModel;

namespace ZootekniPro.App.ViewModels;

public partial class FeedLibraryViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    [ObservableProperty]
    private ObservableCollection<Feed> _feeds = new();

    [ObservableProperty]
    private Feed? _selectedFeed;

    [ObservableProperty]
    private string _searchText = "";

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private bool _isEditorOpen;

    [ObservableProperty]
    private Feed _editingFeed = new();

    [ObservableProperty]
    private bool _isNewFeed;

    public FeedLibraryViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    [RelayCommand]
    private void LoadFeeds()
    {
        IsLoading = true;
        try
        {
            var feeds = _dbService.GetAllFeeds();
            Feeds.Clear();
            foreach (var feed in feeds)
            {
                Feeds.Add(feed);
            }

            Categories.Clear();
            var uniqueCategories = feeds.Select(f => f.Category).Distinct().OrderBy(c => c);
            foreach (var cat in uniqueCategories)
            {
                Categories.Add(cat);
            }

            StatusMessage = $"{feeds.Count} yem gösteriliyor";
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
    private void SearchFeeds()
    {
        IsLoading = true;
        try
        {
            var feeds = _dbService.GetAllFeeds();
            
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                feeds = feeds.Where(f => 
                    f.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    f.Origin.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(SelectedCategory))
            {
                feeds = feeds.Where(f => f.Category == SelectedCategory).ToList();
            }

            Feeds.Clear();
            foreach (var feed in feeds)
            {
                Feeds.Add(feed);
            }

            StatusMessage = $"{feeds.Count} yem bulundu";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Arama hatası: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void NewFeed()
    {
        EditingFeed = new Feed
        {
            Category = "Yem",
            DM = 90,
            CP = 15,
            NDF = 30,
            ADF = 20,
            NEL = 1.5,
            ME = 2.3,
            Ca = 0.5,
            P = 0.3,
            Mg = 0.2,
            K = 0.8,
            Price = 3.0
        };
        IsNewFeed = true;
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void EditFeed()
    {
        if (SelectedFeed == null) return;

        EditingFeed = new Feed
        {
            Id = SelectedFeed.Id,
            Name = SelectedFeed.Name,
            Category = SelectedFeed.Category,
            Origin = SelectedFeed.Origin,
            DM = SelectedFeed.DM,
            CP = SelectedFeed.CP,
            NDF = SelectedFeed.NDF,
            ADF = SelectedFeed.ADF,
            NEL = SelectedFeed.NEL,
            ME = SelectedFeed.ME,
            Ca = SelectedFeed.Ca,
            P = SelectedFeed.P,
            Mg = SelectedFeed.Mg,
            K = SelectedFeed.K,
            RDP = SelectedFeed.RDP,
            RUP = SelectedFeed.RUP,
            NPN = SelectedFeed.NPN,
            Price = SelectedFeed.Price,
            VegetationPeriod = SelectedFeed.VegetationPeriod,
            Notes = SelectedFeed.Notes
        };
        IsNewFeed = false;
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void SaveFeed()
    {
        try
        {
            if (IsNewFeed)
            {
                _dbService.AddFeed(EditingFeed);
                StatusMessage = $"{EditingFeed.Name} başarıyla eklendi!";
            }
            else
            {
                _dbService.UpdateFeed(EditingFeed);
                StatusMessage = $"{EditingFeed.Name} başarıyla güncellendi!";
            }

            IsEditorOpen = false;
            LoadFeeds();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kaydetme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void DeleteFeed()
    {
        if (SelectedFeed == null) return;

        try
        {
            _dbService.DeleteFeed(SelectedFeed.Id);
            StatusMessage = $"{SelectedFeed.Name} silindi!";
            LoadFeeds();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Silme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditorOpen = false;
    }
}