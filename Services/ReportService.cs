using ZootekniPro.App.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ZootekniPro.App.Services;

public class ReportService
{
    public byte[] GenerateRationReport(Ration ration, string businessName = "Zootekni Pro")
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, businessName, ration.Name, ration.Version));
                page.Content().Element(c => ComposeContent(c, ration));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, string businessName, string rationName, string rationVersion)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("ZOOTEKNİ PRO")
                    .Bold().FontSize(20).FontColor(Colors.Indigo.Medium);
                col.Item().Text("Intelligent Rationing System")
                    .FontSize(12).FontColor(Colors.Grey.Medium);
            });

            row.ConstantItem(150).Column(col =>
            {
                col.Item().AlignRight().Text($"Tarih: {DateTime.Now:dd.MM.yyyy}")
                    .FontSize(10);
                col.Item().AlignRight().Text($"Rasyon: {rationName}")
                    .FontSize(10);
                col.Item().AlignRight().Text($"Versiyon: {rationVersion}")
                    .FontSize(10);
            });
        });

        container.PaddingVertical(10);
    }

    private void ComposeContent(IContainer container, Ration ration)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Animal Group Info
            col.Spacing(10);
            col.Item().Element(c => ComposeSection(c, "Hayvan Grubu Bilgileri"));
            
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"Grup: {ration.AnimalGroupName}");
                row.RelativeItem().Text($"Canlı Ağırlık: {ration.Result?.Nutrients.GetValueOrDefault("BodyWeight", 0):F0} kg");
                row.RelativeItem().Text($"Süt Verimi: {ration.Result?.Nutrients.GetValueOrDefault("MilkYield", 0):F1} kg/gün");
            });

            // Ration Summary
            col.Item().Element(c => ComposeSection(c, "Rasyon Özeti"));
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"Toplam Kuru Madde: {ration.TotalDM:F2} kg/gün");
                row.RelativeItem().Text($"Toplam Maliyet: {ration.TotalCost:F2} TL/gün");
                row.RelativeItem().Text($"IOFC: {ration.Result?.IOFC:F2} TL/gün");
            });

            // Feed Composition
            col.Item().Element(c => ComposeSection(c, "Yem Kompozisyonu"));
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Indigo.Medium).Padding(5)
                        .Text("Yem Adı").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Indigo.Medium).Padding(5)
                        .Text("Miktar (kg)").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Indigo.Medium).Padding(5)
                        .Text("Maliyet (TL)").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Indigo.Medium).Padding(5)
                        .Text("HP (%)").FontColor(Colors.White).Bold();
                    header.Cell().Background(Colors.Indigo.Medium).Padding(5)
                        .Text("NEL (Mcal)").FontColor(Colors.White).Bold();
                });

                foreach (var feed in ration.Feeds)
                {
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text(feed.FeedName);
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{feed.Amount:F2}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{feed.Cost:F2}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{feed.CP / feed.Amount * 100:F1}");
                    table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                        .Text($"{feed.NEL:F2}");
                }
            });

            // Nutrient Analysis
            col.Item().Element(c => ComposeSection(c, "Besin Madde Analizi"));
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Enerji:");
                    c.Item().Text($"  - NEL: {ration.Result?.Nutrients.GetValueOrDefault("NEL", 0):F2} Mcal/kg DM");
                    c.Item().Text($"  - ME: {ration.Result?.Nutrients.GetValueOrDefault("ME", 0):F2} Mcal/kg DM");
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Protein:");
                    c.Item().Text($"  - HP: {ration.Result?.Nutrients.GetValueOrDefault("CP", 0):F1}% DM");
                    c.Item().Text($"  - RDP: {ration.Result?.Nutrients.GetValueOrDefault("RDP", 0):F1}% DM");
                    c.Item().Text($"  - RUP: {ration.Result?.Nutrients.GetValueOrDefault("RUP", 0):F1}% DM");
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Lif:");
                    c.Item().Text($"  - NDF: {ration.Result?.Nutrients.GetValueOrDefault("NDF", 0):F1}% DM");
                    c.Item().Text($"  - ADF: {ration.Result?.Nutrients.GetValueOrDefault("ADF", 0):F1}% DM");
                    c.Item().Text($"  - eNDF: {ration.Result?.Nutrients.GetValueOrDefault("eNDF", 0):F1}% DM");
                });
            });

            // Environmental Impact
            col.Item().Element(c => ComposeSection(c, "Çevresel Etki"));
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"Metan Emisyonu: {ration.Result?.MethaneEmission:F1} g/gün");
                row.RelativeItem().Text($"Azot Atılımı: {ration.Result?.NitrogenExcretion:F1} g/gün");
            });

            // Economic Summary
            col.Item().Element(c => ComposeSection(c, "Ekonomik Özet"));
            col.Item().Row(row =>
            {
                row.RelativeItem().Text($"Günlük Süt Geliri: {ration.Result?.MilkRevenue:F2} TL");
                row.RelativeItem().Text($"Günlük Yem Maliyeti: {ration.TotalCost:F2} TL");
                row.RelativeItem().Text($"IOFC: {ration.Result?.IOFC:F2} TL").Bold();
            });
        });
    }

    private void ComposeSection(IContainer container, string title)
    {
        container.Background(Colors.Indigo.Lighten5).Padding(10).Column(col =>
        {
            col.Item().Text(title).Bold().FontSize(12).FontColor(Colors.Indigo.Medium);
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.Span("Sayfa ").FontSize(8);
            text.CurrentPageNumber().FontSize(8);
            text.Span(" / ").FontSize(8);
            text.TotalPages().FontSize(8);
            text.Span(" - Zootekni Pro v5.0").FontSize(8).FontColor(Colors.Grey.Medium);
        });
    }
}