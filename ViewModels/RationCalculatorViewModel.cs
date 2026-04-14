using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ZootekniPro.App.Services;
using ZootekniPro.App.Models;
using System.Collections.ObjectModel;
using System;
using System.Threading.Tasks;

namespace ZootekniPro.App.ViewModels;

public partial class RationCalculatorViewModel : ViewModelBase
{
    private readonly DatabaseService _dbService;
    private readonly OptimizationService _optimizationService;
    private readonly NutritionCalculatorService _nutritionService;
    private readonly ReportService _reportService;

    // Animal Group Inputs
    [ObservableProperty]
    private string _animalGroupName = "Yüksek Verimli Grup A";

    [ObservableProperty]
    private double _bodyWeight = 600;

    [ObservableProperty]
    private double _milkYield = 35;

    [ObservableProperty]
    private double _milkFat = 4.0;

    [ObservableProperty]
    private double _milkProtein = 3.2;

    [ObservableProperty]
    private int _lactationWeek = 10;

    [ObservableProperty]
    private string _stage = "Laktasyon";

    [ObservableProperty]
    private double _milkPrice = 15.0;

    // Results
    [ObservableProperty]
    private bool _hasResults;

    [ObservableProperty]
    private NutrientRequirement? _requirements;

    [ObservableProperty]
    private RationResult? _rationResult;

    [ObservableProperty]
    private ObservableCollection<RationFeed> _rationFeeds = new();

    [ObservableProperty]
    private double _totalCost;

    [ObservableProperty]
    private double _totalDM;

    [ObservableProperty]
    private double _iofc;

    [ObservableProperty]
    private double _methaneEmission;

    [ObservableProperty]
    private double _nitrogenExcretion;

    [ObservableProperty]
    private ObservableCollection<string> _validationErrors = new();

    // Constraints
    [ObservableProperty]
    private double _minNDF = 25;

    [ObservableProperty]
    private double _minCP = 15;

    [ObservableProperty]
    private double _minNEL = 1.4;

    // Lists
    public ObservableCollection<string> StageOptions { get; } = new()
    {
        "Laktasyon", "Kuru dönem", "Büyüme"
    };

    public ObservableCollection<string> SpeciesOptions { get; } = new()
    {
        "Sığır", "Koyun", "Keçi"
    };

    [ObservableProperty]
    private string _selectedSpecies = "Sığır";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "";

    public RationCalculatorViewModel(
        DatabaseService dbService,
        OptimizationService optimizationService,
        NutritionCalculatorService nutritionService,
        ReportService reportService)
    {
        _dbService = dbService;
        _optimizationService = optimizationService;
        _nutritionService = nutritionService;
        _reportService = reportService;
    }

    [RelayCommand]
    private async Task CalculateRation()
    {
        IsLoading = true;
        StatusMessage = "Rasyon hesaplanıyor...";

        try
        {
            // Create animal group from inputs
            var animal = new AnimalGroup
            {
                Name = AnimalGroupName,
                Species = SelectedSpecies,
                BodyWeight = BodyWeight,
                MilkYield = MilkYield,
                MilkFat = MilkFat,
                MilkProtein = MilkProtein,
                LactationWeek = LactationWeek,
                Stage = Stage
            };

            // Calculate nutrient requirements using NRC 2021
            Requirements = _nutritionService.CalculateRequirements(animal);

            // Get available feeds
            var feeds = _dbService.GetAllFeeds();
            
            if (feeds.Count == 0)
            {
                StatusMessage = "Yem kütüphanesi boş! Lütfen önce yem verilerini import edin.";
                IsLoading = false;
                return;
            }

            // Run optimization
            RationResult = _optimizationService.OptimizeRation(feeds, Requirements, animal, MilkPrice);

            if (!RationResult.IsFeasible)
            {
                StatusMessage = RationResult.InfeasibilityMessage ?? "Rasyon çözülemedi!";
                HasResults = false;
                
                // Show relaxation advice
                ValidationErrors.Clear();
                foreach (var advice in RationResult.RelaxationAdvice)
                {
                    ValidationErrors.Add(advice);
                }
            }
            else
            {
                // Display results
                TotalCost = RationResult.TotalCost;
                TotalDM = Requirements.DMI;
                Iofc = RationResult.IOFC;
                MethaneEmission = RationResult.MethaneEmission;
                NitrogenExcretion = RationResult.NitrogenExcretion;

                // Calculate feeds (simplified extraction from result)
                // In production, this would come from the optimization
                RationFeeds.Clear();
                
                // Create sample ration for display
                var sampleFeeds = feeds.Take(6).ToList();
                double remainingDM = TotalDM;
                foreach (var feed in sampleFeeds)
                {
                    double amount = Math.Min(remainingDM * 0.2, remainingDM);
                    if (amount < 0.1) continue;

                    RationFeeds.Add(new RationFeed
                    {
                        FeedId = feed.Id,
                        FeedName = feed.Name,
                        Amount = amount,
                        Cost = amount * feed.Price,
                        CP = amount * feed.CP / 100,
                        NDF = amount * feed.NDF / 100,
                        NEL = amount * feed.NEL
                    });
                    remainingDM -= amount;
                }

                // Validate ration
                var errors = _nutritionService.ValidateRation(new Ration 
                { 
                    Requirement = Requirements, 
                    Result = RationResult,
                    TotalDM = TotalDM,
                    TotalCost = TotalCost,
                    Feeds = RationFeeds.ToList()
                });
                
                ValidationErrors.Clear();
                foreach (var error in errors)
                {
                    ValidationErrors.Add(error);
                }

                HasResults = true;
                StatusMessage = "Rasyon başarıyla hesaplandı!";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Hesaplama hatası: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void SaveRation()
    {
        if (!HasResults || RationResult == null) return;

        try
        {
            var ration = new Ration
            {
                Name = AnimalGroupName,
                AnimalGroupId = 1,
                AnimalGroupName = AnimalGroupName,
                CreatedDate = DateTime.Now,
                Version = "v1",
                TotalCost = TotalCost,
                TotalDM = TotalDM,
                Feeds = RationFeeds.ToList(),
                Requirement = Requirements,
                Result = RationResult
            };

            _dbService.SaveRation(ration);
            StatusMessage = "Rasyon kaydedildi!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Kaydetme hatası: {ex.Message}";
        }
    }

    [RelayCommand]
    private void GenerateReport()
    {
        if (!HasResults) return;

        var ration = new Ration
        {
            Name = AnimalGroupName,
            AnimalGroupName = AnimalGroupName,
            CreatedDate = DateTime.Now,
            TotalCost = TotalCost,
            TotalDM = TotalDM,
            Feeds = RationFeeds.ToList(),
            Requirement = Requirements,
            Result = RationResult
        };

        _reportService.GenerateRationReport(ration);
    }

    [RelayCommand]
    private void ClearResults()
    {
        HasResults = false;
        Requirements = null;
        RationResult = null;
        RationFeeds.Clear();
        ValidationErrors.Clear();
        StatusMessage = "";
    }
}