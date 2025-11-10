using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Dedicated seeder for Reports Hub screen translations
/// </summary>
public static class ReportsHubTranslationSeeder
{
    public static async Task SeedReportsHubTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var reportsHubTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page Header
            {
                "reports_hub_title",
                new Dictionary<string, string>
                {
                    { "en", "Reports" },
                    { "ur", "رپورٹس" }
                }
            },
            {
                "reports_hub_subtitle",
                new Dictionary<string, string>
                {
                    { "en", "Access comprehensive business reports and analytics" },
                    { "ur", "جامع کاروباری رپورٹس اور تجزیات تک رسائی حاصل کریں" }
                }
            },
            
            // Sales Report Card
            {
                "reports_hub_sales_title",
                new Dictionary<string, string>
                {
                    { "en", "Sales Report" },
                    { "ur", "سیلز رپورٹ" }
                }
            },
            {
                "reports_hub_sales_description",
                new Dictionary<string, string>
                {
                    { "en", "View comprehensive sales analytics including daily, monthly, and yearly reports. Analyze product performance, payment methods, and customer insights." },
                    { "ur", "روزانہ، ماہانہ، اور سالانہ رپورٹس سمیت جامع سیلز تجزیات دیکھیں۔ مصنوعات کی کارکردگی، ادائیگی کے طریقے، اور کسٹمر کی معلومات کا تجزیہ کریں۔" }
                }
            },
            
            // Inventory Report Card
            {
                "reports_hub_inventory_title",
                new Dictionary<string, string>
                {
                    { "en", "Inventory Report" },
                    { "ur", "انوینٹری رپورٹ" }
                }
            },
            {
                "reports_hub_inventory_description",
                new Dictionary<string, string>
                {
                    { "en", "Monitor stock levels, track inventory movements, identify low stock items, and analyze stock valuation across all products." },
                    { "ur", "اسٹاک کی سطح کی نگرانی کریں، انوینٹری کی نقل و حرکت کو ٹریک کریں، کم اسٹاک آئٹمز کی شناخت کریں، اور تمام مصنوعات میں اسٹاک کی قیمت کا تجزیہ کریں۔" }
                }
            },
            
            // Cash Report Card
            {
                "reports_hub_cash_title",
                new Dictionary<string, string>
                {
                    { "en", "Cash Report" },
                    { "ur", "نقد رپورٹ" }
                }
            },
            {
                "reports_hub_cash_description",
                new Dictionary<string, string>
                {
                    { "en", "Track cash flow, analyze payment methods, review shift summaries, and monitor cash drawer reconciliation." },
                    { "ur", "نقد رقم کے بہاؤ کو ٹریک کریں، ادائیگی کے طریقوں کا تجزیہ کریں، شفٹ کے خلاصوں کا جائزہ لیں، اور کیش ڈرار ملاپ کی نگرانی کریں۔" }
                }
            },
            
            // Customer Report Card
            {
                "reports_hub_customer_title",
                new Dictionary<string, string>
                {
                    { "en", "Customer Report" },
                    { "ur", "کسٹمر رپورٹ" }
                }
            },
            {
                "reports_hub_customer_description",
                new Dictionary<string, string>
                {
                    { "en", "Analyze customer purchase patterns, identify top customers, track loyalty metrics, and review customer lifetime value." },
                    { "ur", "کسٹمر کی خریداری کے نمونوں کا تجزیہ کریں، اعلیٰ کسٹمرز کی شناخت کریں، وفاداری کی میٹرکس کو ٹریک کریں، اور کسٹمر کی زندگی بھر کی قیمت کا جائزہ لیں۔" }
                }
            },
            
            // Common Labels
            {
                "reports_hub_view_report",
                new Dictionary<string, string>
                {
                    { "en", "View Report →" },
                    { "ur", "← رپورٹ دیکھیں" }
                }
            },
            {
                "reports_hub_coming_soon",
                new Dictionary<string, string>
                {
                    { "en", "Coming Soon" },
                    { "ur", "جلد آرہا ہے" }
                }
            }
        };

        foreach (var translation in reportsHubTranslations)
        {
            var keyword = translation.Key;
            var translations = translation.Value;

            // Add language keyword if it doesn't exist
            await localizationService.AddLanguageKeywordAsync(keyword);

            // Add translations for each language
            foreach (var lang in translations)
            {
                await localizationService.SaveTranslationAsync(keyword, lang.Value, lang.Key);
            }
        }
    }
}
