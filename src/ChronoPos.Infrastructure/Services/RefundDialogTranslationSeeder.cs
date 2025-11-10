using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Dedicated seeder for Refund Dialog translations
/// </summary>
public static class RefundDialogTranslationSeeder
{
    public static async Task SeedRefundDialogTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var refundDialogTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "refund_dialog_title",
                new Dictionary<string, string>
                {
                    { "en", "Process Refund" },
                    { "ur", "واپسی کی کارروائی" }
                }
            },
            {
                "refund_dialog_select_items",
                new Dictionary<string, string>
                {
                    { "en", "Select Items to Refund" },
                    { "ur", "واپس کرنے کے لیے اشیاء منتخب کریں" }
                }
            },
            {
                "refund_dialog_unit_price",
                new Dictionary<string, string>
                {
                    { "en", "Unit Price:" },
                    { "ur", ":یونٹ قیمت" }
                }
            },
            {
                "refund_dialog_original_qty",
                new Dictionary<string, string>
                {
                    { "en", "Original Qty:" },
                    { "ur", ":اصل مقدار" }
                }
            },
            {
                "refund_dialog_refund_qty",
                new Dictionary<string, string>
                {
                    { "en", "Refund Qty:" },
                    { "ur", ":واپسی کی مقدار" }
                }
            },
            {
                "refund_dialog_summary",
                new Dictionary<string, string>
                {
                    { "en", "Refund Summary" },
                    { "ur", "واپسی کا خلاصہ" }
                }
            },
            {
                "refund_dialog_subtotal",
                new Dictionary<string, string>
                {
                    { "en", "Subtotal:" },
                    { "ur", ":ذیلی کل" }
                }
            },
            {
                "refund_dialog_tax_vat",
                new Dictionary<string, string>
                {
                    { "en", "Tax/VAT:" },
                    { "ur", ":ٹیکس/ویٹ" }
                }
            },
            {
                "refund_dialog_total_refund",
                new Dictionary<string, string>
                {
                    { "en", "Total Refund:" },
                    { "ur", ":کل واپسی" }
                }
            },
            {
                "refund_dialog_cancel_button",
                new Dictionary<string, string>
                {
                    { "en", "Cancel" },
                    { "ur", "منسوخ کریں" }
                }
            },
            {
                "refund_dialog_confirm_button",
                new Dictionary<string, string>
                {
                    { "en", "Confirm Refund" },
                    { "ur", "واپسی کی تصدیق کریں" }
                }
            },
            {
                "refund_dialog_transaction_info",
                new Dictionary<string, string>
                {
                    { "en", "Transaction" },
                    { "ur", "لین دین" }
                }
            },
            {
                "refund_dialog_customer",
                new Dictionary<string, string>
                {
                    { "en", "Customer:" },
                    { "ur", ":گاہک" }
                }
            }
        };

        await SeedTranslationCategory("RefundDialog", refundDialogTranslations, localizationService);
    }

    private static async Task SeedTranslationCategory(string category, Dictionary<string, Dictionary<string, string>> translations, IDatabaseLocalizationService localizationService)
    {
        Console.WriteLine($"🔧 [RefundDialogTranslationSeeder] Seeding {category} translations...");
        
        foreach (var keywordPair in translations)
        {
            var key = keywordPair.Key;
            var languageTranslations = keywordPair.Value;
            
            // Add keyword if it doesn't exist
            await localizationService.AddLanguageKeywordAsync(key, $"{category} - {key}");
            
            // Add translations for each language
            foreach (var translation in languageTranslations)
            {
                await localizationService.SaveTranslationAsync(key, translation.Value, translation.Key);
            }
        }
        
        Console.WriteLine($"✅ [RefundDialogTranslationSeeder] {category} translations seeded successfully");
    }
}
