using ChronoPos.Infrastructure.Services;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Desktop.Utilities
{
    /// <summary>
    /// Utility class for managing language keywords and translations
    /// This makes it easy to add new keywords and translations programmatically
    /// </summary>
    public class LanguageManager
    {
        private readonly IDatabaseLocalizationService _localizationService;

        public LanguageManager(IDatabaseLocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        /// <summary>
        /// Add a new translation keyword with translations for all supported languages
        /// </summary>
        public async Task<bool> AddKeywordWithTranslationsAsync(string key, string description, 
            Dictionary<string, string> translations)
        {
            try
            {
                // First add the keyword
                var keywordAdded = await _localizationService.AddLanguageKeywordAsync(key, description);
                if (!keywordAdded)
                    return false;

                // Then add translations for each language
                foreach (var translation in translations)
                {
                    var languageCode = translation.Key;
                    var value = translation.Value;
                    
                    await _localizationService.SaveTranslationAsync(key, value, languageCode);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding keyword '{key}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Batch add multiple keywords with their translations
        /// </summary>
        public async Task<bool> AddMultipleKeywordsAsync(
            Dictionary<string, (string description, Dictionary<string, string> translations)> keywordData)
        {
            var allSuccessful = true;

            foreach (var keyword in keywordData)
            {
                var key = keyword.Key;
                var description = keyword.Value.description;
                var translations = keyword.Value.translations;

                var success = await AddKeywordWithTranslationsAsync(key, description, translations);
                if (!success)
                {
                    allSuccessful = false;
                    Console.WriteLine($"Failed to add keyword: {key}");
                }
            }

            return allSuccessful;
        }

        /// <summary>
        /// Add common POS system keywords
        /// </summary>
        public async Task<bool> AddCommonPOSKeywordsAsync()
        {
            var keywords = new Dictionary<string, (string description, Dictionary<string, string> translations)>
            {
                // Sales Related
                ["sales.new_sale"] = ("New sale button", new Dictionary<string, string>
                {
                    ["en"] = "New Sale",
                    ["ur"] = "نئی فروخت"
                }),
                ["sales.total"] = ("Total amount", new Dictionary<string, string>
                {
                    ["en"] = "Total",
                    ["ur"] = "کل"
                }),
                ["sales.subtotal"] = ("Subtotal amount", new Dictionary<string, string>
                {
                    ["en"] = "Subtotal",
                    ["ur"] = "ذیلی کل"
                }),
                ["sales.discount"] = ("Discount amount", new Dictionary<string, string>
                {
                    ["en"] = "Discount",
                    ["ur"] = "رعایت"
                }),
                ["sales.tax"] = ("Tax amount", new Dictionary<string, string>
                {
                    ["en"] = "Tax",
                    ["ur"] = "ٹیکس"
                }),
                ["sales.payment"] = ("Payment", new Dictionary<string, string>
                {
                    ["en"] = "Payment",
                    ["ur"] = "ادائیگی"
                }),
                ["sales.cash"] = ("Cash payment", new Dictionary<string, string>
                {
                    ["en"] = "Cash",
                    ["ur"] = "نقد"
                }),
                ["sales.card"] = ("Card payment", new Dictionary<string, string>
                {
                    ["en"] = "Card",
                    ["ur"] = "کارڈ"
                }),

                // Inventory Related
                ["inventory.title"] = ("Inventory page title", new Dictionary<string, string>
                {
                    ["en"] = "Inventory Management",
                    ["ur"] = "انوینٹری کا انتظام"
                }),
                ["inventory.quantity"] = ("Quantity field", new Dictionary<string, string>
                {
                    ["en"] = "Quantity",
                    ["ur"] = "مقدار"
                }),
                ["inventory.reorder_level"] = ("Reorder level", new Dictionary<string, string>
                {
                    ["en"] = "Reorder Level",
                    ["ur"] = "دوبارہ آرڈر کی سطح"
                }),

                // Customer Related
                ["customer.add"] = ("Add customer button", new Dictionary<string, string>
                {
                    ["en"] = "Add Customer",
                    ["ur"] = "گاہک شامل کریں"
                }),
                ["customer.name"] = ("Customer name field", new Dictionary<string, string>
                {
                    ["en"] = "Customer Name",
                    ["ur"] = "گاہک کا نام"
                }),
                ["customer.phone"] = ("Phone number field", new Dictionary<string, string>
                {
                    ["en"] = "Phone Number",
                    ["ur"] = "فون نمبر"
                }),
                ["customer.email"] = ("Email field", new Dictionary<string, string>
                {
                    ["en"] = "Email",
                    ["ur"] = "ای میل"
                }),
                ["customer.address"] = ("Address field", new Dictionary<string, string>
                {
                    ["en"] = "Address",
                    ["ur"] = "پتہ"
                }),

                // Report Related
                ["reports.title"] = ("Reports page title", new Dictionary<string, string>
                {
                    ["en"] = "Reports & Analytics",
                    ["ur"] = "رپورٹس اور تجزیات"
                }),
                ["reports.daily"] = ("Daily reports", new Dictionary<string, string>
                {
                    ["en"] = "Daily Report",
                    ["ur"] = "روزانہ رپورٹ"
                }),
                ["reports.monthly"] = ("Monthly reports", new Dictionary<string, string>
                {
                    ["en"] = "Monthly Report",
                    ["ur"] = "ماہانہ رپورٹ"
                }),
                ["reports.sales"] = ("Sales report", new Dictionary<string, string>
                {
                    ["en"] = "Sales Report",
                    ["ur"] = "فروخت کی رپورٹ"
                }),

                // Common Messages
                ["msg.success"] = ("Success message", new Dictionary<string, string>
                {
                    ["en"] = "Operation completed successfully",
                    ["ur"] = "عمل کامیابی سے مکمل ہوا"
                }),
                ["msg.error"] = ("Error message", new Dictionary<string, string>
                {
                    ["en"] = "An error occurred",
                    ["ur"] = "ایک خرابی ہوئی"
                }),
                ["msg.confirm"] = ("Confirmation message", new Dictionary<string, string>
                {
                    ["en"] = "Are you sure?",
                    ["ur"] = "کیا آپ کو یقین ہے؟"
                }),
                ["msg.loading"] = ("Loading message", new Dictionary<string, string>
                {
                    ["en"] = "Loading...",
                    ["ur"] = "لوڈ ہو رہا ہے..."
                }),

                // Form Labels
                ["form.required"] = ("Required field indicator", new Dictionary<string, string>
                {
                    ["en"] = "Required",
                    ["ur"] = "لازمی"
                }),
                ["form.optional"] = ("Optional field indicator", new Dictionary<string, string>
                {
                    ["en"] = "Optional",
                    ["ur"] = "اختیاری"
                })
            };

            return await AddMultipleKeywordsAsync(keywords);
        }

        /// <summary>
        /// Add restaurant-specific keywords (for restaurant POS)
        /// </summary>
        public async Task<bool> AddRestaurantKeywordsAsync()
        {
            var keywords = new Dictionary<string, (string description, Dictionary<string, string> translations)>
            {
                ["restaurant.table"] = ("Table number", new Dictionary<string, string>
                {
                    ["en"] = "Table",
                    ["ur"] = "میز"
                }),
                ["restaurant.order"] = ("Order", new Dictionary<string, string>
                {
                    ["en"] = "Order",
                    ["ur"] = "آرڈر"
                }),
                ["restaurant.menu"] = ("Menu", new Dictionary<string, string>
                {
                    ["en"] = "Menu",
                    ["ur"] = "مینو"
                }),
                ["restaurant.kitchen"] = ("Kitchen", new Dictionary<string, string>
                {
                    ["en"] = "Kitchen",
                    ["ur"] = "باورچی خانہ"
                }),
                ["restaurant.reservation"] = ("Reservation", new Dictionary<string, string>
                {
                    ["en"] = "Reservation",
                    ["ur"] = "بکنگ"
                })
            };

            return await AddMultipleKeywordsAsync(keywords);
        }

        /// <summary>
        /// Get translation statistics
        /// </summary>
        public async Task<Dictionary<string, int>> GetTranslationStatsAsync()
        {
            try
            {
                var languages = await _localizationService.GetAvailableLanguagesAsync();
                var stats = new Dictionary<string, int>();

                foreach (var language in languages)
                {
                    var translations = await _localizationService.GetAllTranslationsAsync(language.LanguageCode);
                    stats[language.LanguageName] = translations.Count;
                }

                return stats;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting translation stats: {ex.Message}");
                return new Dictionary<string, int>();
            }
        }

        /// <summary>
        /// Find missing translations for a language
        /// </summary>
        public async Task<List<string>> FindMissingTranslationsAsync(string languageCode)
        {
            try
            {
                var allLanguages = await _localizationService.GetAvailableLanguagesAsync();
                var targetLanguage = allLanguages.FirstOrDefault(l => l.LanguageCode == languageCode);
                
                if (targetLanguage == null)
                    return new List<string>();

                // Get all translations for English (as reference)
                var englishTranslations = await _localizationService.GetAllTranslationsAsync("en");
                var targetTranslations = await _localizationService.GetAllTranslationsAsync(languageCode);

                // Find keys that exist in English but not in target language
                var missingKeys = englishTranslations.Keys
                    .Where(key => !targetTranslations.ContainsKey(key))
                    .ToList();

                return missingKeys;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error finding missing translations: {ex.Message}");
                return new List<string>();
            }
        }
    }
}
