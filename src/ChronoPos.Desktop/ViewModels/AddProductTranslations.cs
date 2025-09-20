using ChronoPos.Infrastructure.Services;
using DesktopServices = ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// Translation keywords management for Add Product screen
/// </summary>
public static class AddProductTranslations
{
    public static async Task EnsureTranslationKeywordsAsync(IDatabaseLocalizationService localizationService)
    {
        var keywords = GetAddProductKeywords();
        
        foreach (var keywordPair in keywords)
        {
            var key = keywordPair.Key;
            var translations = keywordPair.Value;
            
            // Add keyword if it doesn't exist
            await localizationService.AddLanguageKeywordAsync(key, $"Add Product - {key}");
            
            // Add translations for each language
            foreach (var translation in translations)
            {
                await localizationService.SaveTranslationAsync(key, translation.Value, translation.Key);
            }
        }
    }
    
    private static Dictionary<string, Dictionary<string, string>> GetAddProductKeywords()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            // Page and Navigation
            {
                "add_product_title",
                new Dictionary<string, string>
                {
                    { "en", "Add New Product" },
                    { "ur", "نیا پروڈکٹ شامل کریں" }
                }
            },
            {
                "back_button",
                new Dictionary<string, string>
                {
                    { "en", "Back" },
                    { "ur", "واپس" }
                }
            },
            {
                "save_button",
                new Dictionary<string, string>
                {
                    { "en", "Save Product" },
                    { "ur", "پروڈکٹ محفوظ کریں" }
                }
            },
            {
                "cancel_button",
                new Dictionary<string, string>
                {
                    { "en", "Cancel" },
                    { "ur", "منسوخ کریں" }
                }
            },
            {
                "reset_button",
                new Dictionary<string, string>
                {
                    { "en", "Reset Form" },
                    { "ur", "فارم ری سیٹ کریں" }
                }
            },
            {
                "save_changes_button",
                new Dictionary<string, string>
                {
                    { "en", "Save Changes" },
                    { "ur", "تبدیلیاں محفوظ کریں" }
                }
            },

            // Navigation Sections
            {
                "basic_info_section",
                new Dictionary<string, string>
                {
                    { "en", "Basic Information" },
                    { "ur", "بنیادی معلومات" }
                }
            },
            {
                "pricing_section",
                new Dictionary<string, string>
                {
                    { "en", "Pricing & Cost" },
                    { "ur", "قیمت اور لاگت" }
                }
            },
            {
                "stock_section",
                new Dictionary<string, string>
                {
                    { "en", "Stock Management" },
                    { "ur", "اسٹاک کا انتظام" }
                }
            },
            {
                "barcodes_section",
                new Dictionary<string, string>
                {
                    { "en", "Barcodes & SKU" },
                    { "ur", "بارکوڈز اور SKU" }
                }
            },
            {
                "tax_section",
                new Dictionary<string, string>
                {
                    { "en", "Tax & Discounts" },
                    { "ur", "ٹیکس اور رعایات" }
                }
            },
            {
                "advanced_section",
                new Dictionary<string, string>
                {
                    { "en", "Advanced Settings" },
                    { "ur", "ایڈوانسڈ سیٹنگز" }
                }
            },
            {
                "Pictures",
                new Dictionary<string, string>
                {
                    { "en", "Pictures" },
                    { "ur", "تصاویر" }
                }
            },
            {
                "Attributes",
                new Dictionary<string, string>
                {
                    { "en", "Attributes" },
                    { "ur", "خصوصیات" }
                }
            },
            {
                "UnitPrices",
                new Dictionary<string, string>
                {
                    { "en", "Unit Prices" },
                    { "ur", "یونٹ قیمتیں" }
                }
            },

            // Basic Information Fields
            {
                "product_code_label",
                new Dictionary<string, string>
                {
                    { "en", "Product Code" },
                    { "ur", "پروڈکٹ کوڈ" }
                }
            },
            {
                "product_name_label",
                new Dictionary<string, string>
                {
                    { "en", "Product Name" },
                    { "ur", "پروڈکٹ کا نام" }
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
                "category_label",
                new Dictionary<string, string>
                {
                    { "en", "Category" },
                    { "ur", "کیٹگری" }
                }
            },
            {
                "brand_label",
                new Dictionary<string, string>
                {
                    { "en", "Brand" },
                    { "ur", "برانڈ" }
                }
            },
            {
                "purchase_unit_label",
                new Dictionary<string, string>
                {
                    { "en", "Purchase Unit" },
                    { "ur", "خریداری کی اکائی" }
                }
            },
            {
                "selling_unit_label", 
                new Dictionary<string, string>
                {
                    { "en", "Selling Unit" },
                    { "ur", "فروخت کی اکائی" }
                }
            },
            {
                "unit_of_measurement_label",
                new Dictionary<string, string>
                {
                    { "en", "Unit of Measurement" },
                    { "ur", "پیمائش کی اکائی" }
                }
            },
            {
                "image_label",
                new Dictionary<string, string>
                {
                    { "en", "Product Image" },
                    { "ur", "پروڈکٹ کی تصویر" }
                }
            },
            {
                "color_label",
                new Dictionary<string, string>
                {
                    { "en", "Color" },
                    { "ur", "رنگ" }
                }
            },

            // Pricing Fields
            {
                "selling_price_label",
                new Dictionary<string, string>
                {
                    { "en", "Selling Price" },
                    { "ur", "فروخت کی قیمت" }
                }
            },
            {
                "cost_price_label",
                new Dictionary<string, string>
                {
                    { "en", "Cost Price" },
                    { "ur", "لاگت کی قیمت" }
                }
            },
            {
                "markup_label",
                new Dictionary<string, string>
                {
                    { "en", "Markup %" },
                    { "ur", "مارک اپ %" }
                }
            },
            {
                "tax_inclusive_label",
                new Dictionary<string, string>
                {
                    { "en", "Tax Inclusive Price" },
                    { "ur", "ٹیکس شامل قیمت" }
                }
            },

            // Stock Management Fields
            {
                "track_stock_label",
                new Dictionary<string, string>
                {
                    { "en", "Track Stock" },
                    { "ur", "اسٹاک ٹریک کریں" }
                }
            },
            {
                "initial_stock_label",
                new Dictionary<string, string>
                {
                    { "en", "Initial Stock Quantity" },
                    { "ur", "ابتدائی اسٹاک مقدار" }
                }
            },
            {
                "minimum_stock_label",
                new Dictionary<string, string>
                {
                    { "en", "Minimum Stock Level" },
                    { "ur", "کم سے کم اسٹاک لیول" }
                }
            },
            {
                "maximum_stock_label",
                new Dictionary<string, string>
                {
                    { "en", "Maximum Stock Level" },
                    { "ur", "زیادہ سے زیادہ اسٹاک لیول" }
                }
            },
            {
                "reorder_level_label",
                new Dictionary<string, string>
                {
                    { "en", "Reorder Level" },
                    { "ur", "دوبارہ آرڈر لیول" }
                }
            },
            {
                "reorder_quantity_label",
                new Dictionary<string, string>
                {
                    { "en", "Reorder Quantity" },
                    { "ur", "دوبارہ آرڈر مقدار" }
                }
            },
            {
                "allow_negative_stock_label",
                new Dictionary<string, string>
                {
                    { "en", "Allow Negative Stock" },
                    { "ur", "منفی اسٹاک کی اجازت دیں" }
                }
            },

            // Barcode Fields
            {
                "barcode_label",
                new Dictionary<string, string>
                {
                    { "en", "Barcode" },
                    { "ur", "بارکوڈ" }
                }
            },
            {
                "generate_barcode_button",
                new Dictionary<string, string>
                {
                    { "en", "Generate Barcode" },
                    { "ur", "بارکوڈ بنائیں" }
                }
            },
            {
                "add_barcode_button",
                new Dictionary<string, string>
                {
                    { "en", "Add Barcode" },
                    { "ur", "بارکوڈ شامل کریں" }
                }
            },
            {
                "remove_barcode_button",
                new Dictionary<string, string>
                {
                    { "en", "Remove" },
                    { "ur", "ہٹائیں" }
                }
            },

            // Tax and Discount Fields
            {
                "tax_rate_label",
                new Dictionary<string, string>
                {
                    { "en", "Tax Rate %" },
                    { "ur", "ٹیکس کی شرح %" }
                }
            },
            {
                "excise_label",
                new Dictionary<string, string>
                {
                    { "en", "Excise" },
                    { "ur", "ایکسائز" }
                }
            },
            {
                "discount_allowed_label",
                new Dictionary<string, string>
                {
                    { "en", "Discount Allowed" },
                    { "ur", "رعایت کی اجازت" }
                }
            },
            {
                "max_discount_label",
                new Dictionary<string, string>
                {
                    { "en", "Maximum Discount %" },
                    { "ur", "زیادہ سے زیادہ رعایت %" }
                }
            },

            // Advanced Settings
            {
                "serial_numbers_label",
                new Dictionary<string, string>
                {
                    { "en", "Use Serial Numbers" },
                    { "ur", "سیریل نمبرز استعمال کریں" }
                }
            },
            {
                "service_item_label",
                new Dictionary<string, string>
                {
                    { "en", "Service Item" },
                    { "ur", "سروس آئٹم" }
                }
            },
            {
                "price_change_allowed_label",
                new Dictionary<string, string>
                {
                    { "en", "Price Change Allowed" },
                    { "ur", "قیمت تبدیل کرنے کی اجازت" }
                }
            },
            {
                "age_restriction_label",
                new Dictionary<string, string>
                {
                    { "en", "Age Restriction" },
                    { "ur", "عمر کی پابندی" }
                }
            },
            {
                "enabled_label",
                new Dictionary<string, string>
                {
                    { "en", "Product Enabled" },
                    { "ur", "پروڈکٹ فعال" }
                }
            },

            // Category Panel
            {
                "add_category_title",
                new Dictionary<string, string>
                {
                    { "en", "Add New Category" },
                    { "ur", "نئی کیٹگری شامل کریں" }
                }
            },
            {
                "edit_category_title",
                new Dictionary<string, string>
                {
                    { "en", "Edit Category" },
                    { "ur", "کیٹگری میں تبدیلی" }
                }
            },
            {
                "category_name_label",
                new Dictionary<string, string>
                {
                    { "en", "Category Name" },
                    { "ur", "کیٹگری کا نام" }
                }
            },
            {
                "category_description_label",
                new Dictionary<string, string>
                {
                    { "en", "Category Description" },
                    { "ur", "کیٹگری کی تفصیل" }
                }
            },
            {
                "parent_category_label",
                new Dictionary<string, string>
                {
                    { "en", "Parent Category" },
                    { "ur", "بنیادی کیٹگری" }
                }
            },
            {
                "save_category_button",
                new Dictionary<string, string>
                {
                    { "en", "Save Category" },
                    { "ur", "کیٹگری محفوظ کریں" }
                }
            },
            {
                "close_panel_button",
                new Dictionary<string, string>
                {
                    { "en", "Close" },
                    { "ur", "بند کریں" }
                }
            },

            // Image Operations
            {
                "choose_image_button",
                new Dictionary<string, string>
                {
                    { "en", "Choose Image" },
                    { "ur", "تصویر منتخب کریں" }
                }
            },
            {
                "remove_image_button",
                new Dictionary<string, string>
                {
                    { "en", "Remove Image" },
                    { "ur", "تصویر ہٹائیں" }
                }
            },

            // Comments
            {
                "comments_label",
                new Dictionary<string, string>
                {
                    { "en", "Comments" },
                    { "ur", "تبصرے" }
                }
            },
            {
                "add_comment_button",
                new Dictionary<string, string>
                {
                    { "en", "Add Comment" },
                    { "ur", "تبصرہ شامل کریں" }
                }
            },
            {
                "new_comment_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Enter comment..." },
                    { "ur", "تبصرہ درج کریں..." }
                }
            },

            // Validation Messages
            {
                "validation_required_field",
                new Dictionary<string, string>
                {
                    { "en", "This field is required" },
                    { "ur", "یہ فیلڈ ضروری ہے" }
                }
            },
            {
                "validation_invalid_price",
                new Dictionary<string, string>
                {
                    { "en", "Please enter a valid price" },
                    { "ur", "براہ کرم درست قیمت درج کریں" }
                }
            },
            {
                "validation_invalid_barcode",
                new Dictionary<string, string>
                {
                    { "en", "Invalid barcode format" },
                    { "ur", "بارکوڈ کا غلط فارمیٹ" }
                }
            },

            // Status Messages
            {
                "status_ready",
                new Dictionary<string, string>
                {
                    { "en", "Ready to create new product" },
                    { "ur", "نیا پروڈکٹ بنانے کے لیے تیار" }
                }
            },
            {
                "status_saving",
                new Dictionary<string, string>
                {
                    { "en", "Saving product..." },
                    { "ur", "پروڈکٹ محفوظ کیا جا رہا ہے..." }
                }
            },
            {
                "status_saved",
                new Dictionary<string, string>
                {
                    { "en", "Product saved successfully!" },
                    { "ur", "پروڈکٹ کامیابی سے محفوظ ہو گیا!" }
                }
            },
            {
                "status_loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading..." },
                    { "ur", "لوڈ ہو رہا ہے..." }
                }
            },

            // Tooltips
            {
                "tooltip_generate_code",
                new Dictionary<string, string>
                {
                    { "en", "Generate unique product code" },
                    { "ur", "منفرد پروڈکٹ کوڈ بنائیں" }
                }
            },
            {
                "tooltip_tax_inclusive",
                new Dictionary<string, string>
                {
                    { "en", "Check if the entered price includes tax" },
                    { "ur", "چیک کریں کہ درج کردہ قیمت میں ٹیکس شامل ہے" }
                }
            },
            {
                "tooltip_serial_numbers",
                new Dictionary<string, string>
                {
                    { "en", "Enable individual serial number tracking for this product" },
                    { "ur", "اس پروڈکٹ کے لیے انفرادی سیریل نمبر ٹریکنگ فعال کریں" }
                }
            },

            // Missing Localization Keys
            {
                "price_including_tax_label",
                new Dictionary<string, string>
                {
                    { "en", "Price Including Tax" },
                    { "ur", "ٹیکس شامل قیمت" }
                }
            },
            {
                "tax_types_label",
                new Dictionary<string, string>
                {
                    { "en", "Tax Types" },
                    { "ur", "ٹیکس کی اقسام" }
                }
            },
            {
                "selected_tax_types_label",
                new Dictionary<string, string>
                {
                    { "en", "Selected Tax Types" },
                    { "ur", "منتخب کردہ ٹیکس کی اقسام" }
                }
            },
            {
                "discounts_label",
                new Dictionary<string, string>
                {
                    { "en", "Product Discounts" },
                    { "ur", "پروڈکٹ کی رعایات" }
                }
            },
            {
                "selected_discounts_label",
                new Dictionary<string, string>
                {
                    { "en", "Selected Product Discounts" },
                    { "ur", "منتخب کردہ پروڈکٹ کی رعایات" }
                }
            },
            {
                "barcode_value_label",
                new Dictionary<string, string>
                {
                    { "en", "Barcode Value:" },
                    { "ur", "بارکوڈ کی قدر:" }
                }
            },
            {
                "add_new_barcode_title",
                new Dictionary<string, string>
                {
                    { "en", "Add New Barcode" },
                    { "ur", "نیا بارکوڈ شامل کریں" }
                }
            },
            {
                "product_barcodes_title",
                new Dictionary<string, string>
                {
                    { "en", "Product Barcodes" },
                    { "ur", "پروڈکٹ بارکوڈز" }
                }
            },
            {
                "no_barcodes_message",
                new Dictionary<string, string>
                {
                    { "en", "No barcodes added yet" },
                    { "ur", "ابھی تک کوئی بارکوڈ شامل نہیں کیا گیا" }
                }
            },
            {
                "add_barcodes_instruction",
                new Dictionary<string, string>
                {
                    { "en", "Add barcode values above or click Generate" },
                    { "ur", "اوپر بارکوڈ کی قیمتیں شامل کریں یا جنریٹ پر کلک کریں" }
                }
            },
            {
                "barcode_type_label",
                new Dictionary<string, string>
                {
                    { "en", "Type:" },
                    { "ur", "قسم:" }
                }
            },
            {
                "primary_image_label",
                new Dictionary<string, string>
                {
                    { "en", "Primary" },
                    { "ur", "بنیادی" }
                }
            },
            {
                "add_image_label",
                new Dictionary<string, string>
                {
                    { "en", "Add Image" },
                    { "ur", "تصویر شامل کریں" }
                }
            },
            {
                "no_images_message",
                new Dictionary<string, string>
                {
                    { "en", "No images added yet" },
                    { "ur", "ابھی تک کوئی تصویر شامل نہیں کی گئی" }
                }
            },
            {
                "add_images_instruction",
                new Dictionary<string, string>
                {
                    { "en", "Click the + button above to add your first image" },
                    { "ur", "اپنی پہلی تصویر شامل کرنے کے لیے اوپر + بٹن پر کلک کریں" }
                }
            },
            {
                "choose_color_label",
                new Dictionary<string, string>
                {
                    { "en", "Choose Color:" },
                    { "ur", "رنگ منتخب کریں:" }
                }
            },
            {
                "selected_color_label",
                new Dictionary<string, string>
                {
                    { "en", "Selected:" },
                    { "ur", "منتخب کردہ:" }
                }
            },
            {
                "add_button_text",
                new Dictionary<string, string>
                {
                    { "en", "Add" },
                    { "ur", "شامل کریں" }
                }
            },
            {
                "generate_button_text",
                new Dictionary<string, string>
                {
                    { "en", "Generate" },
                    { "ur", "بنائیں" }
                }
            },

            // Missing keys from log analysis
            {
                "Categories",
                new Dictionary<string, string>
                {
                    { "en", "Categories" },
                    { "ur", "اقسام" }
                }
            },
            {
                "Comments",
                new Dictionary<string, string>
                {
                    { "en", "Comments" },
                    { "ur", "تبصرے" }
                }
            },
            {
                "plu_label",
                new Dictionary<string, string>
                {
                    { "en", "PLU" },
                    { "ur", "PLU" }
                }
            },
            {
                "price_label",
                new Dictionary<string, string>
                {
                    { "en", "Price" },
                    { "ur", "قیمت" }
                }
            },
            {
                "cost_label",
                new Dictionary<string, string>
                {
                    { "en", "Cost" },
                    { "ur", "لاگت" }
                }
            },
            {
                "last_purchase_price_label",
                new Dictionary<string, string>
                {
                    { "en", "Last Purchase Price" },
                    { "ur", "آخری خریداری کی قیمت" }
                }
            },
            {
                "tax_inclusive_price_label",
                new Dictionary<string, string>
                {
                    { "en", "Tax Inclusive Price" },
                    { "ur", "ٹیکس شامل قیمت" }
                }
            },
            {
                "stock_tracked_label",
                new Dictionary<string, string>
                {
                    { "en", "Stock Tracked" },
                    { "ur", "اسٹاک ٹریک کیا گیا" }
                }
            },
            {
                "average_cost_label",
                new Dictionary<string, string>
                {
                    { "en", "Average Cost" },
                    { "ur", "اوسط لاگت" }
                }
            },
            {
                "store_label",
                new Dictionary<string, string>
                {
                    { "en", "Store" },
                    { "ur", "سٹور" }
                }
            },
            {
                "image_path_label",
                new Dictionary<string, string>
                {
                    { "en", "Image Path" },
                    { "ur", "تصویر کا پتہ" }
                }
            }
        };
    }
}
