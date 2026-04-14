using System;
using System.Collections.Generic;

namespace ZootekniPro.App.Models;

public class AnimalGroup
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = "Sığır"; // Sığır, Koyun, Keçi, Kanatlı
    public double BodyWeight { get; set; } // kg
    public double MilkYield { get; set; } // kg/gün
    public double MilkFat { get; set; } // %
    public double MilkProtein { get; set; } // %
    public int LactationWeek { get; set; } // 1-40
    public string Stage { get; set; } = "Laktasyon"; // Laktasyon, Kuru dönem, Büyüme
    public double ADG { get; set; } // Average Daily Gain - günlük canlı ağırlık artışı
    public bool IsHeifer { get; set; } // Doğurganlık öncesi
}

public class NutrientRequirement
{
    public double DMI { get; set; } // Dry Matter Intake - kg/gün
    public double DMIPercentBW { get; set; } // DMI as % of BW
    public double CP { get; set; } // Crude Protein g/gün
    public double NDF { get; set; } // NDF % DM
    public double eNDF { get; set; } // Effective NDF % DM
    public double ADF { get; set; } // ADF % DM
    public double NEL { get; set; } // NEL Mcal/kg DM
    public double ME { get; set; } // ME Mcal/kg DM
    public double Ca { get; set; } // Calcium g/gün
    public double P { get; set; } // Phosphorus g/gün
    public double Mg { get; set; } // Magnesium g/gün
    public double K { get; set; } // Potassium g/gün
    public double RDP { get; set; } // Rumen Degradable Protein g/gün
    public double RUP { get; set; } // Rumen Undegradable Protein g/gün
    public double DCAD { get; set; } // Dietary Cation-Anion Difference mEq/kg DM
}

public class Ration
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int AnimalGroupId { get; set; }
    public string AnimalGroupName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public string Version { get; set; } = "v1";
    public double TotalCost { get; set; }
    public double TotalDM { get; set; }
    public List<RationFeed> Feeds { get; set; } = new();
    public NutrientRequirement? Requirement { get; set; }
    public RationResult? Result { get; set; }
}

public class RationFeed
{
    public int FeedId { get; set; }
    public string FeedName { get; set; } = string.Empty;
    public double Amount { get; set; } // kg DM
    public double Cost { get; set; }
    public double CP { get; set; }
    public double NDF { get; set; }
    public double NEL { get; set; }
}

public class RationResult
{
    public bool IsFeasible { get; set; }
    public double TotalCost { get; set; }
    public double IOFC { get; set; } // Income Over Feed Cost
    public double MilkRevenue { get; set; }
    public Dictionary<string, double> Nutrients { get; set; } = new();
    public Dictionary<string, double> ShadowPrices { get; set; } = new();
    public double MethaneEmission { get; set; } // g/gün
    public double NitrogenExcretion { get; set; } // g/gün
    public string? InfeasibilityMessage { get; set; }
    public List<string> RelaxationAdvice { get; set; } = new();
}

public class EconomicAnalysis
{
    public double DailyMilkRevenue { get; set; }
    public double DailyFeedCost { get; set; }
    public double IOFC { get; set; }
    public double MilkPrice { get; set; } // TL/kg
    public double MarginalAnalysis { get; set; }
    public double PriceVolatilityImpact { get; set; } // %10 artış etkisi
    public Dictionary<string, double> CostDistribution { get; set; } = new();
}