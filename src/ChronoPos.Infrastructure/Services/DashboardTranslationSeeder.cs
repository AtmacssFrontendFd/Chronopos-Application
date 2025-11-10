using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Extracted seeder for Dashboard-specific translations.
/// This keeps the LanguageSeedingService smaller and focused.
/// </summary>
public static class DashboardTranslationSeeder
{
    public static async Task SeedDashboardTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var dashboardTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page Title and Subtitles
            {
                "dashboard_title",
                new Dictionary<string, string>
                {
                    { "en", "Dashboard" },
                    { "ur", "ڈیش بورڈ" }
                }
            },
            {
                "dashboard_subtitle",
                new Dictionary<string, string>
                {
                    { "en", "Your business overview and key metrics" },
                    { "ur", "آپ کے کاروبار کا جائزہ اور اہم اعداد و شمار" }
                }
            },
            {
                "welcome_message",
                new Dictionary<string, string>
                {
                    { "en", "Welcome back!" },
                    { "ur", "خوش آمدید!" }
                }
            },
            
            // KPI Cards
            {
                "todays_sales",
                new Dictionary<string, string>
                {
                    { "en", "Today's Sales" },
                    { "ur", "آج کی فروخت" }
                }
            },
            {
                "monthly_sales",
                new Dictionary<string, string>
                {
                    { "en", "Monthly Sales" },
                    { "ur", "ماہانہ فروخت" }
                }
            },
            {
                "growth",
                new Dictionary<string, string>
                {
                    { "en", "Growth" },
                    { "ur", "ترقی" }
                }
            },
            {
                "vs_yesterday",
                new Dictionary<string, string>
                {
                    { "en", "vs yesterday" },
                    { "ur", "کل سے موازنہ" }
                }
            },
            {
                "vs_last_month",
                new Dictionary<string, string>
                {
                    { "en", "vs last month" },
                    { "ur", "پچھلے ماہ سے موازنہ" }
                }
            },
            {
                "active_tables",
                new Dictionary<string, string>
                {
                    { "en", "Active Tables" },
                    { "ur", "فعال میزیں" }
                }
            },
            {
                "pending_orders",
                new Dictionary<string, string>
                {
                    { "en", "Pending Orders" },
                    { "ur", "زیر التواء آرڈرز" }
                }
            },
            {
                "low_stock_items",
                new Dictionary<string, string>
                {
                    { "en", "Low Stock Items" },
                    { "ur", "کم اسٹاک اشیاء" }
                }
            },
            {
                "total_customers",
                new Dictionary<string, string>
                {
                    { "en", "Total Customers" },
                    { "ur", "کل گاہک" }
                }
            },
            {
                "avg_transaction_value",
                new Dictionary<string, string>
                {
                    { "en", "Avg. Transaction" },
                    { "ur", "اوسط لین دین" }
                }
            },
            
            // Popular Products Section
            {
                "popular_products",
                new Dictionary<string, string>
                {
                    { "en", "Popular Products" },
                    { "ur", "مقبول پروڈکٹس" }
                }
            },
            {
                "product_name",
                new Dictionary<string, string>
                {
                    { "en", "Product" },
                    { "ur", "پروڈکٹ" }
                }
            },
            {
                "quantity_sold",
                new Dictionary<string, string>
                {
                    { "en", "Sold" },
                    { "ur", "فروخت" }
                }
            },
            {
                "revenue",
                new Dictionary<string, string>
                {
                    { "en", "Revenue" },
                    { "ur", "آمدن" }
                }
            },
            {
                "view_all_products",
                new Dictionary<string, string>
                {
                    { "en", "View All Products" },
                    { "ur", "تمام پروڈکٹس دیکھیں" }
                }
            },
            
            // Recent Sales Section
            {
                "recent_sales",
                new Dictionary<string, string>
                {
                    { "en", "Recent Sales" },
                    { "ur", "حالیہ فروخت" }
                }
            },
            {
                "invoice_no",
                new Dictionary<string, string>
                {
                    { "en", "Invoice#" },
                    { "ur", "رسید نمبر" }
                }
            },
            {
                "customer",
                new Dictionary<string, string>
                {
                    { "en", "Customer" },
                    { "ur", "گاہک" }
                }
            },
            {
                "amount",
                new Dictionary<string, string>
                {
                    { "en", "Amount" },
                    { "ur", "رقم" }
                }
            },
            {
                "time",
                new Dictionary<string, string>
                {
                    { "en", "Time" },
                    { "ur", "وقت" }
                }
            },
            {
                "status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حیثیت" }
                }
            },
            {
                "view_all_sales",
                new Dictionary<string, string>
                {
                    { "en", "View All Sales" },
                    { "ur", "تمام فروخت دیکھیں" }
                }
            },
            
            // Sales Analytics Chart
            {
                "sales_analytics",
                new Dictionary<string, string>
                {
                    { "en", "Sales Analytics" },
                    { "ur", "فروخت کی تجزیہ" }
                }
            },
            {
                "daily",
                new Dictionary<string, string>
                {
                    { "en", "Daily" },
                    { "ur", "یومیہ" }
                }
            },
            {
                "weekly",
                new Dictionary<string, string>
                {
                    { "en", "Weekly" },
                    { "ur", "ہفتہ وار" }
                }
            },
            {
                "monthly",
                new Dictionary<string, string>
                {
                    { "en", "Monthly" },
                    { "ur", "ماہانہ" }
                }
            },
            
            // Top Categories Section
            {
                "top_categories",
                new Dictionary<string, string>
                {
                    { "en", "Top Categories" },
                    { "ur", "اعلیٰ زمرے" }
                }
            },
            {
                "category",
                new Dictionary<string, string>
                {
                    { "en", "Category" },
                    { "ur", "زمرہ" }
                }
            },
            {
                "sales",
                new Dictionary<string, string>
                {
                    { "en", "Sales" },
                    { "ur", "فروخت" }
                }
            },
            
            // Customer Insights Section
            {
                "customer_insights",
                new Dictionary<string, string>
                {
                    { "en", "Customer Insights" },
                    { "ur", "گاہکوں کی بصیرت" }
                }
            },
            {
                "new_customers_today",
                new Dictionary<string, string>
                {
                    { "en", "New Today" },
                    { "ur", "آج نئے" }
                }
            },
            {
                "new_customers_week",
                new Dictionary<string, string>
                {
                    { "en", "New This Week" },
                    { "ur", "اس ہفتے نئے" }
                }
            },
            {
                "new_customers_month",
                new Dictionary<string, string>
                {
                    { "en", "New This Month" },
                    { "ur", "اس ماہ نئے" }
                }
            },
            {
                "returning_customers",
                new Dictionary<string, string>
                {
                    { "en", "Returning Customers" },
                    { "ur", "واپس آنے والے گاہک" }
                }
            },
            {
                "customer_growth",
                new Dictionary<string, string>
                {
                    { "en", "Customer Growth" },
                    { "ur", "گاہکوں کی ترقی" }
                }
            },
            {
                "avg_customer_value",
                new Dictionary<string, string>
                {
                    { "en", "Avg. Customer Value" },
                    { "ur", "اوسط گاہک کی قیمت" }
                }
            },
            {
                "top_customers",
                new Dictionary<string, string>
                {
                    { "en", "Top Customers" },
                    { "ur", "اعلیٰ گاہک" }
                }
            },
            {
                "view_all_customers",
                new Dictionary<string, string>
                {
                    { "en", "View All Customers" },
                    { "ur", "تمام گاہک دیکھیں" }
                }
            },
            
            // Quick Actions
            {
                "quick_actions",
                new Dictionary<string, string>
                {
                    { "en", "Quick Actions" },
                    { "ur", "فوری اقدامات" }
                }
            },
            {
                "new_sale",
                new Dictionary<string, string>
                {
                    { "en", "New Sale" },
                    { "ur", "نئی فروخت" }
                }
            },
            {
                "add_product",
                new Dictionary<string, string>
                {
                    { "en", "Add Product" },
                    { "ur", "پروڈکٹ شامل کریں" }
                }
            },
            {
                "view_customers",
                new Dictionary<string, string>
                {
                    { "en", "View Customers" },
                    { "ur", "گاہک دیکھیں" }
                }
            },
            {
                "generate_report",
                new Dictionary<string, string>
                {
                    { "en", "Generate Report" },
                    { "ur", "رپورٹ بنائیں" }
                }
            },
            {
                "view_low_stock",
                new Dictionary<string, string>
                {
                    { "en", "View Low Stock" },
                    { "ur", "کم اسٹاک دیکھیں" }
                }
            },
            
            // Refresh and Status
            {
                "last_refresh",
                new Dictionary<string, string>
                {
                    { "en", "Last refresh" },
                    { "ur", "آخری تازہ کاری" }
                }
            },
            {
                "refresh_now",
                new Dictionary<string, string>
                {
                    { "en", "Refresh Now" },
                    { "ur", "ابھی تازہ کریں" }
                }
            },
            {
                "just_now",
                new Dictionary<string, string>
                {
                    { "en", "Just now" },
                    { "ur", "ابھی" }
                }
            },
            {
                "loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading dashboard data..." },
                    { "ur", "ڈیش بورڈ ڈیٹا لوڈ ہو رہا ہے..." }
                }
            },
            {
                "error_loading_data",
                new Dictionary<string, string>
                {
                    { "en", "Error loading dashboard data" },
                    { "ur", "ڈیش بورڈ ڈیٹا لوڈ کرنے میں خرابی" }
                }
            },
            {
                "retry",
                new Dictionary<string, string>
                {
                    { "en", "Retry" },
                    { "ur", "دوبارہ کوشش کریں" }
                }
            }
            ,
            // Small utility / suffix translations used in the view
            {
                "transactions_suffix",
                new Dictionary<string, string>
                {
                    { "en", "transactions" },
                    { "ur", "لین دین" }
                }
            },
            {
                "items_sold",
                new Dictionary<string, string>
                {
                    { "en", "items sold" },
                    { "ur", "بیچے گئے آئٹمز" }
                }
            },
            {
                "orders_suffix",
                new Dictionary<string, string>
                {
                    { "en", "orders" },
                    { "ur", "آرڈرز" }
                }
            }
        };

        await SeedTranslationCategory("Dashboard", dashboardTranslations, localizationService);
    }

    private static async Task SeedTranslationCategory(string category, Dictionary<string, Dictionary<string, string>> translations, IDatabaseLocalizationService localizationService)
    {
        Console.WriteLine($"🔧 [DashboardTranslationSeeder] Seeding {category} translations...");

        foreach (var keywordPair in translations)
        {
            var key = keywordPair.Key;
            var languageTranslations = keywordPair.Value;

            await localizationService.AddLanguageKeywordAsync(key, $"{category} - {key}");

            foreach (var translation in languageTranslations)
            {
                await localizationService.SaveTranslationAsync(key, translation.Value, translation.Key);
            }
        }

        Console.WriteLine($"✅ [DashboardTranslationSeeder] {category} translations seeded successfully");
    }
}
