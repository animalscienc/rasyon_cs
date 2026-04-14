namespace ZootekniPro.App.Models;

public class Feed
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public double DM { get; set; }  // Dry Matter %
    public double CP { get; set; }  // Crude Protein % DM
    public double NDF { get; set; } // NDF % DM
    public double ADF { get; set; } // ADF % DM
    public double NEL { get; set; } // NEL Mcal/kg DM
    public double ME { get; set; }  // ME Mcal/kg DM
    public double Ca { get; set; }  // Calcium % DM
    public double P { get; set; }   // Phosphorus % DM
    public double Mg { get; set; }  // Magnesium % DM
    public double K { get; set; }   // Potassium % DM
    public double RDP { get; set; } // Rumen Degradable Protein % DM
    public double RUP { get; set; } // Rumen Undegradable Protein % DM
    public double NPN { get; set; } // Non-Protein Nitrogen % DM
    public double Price { get; set; } // TL/kg DM
    public string VegetationPeriod { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}