using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZootekniPro.App.Services;
using ZootekniPro.App.Models;
using System.IO;

namespace ZootekniPro.App.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly ReportService _reportService;

    [ObservableProperty]
    private ObservableCollection<SavedRation> _savedRations = new();

    [ObservableProperty]
    private SavedRation? _selectedRation;

    [ObservableProperty]
    private bool _isFullReport = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ReportInfo> _recentReports = new();

    public ReportsViewModel(DatabaseService dbService, ReportService reportService)
    {
        _dbService = dbService;
        _reportService = reportService;
        LoadSavedRations();
    }

    private void LoadSavedRations()
    {
        var rations = _dbService.GetSavedRations();
        SavedRations.Clear();
        foreach (var r in rations)
            SavedRations.Add(r);
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        if (SelectedRation == null)
        {
            StatusMessage = "Lütfen bir rasyon seçin!";
            return;
        }

        try
        {
            var rations = _dbService.GetSavedRations();
            var ration = rations.FirstOrDefault(x => x.Id == SelectedRation.Id);
            if (ration != null)
            {
                var pdfBytes = _reportService.GenerateRationReport(new Ration { Name = ration.Name, Version = "v" + ration.Version });
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ration.Name + ".pdf");
                await File.WriteAllBytesAsync(path, pdfBytes);
                StatusMessage = "Rapor oluşturuldu: " + ration.Name + ".pdf";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = "Hata: " + ex.Message;
        }
    }
}

public class ReportInfo
{
    public string Date { get; set; } = "";
    public string Name { get; set; } = "";
    public string AnimalGroup { get; set; } = "";
}