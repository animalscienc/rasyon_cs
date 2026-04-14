using ZootekniPro.App.Models;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ZootekniPro.App.Services;

public class FeedDataService
{
    private readonly DatabaseService _dbService;

    public FeedDataService(DatabaseService dbService)
    {
        _dbService = dbService;
    }

    public async Task ImportFeedsFromCsvAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Feed data file not found: {filePath}");

        var lines = await File.ReadAllLinesAsync(filePath);
        if (lines.Length < 2) return; // No data

        var header = lines[0].Split(';');
        
        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(';');
            if (values.Length < 5) continue;

            var feed = new Feed
            {
                Name = GetValue(values, header, "Ad", ""),
                Category = GetValue(values, header, "Kategori", "Yem"),
                Origin = GetValue(values, header, "Köken", ""),
                DM = GetDoubleValue(values, header, "KM"),
                CP = GetDoubleValue(values, header, "HP"),
                NDF = GetDoubleValue(values, header, "NDF"),
                ADF = GetDoubleValue(values, header, "ADF"),
                NEL = GetDoubleValue(values, header, "NEL"),
                ME = GetDoubleValue(values, header, "ME"),
                Ca = GetDoubleValue(values, header, "Ca"),
                P = GetDoubleValue(values, header, "P"),
                Mg = GetDoubleValue(values, header, "Mg"),
                K = GetDoubleValue(values, header, "K"),
                RDP = GetDoubleValue(values, header, "RDP"),
                RUP = GetDoubleValue(values, header, "RUP"),
                NPN = GetDoubleValue(values, header, "NPN"),
                Price = GetDoubleValue(values, header, "Fiyat"),
                VegetationPeriod = GetValue(values, header, "Vejetasyon", ""),
                Notes = GetValue(values, header, "Notlar", "")
            };

            _dbService.AddFeed(feed);
        }
    }

    public async Task ExportFeedsToCsvAsync(string filePath)
    {
        var feeds = _dbService.GetAllFeeds();
        var lines = new List<string>
        {
            "Id;Ad;Kategori;Köken;KM;HP;NDF;ADF;NEL;ME;Ca;P;Mg;K;RDP;RUP;NPN;Fiyat;Vejetasyon;Notlar"
        };

        foreach (var feed in feeds)
        {
            lines.Add($"{feed.Id};{feed.Name};{feed.Category};{feed.Origin};{feed.DM};{feed.CP};" +
                     $"{feed.NDF};{feed.ADF};{feed.NEL};{feed.ME};{feed.Ca};{feed.P};{feed.Mg};" +
                     $"{feed.K};{feed.RDP};{feed.RUP};{feed.NPN};{feed.Price};{feed.VegetationPeriod};{feed.Notes}");
        }

        await File.WriteAllLinesAsync(filePath, lines);
    }

    public List<Feed> SearchFeeds(string searchText, string? category = null)
    {
        var allFeeds = _dbService.GetAllFeeds();
        
        if (string.IsNullOrWhiteSpace(searchText) && string.IsNullOrWhiteSpace(category))
            return allFeeds;

        return allFeeds.Where(f =>
        {
            var matchesSearch = string.IsNullOrWhiteSpace(searchText) ||
                f.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                f.Origin.Contains(searchText, StringComparison.OrdinalIgnoreCase);
            
            var matchesCategory = string.IsNullOrWhiteSpace(category) ||
                f.Category.Equals(category, StringComparison.OrdinalIgnoreCase);
            
            return matchesSearch && matchesCategory;
        }).ToList();
    }

    public List<string> GetCategories()
    {
        return _dbService.GetAllFeeds()
            .Select(f => f.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    private string GetValue(string[] values, string[] header, string columnName, string defaultValue)
    {
        var index = Array.FindIndex(header, h => h.Trim().Equals(columnName, StringComparison.OrdinalIgnoreCase));
        if (index >= 0 && index < values.Length && !string.IsNullOrWhiteSpace(values[index]))
            return values[index].Trim();
        return defaultValue;
    }

    private double GetDoubleValue(string[] values, string[] header, string columnName)
    {
        var value = GetValue(values, header, columnName, "0");
        if (double.TryParse(value.Replace(',', '.'), System.Globalization.NumberStyles.Any, 
            System.Globalization.CultureInfo.InvariantCulture, out var result))
            return result;
        return 0;
    }
}