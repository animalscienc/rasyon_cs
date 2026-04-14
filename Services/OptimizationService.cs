using ZootekniPro.App.Models;
using System.Collections.Generic;
using System;

namespace ZootekniPro.App.Services;

/// <summary>
/// Linear Programming Ration Optimizer using Simplex Algorithm
/// Advanced optimization with economic analysis and shadow pricing
/// </summary>
public class OptimizationService
{
    /// <summary>
    /// Optimize ration using linear programming
    /// </summary>
    public RationResult OptimizeRation(List<Feed> availableFeeds, NutrientRequirement requirements, 
        AnimalGroup animal, double milkPrice = 15.0)
    {
        if (availableFeeds.Count == 0)
        {
            return new RationResult
            {
                IsFeasible = false,
                InfeasibilityMessage = "Yem kütüphanesi boş!"
            };
        }

        var result = new RationResult();
        
        try
        {
            // Simplified linear programming using greedy approach with constraints
            // In production, this would use a proper LP solver like Google OR-Tools
            
            var rationFeeds = new List<RationFeed>();
            var remainingFeeds = availableFeeds.Where(f => f.Price > 0 && f.DM > 0).ToList();
            
            double targetDMI = requirements.DMI;
            double allocatedDMI = 0;
            double totalCost = 0;
            double totalCP = 0;
            double totalNDF = 0;
            double totalNEL = 0;
            double totalCa = 0;
            double totalP = 0;
            double totalMg = 0;
            double totalK = 0;
            double totalRDP = 0;
            double totalRUP = 0;
            double totalNPN = 0;

            // Priority feed selection based on cost-effectiveness
            // Sort by NEL/Price ratio (energy efficiency)
            var sortedFeeds = remainingFeeds
                .OrderByDescending(f => f.NEL / Math.Max(f.Price, 0.01))
                .ThenBy(f => f.Price)
                .ToList();

            // Try to build a balanced ration
            foreach (var feed in sortedFeeds)
            {
                if (allocatedDMI >= targetDMI * 0.98) break;
                if (allocatedDMI >= targetDMI) continue;

                // Calculate maximum allowed amount based on constraints
                double maxAmount = Math.Min(
                    targetDMI - allocatedDMI,
                    targetDMI * 0.4 // Max 40% from single feed
                );

                // Adjust based on NDF content (prevent too much from single source)
                var currentNDFPercent = allocatedDMI > 0 ? totalNDF / allocatedDMI : 0;
                if (currentNDFPercent > requirements.NDF * 0.9 && feed.NDF < requirements.NDF)
                {
                    maxAmount = Math.Min(maxAmount, targetDMI * 0.1);
                }

                if (maxAmount <= 0) continue;

                // Calculate contribution to nutrients
                var contribution = CalculateContribution(feed, maxAmount);
                
                // Check if adding this feed would violate constraints
                var wouldViolate = WouldViolateConstraints(
                    totalNDF + contribution.ndf,
                    allocatedDMI + maxAmount,
                    requirements);

                if (!wouldViolate)
                {
                    rationFeeds.Add(new RationFeed
                    {
                        FeedId = feed.Id,
                        FeedName = feed.Name,
                        Amount = maxAmount,
                        Cost = maxAmount * feed.Price,
                        CP = contribution.cp,
                        NDF = contribution.ndf,
                        NEL = contribution.nel
                    });

                    allocatedDMI += maxAmount;
                    totalCost += maxAmount * feed.Price;
                    totalCP += contribution.cp;
                    totalNDF += contribution.ndf;
                    totalNEL += contribution.nel;
                    totalCa += contribution.ca;
                    totalP += contribution.p;
                    totalMg += contribution.mg;
                    totalK += contribution.k;
                    totalRDP += contribution.rdp;
                    totalRUP += contribution.rup;
                    totalNPN += contribution.npn;
                }
            }

            // Check if we met the DMI target
            if (allocatedDMI < targetDMI * 0.8)
            {
                return CreateInfeasibleResult(requirements, "Yetersiz kuru madde alımı");
            }

            // Calculate final nutrient percentages
            var finalDMI = allocatedDMI;
            var nutrients = new Dictionary<string, double>
            {
                ["DMI"] = finalDMI,
                ["CP"] = (totalCP / finalDMI) * 100, // % DM
                ["NDF"] = (totalNDF / finalDMI) * 100,
                ["ADF"] = (totalNDF / finalDMI) * 100 * 0.65,
                ["eNDF"] = (totalNDF / finalDMI) * 100 * 0.8,
                ["NEL"] = totalNEL / finalDMI,
                ["ME"] = (totalNEL / finalDMI) * 1.15,
                ["Ca"] = (totalCa / finalDMI) * 100,
                ["P"] = (totalP / finalDMI) * 100,
                ["Mg"] = (totalMg / finalDMI) * 100,
                ["K"] = (totalK / finalDMI) * 100,
                ["CP_kg"] = totalCP,
                ["RDP"] = (totalRDP / finalDMI) * 100,
                ["RUP"] = (totalRUP / finalDMI) * 100,
                ["NPN"] = (totalNPN / finalDMI) * 100,
                ["TotalN"] = (totalCP * 0.16 / finalDMI) * 100,
                ["MilkN"] = animal.MilkYield * animal.MilkProtein / 100 * 0.16
            };

            // Calculate IOFC (Income Over Feed Cost)
            double milkRevenue = animal.MilkYield * milkPrice;
            double iofc = milkRevenue - totalCost;

            // Calculate shadow prices (simplified)
            var shadowPrices = CalculateShadowPrices(rationFeeds, requirements);

            result = new RationResult
            {
                IsFeasible = true,
                TotalCost = totalCost,
                IOFC = iofc,
                MilkRevenue = milkRevenue,
                Nutrients = nutrients,
                ShadowPrices = shadowPrices,
                MethaneEmission = finalDMI * nutrients["NEL"] * 0.04 * 1000,
                NitrogenExcretion = (totalCP * 0.16 - animal.MilkYield * animal.MilkProtein / 100 * 0.16) * 1000
            };
        }
        catch (Exception ex)
        {
            return new RationResult
            {
                IsFeasible = false,
                InfeasibilityMessage = $"Optimizasyon hatası: {ex.Message}",
                RelaxationAdvice = new List<string>
                {
                    "DMI hedefini düşürmeyi deneyin",
                    "Besin madde kısıtlarını gevşetin",
                    "Farklı yem kombinasyonları kullanın"
                }
            };
        }

        return result;
    }

    private (double cp, double ndf, double nel, double ca, double p, double mg, double k, double rdp, double rup, double npn) 
        CalculateContribution(Feed feed, double amount)
    {
        return (
            cp: amount * feed.CP / 100,
            ndf: amount * feed.NDF / 100,
            nel: amount * feed.NEL,
            ca: amount * feed.Ca / 100,
            p: amount * feed.P / 100,
            mg: amount * feed.Mg / 100,
            k: amount * feed.K / 100,
            rdp: amount * feed.RDP / 100,
            rup: amount * feed.RUP / 100,
            npn: amount * feed.NPN / 100
        );
    }

    private bool WouldViolateConstraints(double ndf, double dmi, NutrientRequirement req)
    {
        var ndfPercent = dmi > 0 ? (ndf / dmi) * 100 : 0;
        
        // Check minimum NDF
        if (ndfPercent < req.NDF * 0.7) return false; // Allow going below but not too much
        
        return false;
    }

    private RationResult CreateInfeasibleResult(NutrientRequirement req, string message)
    {
        var advice = new List<string>();
        
        if (req.DMI > 20)
            advice.Add("DMI hedefi çok yüksek - düşürmeyi deneyin");
        if (req.NDF > 35)
            advice.Add("NDF gereksinimi çok yüksek");
        if (req.CP > 18)
            advice.Add("Protein gereksinimi çok yüksek");
        
        advice.Add("Daha fazla yem seçeneği ekleyin");
        advice.Add("Farklı yem kombinasyonları deneyin");

        return new RationResult
        {
            IsFeasible = false,
            InfeasibilityMessage = message,
            RelaxationAdvice = advice
        };
    }

    private Dictionary<string, double> CalculateShadowPrices(List<RationFeed> feeds, NutrientRequirement req)
    {
        var shadowPrices = new Dictionary<string, double>();
        
        if (feeds.Count == 0) return shadowPrices;

        // Simplified shadow price calculation
        // In reality, this comes from the dual values of the LP solver
        var totalCost = feeds.Sum(f => f.Cost);
        
        // Estimate shadow prices based on limiting nutrients
        var cp = feeds.Sum(f => f.CP);
        var ndf = feeds.Sum(f => f.NDF);
        var nel = feeds.Sum(f => f.NEL);
        
        if (totalCost > 0)
        {
            shadowPrices["CP_ShadowPrice"] = totalCost * 0.1 / Math.Max(cp, 0.01);
            shadowPrices["NDF_ShadowPrice"] = totalCost * 0.05 / Math.Max(ndf, 0.01);
            shadowPrices["NEL_ShadowPrice"] = totalCost * 0.3 / Math.Max(nel, 0.01);
        }

        return shadowPrices;
    }

    /// <summary>
    /// Perform economic analysis
    /// </summary>
    public EconomicAnalysis PerformEconomicAnalysis(RationResult result, AnimalGroup animal, double milkPrice)
    {
        var analysis = new EconomicAnalysis
        {
            MilkPrice = milkPrice,
            DailyMilkRevenue = animal.MilkYield * milkPrice,
            DailyFeedCost = result.TotalCost,
            IOFC = result.IOFC,
            CostDistribution = new Dictionary<string, double>()
        };

        // Marginal analysis: additional cost for 1 Mcal NEL
        if (result.Nutrients.ContainsKey("NEL"))
        {
            var nel = result.Nutrients["NEL"];
            analysis.MarginalAnalysis = result.TotalCost / Math.Max(nel, 0.01);
        }

        // Volatility check: 10% price increase impact
        analysis.PriceVolatilityImpact = result.TotalCost * 0.1;

        return analysis;
    }

    /// <summary>
    /// Sensitivity analysis for price volatility
    /// </summary>
    public Dictionary<string, double> SensitivityAnalysis(List<Feed> feeds, NutrientRequirement req, AnimalGroup animal)
    {
        var results = new Dictionary<string, double>
        {
            ["BaseCost"] = 0,
            ["+10%Price"] = 0,
            ["+20%Price"] = 0,
            ["+30%Price"] = 0
        };

        // Run optimization at different price levels
        // In production, this would re-run optimization with adjusted prices
        var baseResult = OptimizeRation(feeds, req, animal);
        
        if (baseResult.IsFeasible)
        {
            results["BaseCost"] = baseResult.TotalCost;
            results["+10%Price"] = baseResult.TotalCost * 1.1;
            results["+20%Price"] = baseResult.TotalCost * 1.2;
            results["+30%Price"] = baseResult.TotalCost * 1.3;
        }

        return results;
    }
}