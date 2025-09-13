using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// Translation keywords management for Product Management screen
/// </summary>
public static class ProductManagementTranslations
{
    public static async Task EnsureTranslationKeywordsAsync(IDatabaseLocalizationService localizationService)
    {
        var keywords = GetProductManagementKeywords();
        
        foreach (var keywordPair in keywords)
        {
            var key = keywordPair.Key;
            var translations = keywordPair.Value;
            
            // Add keyword if it doesn't exist
            await localizationService.AddLanguageKeywordAsync(key, $"Product Management - {key}");
            
            // Add translations for each language
            foreach (var translation in translations)
            {
                await localizationService.SaveTranslationAsync(key, translation.Value, translation.Key);
            }
        }
    }
    
    private static Dictionary<string, Dictionary<string, string>> GetProductManagementKeywords()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            // Page and Navigation
            {
                "product_management_title",
                new Dictionary<string, string>
                {
                    { "en", "Product Management" },
                    { "ur", "پروڈکٹ کا انتظام" }
                }
            },
            {
                "back_button",
                new Dictionary<string, string>
                {
                    { "en", "← Back" },
                    { "ur", "← واپس" }
                }
            },
            {
                "refresh_button",
                new Dictionary<string, string>
                {
                    { "en", "🔄 Refresh" },
                    { "ur", "🔄 ریفریش" }
                }
            },

            // Categories Section
            {
                "categories_header",
                new Dictionary<string, string>
                {
                    { "en", "Categories" },
                    { "ur", "اقسام" }
                }
            },
            {
                "add_new_category_button",
                new Dictionary<string, string>
                {
                    { "en", "➕ Add Category" },
                    { "ur", "➕ قسم شامل کریں" }
                }
            },
            {
                "all_categories",
                new Dictionary<string, string>
                {
                    { "en", "All" },
                    { "ur", "تمام" }
                }
            },
            {
                "items_count",
                new Dictionary<string, string>
                {
                    { "en", "items" },
                    { "ur", "اشیاء" }
                }
            },

            // Search Section
            {
                "search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search products..." },
                    { "ur", "پروڈکٹس تلاش کریں..." }
                }
            },
            {
                "search_type_product_name",
                new Dictionary<string, string>
                {
                    { "en", "Product Name" },
                    { "ur", "پروڈکٹ کا نام" }
                }
            },
            {
                "search_type_category",
                new Dictionary<string, string>
                {
                    { "en", "Category" },
                    { "ur", "قسم" }
                }
            },
            {
                "showing_products_count",
                new Dictionary<string, string>
                {
                    { "en", "Showing {0} products" },
                    { "ur", "{0} پروڈکٹس دکھا رہے ہیں" }
                }
            },
            {
                "add_new_product_button",
                new Dictionary<string, string>
                {
                    { "en", "➕ Add Product" },
                    { "ur", "➕ پروڈکٹ شامل کریں" }
                }
            },

            // Table Headers
            {
                "column_product_name",
                new Dictionary<string, string>
                {
                    { "en", "Product Name" },
                    { "ur", "پروڈکٹ کا نام" }
                }
            },
            {
                "column_item_id",
                new Dictionary<string, string>
                {
                    { "en", "Item ID" },
                    { "ur", "آئٹم آئی ڈی" }
                }
            },
            {
                "column_stock",
                new Dictionary<string, string>
                {
                    { "en", "Stock" },
                    { "ur", "اسٹاک" }
                }
            },
            {
                "column_category",
                new Dictionary<string, string>
                {
                    { "en", "Category" },
                    { "ur", "قسم" }
                }
            },
            {
                "column_price",
                new Dictionary<string, string>
                {
                    { "en", "Price" },
                    { "ur", "قیمت" }
                }
            },
            {
                "column_actions",
                new Dictionary<string, string>
                {
                    { "en", "Actions" },
                    { "ur", "عمل" }
                }
            },

            // Actions
            {
                "action_edit",
                new Dictionary<string, string>
                {
                    { "en", "Edit Product" },
                    { "ur", "پروڈکٹ میں ترمیم" }
                }
            },
            {
                "action_delete",
                new Dictionary<string, string>
                {
                    { "en", "Delete Product" },
                    { "ur", "پروڈکٹ ڈیلیٹ کریں" }
                }
            },
            {
                "action_duplicate",
                new Dictionary<string, string>
                {
                    { "en", "Duplicate Product" },
                    { "ur", "پروڈکٹ کاپی کریں" }
                }
            },

            // Category Form
            {
                "add_new_category_title",
                new Dictionary<string, string>
                {
                    { "en", "Add New Category" },
                    { "ur", "نئی قسم شامل کریں" }
                }
            },
            {
                "edit_category_title",
                new Dictionary<string, string>
                {
                    { "en", "Edit Category" },
                    { "ur", "قسم میں ترمیم" }
                }
            },
            {
                "category_name_label",
                new Dictionary<string, string>
                {
                    { "en", "Category Name *" },
                    { "ur", "* قسم کا نام" }
                }
            },
            {
                "category_name_arabic_label",
                new Dictionary<string, string>
                {
                    { "en", "Category Name (Arabic)" },
                    { "ur", "قسم کا نام (عربی)" }
                }
            },
            {
                "parent_category_label",
                new Dictionary<string, string>
                {
                    { "en", "Parent Category" },
                    { "ur", "بنیادی قسم" }
                }
            },
            {
                "display_order_label",
                new Dictionary<string, string>
                {
                    { "en", "Display Order" },
                    { "ur", "ترتیب" }
                }
            },
            {
                "description_label",
                new Dictionary<string, string>
                {
                    { "en", "Description" },
                    { "ur", "تفصیل" }
                }
            },
            {
                "active_category_label",
                new Dictionary<string, string>
                {
                    { "en", "Active Category" },
                    { "ur", "فعال قسم" }
                }
            },
            {
                "save_category_button",
                new Dictionary<string, string>
                {
                    { "en", "Save Category" },
                    { "ur", "قسم محفوظ کریں" }
                }
            },
            {
                "cancel_button",
                new Dictionary<string, string>
                {
                    { "en", "Cancel" },
                    { "ur", "منسوخ" }
                }
            },
            {
                "close_panel_tooltip",
                new Dictionary<string, string>
                {
                    { "en", "Close panel" },
                    { "ur", "پینل بند کریں" }
                }
            },

            // Info Panel
            {
                "category_info_title",
                new Dictionary<string, string>
                {
                    { "en", "ℹ️ Category Information" },
                    { "ur", "ℹ️ قسم کی معلومات" }
                }
            },
            {
                "category_info_name_required",
                new Dictionary<string, string>
                {
                    { "en", "• Category name is required and will be used in product listings" },
                    { "ur", "• قسم کا نام ضروری ہے اور پروڈکٹ فہرست میں استعمال ہوگا" }
                }
            },
            {
                "category_info_arabic_optional",
                new Dictionary<string, string>
                {
                    { "en", "• Arabic name is optional for multilingual support" },
                    { "ur", "• عربی نام متعدد زبان کی سپورٹ کے لیے اختیاری ہے" }
                }
            },
            {
                "category_info_parent_hierarchy",
                new Dictionary<string, string>
                {
                    { "en", "• Parent category creates a hierarchical structure" },
                    { "ur", "• بنیادی قسم درجہ بندی کی ساخت بناتا ہے" }
                }
            },
            {
                "category_info_display_order",
                new Dictionary<string, string>
                {
                    { "en", "• Display order controls the sorting in category lists" },
                    { "ur", "• ترتیب قسم کی فہرست میں ترتیب کو کنٹرول کرتا ہے" }
                }
            },
            {
                "display_order_help",
                new Dictionary<string, string>
                {
                    { "en", "Lower numbers appear first in the list" },
                    { "ur", "کم نمبر فہرست میں پہلے آتے ہیں" }
                }
            },
            {
                "no_parent_category",
                new Dictionary<string, string>
                {
                    { "en", "No Parent Category" },
                    { "ur", "کوئی بنیادی قسم نہیں" }
                }
            },

            // Status Messages
            {
                "status_ready",
                new Dictionary<string, string>
                {
                    { "en", "Ready" },
                    { "ur", "تیار" }
                }
            },
            {
                "status_loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading..." },
                    { "ur", "لوڈ کر رہا ہے..." }
                }
            },
            {
                "status_saving",
                new Dictionary<string, string>
                {
                    { "en", "Saving..." },
                    { "ur", "محفوظ کر رہا ہے..." }
                }
            },
            {
                "status_refreshing",
                new Dictionary<string, string>
                {
                    { "en", "Refreshing data..." },
                    { "ur", "ڈیٹا ریفریش کر رہا ہے..." }
                }
            },
            {
                "status_deleting",
                new Dictionary<string, string>
                {
                    { "en", "Deleting..." },
                    { "ur", "ڈیلیٹ کر رہا ہے..." }
                }
            },
            {
                "status_loaded_categories_products",
                new Dictionary<string, string>
                {
                    { "en", "Loaded {0} categories and {1} products" },
                    { "ur", "{0} اقسام اور {1} پروڈکٹس لوڈ ہوئے" }
                }
            },

            // Confirmation Messages
            {
                "confirm_delete_product",
                new Dictionary<string, string>
                {
                    { "en", "Are you sure you want to delete '{0}'?" },
                    { "ur", "کیا آپ '{0}' کو ڈیلیٹ کرنا چاہتے ہیں؟" }
                }
            },
            {
                "confirm_delete_title",
                new Dictionary<string, string>
                {
                    { "en", "Confirm Delete" },
                    { "ur", "ڈیلیٹ کی تصدیق" }
                }
            },

            // Validation Messages
            {
                "validation_category_name_required",
                new Dictionary<string, string>
                {
                    { "en", "Category name is required" },
                    { "ur", "قسم کا نام ضروری ہے" }
                }
            },
            {
                "validation_category_name_length",
                new Dictionary<string, string>
                {
                    { "en", "Category name cannot exceed 100 characters" },
                    { "ur", "قسم کا نام 100 حروف سے زیادہ نہیں ہو سکتا" }
                }
            },
            {
                "validation_description_length",
                new Dictionary<string, string>
                {
                    { "en", "Category description cannot exceed 500 characters" },
                    { "ur", "قسم کی تفصیل 500 حروف سے زیادہ نہیں ہو سکتی" }
                }
            },
            {
                "validation_error_title",
                new Dictionary<string, string>
                {
                    { "en", "Validation Error" },
                    { "ur", "توثیق کی خرابی" }
                }
            }
        };
    }
}
