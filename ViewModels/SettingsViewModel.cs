using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZootekniPro.App.Services;

namespace ZootekniPro.App.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;

    [ObservableProperty]
    private double _endfMin = 20;

    [ObservableProperty]
    private double _rdpCpMin = 60;

    [ObservableProperty]
    private double _caPRatio = 1.8;

    [ObservableProperty]
    private double _dcad = 100;

    [ObservableProperty]
    private double _npnMax = 30;

    [ObservableProperty]
    private double _milkPrice = 15;

    [ObservableProperty]
    private double _feedCostMultiplier = 1.0;

    [ObservableProperty]
    private double _iofcTarget = 50;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(DatabaseService dbService)
    {
        _dbService = dbService;
        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _dbService.GetSettings();
        if (settings != null)
        {
            EndfMin = settings.EndfMin;
            RdpCpMin = settings.RdpCpMin;
            CaPRatio = settings.CaPRatio;
            Dcad = settings.Dcad;
            NpnMax = settings.NpnMax;
            MilkPrice = settings.MilkPrice;
            FeedCostMultiplier = settings.FeedCostMultiplier;
            IofcTarget = settings.IofcTarget;
        }
    }

    [RelayCommand]
    private void SaveSettings()
    {
        var settings = new AppSettings
        {
            EndfMin = EndfMin,
            RdpCpMin = RdpCpMin,
            CaPRatio = CaPRatio,
            Dcad = Dcad,
            NpnMax = NpnMax,
            MilkPrice = MilkPrice,
            FeedCostMultiplier = FeedCostMultiplier,
            IofcTarget = IofcTarget
        };

        _dbService.SaveSettings(settings);
        StatusMessage = "Ayarlar kaydedildi! ✅";

        Task.Delay(3000).ContinueWith(_ =>
        {
            StatusMessage = "";
        });
    }
}

public class AppSettings
{
    public double EndfMin { get; set; } = 20;
    public double RdpCpMin { get; set; } = 60;
    public double CaPRatio { get; set; } = 1.8;
    public double Dcad { get; set; } = 100;
    public double NpnMax { get; set; } = 30;
    public double MilkPrice { get; set; } = 15;
    public double FeedCostMultiplier { get; set; } = 1.0;
    public double IofcTarget { get; set; } = 50;
}