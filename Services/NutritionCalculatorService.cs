using ZootekniPro.App.Models;
using System.Collections.Generic;

namespace ZootekniPro.App.Services;

/// <summary>
/// NRC 2021 Besin Madde İhtiyaç Hesaplama Servisi
/// Advanced nutrient requirement calculations based on NRC 2021, INRA, and ARC standards
/// </summary>
public class NutritionCalculatorService
{
    /// <summary>
    /// NRC 2021'e göre kuru madde alımı (DMI) hesapla
    /// </summary>
    public double CalculateDMI(AnimalGroup animal)
    {
        // NRC 2021 Dairy Cattle equations
        double dmi = 0;
        
        if (animal.Stage == "Laktasyon" && animal.MilkYield > 0)
        {
            // Lactating cow DMI equation
            double nelson_1985 = (0.372 * animal.MilkYield * (0.096 * animal.MilkFat / 100 + 0.032)) / 
                                  (1 - 0.072 * (0.096 * animal.MilkFat / 100 + 0.032));
            
            double bodyWeight = animal.BodyWeight;
            double fcm = animal.MilkYield * (0.4 + 0.15 * animal.MilkFat / 100); // Fat corrected milk
            
            // NRC 2021 equation for lactating cows
            dmi = (0.372 * fcm + 0.0968 * Math.Pow(bodyWeight, 0.75) * 
                   Math.Exp(-0.0008 * animal.LactationWeek)) * 1.05; // 1.05 for safety margin
        }
        else if (animal.Stage == "Kuru dönem")
        {
            // Dry period DMI - 1.3-1.5% of body weight
            dmi = animal.BodyWeight * (animal.LactationWeek > 21 ? 0.013 : 0.015);
        }
        else if (animal.IsHeifer || animal.Stage == "Büyüme")
        {
            // Growing heifer DMI
            double shrunkBw = animal.BodyWeight * 0.96;
            double adg = animal.ADG > 0 ? animal.ADG : 0.5; // Default 500g/day
            
            // NRC 2021 growing heifer equation
            dmi = (0.072 * shrunkBw * 0.75) + 
                  (adg * (1.3 + 0.03 * adg)) / (0.85 * (2.7 + 0.31 * adg));
        }
        else
        {
            // Maintenance DMI
            dmi = animal.BodyWeight * 0.025;
        }

        return Math.Max(dmi, 1.0); // Minimum 1 kg DMI
    }

    /// <summary>
    /// NRC 2021 Besin madde ihtiyaçlarını hesapla
    /// </summary>
    public NutrientRequirement CalculateRequirements(AnimalGroup animal)
    {
        var req = new NutrientRequirement();
        
        // Calculate DMI first
        req.DMI = CalculateDMI(animal);
        req.DMIPercentBW = (req.DMI / animal.BodyWeight) * 100;

        if (animal.Stage == "Laktasyon" && animal.MilkYield > 0)
        {
            // Energy requirements (NEL Mcal/kg DM)
            double maintenanceNEl = 0.08 * Math.Pow(animal.BodyWeight, 0.75);
            double lactationNEl = animal.MilkYield * (0.092 * animal.MilkFat / 100 + 0.056 * animal.MilkProtein / 100 + 0.04);
            
            // Total NEL requirement (with 10% safety margin)
            req.NEL = ((maintenanceNEl + lactationNEl) / req.DMI) * 1.10;
            req.ME = req.NEL * 1.15; // Convert NEL to ME

            // Protein requirements (g/day)
            double milkProtein = animal.MilkYield * animal.MilkProtein / 100;
            double maintenanceCP = animal.BodyWeight * 0.006; // 6g CP per kg BW
            double lactationCP = milkProtein * 1.36; // 36% efficiency
            
            req.CP = ((maintenanceCP + lactationCP) / req.DMI) * 1000; // Convert to % DM
            req.RDP = req.CP * 0.65; // 65% of CP is RDP
            req.RUP = req.CP * 0.35; // 35% of CP is RUP

            // Fiber requirements (NDF % DM)
            req.NDF = Math.Max(25, 28 - 0.05 * animal.MilkYield);
            req.eNDF = Math.Max(20, req.NDF - 5); // Effective NDF
            req.ADF = req.NDF * 0.65;

            // Mineral requirements
            req.Ca = 0.45 + (animal.MilkYield * 0.019); // g/day -> % DM
            req.P = 0.30 + (animal.MilkYield * 0.009);
            req.Mg = 0.15 + (animal.MilkYield * 0.003);
            req.K = 0.8 + (animal.MilkYield * 0.04);
            req.DCAD = 150; // mEq/kg DM for lactating cows
        }
        else if (animal.Stage == "Kuru dönem")
        {
            // Dry period requirements
            req.NEL = 1.0; // Mcal/kg DM
            req.ME = 1.15;
            req.CP = 12; // % DM
            req.RDP = req.CP * 0.80;
            req.RUP = req.CP * 0.20;
            req.NDF = 35;
            req.eNDF = 25;
            req.ADF = 30;
            
            // Minerals - negative DCAD for dry period
            req.Ca = 0.6;
            req.P = 0.4;
            req.Mg = 0.4;
            req.K = 0.7;
            req.DCAD = -150; // Negative for dry period (anionic)
        }
        else
        {
            // Growing/ maintenance
            req.NEL = 1.1;
            req.ME = 1.25;
            req.CP = 14;
            req.RDP = req.CP * 0.60;
            req.RUP = req.CP * 0.40;
            req.NDF = 30;
            req.eNDF = 22;
            req.ADF = 25;
            req.Ca = 0.5;
            req.P = 0.35;
            req.Mg = 0.2;
            req.K = 0.7;
        }

        // Convert to g/day for minerals
        req.Ca = (req.Ca / 100 * req.DMI * 1000); // g/day - fix property name
        req.P = (req.P / 100 * req.DMI * 1000);
        req.Mg = (req.Mg / 100 * req.DMI * 1000);
        req.K = (req.K / 100 * req.DMI * 1000);

        return req;
    }

    /// <summary>
    /// Validate ration against constraints
    /// </summary>
    public List<string> ValidateRation(Ration ration)
    {
        var errors = new List<string>();
        
        if (ration.Result == null || !ration.Result.IsFeasible)
        {
            errors.Add("Rasyon çözülebilir değil!");
            return errors;
        }

        if (ration.Requirement == null)
            return errors;

        var result = ration.Result;
        
        // Check NDF constraint
        var ndfActual = result.Nutrients.GetValueOrDefault("NDF", 0);
        if (ndfActual < ration.Requirement.NDF)
            errors.Add($"NDF yetersiz: %{ndfActual:F1} (Min: %{ration.Requirement.NDF:F1})");

        // Check eNDF constraint
        var endfActual = result.Nutrients.GetValueOrDefault("eNDF", 0);
        if (endfActual < ration.Requirement.eNDF)
            errors.Add($"eNDF yetersiz: %{endfActual:F1} (Min: %{ration.Requirement.eNDF:F1})");

        // Check Ca/P ratio
        var ca = result.Nutrients.GetValueOrDefault("Ca", 0);
        var p = result.Nutrients.GetValueOrDefault("P", 0);
        if (p > 0)
        {
            var ratio = ca / p;
            if (ratio < 1.6 || ratio > 2.1)
                errors.Add($"Ca/P oranı: {ratio:F2} (İdeal: 1.6-2.1)");
        }

        // Check K/Mg for tetany risk
        var k = result.Nutrients.GetValueOrDefault("K", 0);
        var mg = result.Nutrients.GetValueOrDefault("Mg", 0);
        if (mg > 0)
        {
            var k_mg_ratio = k / mg;
            if (k_mg_ratio > 2.2)
                errors.Add($"K/Mg oranı yüksek: {k_mg_ratio:F2} (Tetani riski)");
        }

        // Check NPN limit (max 30% of total N)
        var npn = result.Nutrients.GetValueOrDefault("NPN", 0);
        var totalN = result.Nutrients.GetValueOrDefault("TotalN", 1);
        if (totalN > 0 && npn / totalN > 0.30)
            errors.Add($"NPN sınırı aşıldı: %{npn/totalN*100:F1} (Max: %30)");

        return errors;
    }

    /// <summary>
    /// Calculate environmental impact (methane and nitrogen)
    /// </summary>
    public (double methane, double nitrogen) CalculateEnvironmentalImpact(Ration ration)
    {
        if (ration.Result == null)
            return (0, 0);

        double methane = 0;
        double nitrogen = 0;

        // Methane emission estimation (Mills et al. approach)
        // CH4 (g/day) = (NEM intake * 0.04 + NEF * 0.02) * 1000
        var nel = ration.Result.Nutrients.GetValueOrDefault("NEL", 0);
        var dmi = ration.TotalDM;
        
        if (nel > 0)
        {
            // Using IPCC Tier 2 method
            methane = dmi * nel * 0.04 * 1000; // g/day
        }

        // Nitrogen excretion (N-balance)
        var cpIntake = ration.Result.Nutrients.GetValueOrDefault("CP_kg", 0);
        var milkN = ration.Result.Nutrients.GetValueOrDefault("MilkN", 0);
        
        if (cpIntake > 0)
        {
            // N excretion = Intake N - Milk N - retained N
            var intakeN = cpIntake * 0.16; // CP * 0.16 = N
            nitrogen = (intakeN - milkN) * 1000; // g/day
        }

        return (methane, nitrogen);
    }
}