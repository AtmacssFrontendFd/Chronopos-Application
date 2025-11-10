using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Dedicated seeder for Add Sales screen translations
/// </summary>
public static class AddSalesTranslationSeeder
{
    public static async Task SeedAddSalesTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var addSalesTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page Title
            {
                "add_sales_title",
                new Dictionary<string, string>
                {
                    { "en", "Sales Window" },
                    { "ur", "فروخت ونڈو" }
                }
            },
            
            // Customer Selection Popup
            {
                "select_customer_title",
                new Dictionary<string, string>
                {
                    { "en", "Select Customer" },
                    { "ur", "کسٹمر منتخب کریں" }
                }
            },
            {
                "search_customer_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search customer..." },
                    { "ur", "...کسٹمر تلاش کریں" }
                }
            },
            {
                "customer_name_label",
                new Dictionary<string, string>
                {
                    { "en", "Name" },
                    { "ur", "نام" }
                }
            },
            {
                "customer_phone_label",
                new Dictionary<string, string>
                {
                    { "en", "Phone" },
                    { "ur", "فون" }
                }
            },
            {
                "customer_balance_label",
                new Dictionary<string, string>
                {
                    { "en", "Balance" },
                    { "ur", "بیلنس" }
                }
            },
            
            // Discount Popup
            {
                "discount_popup_title",
                new Dictionary<string, string>
                {
                    { "en", "Apply Discount" },
                    { "ur", "رعایت لگائیں" }
                }
            },
            {
                "discount_type_label",
                new Dictionary<string, string>
                {
                    { "en", "Discount Type:" },
                    { "ur", ":رعایت کی قسم" }
                }
            },
            {
                "percentage_discount",
                new Dictionary<string, string>
                {
                    { "en", "Percentage (%)" },
                    { "ur", "فیصد (%)" }
                }
            },
            {
                "fixed_discount",
                new Dictionary<string, string>
                {
                    { "en", "Fixed Amount" },
                    { "ur", "مقررہ رقم" }
                }
            },
            {
                "discount_value_label",
                new Dictionary<string, string>
                {
                    { "en", "Discount Value:" },
                    { "ur", ":رعایت کی مقدار" }
                }
            },
            {
                "apply_button",
                new Dictionary<string, string>
                {
                    { "en", "Apply" },
                    { "ur", "لاگو کریں" }
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
            
            // Tax Popup
            {
                "tax_popup_title",
                new Dictionary<string, string>
                {
                    { "en", "Apply Tax" },
                    { "ur", "ٹیکس لگائیں" }
                }
            },
            {
                "tax_type_label",
                new Dictionary<string, string>
                {
                    { "en", "Tax Type:" },
                    { "ur", ":ٹیکس کی قسم" }
                }
            },
            {
                "percentage_tax",
                new Dictionary<string, string>
                {
                    { "en", "Percentage (%)" },
                    { "ur", "فیصد (%)" }
                }
            },
            {
                "fixed_tax",
                new Dictionary<string, string>
                {
                    { "en", "Fixed Amount" },
                    { "ur", "مقررہ رقم" }
                }
            },
            {
                "tax_value_label",
                new Dictionary<string, string>
                {
                    { "en", "Tax Value:" },
                    { "ur", ":ٹیکس کی مقدار" }
                }
            },
            
            // Service Charge Popup
            {
                "service_charge_popup_title",
                new Dictionary<string, string>
                {
                    { "en", "Apply Service Charge" },
                    { "ur", "سروس چارج لگائیں" }
                }
            },
            {
                "service_charge_type_label",
                new Dictionary<string, string>
                {
                    { "en", "Service Charge Type:" },
                    { "ur", ":سروس چارج کی قسم" }
                }
            },
            {
                "percentage_service_charge",
                new Dictionary<string, string>
                {
                    { "en", "Percentage (%)" },
                    { "ur", "فیصد (%)" }
                }
            },
            {
                "fixed_service_charge",
                new Dictionary<string, string>
                {
                    { "en", "Fixed Amount" },
                    { "ur", "مقررہ رقم" }
                }
            },
            {
                "service_charge_value_label",
                new Dictionary<string, string>
                {
                    { "en", "Service Charge Value:" },
                    { "ur", ":سروس چارج کی مقدار" }
                }
            },
            
            // Payment/Settle Popup
            {
                "payment_popup_title",
                new Dictionary<string, string>
                {
                    { "en", "Payment" },
                    { "ur", "ادائیگی" }
                }
            },
            {
                "sale_amount_label",
                new Dictionary<string, string>
                {
                    { "en", "Sale Amount:" },
                    { "ur", ":فروخت کی رقم" }
                }
            },
            {
                "bill_total_label",
                new Dictionary<string, string>
                {
                    { "en", "Bill Total:" },
                    { "ur", ":بل کی کل رقم" }
                }
            },
            {
                "payment_method_label",
                new Dictionary<string, string>
                {
                    { "en", "Payment Method:" },
                    { "ur", ":ادائیگی کا طریقہ" }
                }
            },
            {
                "amount_paid_label",
                new Dictionary<string, string>
                {
                    { "en", "Amount Paid:" },
                    { "ur", ":ادا شدہ رقم" }
                }
            },
            {
                "credit_days_label",
                new Dictionary<string, string>
                {
                    { "en", "Credit Days (for partial payment):" },
                    { "ur", ":(جزوی ادائیگی کے لیے) کریڈٹ دن" }
                }
            },
            {
                "save_settle_button",
                new Dictionary<string, string>
                {
                    { "en", "Save & Settle" },
                    { "ur", "محفوظ اور مکمل کریں" }
                }
            },
            {
                "customer_pending_label",
                new Dictionary<string, string>
                {
                    { "en", "Customer Pending:" },
                    { "ur", ":کسٹمر کا باقی" }
                }
            },
            {
                "your_balance_label",
                new Dictionary<string, string>
                {
                    { "en", "Your Balance:" },
                    { "ur", ":آپ کا باقی" }
                }
            },
            
            // Main Screen Labels
            {
                "categories_label",
                new Dictionary<string, string>
                {
                    { "en", "Categories" },
                    { "ur", "اقسام" }
                }
            },
            {
                "products_label",
                new Dictionary<string, string>
                {
                    { "en", "Products" },
                    { "ur", "مصنوعات" }
                }
            },
            {
                "cart_label",
                new Dictionary<string, string>
                {
                    { "en", "Cart" },
                    { "ur", "ٹوکری" }
                }
            },
            {
                "subtotal_label",
                new Dictionary<string, string>
                {
                    { "en", "Subtotal" },
                    { "ur", "ذیلی کل" }
                }
            },
            {
                "discount_label",
                new Dictionary<string, string>
                {
                    { "en", "Discount" },
                    { "ur", "رعایت" }
                }
            },
            {
                "tax_label",
                new Dictionary<string, string>
                {
                    { "en", "Tax" },
                    { "ur", "ٹیکس" }
                }
            },
            {
                "service_charge_label",
                new Dictionary<string, string>
                {
                    { "en", "Service Charge" },
                    { "ur", "سروس چارج" }
                }
            },
            {
                "total_label",
                new Dictionary<string, string>
                {
                    { "en", "Total" },
                    { "ur", "کل" }
                }
            },
            {
                "save_draft_button",
                new Dictionary<string, string>
                {
                    { "en", "Save Draft" },
                    { "ur", "مسودہ محفوظ کریں" }
                }
            },
            {
                "hold_button",
                new Dictionary<string, string>
                {
                    { "en", "Hold" },
                    { "ur", "روکیں" }
                }
            },
            {
                "settle_button",
                new Dictionary<string, string>
                {
                    { "en", "Settle" },
                    { "ur", "مکمل کریں" }
                }
            },
            {
                "clear_cart_button",
                new Dictionary<string, string>
                {
                    { "en", "Clear Cart" },
                    { "ur", "ٹوکری خالی کریں" }
                }
            },
            {
                "select_customer_button",
                new Dictionary<string, string>
                {
                    { "en", "Select Customer" },
                    { "ur", "کسٹمر منتخب کریں" }
                }
            },
            {
                "select_table_button",
                new Dictionary<string, string>
                {
                    { "en", "Select Table" },
                    { "ur", "میز منتخب کریں" }
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
            {
                "empty_cart_message",
                new Dictionary<string, string>
                {
                    { "en", "Your cart is empty" },
                    { "ur", "آپ کی ٹوکری خالی ہے" }
                }
            },
            {
                "add_products_message",
                new Dictionary<string, string>
                {
                    { "en", "Add products to get started" },
                    { "ur", "شروع کرنے کے لیے مصنوعات شامل کریں" }
                }
            },
            {
                "walk_in_customer",
                new Dictionary<string, string>
                {
                    { "en", "Walk-in Customer" },
                    { "ur", "واک ان کسٹمر" }
                }
            },
            {
                "no_table_selected",
                new Dictionary<string, string>
                {
                    { "en", "No Table" },
                    { "ur", "کوئی میز نہیں" }
                }
            },
            {
                "search_products_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search products..." },
                    { "ur", "...مصنوعات تلاش کریں" }
                }
            },
            {
                "validation_error",
                new Dictionary<string, string>
                {
                    { "en", "Validation Error" },
                    { "ur", "توثیق کی خرابی" }
                }
            },
            {
                "please_enter_valid_value",
                new Dictionary<string, string>
                {
                    { "en", "Please enter a valid value." },
                    { "ur", ".براہ کرم درست قدر درج کریں" }
                }
            },
            
            // Additional Main Screen Labels
            {
                "add_sales_categories_header",
                new Dictionary<string, string>
                {
                    { "en", "Categories" },
                    { "ur", "زمرے" }
                }
            },
            {
                "add_sales_products_header",
                new Dictionary<string, string>
                {
                    { "en", "Products" },
                    { "ur", "مصنوعات" }
                }
            },
            {
                "add_sales_product_groups_header",
                new Dictionary<string, string>
                {
                    { "en", "Product Groups" },
                    { "ur", "مصنوعات کے گروپ" }
                }
            },
            {
                "add_sales_cart_header",
                new Dictionary<string, string>
                {
                    { "en", "Cart" },
                    { "ur", "کارٹ" }
                }
            },
            {
                "add_sales_save_button",
                new Dictionary<string, string>
                {
                    { "en", "Save" },
                    { "ur", "محفوظ کریں" }
                }
            },
            {
                "add_sales_save_print_button",
                new Dictionary<string, string>
                {
                    { "en", "Save & Print" },
                    { "ur", "محفوظ اور پرنٹ کریں" }
                }
            },
            {
                "add_sales_pay_later_button",
                new Dictionary<string, string>
                {
                    { "en", "Pay Later" },
                    { "ur", "بعد میں ادا کریں" }
                }
            },
            {
                "add_sales_settle_button",
                new Dictionary<string, string>
                {
                    { "en", "Settle" },
                    { "ur", "طے کریں" }
                }
            },
            {
                "add_sales_refund_button",
                new Dictionary<string, string>
                {
                    { "en", "Refund" },
                    { "ur", "واپسی" }
                }
            },
            {
                "add_sales_exchange_button",
                new Dictionary<string, string>
                {
                    { "en", "Exchange" },
                    { "ur", "تبادلہ" }
                }
            },
            {
                "add_sales_clear_cart_button",
                new Dictionary<string, string>
                {
                    { "en", "Clear Cart" },
                    { "ur", "کارٹ صاف کریں" }
                }
            },
            {
                "add_sales_customer_label",
                new Dictionary<string, string>
                {
                    { "en", "Customer" },
                    { "ur", "گاہک" }
                }
            },
            {
                "add_sales_table_label",
                new Dictionary<string, string>
                {
                    { "en", "Table" },
                    { "ur", "میز" }
                }
            },
            {
                "add_sales_location_label",
                new Dictionary<string, string>
                {
                    { "en", "Location" },
                    { "ur", "مقام" }
                }
            },
            {
                "add_sales_reservation_label",
                new Dictionary<string, string>
                {
                    { "en", "Reservation" },
                    { "ur", "ریزرویشن" }
                }
            },
            {
                "add_sales_subtotal_label",
                new Dictionary<string, string>
                {
                    { "en", "Subtotal:" },
                    { "ur", ":ذیلی کل" }
                }
            },
            {
                "add_sales_tax_label",
                new Dictionary<string, string>
                {
                    { "en", "Tax:" },
                    { "ur", ":ٹیکس" }
                }
            },
            {
                "add_sales_discount_label",
                new Dictionary<string, string>
                {
                    { "en", "Discount:" },
                    { "ur", ":رعایت" }
                }
            },
            {
                "add_sales_service_charge_label",
                new Dictionary<string, string>
                {
                    { "en", "Service Charge:" },
                    { "ur", ":سروس چارج" }
                }
            },
            {
                "add_sales_total_label",
                new Dictionary<string, string>
                {
                    { "en", "Total:" },
                    { "ur", ":کل" }
                }
            },
            {
                "add_sales_add_discount_button",
                new Dictionary<string, string>
                {
                    { "en", "Add Discount" },
                    { "ur", "رعایت شامل کریں" }
                }
            },
            {
                "add_sales_add_tax_button",
                new Dictionary<string, string>
                {
                    { "en", "Add Tax" },
                    { "ur", "ٹیکس شامل کریں" }
                }
            },
            {
                "add_sales_add_service_charge_button",
                new Dictionary<string, string>
                {
                    { "en", "Add Service Charge" },
                    { "ur", "سروس چارج شامل کریں" }
                }
            },
            {
                "add_sales_all_categories",
                new Dictionary<string, string>
                {
                    { "en", "All" },
                    { "ur", "تمام" }
                }
            },
            {
                "add_sales_items_label",
                new Dictionary<string, string>
                {
                    { "en", "items" },
                    { "ur", "اشیاء" }
                }
            },
            {
                "add_sales_search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search products..." },
                    { "ur", "...مصنوعات تلاش کریں" }
                }
            },
            {
                "add_sales_quantity_label",
                new Dictionary<string, string>
                {
                    { "en", "Qty:" },
                    { "ur", ":مقدار" }
                }
            },
            {
                "add_sales_price_label",
                new Dictionary<string, string>
                {
                    { "en", "Price:" },
                    { "ur", ":قیمت" }
                }
            },
            {
                "add_sales_remove_label",
                new Dictionary<string, string>
                {
                    { "en", "Remove" },
                    { "ur", "ہٹائیں" }
                }
            },
            {
                "add_sales_add_customer_button",
                new Dictionary<string, string>
                {
                    { "en", "+ Add Customer" },
                    { "ur", "+ گاہک شامل کریں" }
                }
            },
            {
                "add_sales_table_mode",
                new Dictionary<string, string>
                {
                    { "en", "Table" },
                    { "ur", "میز" }
                }
            },
            {
                "add_sales_reservation_mode",
                new Dictionary<string, string>
                {
                    { "en", "Reservation" },
                    { "ur", "ریزرویشن" }
                }
            },
            {
                "add_sales_header",
                new Dictionary<string, string>
                {
                    { "en", "Add Sales" },
                    { "ur", "فروخت شامل کریں" }
                }
            },
            {
                "add_sales_scan_barcode",
                new Dictionary<string, string>
                {
                    { "en", "Scan Barcode" },
                    { "ur", "بارکوڈ اسکین کریں" }
                }
            },
            {
                "add_sales_phone_number",
                new Dictionary<string, string>
                {
                    { "en", "Phone Number" },
                    { "ur", "فون نمبر" }
                }
            },
            {
                "add_sales_create_button",
                new Dictionary<string, string>
                {
                    { "en", "+ Create" },
                    { "ur", "+ بنائیں" }
                }
            },
            {
                "add_sales_qty_label",
                new Dictionary<string, string>
                {
                    { "en", "Qty: " },
                    { "ur", " :مقدار" }
                }
            }
        };

        await SeedTranslationCategory("AddSales", addSalesTranslations, localizationService);
    }

    private static async Task SeedTranslationCategory(string category, Dictionary<string, Dictionary<string, string>> translations, IDatabaseLocalizationService localizationService)
    {
        Console.WriteLine($"🔧 [AddSalesTranslationSeeder] Seeding {category} translations...");
        
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
        
        Console.WriteLine($"✅ [AddSalesTranslationSeeder] {category} translations seeded successfully");
    }
}
