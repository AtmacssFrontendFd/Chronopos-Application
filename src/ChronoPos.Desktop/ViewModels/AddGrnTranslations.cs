using ChronoPos.Infrastructure.Services;
using DesktopServices = ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// Translation keywords management for Add GRN screen
/// </summary>
public static class AddGrnTranslations
{
    public static async Task EnsureTranslationKeywordsAsync(IDatabaseLocalizationService localizationService)
    {
        var keywords = GetAddGrnKeywords();
        
        foreach (var keywordPair in keywords)
        {
            var key = keywordPair.Key;
            var translations = keywordPair.Value;
            
            // Add keyword if it doesn't exist
            await localizationService.AddLanguageKeywordAsync(key, $"Add GRN - {key}");
            
            // Add translations for each language
            foreach (var translation in translations)
            {
                await localizationService.SaveTranslationAsync(key, translation.Value, translation.Key);
            }
        }
    }
    
    private static Dictionary<string, Dictionary<string, string>> GetAddGrnKeywords()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            // Main Title
            {
                "grn.add_title",
                new Dictionary<string, string>
                {
                    { "en", "Add Goods Received Note" },
                    { "hi", "गुड्स रिसीव्ड नोट जोड़ें" },
                    { "gu", "ગુડ્સ રીસીવ્ડ નોટ ઉમેરો" }
                }
            },

            // Section Titles
            {
                "grn.header_section",
                new Dictionary<string, string>
                {
                    { "en", "GRN Header" },
                    { "hi", "GRN हेडर" },
                    { "gu", "GRN હેડર" }
                }
            },
            {
                "grn.items_section",
                new Dictionary<string, string>
                {
                    { "en", "GRN Items" },
                    { "hi", "GRN आइटम" },
                    { "gu", "GRN આઇટમ" }
                }
            },
            {
                "grn.summary_section",
                new Dictionary<string, string>
                {
                    { "en", "Summary" },
                    { "hi", "सारांश" },
                    { "gu", "સારાંશ" }
                }
            },

            // Form Labels
            {
                "grn.grn_no_label",
                new Dictionary<string, string>
                {
                    { "en", "GRN No:" },
                    { "hi", "GRN नं:" },
                    { "gu", "GRN નં:" }
                }
            },
            {
                "grn.status_label",
                new Dictionary<string, string>
                {
                    { "en", "Status:" },
                    { "hi", "स्थिति:" },
                    { "gu", "સ્થિતિ:" }
                }
            },
            {
                "grn.supplier_label",
                new Dictionary<string, string>
                {
                    { "en", "Supplier:" },
                    { "hi", "आपूर्तिकर्ता:" },
                    { "gu", "સપ્લાયર:" }
                }
            },
            {
                "grn.store_label",
                new Dictionary<string, string>
                {
                    { "en", "Store:" },
                    { "hi", "स्टोर:" },
                    { "gu", "સ્ટોર:" }
                }
            },
            {
                "grn.invoice_no_label",
                new Dictionary<string, string>
                {
                    { "en", "Invoice No:" },
                    { "hi", "इनवॉइस नं:" },
                    { "gu", "ઇન્વૉઇસ નં:" }
                }
            },
            {
                "grn.invoice_date_label",
                new Dictionary<string, string>
                {
                    { "en", "Invoice Date:" },
                    { "hi", "इनवॉइस दिनांक:" },
                    { "gu", "ઇન્વૉઇસ તારીખ:" }
                }
            },
            {
                "grn.received_date_label",
                new Dictionary<string, string>
                {
                    { "en", "Received Date:" },
                    { "hi", "प्राप्त दिनांक:" },
                    { "gu", "પ્રાપ્ત તારીખ:" }
                }
            },
            {
                "grn.remarks_label",
                new Dictionary<string, string>
                {
                    { "en", "Remarks:" },
                    { "hi", "टिप्पणी:" },
                    { "gu", "ટિપ્પણી:" }
                }
            },

            // Button Labels
            {
                "grn.save_draft_button",
                new Dictionary<string, string>
                {
                    { "en", "Save Draft" },
                    { "hi", "ड्राफ्ट सेव करें" },
                    { "gu", "ડ્રાફ્ટ સેવ કરો" }
                }
            },
            {
                "grn.post_grn_button",
                new Dictionary<string, string>
                {
                    { "en", "Post GRN" },
                    { "hi", "GRN पोस्ट करें" },
                    { "gu", "GRN પોસ્ટ કરો" }
                }
            },

            // Common translations (already exist, but ensure consistency)
            {
                "common.back",
                new Dictionary<string, string>
                {
                    { "en", "Back" },
                    { "hi", "वापस" },
                    { "gu", "પાછું" }
                }
            },
            {
                "common.cancel",
                new Dictionary<string, string>
                {
                    { "en", "Cancel" },
                    { "hi", "रद्द करें" },
                    { "gu", "રદ કરો" }
                }
            }
        };
    }
}