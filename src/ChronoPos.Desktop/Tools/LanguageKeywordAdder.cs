using ChronoPos.Infrastructure.Services;
using ChronoPos.Desktop.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChronoPos.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Desktop.Tools
{
    /// <summary>
    /// Console tool to add new language keywords and translations
    /// Run this to easily extend the language system
    /// </summary>
    public class LanguageKeywordAdder
    {
        public static async Task<bool> AddNewKeywordsAsync(IServiceProvider serviceProvider)
        {
            var dbLocalizationService = serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            var languageManager = new LanguageManager(dbLocalizationService);

            Console.WriteLine("Adding new language keywords...");

            // Example: Add some additional keywords
            var additionalKeywords = new Dictionary<string, (string description, Dictionary<string, string> translations)>
            {
                // Dashboard specific
                ["dashboard.welcome"] = ("Welcome message", new Dictionary<string, string>
                {
                    ["en"] = "Welcome to ChronoPos",
                    ["ur"] = "ChronoPos میں خوش آمدید"
                }),
                ["dashboard.total_sales"] = ("Total sales label", new Dictionary<string, string>
                {
                    ["en"] = "Total Sales",
                    ["ur"] = "کل فروخت"
                }),
                ["dashboard.today"] = ("Today label", new Dictionary<string, string>
                {
                    ["en"] = "Today",
                    ["ur"] = "آج"
                }),
                ["dashboard.this_month"] = ("This month label", new Dictionary<string, string>
                {
                    ["en"] = "This Month",
                    ["ur"] = "اس مہینے"
                }),

                // Settings specific (translated)
                ["settings.choose_language"] = ("Choose language instruction", new Dictionary<string, string>
                {
                    ["en"] = "Choose your preferred language for the application",
                    ["ur"] = "اپلیکیشن کے لیے اپنی پسندیدہ زبان منتخب کریں"
                }),
                ["settings.primary_colors"] = ("Primary colors label", new Dictionary<string, string>
                {
                    ["en"] = "Primary Colors:",
                    ["ur"] = "بنیادی رنگ:"
                }),
                ["settings.background_colors"] = ("Background colors label", new Dictionary<string, string>
                {
                    ["en"] = "Background Colors:",
                    ["ur"] = "پس منظر کے رنگ:"
                }),

                // Button translations
                ["btn.apply_settings"] = ("Apply settings button", new Dictionary<string, string>
                {
                    ["en"] = "Apply Settings",
                    ["ur"] = "ترتیبات لاگو کریں"
                }),
                ["btn.reset_settings"] = ("Reset settings button", new Dictionary<string, string>
                {
                    ["en"] = "Reset to Default",
                    ["ur"] = "پہلے جیسا کریں"
                }),

                // Status messages
                ["status.settings_saved"] = ("Settings saved message", new Dictionary<string, string>
                {
                    ["en"] = "All settings saved successfully!",
                    ["ur"] = "تمام ترتیبات کامیابی سے محفوظ ہو گئیں!"
                }),
                ["status.language_changed"] = ("Language changed message", new Dictionary<string, string>
                {
                    ["en"] = "Language changed successfully",
                    ["ur"] = "زبان کامیابی سے تبدیل ہو گئی"
                }),

                // Product management
                ["products.add_new"] = ("Add new product button", new Dictionary<string, string>
                {
                    ["en"] = "Add New Product",
                    ["ur"] = "نئی مصنوعات شامل کریں"
                }),
                ["products.edit_product"] = ("Edit product button", new Dictionary<string, string>
                {
                    ["en"] = "Edit Product",
                    ["ur"] = "مصنوعات میں ترمیم"
                }),
                ["products.delete_product"] = ("Delete product button", new Dictionary<string, string>
                {
                    ["en"] = "Delete Product",
                    ["ur"] = "مصنوعات کو حذف کریں"
                }),

                // Sales related
                ["sales.process_sale"] = ("Process sale button", new Dictionary<string, string>
                {
                    ["en"] = "Process Sale",
                    ["ur"] = "فروخت کا عمل"
                }),
                ["sales.void_sale"] = ("Void sale button", new Dictionary<string, string>
                {
                    ["en"] = "Void Sale",
                    ["ur"] = "فروخت منسوخ کریں"
                }),
                ["sales.receipt"] = ("Receipt label", new Dictionary<string, string>
                {
                    ["en"] = "Receipt",
                    ["ur"] = "رسید"
                })
            };

            var success = await languageManager.AddMultipleKeywordsAsync(additionalKeywords);
            
            if (success)
            {
                Console.WriteLine("✅ All new keywords added successfully!");
                
                // Also add common POS keywords
                await languageManager.AddCommonPOSKeywordsAsync();
                Console.WriteLine("✅ Common POS keywords added successfully!");
                
                return true;
            }
            else
            {
                Console.WriteLine("❌ Some keywords failed to add");
                return false;
            }
        }

        /// <summary>
        /// Add a single keyword with translations
        /// </summary>
        public static async Task<bool> AddSingleKeywordAsync(IServiceProvider serviceProvider, 
            string key, string description, string englishText, string urduText)
        {
            var dbLocalizationService = serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            var languageManager = new LanguageManager(dbLocalizationService);

            var translations = new Dictionary<string, string>
            {
                ["en"] = englishText,
                ["ur"] = urduText
            };

            return await languageManager.AddKeywordWithTranslationsAsync(key, description, translations);
        }

        /// <summary>
        /// Show translation statistics
        /// </summary>
        public static async Task ShowTranslationStatsAsync(IServiceProvider serviceProvider)
        {
            var dbLocalizationService = serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
            var languageManager = new LanguageManager(dbLocalizationService);

            Console.WriteLine("\n📊 Translation Statistics:");
            var stats = await languageManager.GetTranslationStatsAsync();
            
            foreach (var stat in stats)
            {
                Console.WriteLine($"  {stat.Key}: {stat.Value} translations");
            }

            // Show missing translations for Urdu
            Console.WriteLine("\n🔍 Missing Urdu Translations:");
            var missingUrdu = await languageManager.FindMissingTranslationsAsync("ur");
            if (missingUrdu.Any())
            {
                foreach (var missing in missingUrdu.Take(10)) // Show first 10
                {
                    Console.WriteLine($"  - {missing}");
                }
                if (missingUrdu.Count > 10)
                {
                    Console.WriteLine($"  ... and {missingUrdu.Count - 10} more");
                }
            }
            else
            {
                Console.WriteLine("  ✅ All translations are complete!");
            }
        }
    }
}
