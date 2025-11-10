using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Dedicated seeder for Product Selection Dialog translations
/// </summary>
public static class ProductSelectionDialogTranslationSeeder
{
    public static async Task SeedProductSelectionDialogTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var productSelectionTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Dialog Title
            {
                "product_selection_dialog_title",
                new Dictionary<string, string>
                {
                    { "en", "Select Product Options" },
                    { "ur", "مصنوعات کے اختیارات منتخب کریں" }
                }
            },
            
            // Base Price Label
            {
                "product_selection_base_price",
                new Dictionary<string, string>
                {
                    { "en", "Base Price:" },
                    { "ur", ":بنیادی قیمت" }
                }
            },
            
            // Close Tooltip
            {
                "product_selection_close",
                new Dictionary<string, string>
                {
                    { "en", "Close" },
                    { "ur", "بند کریں" }
                }
            },
            
            // Product Units Tab
            {
                "product_selection_units_header",
                new Dictionary<string, string>
                {
                    { "en", "Product Units" },
                    { "ur", "مصنوعات کی اکائیاں" }
                }
            },
            {
                "product_selection_select_unit",
                new Dictionary<string, string>
                {
                    { "en", "Select a unit of measurement:" },
                    { "ur", ":پیمائش کی اکائی منتخب کریں" }
                }
            },
            
            // Product Combinations Tab
            {
                "product_selection_combinations_header",
                new Dictionary<string, string>
                {
                    { "en", "Product Combinations" },
                    { "ur", "مصنوعات کی ترکیبیں" }
                }
            },
            {
                "product_selection_select_combination",
                new Dictionary<string, string>
                {
                    { "en", "Select a product combination:" },
                    { "ur", ":مصنوعات کی ترکیب منتخب کریں" }
                }
            },
            
            // Modifiers Tab
            {
                "product_selection_modifiers_header",
                new Dictionary<string, string>
                {
                    { "en", "Modifiers" },
                    { "ur", "ترمیم کار" }
                }
            },
            {
                "product_selection_customize_product",
                new Dictionary<string, string>
                {
                    { "en", "Customize your product:" },
                    { "ur", ":اپنی مصنوعات کو حسب ضرورت بنائیں" }
                }
            },
            
            // Product Groups Tab
            {
                "product_selection_groups_header",
                new Dictionary<string, string>
                {
                    { "en", "Product Groups" },
                    { "ur", "مصنوعات کے گروپ" }
                }
            },
            {
                "product_selection_select_group",
                new Dictionary<string, string>
                {
                    { "en", "Select a product group:" },
                    { "ur", ":مصنوعات کا گروپ منتخب کریں" }
                }
            },
            
            // Footer Labels
            {
                "product_selection_total_price",
                new Dictionary<string, string>
                {
                    { "en", "Total Price:" },
                    { "ur", ":کل قیمت" }
                }
            },
            {
                "product_selection_cancel",
                new Dictionary<string, string>
                {
                    { "en", "Cancel" },
                    { "ur", "منسوخ" }
                }
            },
            {
                "product_selection_add_to_cart",
                new Dictionary<string, string>
                {
                    { "en", "Add to Cart" },
                    { "ur", "کارٹ میں شامل کریں" }
                }
            }
        };

        foreach (var translation in productSelectionTranslations)
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
