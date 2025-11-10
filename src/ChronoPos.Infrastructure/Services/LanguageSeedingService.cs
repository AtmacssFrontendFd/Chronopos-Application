using ChronoPos.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service responsible for seeding all language translations during application startup
/// </summary>
public interface ILanguageSeedingService
{
    Task SeedAllTranslationsAsync();
    Task EnsureBasicLanguagesExistAsync();
}

public class LanguageSeedingService : ILanguageSeedingService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LanguageSeedingService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task EnsureBasicLanguagesExistAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();

        // Ensure English language exists
        var englishLang = await context.Languages
            .FirstOrDefaultAsync(l => l.LanguageCode == "en");
        
        if (englishLang == null)
        {
            englishLang = new Language
            {
                LanguageName = "English",
                LanguageCode = "en",
                IsRtl = false,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            context.Languages.Add(englishLang);
        }

        // Ensure Urdu language exists
        var urduLang = await context.Languages
            .FirstOrDefaultAsync(l => l.LanguageCode == "ur");
        
        if (urduLang == null)
        {
            urduLang = new Language
            {
                LanguageName = "اردو",
                LanguageCode = "ur",
                IsRtl = true,
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            };
            context.Languages.Add(urduLang);
        }

        await context.SaveChangesAsync();
    }

    public async Task SeedAllTranslationsAsync()
    {
        Console.WriteLine("🌐 [LanguageSeedingService] Starting comprehensive language seeding...");
        
        // Ensure basic languages exist first
        await EnsureBasicLanguagesExistAsync();
        
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
        var localizationService = scope.ServiceProvider.GetRequiredService<IDatabaseLocalizationService>();

        // Seed all translation categories
        await SeedNavigationTranslationsAsync(localizationService);
        await SeedCommonTranslationsAsync(localizationService);
        await SeedAddProductTranslationsAsync(localizationService);
        await SeedAddGrnTranslationsAsync(localizationService);
        await SeedProductManagementTranslationsAsync(localizationService);
        await SeedStockManagementTranslationsAsync(localizationService);
        await SeedManagementTranslationsAsync(localizationService);
        await SeedAddOptionsTranslationsAsync(localizationService);
        await SeedSettingsTranslationsAsync(localizationService);
        await SeedSalesTranslationsAsync(localizationService);
        await SeedReportsTranslationsAsync(localizationService);
        await SeedBrandTranslationsAsync(localizationService);
        await SeedCategoryTranslationsAsync(localizationService);
        await SeedProductAttributeTranslationsAsync(localizationService);
        await SeedProductGroupTranslationsAsync(localizationService);
        await SeedProductModifierTranslationsAsync(localizationService);
        await SeedProductCombinationTranslationsAsync(localizationService);
        await SeedPriceTypeTranslationsAsync(localizationService);
        await SeedPaymentTypeTranslationsAsync(localizationService);
        await SeedTaxTypeTranslationsAsync(localizationService);
        await SeedUomTranslationsAsync(localizationService);
        await SeedStoreTranslationsAsync(localizationService);
        await SeedDiscountTranslationsAsync(localizationService);
        await SeedCurrencyTranslationsAsync(localizationService);
        
        Console.WriteLine("✅ [LanguageSeedingService] All translations seeded successfully");
    }

    private async Task SeedNavigationTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var navigationTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "Dashboard",
                new Dictionary<string, string>
                {
                    { "en", "Dashboard" },
                    { "ur", "ڈیش بورڈ" }
                }
            },
            {
                "Transactions",
                new Dictionary<string, string>
                {
                    { "en", "Transactions" },
                    { "ur", "لین دین" }
                }
            },
            {
                "Management",
                new Dictionary<string, string>
                {
                    { "en", "Management" },
                    { "ur", "انتظام" }
                }
            },
            {
                "Reservation",
                new Dictionary<string, string>
                {
                    { "en", "Reservation" },
                    { "ur", "بکنگ" }
                }
            },
            {
                "OrderTable",
                new Dictionary<string, string>
                {
                    { "en", "Order Table" },
                    { "ur", "آرڈر ٹیبل" }
                }
            },
            {
                "Reports",
                new Dictionary<string, string>
                {
                    { "en", "Reports" },
                    { "ur", "رپورٹس" }
                }
            },
            {
                "Settings",
                new Dictionary<string, string>
                {
                    { "en", "Settings" },
                    { "ur", "ترتیبات" }
                }
            },
            {
                "Logout",
                new Dictionary<string, string>
                {
                    { "en", "Logout" },
                    { "ur", "لاگ آؤٹ" }
                }
            }
        };

        await SeedTranslationCategory("Navigation", navigationTranslations, localizationService);
    }

    private async Task SeedCommonTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var commonTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "Save",
                new Dictionary<string, string>
                {
                    { "en", "Save" },
                    { "ur", "محفوظ کریں" }
                }
            },
            {
                "Cancel",
                new Dictionary<string, string>
                {
                    { "en", "Cancel" },
                    { "ur", "منسوخ" }
                }
            },
            {
                "Delete",
                new Dictionary<string, string>
                {
                    { "en", "Delete" },
                    { "ur", "حذف کریں" }
                }
            },
            {
                "Edit",
                new Dictionary<string, string>
                {
                    { "en", "Edit" },
                    { "ur", "تبدیل کریں" }
                }
            },
            {
                "Add",
                new Dictionary<string, string>
                {
                    { "en", "Add" },
                    { "ur", "شامل کریں" }
                }
            },
            {
                "Search",
                new Dictionary<string, string>
                {
                    { "en", "Search" },
                    { "ur", "تلاش" }
                }
            },
            {
                "Filter",
                new Dictionary<string, string>
                {
                    { "en", "Filter" },
                    { "ur", "فلٹر" }
                }
            },
            {
                "Export",
                new Dictionary<string, string>
                {
                    { "en", "Export" },
                    { "ur", "برآمد" }
                }
            },
            {
                "Import",
                new Dictionary<string, string>
                {
                    { "en", "Import" },
                    { "ur", "درآمد" }
                }
            },
            {
                "Print",
                new Dictionary<string, string>
                {
                    { "en", "Print" },
                    { "ur", "پرنٹ" }
                }
            },
            {
                "Loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading..." },
                    { "ur", "لوڈ ہو رہا ہے..." }
                }
            },
            {
                "Error",
                new Dictionary<string, string>
                {
                    { "en", "Error" },
                    { "ur", "خرابی" }
                }
            },
            {
                "Success",
                new Dictionary<string, string>
                {
                    { "en", "Success" },
                    { "ur", "کامیابی" }
                }
            },
            {
                "Warning",
                new Dictionary<string, string>
                {
                    { "en", "Warning" },
                    { "ur", "انتباہ" }
                }
            },
            {
                "Information",
                new Dictionary<string, string>
                {
                    { "en", "Information" },
                    { "ur", "معلومات" }
                }
            },
            {
                "Yes",
                new Dictionary<string, string>
                {
                    { "en", "Yes" },
                    { "ur", "ہاں" }
                }
            },
            {
                "No",
                new Dictionary<string, string>
                {
                    { "en", "No" },
                    { "ur", "نہیں" }
                }
            },
            {
                "OK",
                new Dictionary<string, string>
                {
                    { "en", "OK" },
                    { "ur", "ٹھیک ہے" }
                }
            },
            {
                "Administrator",
                new Dictionary<string, string>
                {
                    { "en", "Administrator" },
                    { "ur", "ایڈمنسٹریٹر" }
                }
            },
            // Prefixed common translations for consistency
            {
                "common.refresh",
                new Dictionary<string, string>
                {
                    { "en", "Refresh" },
                    { "ur", "تازہ کریں" }
                }
            },
            {
                "common.import",
                new Dictionary<string, string>
                {
                    { "en", "Import" },
                    { "ur", "درآمد" }
                }
            },
            {
                "common.export",
                new Dictionary<string, string>
                {
                    { "en", "Export" },
                    { "ur", "برآمد" }
                }
            },
            {
                "common.edit",
                new Dictionary<string, string>
                {
                    { "en", "Edit" },
                    { "ur", "ترمیم" }
                }
            },
            {
                "common.delete",
                new Dictionary<string, string>
                {
                    { "en", "Delete" },
                    { "ur", "حذف کریں" }
                }
            },
            {
                "common.save",
                new Dictionary<string, string>
                {
                    { "en", "Save" },
                    { "ur", "محفوظ کریں" }
                }
            },
            {
                "common.cancel",
                new Dictionary<string, string>
                {
                    { "en", "Cancel" },
                    { "ur", "منسوخ" }
                }
            },
            {
                "common.clear_filters",
                new Dictionary<string, string>
                {
                    { "en", "Clear Filters" },
                    { "ur", "فلٹر صاف کریں" }
                }
            },
            {
                "common.search",
                new Dictionary<string, string>
                {
                    { "en", "Search" },
                    { "ur", "تلاش" }
                }
            },
            {
                "common.add",
                new Dictionary<string, string>
                {
                    { "en", "Add" },
                    { "ur", "شامل کریں" }
                }
            },
            {
                "common.active",
                new Dictionary<string, string>
                {
                    { "en", "Active" },
                    { "ur", "فعال" }
                }
            },
            {
                "common.inactive",
                new Dictionary<string, string>
                {
                    { "en", "Inactive" },
                    { "ur", "غیر فعال" }
                }
            },
            {
                "common.loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading product groups..." },
                    { "ur", "پروڈکٹ گروپس لوڈ ہو رہے ہیں..." }
                }
            },
            {
                "common.of",
                new Dictionary<string, string>
                {
                    { "en", "of" },
                    { "ur", "میں سے" }
                }
            },
            {
                "common.showing",
                new Dictionary<string, string>
                {
                    { "en", "Showing" },
                    { "ur", "دکھایا جا رہا ہے" }
                }
            }
        };

        await SeedTranslationCategory("Common", commonTranslations, localizationService);
    }

    private async Task SeedAddProductTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var addProductTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page and Navigation
            {
                "add_product_title",
                new Dictionary<string, string>
                {
                    { "en", "Add Product" },
                    { "ur", "نیا پروڈکٹ شامل کریں" }
                }
            },
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
                    { "en", "Tax & Pricing" },
                    { "ur", "ٹیکس اور قیمت" }
                }
            },
            {
                "barcodes_section",
                new Dictionary<string, string>
                {
                    { "en", "Product Barcodes" },
                    { "ur", "پروڈکٹ بارکوڈز" }
                }
            },
            {
                "Pictures",
                new Dictionary<string, string>
                {
                    { "en", "Product Pictures" },
                    { "ur", "پروڈکٹ تصاویر" }
                }
            },
            {
                "Attributes",
                new Dictionary<string, string>
                {
                    { "en", "Product Attributes" },
                    { "ur", "پروڈکٹ خصوصیات" }
                }
            },
            {
                "UnitPrices",
                new Dictionary<string, string>
                {
                    { "en", "Stock Control & Unit Prices" },
                    { "ur", "اسٹاک کنٹرول اور یونٹ قیمتیں" }
                }
            },
            {
                "modifier_groups_title",
                new Dictionary<string, string>
                {
                    { "en", "Modifier Groups" },
                    { "ur", "موڈیفائر گروپس" }
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
                "category_label",
                new Dictionary<string, string>
                {
                    { "en", "Category" },
                    { "ur", "کیٹگری" }
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
                "save_category_button",
                new Dictionary<string, string>
                {
                    { "en", "Save Category" },
                    { "ur", "کیٹگری محفوظ کریں" }
                }
            },
            {
                "add_category_title",
                new Dictionary<string, string>
                {
                    { "en", "Add New Category" },
                    { "ur", "نئی کیٹگری شامل کریں" }
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
                "group_label",
                new Dictionary<string, string>
                {
                    { "en", "Group" },
                    { "ur", "گروپ" }
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
                "can_return_label",
                new Dictionary<string, string>
                {
                    { "en", "Can Return" },
                    { "ur", "واپس کر سکتے ہیں" }
                }
            },
            {
                "is_grouped_label",
                new Dictionary<string, string>
                {
                    { "en", "Is Grouped" },
                    { "ur", "گروپ کیا گیا ہے" }
                }
            },
            {
                "selling_price_label",
                new Dictionary<string, string>
                {
                    { "en", "Selling Price *" },
                    { "ur", "فروخت کی قیمت *" }
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
                "markup_percent_label",
                new Dictionary<string, string>
                {
                    { "en", "Markup %" },
                    { "ur", "مارک اپ %" }
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
                "choose_image_label",
                new Dictionary<string, string>
                {
                    { "en", "Choose Image" },
                    { "ur", "تصویر منتخب کریں" }
                }
            },
            {
                "remove_image_label",
                new Dictionary<string, string>
                {
                    { "en", "Remove Image" },
                    { "ur", "تصویر ہٹائیں" }
                }
            },
            {
                "no_image_selected_label",
                new Dictionary<string, string>
                {
                    { "en", "No Image Selected" },
                    { "ur", "کوئی تصویر منتخب نہیں" }
                }
            },
            {
                "click_choose_image_label",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Choose Image' to add a product picture" },
                    { "ur", "پروڈکٹ کی تصویر شامل کرنے کے لیے 'تصویر منتخب کریں' پر کلک کریں" }
                }
            },
            {
                "track_stock_for_product_label",
                new Dictionary<string, string>
                {
                    { "en", "Track Stock for this Product" },
                    { "ur", "اس پروڈکٹ کے لیے اسٹاک ٹریک کریں" }
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
                "initial_stock_label",
                new Dictionary<string, string>
                {
                    { "en", "Initial Stock" },
                    { "ur", "ابتدائی اسٹاک" }
                }
            },
            {
                "minimum_stock_label",
                new Dictionary<string, string>
                {
                    { "en", "Minimum Stock" },
                    { "ur", "کم سے کم اسٹاک" }
                }
            },
            {
                "maximum_stock_label",
                new Dictionary<string, string>
                {
                    { "en", "Maximum Stock" },
                    { "ur", "زیادہ سے زیادہ اسٹاک" }
                }
            },
            {
                "reorder_quantity_label",
                new Dictionary<string, string>
                {
                    { "en", "Reorder Quantity" },
                    { "ur", "دوبارہ آرڈر کی مقدار" }
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
                "allow_discounts_label",
                new Dictionary<string, string>
                {
                    { "en", "Allow Discounts" },
                    { "ur", "رعایات کی اجازت دیں" }
                }
            },
            {
                "allow_price_changes_label",
                new Dictionary<string, string>
                {
                    { "en", "Allow Price Changes" },
                    { "ur", "قیمت تبدیل کرنے کی اجازت دیں" }
                }
            },
            {
                "use_serial_numbers_label",
                new Dictionary<string, string>
                {
                    { "en", "Use Serial Numbers" },
                    { "ur", "سیریل نمبرز استعمال کریں" }
                }
            },
            {
                "is_service_label",
                new Dictionary<string, string>
                {
                    { "en", "Is Service" },
                    { "ur", "سروس ہے" }
                }
            },
            {
                "age_restriction_years_label",
                new Dictionary<string, string>
                {
                    { "en", "Age Restriction (years)" },
                    { "ur", "عمر کی پابندی (سال)" }
                }
            },
            {
                "product_color_label",
                new Dictionary<string, string>
                {
                    { "en", "Product Color" },
                    { "ur", "پروڈکٹ کا رنگ" }
                }
            },
            {
                "stock_control_unit_prices_label",
                new Dictionary<string, string>
                {
                    { "en", "Stock Control & Unit Prices" },
                    { "ur", "اسٹاک کنٹرول اور یونٹ قیمتیں" }
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
            {
                "max_discount_label",
                new Dictionary<string, string>
                {
                    { "en", "Max Discount %" },
                    { "ur", "زیادہ سے زیادہ رعایت %" }
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
                "description_label",
                new Dictionary<string, string>
                {
                    { "en", "Description" },
                    { "ur", "تفصیل" }
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

            // Multi-UOM Labels
            {
                "uom_label",
                new Dictionary<string, string>
                {
                    { "en", "UOM" },
                    { "ur", "یونٹ" }
                }
            },
            {
                "qty_in_unit_label",
                new Dictionary<string, string>
                {
                    { "en", "Qty in Unit" },
                    { "ur", "یونٹ میں مقدار" }
                }
            },
            {
                "cost_of_unit_label",
                new Dictionary<string, string>
                {
                    { "en", "Cost of Unit" },
                    { "ur", "یونٹ کی لاگت" }
                }
            },
            {
                "price_of_unit_label",
                new Dictionary<string, string>
                {
                    { "en", "Price of Unit" },
                    { "ur", "یونٹ کی قیمت" }
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
                "is_base_label",
                new Dictionary<string, string>
                {
                    { "en", "Is Base" },
                    { "ur", "بنیادی ہے" }
                }
            },
            {
                "action_label",
                new Dictionary<string, string>
                {
                    { "en", "Action" },
                    { "ur", "عمل" }
                }
            },
            {
                "pricing_cost_section",
                new Dictionary<string, string>
                {
                    { "en", "Pricing & Cost" },
                    { "ur", "قیمت اور لاگت" }
                }
            }
        };

        await SeedTranslationCategory("AddProduct", addProductTranslations, localizationService);
    }

    private async Task SeedAddGrnTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var addGrnTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page and Navigation
            {
                "add_grn_title",
                new Dictionary<string, string>
                {
                    { "en", "Add Goods Received Note" },
                    { "ur", "اجناس وصولی نوٹ شامل کریں" }
                }
            },
            {
                "grn_header_section",
                new Dictionary<string, string>
                {
                    { "en", "GRN Header" },
                    { "ur", "جی آر این ہیڈر" }
                }
            },
            {
                "grn_items_section",
                new Dictionary<string, string>
                {
                    { "en", "GRN Items" },
                    { "ur", "جی آر این آئٹمز" }
                }
            },
            {
                "summary_section",
                new Dictionary<string, string>
                {
                    { "en", "Summary" },
                    { "ur", "خلاصہ" }
                }
            },

            // GRN Header Fields
            {
                "grn_no_label",
                new Dictionary<string, string>
                {
                    { "en", "GRN No:" },
                    { "ur", "جی آر این نمبر:" }
                }
            },
            {
                "status_label",
                new Dictionary<string, string>
                {
                    { "en", "Status:" },
                    { "ur", "حالت:" }
                }
            },
            {
                "supplier_label",
                new Dictionary<string, string>
                {
                    { "en", "Supplier:" },
                    { "ur", "سپلائر:" }
                }
            },
            {
                "store_label",
                new Dictionary<string, string>
                {
                    { "en", "Store:" },
                    { "ur", "اسٹور:" }
                }
            },
            {
                "invoice_no_label",
                new Dictionary<string, string>
                {
                    { "en", "Invoice No:" },
                    { "ur", "انوائس نمبر:" }
                }
            },
            {
                "invoice_date_label",
                new Dictionary<string, string>
                {
                    { "en", "Invoice Date:" },
                    { "ur", "انوائس کی تاریخ:" }
                }
            },
            {
                "received_date_label",
                new Dictionary<string, string>
                {
                    { "en", "Received Date:" },
                    { "ur", "وصولی کی تاریخ:" }
                }
            },
            {
                "remarks_label",
                new Dictionary<string, string>
                {
                    { "en", "Remarks:" },
                    { "ur", "تبصرے:" }
                }
            },

            // GRN Items Section
            {
                "add_product_button",
                new Dictionary<string, string>
                {
                    { "en", "+ Add Product" },
                    { "ur", "+ پروڈکٹ شامل کریں" }
                }
            },
            {
                "product_label",
                new Dictionary<string, string>
                {
                    { "en", "Product" },
                    { "ur", "پروڈکٹ" }
                }
            },
            {
                "quantity_label",
                new Dictionary<string, string>
                {
                    { "en", "Quantity" },
                    { "ur", "مقدار" }
                }
            },
            {
                "uom_label",
                new Dictionary<string, string>
                {
                    { "en", "UOM" },
                    { "ur", "یونٹ" }
                }
            },
            {
                "cost_price_label",
                new Dictionary<string, string>
                {
                    { "en", "Cost Price" },
                    { "ur", "لاگت قیمت" }
                }
            },
            {
                "batch_no_label",
                new Dictionary<string, string>
                {
                    { "en", "Batch No" },
                    { "ur", "بیچ نمبر" }
                }
            },
            {
                "manufacture_date_label",
                new Dictionary<string, string>
                {
                    { "en", "Mfg Date" },
                    { "ur", "بنانے کی تاریخ" }
                }
            },
            {
                "expiry_date_label",
                new Dictionary<string, string>
                {
                    { "en", "Expiry Date" },
                    { "ur", "ختم ہونے کی تاریخ" }
                }
            },
            {
                "line_total_label",
                new Dictionary<string, string>
                {
                    { "en", "Line Total" },
                    { "ur", "لائن ٹوٹل" }
                }
            },
            {
                "actions_label",
                new Dictionary<string, string>
                {
                    { "en", "Actions" },
                    { "ur", "عمل" }
                }
            },

            // Summary Section
            {
                "total_items_label",
                new Dictionary<string, string>
                {
                    { "en", "Total Items:" },
                    { "ur", "کل آئٹمز:" }
                }
            },
            {
                "total_quantity_label",
                new Dictionary<string, string>
                {
                    { "en", "Total Quantity:" },
                    { "ur", "کل مقدار:" }
                }
            },
            {
                "total_amount_label",
                new Dictionary<string, string>
                {
                    { "en", "Total Amount:" },
                    { "ur", "کل رقم:" }
                }
            },

            // Button Text
            {
                "back_button",
                new Dictionary<string, string>
                {
                    { "en", "Back" },
                    { "ur", "واپس" }
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
                "post_grn_button",
                new Dictionary<string, string>
                {
                    { "en", "Post GRN" },
                    { "ur", "جی آر این پوسٹ کریں" }
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

            // Messages
            {
                "no_items_message",
                new Dictionary<string, string>
                {
                    { "en", "No items added to GRN" },
                    { "ur", "جی آر این میں کوئی آئٹم شامل نہیں" }
                }
            },
            {
                "add_items_instruction",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Product' to start adding items" },
                    { "ur", "آئٹمز شامل کرنے کے لیے 'پروڈکٹ شامل کریں' پر کلک کریں" }
                }
            }
        };

        await SeedTranslationCategory("AddGrn", addGrnTranslations, localizationService);
    }

    private async Task SeedProductManagementTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var productManagementTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "product_management_title",
                new Dictionary<string, string>
                {
                    { "en", "Product Management" },
                    { "ur", "پروڈکٹ کا انتظام" }
                }
            },
            {
                "products_list",
                new Dictionary<string, string>
                {
                    { "en", "Products List" },
                    { "ur", "پروڈکٹس کی فہرست" }
                }
            },
            {
                "refresh_button",
                new Dictionary<string, string>
                {
                    { "en", "Refresh" },
                    { "ur", "تازہ کریں" }
                }
            },
            {
                "add_new_category_button",
                new Dictionary<string, string>
                {
                    { "en", "Add Category" },
                    { "ur", "کیٹگری شامل کریں" }
                }
            },
            {
                "add_new_product_button",
                new Dictionary<string, string>
                {
                    { "en", "Add Product" },
                    { "ur", "پروڈکٹ شامل کریں" }
                }
            }
        };

        await SeedTranslationCategory("ProductManagement", productManagementTranslations, localizationService);
    }

    private async Task SeedStockManagementTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var stockManagementTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "stock_management_title",
                new Dictionary<string, string>
                {
                    { "en", "Stock Management" },
                    { "ur", "اسٹاک کا انتظام" }
                }
            },
            {
                "stock_adjustment",
                new Dictionary<string, string>
                {
                    { "en", "Stock Adjustment" },
                    { "ur", "اسٹاک میں تبدیلی" }
                }
            }
        };

        await SeedTranslationCategory("StockManagement", stockManagementTranslations, localizationService);
    }

    private async Task SeedSettingsTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var settingsTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "settings_title",
                new Dictionary<string, string>
                {
                    { "en", "Settings" },
                    { "ur", "ترتیبات" }
                }
            },
            {
                "language_settings",
                new Dictionary<string, string>
                {
                    { "en", "Language Settings" },
                    { "ur", "زبان کی ترتیبات" }
                }
            },
            {
                "theme_settings",
                new Dictionary<string, string>
                {
                    { "en", "Theme Settings" },
                    { "ur", "تھیم کی ترتیبات" }
                }
            }
        };

        await SeedTranslationCategory("Settings", settingsTranslations, localizationService);
    }

    private async Task SeedSalesTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var salesTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "sales_title",
                new Dictionary<string, string>
                {
                    { "en", "Sales" },
                    { "ur", "فروخت" }
                }
            },
            {
                "point_of_sale",
                new Dictionary<string, string>
                {
                    { "en", "Point of Sale" },
                    { "ur", "فروخت کا مقام" }
                }
            }
        };

        await SeedTranslationCategory("Sales", salesTranslations, localizationService);
    }

    private async Task SeedReportsTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var reportsTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "reports_title",
                new Dictionary<string, string>
                {
                    { "en", "Reports" },
                    { "ur", "رپورٹس" }
                }
            },
            {
                "sales_report",
                new Dictionary<string, string>
                {
                    { "en", "Sales Report" },
                    { "ur", "فروخت کی رپورٹ" }
                }
            }
        };

        await SeedTranslationCategory("Reports", reportsTranslations, localizationService);
    }

    private async Task SeedManagementTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var managementTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "management.stock",
                new Dictionary<string, string>
                {
                    { "en", "Stock Management" },
                    { "ur", "اسٹاک کا انتظام" }
                }
            },
            {
                "management.products",
                new Dictionary<string, string>
                {
                    { "en", "Product Management" },
                    { "ur", "پروڈکٹ کا انتظام" }
                }
            },
            {
                "management.supplier",
                new Dictionary<string, string>
                {
                    { "en", "Supplier Management" },
                    { "ur", "سپلائر کا انتظام" }
                }
            },
            {
                "management.customers",
                new Dictionary<string, string>
                {
                    { "en", "Customer Management" },
                    { "ur", "کسٹمر کا انتظام" }
                }
            },
            {
                "management.payment",
                new Dictionary<string, string>
                {
                    { "en", "Payment Management" },
                    { "ur", "ادائیگی کا انتظام" }
                }
            },
            {
                "management.service",
                new Dictionary<string, string>
                {
                    { "en", "Service Management" },
                    { "ur", "سروس کا انتظام" }
                }
            },
            {
                "management.add_options",
                new Dictionary<string, string>
                {
                    { "en", "Add Options" },
                    { "ur", "اضافی اختیارات" }
                }
            }
        };

        await SeedTranslationCategory("Management", managementTranslations, localizationService);
    }

    private async Task SeedAddOptionsTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var addOptionsTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "add_options.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Others" },
                    { "ur", "دیگر" }
                }
            },
            {
                "add_options.brand",
                new Dictionary<string, string>
                {
                    { "en", "Brand" },
                    { "ur", "برانڈ" }
                }
            },
            {
                "add_options.category",
                new Dictionary<string, string>
                {
                    { "en", "Category" },
                    { "ur", "کیٹگری" }
                }
            },
            {
                "add_options.currency",
                new Dictionary<string, string>
                {
                    { "en", "Currency" },
                    { "ur", "کرنسی" }
                }
            },
            {
                "add_options.product_attributes",
                new Dictionary<string, string>
                {
                    { "en", "Product Attributes" },
                    { "ur", "پروڈکٹ کی خصوصیات" }
                }
            },
            {
                "add_options.product_combinations",
                new Dictionary<string, string>
                {
                    { "en", "Product Combinations" },
                    { "ur", "پروڈکٹ کے مجموعے" }
                }
            },
            {
                "add_options.product_grouping",
                new Dictionary<string, string>
                {
                    { "en", "Product Grouping" },
                    { "ur", "پروڈکٹ گروپنگ" }
                }
            },
            {
                "add_options.product_modifiers",
                new Dictionary<string, string>
                {
                    { "en", "Product Modifiers" },
                    { "ur", "پروڈکٹ موڈیفائرز" }
                }
            },
            {
                "add_options.price_types",
                new Dictionary<string, string>
                {
                    { "en", "Price Types" },
                    { "ur", "قیمت کی اقسام" }
                }
            },
            {
                "add_options.payment_types",
                new Dictionary<string, string>
                {
                    { "en", "Payment Types" },
                    { "ur", "ادائیگی کی اقسام" }
                }
            },
            {
                "add_options.tax_rates",
                new Dictionary<string, string>
                {
                    { "en", "Tax Rates" },
                    { "ur", "ٹیکس کی شرح" }
                }
            },
            {
                "add_options.customer",
                new Dictionary<string, string>
                {
                    { "en", "Customer" },
                    { "ur", "کسٹمر" }
                }
            },
            {
                "add_options.suppliers",
                new Dictionary<string, string>
                {
                    { "en", "Suppliers" },
                    { "ur", "سپلائرز" }
                }
            },
            {
                "add_options.uom",
                new Dictionary<string, string>
                {
                    { "en", "Unit of Measure (UOM)" },
                    { "ur", "پیمائش کی اکائی" }
                }
            },
            {
                "add_options.shop",
                new Dictionary<string, string>
                {
                    { "en", "Shop" },
                    { "ur", "دکان" }
                }
            },
            {
                "add_options.customer_groups",
                new Dictionary<string, string>
                {
                    { "en", "Customer Groups" },
                    { "ur", "کسٹمر گروپس" }
                }
            },
            {
                "add_options.discounts",
                new Dictionary<string, string>
                {
                    { "en", "Discounts" },
                    { "ur", "رعایت" }
                }
            },
            {
                "add_options.refresh_button",
                new Dictionary<string, string>
                {
                    { "en", "Refresh" },
                    { "ur", "تازہ کریں" }
                }
            }
        };

        await SeedTranslationCategory("Others", addOptionsTranslations, localizationService);
    }

    private async Task SeedBrandTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var brandTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and navigation
            {
                "brand.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Brands" },
                    { "ur", "برانڈز" }
                }
            },
            {
                "brand.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search brands..." },
                    { "ur", "برانڈز تلاش کریں..." }
                }
            },
            {
                "brand.add_brand",
                new Dictionary<string, string>
                {
                    { "en", "Add Brand" },
                    { "ur", "برانڈ شامل کریں" }
                }
            },
            {
                "brand.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            },
            {
                "brand.show_all",
                new Dictionary<string, string>
                {
                    { "en", "Show All" },
                    { "ur", "تمام دکھائیں" }
                }
            },
            // Column headers
            {
                "brand.column.name",
                new Dictionary<string, string>
                {
                    { "en", "Name" },
                    { "ur", "نام" }
                }
            },
            {
                "brand.column.arabic_name",
                new Dictionary<string, string>
                {
                    { "en", "Arabic Name" },
                    { "ur", "عربی نام" }
                }
            },
            {
                "brand.column.description",
                new Dictionary<string, string>
                {
                    { "en", "Description" },
                    { "ur", "تفصیل" }
                }
            },
            {
                "brand.column.products",
                new Dictionary<string, string>
                {
                    { "en", "Products" },
                    { "ur", "مصنوعات" }
                }
            },
            {
                "brand.column.created",
                new Dictionary<string, string>
                {
                    { "en", "Created" },
                    { "ur", "تخلیق شدہ" }
                }
            },
            {
                "brand.column.status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حالت" }
                }
            },
            {
                "brand.column.active",
                new Dictionary<string, string>
                {
                    { "en", "Active" },
                    { "ur", "فعال" }
                }
            },
            {
                "brand.column.actions",
                new Dictionary<string, string>
                {
                    { "en", "Actions" },
                    { "ur", "اعمال" }
                }
            },
            // Empty state
            {
                "brand.no_brands_found",
                new Dictionary<string, string>
                {
                    { "en", "No brands found" },
                    { "ur", "کوئی برانڈ نہیں ملا" }
                }
            },
            {
                "brand.no_brands_message",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Brand' to create your first brand" },
                    { "ur", "اپنا پہلا برانڈ بنانے کے لیے 'برانڈ شامل کریں' پر کلک کریں" }
                }
            },
            {
                "brand.brands_count",
                new Dictionary<string, string>
                {
                    { "en", "brands" },
                    { "ur", "برانڈز" }
                }
            }
        };

        await SeedTranslationCategory("Brand Management", brandTranslations, localizationService);
    }

    private async Task SeedCategoryTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var categoryTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and navigation
            {
                "category.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Categories" },
                    { "ur", "زمرے" }
                }
            },
            {
                "category.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search categories..." },
                    { "ur", "زمرے تلاش کریں..." }
                }
            },
            {
                "category.add_category",
                new Dictionary<string, string>
                {
                    { "en", "Add Category" },
                    { "ur", "زمرہ شامل کریں" }
                }
            },
            {
                "category.add_subcategory",
                new Dictionary<string, string>
                {
                    { "en", "Add Subcategory" },
                    { "ur", "ذیلی زمرہ شامل کریں" }
                }
            },
            // Column headers
            {
                "category.column.name",
                new Dictionary<string, string>
                {
                    { "en", "Name" },
                    { "ur", "نام" }
                }
            },
            {
                "category.column.arabic_name",
                new Dictionary<string, string>
                {
                    { "en", "Arabic Name" },
                    { "ur", "عربی نام" }
                }
            },
            {
                "category.column.description",
                new Dictionary<string, string>
                {
                    { "en", "Description" },
                    { "ur", "تفصیل" }
                }
            },
            {
                "category.column.products",
                new Dictionary<string, string>
                {
                    { "en", "Products" },
                    { "ur", "مصنوعات" }
                }
            },
            {
                "category.column.discounts",
                new Dictionary<string, string>
                {
                    { "en", "Discounts" },
                    { "ur", "رعایات" }
                }
            },
            {
                "category.column.status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حالت" }
                }
            },
            {
                "category.column.actions",
                new Dictionary<string, string>
                {
                    { "en", "Actions" },
                    { "ur", "اعمال" }
                }
            },
            // Empty state
            {
                "category.no_categories_found",
                new Dictionary<string, string>
                {
                    { "en", "No categories found" },
                    { "ur", "کوئی زمرہ نہیں ملا" }
                }
            },
            {
                "category.no_categories_message",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Category' to create your first category" },
                    { "ur", "اپنا پہلا زمرہ بنانے کے لیے 'زمرہ شامل کریں' پر کلک کریں" }
                }
            },
            {
                "category.categories_count",
                new Dictionary<string, string>
                {
                    { "en", "categories" },
                    { "ur", "زمرے" }
                }
            }
        };

        await SeedTranslationCategory("Category Management", categoryTranslations, localizationService);
    }

    private async Task SeedProductAttributeTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var productAttributeTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and navigation
            {
                "productattribute.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Product Attributes" },
                    { "ur", "پروڈکٹ خصوصیات" }
                }
            },
            {
                "productattribute.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search attributes..." },
                    { "ur", "خصوصیات تلاش کریں..." }
                }
            },
            {
                "productattribute.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            },
            {
                "productattribute.show_all",
                new Dictionary<string, string>
                {
                    { "en", "Show All" },
                    { "ur", "سب دکھائیں" }
                }
            },
            // Column headers
            {
                "productattribute.column.attribute",
                new Dictionary<string, string>
                {
                    { "en", "Attribute" },
                    { "ur", "خصوصیت" }
                }
            },
            {
                "productattribute.column.value",
                new Dictionary<string, string>
                {
                    { "en", "Value" },
                    { "ur", "قیمت" }
                }
            },
            {
                "productattribute.column.description",
                new Dictionary<string, string>
                {
                    { "en", "Description" },
                    { "ur", "تفصیل" }
                }
            },
            {
                "productattribute.column.status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حالت" }
                }
            },
            {
                "productattribute.column.actions",
                new Dictionary<string, string>
                {
                    { "en", "Actions" },
                    { "ur", "اعمال" }
                }
            },
            // Empty state
            {
                "productattribute.empty_state_title",
                new Dictionary<string, string>
                {
                    { "en", "No Attributes Found" },
                    { "ur", "کوئی خصوصیت نہیں ملی" }
                }
            },
            {
                "productattribute.empty_state_message",
                new Dictionary<string, string>
                {
                    { "en", "Start by adding a new attribute." },
                    { "ur", "نئی خصوصیت شامل کر کے شروع کریں۔" }
                }
            }
        };

        await SeedTranslationCategory("Product Attribute Management", productAttributeTranslations, localizationService);
    }

    private async Task SeedProductGroupTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var productGroupTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and navigation
            {
                "productgroup.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Product Groups" },
                    { "ur", "پروڈکٹ گروپس" }
                }
            },
            {
                "productgroup.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search product groups..." },
                    { "ur", "پروڈکٹ گروپس تلاش کریں..." }
                }
            },
            {
                "productgroup.add_product_group",
                new Dictionary<string, string>
                {
                    { "en", "Add Product Group" },
                    { "ur", "پروڈکٹ گروپ شامل کریں" }
                }
            },
            {
                "productgroup.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            },
            {
                "productgroup.show_all",
                new Dictionary<string, string>
                {
                    { "en", "Show All" },
                    { "ur", "سب دکھائیں" }
                }
            },
            // Column headers
            {
                "productgroup.column.actions",
                new Dictionary<string, string>
                {
                    { "en", "Actions" },
                    { "ur", "اعمال" }
                }
            },
            {
                "productgroup.column.active",
                new Dictionary<string, string>
                {
                    { "en", "Active" },
                    { "ur", "فعال" }
                }
            },
            {
                "productgroup.column.status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حالت" }
                }
            },
            {
                "productgroup.column.products",
                new Dictionary<string, string>
                {
                    { "en", "Products" },
                    { "ur", "مصنوعات" }
                }
            },
            {
                "productgroup.column.sku_prefix",
                new Dictionary<string, string>
                {
                    { "en", "SKU Prefix" },
                    { "ur", "SKU سابقہ" }
                }
            },
            {
                "productgroup.column.description",
                new Dictionary<string, string>
                {
                    { "en", "Description" },
                    { "ur", "تفصیل" }
                }
            },
            {
                "productgroup.column.arabic_name",
                new Dictionary<string, string>
                {
                    { "en", "Arabic Name" },
                    { "ur", "عربی نام" }
                }
            },
            {
                "productgroup.column.group_name",
                new Dictionary<string, string>
                {
                    { "en", "Group Name" },
                    { "ur", "گروپ کا نام" }
                }
            },
            // Empty state
            {
                "productgroup.empty_state_title",
                new Dictionary<string, string>
                {
                    { "en", "No product groups found" },
                    { "ur", "کوئی پروڈکٹ گروپ نہیں ملا" }
                }
            },
            {
                "productgroup.empty_state_message",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Product Group' to create your first group" },
                    { "ur", "اپنا پہلا گروپ بنانے کے لیے 'پروڈکٹ گروپ شامل کریں' پر کلک کریں" }
                }
            },
            // Status messages
            {
                "productgroup.product_groups",
                new Dictionary<string, string>
                {
                    { "en", "product groups" },
                    { "ur", "پروڈکٹ گروپس" }
                }
            },
            {
                "productgroup.no_active_groups",
                new Dictionary<string, string>
                {
                    { "en", "No active product groups found" },
                    { "ur", "کوئی فعال پروڈکٹ گروپ نہیں ملا" }
                }
            },
            {
                "productgroup.no_search_results",
                new Dictionary<string, string>
                {
                    { "en", "No product groups match your search criteria" },
                    { "ur", "آپ کی تلاش کے معیار سے کوئی پروڈکٹ گروپ مطابقت نہیں رکھتا" }
                }
            }
        };

        await SeedTranslationCategory("Product Group Management", productGroupTranslations, localizationService);
    }

    private async Task SeedProductModifierTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var productModifierTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and buttons
            {
                "product_modifier.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Product Modifiers" },
                    { "ur", "پروڈکٹ موڈیفائر" }
                }
            },
            {
                "product_modifier.refresh_button",
                new Dictionary<string, string>
                {
                    { "en", "Refresh" },
                    { "ur", "تازہ کریں" }
                }
            },
            {
                "product_modifier.add_modifier_group",
                new Dictionary<string, string>
                {
                    { "en", "Add Modifier Group" },
                    { "ur", "موڈیفائر گروپ شامل کریں" }
                }
            },
            {
                "product_modifier.add_modifier",
                new Dictionary<string, string>
                {
                    { "en", "Add Modifier" },
                    { "ur", "موڈیفائر شامل کریں" }
                }
            },
            {
                "product_modifier.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search modifiers..." },
                    { "ur", "موڈیفائرز تلاش کریں..." }
                }
            },
            {
                "product_modifier.show_all",
                new Dictionary<string, string>
                {
                    { "en", "Show All" },
                    { "ur", "سب دکھائیں" }
                }
            },
            {
                "product_modifier.clear_filters",
                new Dictionary<string, string>
                {
                    { "en", "Clear Filters" },
                    { "ur", "فلٹرز صاف کریں" }
                }
            },
            // Empty state messages
            {
                "product_modifier.no_groups",
                new Dictionary<string, string>
                {
                    { "en", "No modifier groups available" },
                    { "ur", "کوئی موڈیفائر گروپ دستیاب نہیں" }
                }
            },
            {
                "product_modifier.click_add_group",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Modifier Group' to create one" },
                    { "ur", "موڈیفائر گروپ بنانے کے لیے 'موڈیفائر گروپ شامل کریں' پر کلک کریں" }
                }
            },
            {
                "product_modifier.no_modifiers",
                new Dictionary<string, string>
                {
                    { "en", "No modifiers available" },
                    { "ur", "کوئی موڈیفائر دستیاب نہیں" }
                }
            },
            {
                "product_modifier.click_add_modifier",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Modifier' to create one" },
                    { "ur", "موڈیفائر بنانے کے لیے 'موڈیفائر شامل کریں' پر کلک کریں" }
                }
            },
            {
                "product_modifier.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            }
        };

        await SeedTranslationCategory("Product Modifier Management", productModifierTranslations, localizationService);
    }

    private async Task SeedProductCombinationTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var productCombinationTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and buttons
            {
                "product_combination.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Product Combinations" },
                    { "ur", "پروڈکٹ مجموعہ" }
                }
            },
            {
                "product_combination.refresh_button",
                new Dictionary<string, string>
                {
                    { "en", "Refresh" },
                    { "ur", "تازہ کریں" }
                }
            },
            {
                "product_combination.add_combination",
                new Dictionary<string, string>
                {
                    { "en", "Add Combination" },
                    { "ur", "مجموعہ شامل کریں" }
                }
            },
            {
                "product_combination.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search combinations..." },
                    { "ur", "مجموعے تلاش کریں..." }
                }
            },
            {
                "product_combination.filter",
                new Dictionary<string, string>
                {
                    { "en", "Filter" },
                    { "ur", "فلٹر" }
                }
            },
            {
                "product_combination.clear_filters",
                new Dictionary<string, string>
                {
                    { "en", "Clear Filters" },
                    { "ur", "فلٹرز صاف کریں" }
                }
            },
            // Empty state messages
            {
                "product_combination.no_combinations",
                new Dictionary<string, string>
                {
                    { "en", "No product combinations available" },
                    { "ur", "کوئی پروڈکٹ مجموعہ دستیاب نہیں" }
                }
            },
            {
                "product_combination.click_add_combination",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Combination' to create one" },
                    { "ur", "مجموعہ بنانے کے لیے 'مجموعہ شامل کریں' پر کلک کریں" }
                }
            }
        };

        await SeedTranslationCategory("Product Combination Management", productCombinationTranslations, localizationService);
    }

    private async Task SeedPriceTypeTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var priceTypeTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and navigation
            {
                "pricetype.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Price Types" },
                    { "ur", "قیمت کی اقسام" }
                }
            },
            {
                "pricetype.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search price types..." },
                    { "ur", "قیمت کی اقسام تلاش کریں..." }
                }
            },
            {
                "pricetype.add_price_type",
                new Dictionary<string, string>
                {
                    { "en", "Add Price Type" },
                    { "ur", "قیمت کی قسم شامل کریں" }
                }
            },
            {
                "pricetype.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            },
            {
                "pricetype.show_all",
                new Dictionary<string, string>
                {
                    { "en", "Show All" },
                    { "ur", "سب دکھائیں" }
                }
            },
            // Column headers
            {
                "pricetype.column.actions",
                new Dictionary<string, string>
                {
                    { "en", "Actions" },
                    { "ur", "اعمال" }
                }
            },
            {
                "pricetype.column.active",
                new Dictionary<string, string>
                {
                    { "en", "Active" },
                    { "ur", "فعال" }
                }
            },
            {
                "pricetype.column.status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حالت" }
                }
            },
            {
                "pricetype.column.created",
                new Dictionary<string, string>
                {
                    { "en", "Created" },
                    { "ur", "تخلیق" }
                }
            },
            {
                "pricetype.column.description",
                new Dictionary<string, string>
                {
                    { "en", "Description" },
                    { "ur", "تفصیل" }
                }
            },
            {
                "pricetype.column.arabic_name",
                new Dictionary<string, string>
                {
                    { "en", "Arabic Name" },
                    { "ur", "عربی نام" }
                }
            },
            {
                "pricetype.column.type_name",
                new Dictionary<string, string>
                {
                    { "en", "Type Name" },
                    { "ur", "قسم کا نام" }
                }
            },
            // Empty state
            {
                "pricetype.empty_state_title",
                new Dictionary<string, string>
                {
                    { "en", "No price types found" },
                    { "ur", "کوئی قیمت کی قسم نہیں ملی" }
                }
            },
            {
                "pricetype.empty_state_message",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Price Type' to create your first price type" },
                    { "ur", "اپنی پہلی قیمت کی قسم بنانے کے لیے 'قیمت کی قسم شامل کریں' پر کلک کریں" }
                }
            },
            {
                "pricetype.loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading price types..." },
                    { "ur", "قیمت کی اقسام لوڈ ہو رہی ہیں..." }
                }
            }
        };

        await SeedTranslationCategory("Price Type Management", priceTypeTranslations, localizationService);
    }

    private async Task SeedPaymentTypeTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var paymentTypeTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and navigation
            {
                "paymenttype.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Payment Types" },
                    { "ur", "ادائیگی کی اقسام" }
                }
            },
            {
                "paymenttype.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search payment types..." },
                    { "ur", "ادائیگی کی اقسام تلاش کریں..." }
                }
            },
            {
                "paymenttype.add_payment_type",
                new Dictionary<string, string>
                {
                    { "en", "Add Payment Type" },
                    { "ur", "ادائیگی کی قسم شامل کریں" }
                }
            },
            {
                "paymenttype.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            },
            {
                "paymenttype.show_all",
                new Dictionary<string, string>
                {
                    { "en", "Show All" },
                    { "ur", "سب دکھائیں" }
                }
            },
            // Column headers
            {
                "paymenttype.column.payment_code",
                new Dictionary<string, string>
                {
                    { "en", "Payment Code" },
                    { "ur", "ادائیگی کوڈ" }
                }
            },
            // Empty state
            {
                "paymenttype.empty_state_title",
                new Dictionary<string, string>
                {
                    { "en", "No payment types found" },
                    { "ur", "کوئی ادائیگی کی قسم نہیں ملی" }
                }
            },
            {
                "paymenttype.empty_state_message",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Payment Type' to create your first payment type" },
                    { "ur", "اپنی پہلی ادائیگی کی قسم بنانے کے لیے 'ادائیگی کی قسم شامل کریں' پر کلک کریں" }
                }
            },
            {
                "paymenttype.loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading payment types..." },
                    { "ur", "ادائیگی کی اقسام لوڈ ہو رہی ہیں..." }
                }
            }
        };

        await SeedTranslationCategory("Payment Type Management", paymentTypeTranslations, localizationService);
    }

    private async Task SeedTaxTypeTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var taxTypeTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            // Page title and navigation
            {
                "taxtype.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Tax Types" },
                    { "ur", "ٹیکس کی اقسام" }
                }
            },
            {
                "taxtype.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search tax types..." },
                    { "ur", "ٹیکس کی اقسام تلاش کریں..." }
                }
            },
            {
                "taxtype.add_tax_type",
                new Dictionary<string, string>
                {
                    { "en", "Add Tax Type" },
                    { "ur", "ٹیکس کی قسم شامل کریں" }
                }
            },
            {
                "taxtype.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            },
            {
                "taxtype.show_all",
                new Dictionary<string, string>
                {
                    { "en", "Show All" },
                    { "ur", "سب دکھائیں" }
                }
            },
            // Column headers
            {
                "taxtype.column.applies_to",
                new Dictionary<string, string>
                {
                    { "en", "Applies To" },
                    { "ur", "لاگو ہوتا ہے" }
                }
            },
            {
                "taxtype.column.type",
                new Dictionary<string, string>
                {
                    { "en", "Type" },
                    { "ur", "قسم" }
                }
            },
            {
                "taxtype.column.value",
                new Dictionary<string, string>
                {
                    { "en", "Value" },
                    { "ur", "قیمت" }
                }
            }
        };

        await SeedTranslationCategory("Tax Type Management", taxTypeTranslations, localizationService);
    }

    private async Task SeedUomTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var uomTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "uom.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Unit of Measurement Management" },
                    { "ur", "پیمائش کی اکائی کا انتظام" }
                }
            },
            {
                "uom.add_new",
                new Dictionary<string, string>
                {
                    { "en", "Add UOM" },
                    { "ur", "UOM شامل کریں" }
                }
            },
            {
                "uom.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search units of measurement..." },
                    { "ur", "پیمائش کی اکائیاں تلاش کریں..." }
                }
            },
            {
                "uom.loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading units of measurement..." },
                    { "ur", "پیمائش کی اکائیاں لوڈ ہو رہی ہیں..." }
                }
            },
            {
                "uom.no_data",
                new Dictionary<string, string>
                {
                    { "en", "No units of measurement found" },
                    { "ur", "پیمائش کی کوئی اکائی نہیں ملی" }
                }
            },
            {
                "uom.no_data_hint",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add UOM' to create your first unit of measurement" },
                    { "ur", "اپنی پہلی پیمائش کی اکائی بنانے کے لیے 'UOM شامل کریں' پر کلک کریں" }
                }
            },
            {
                "uom.items_count",
                new Dictionary<string, string>
                {
                    { "en", "units" },
                    { "ur", "اکائیاں" }
                }
            },
            {
                "uom.base_units_only",
                new Dictionary<string, string>
                {
                    { "en", "Base Units Only" },
                    { "ur", "صرف بنیادی اکائیاں" }
                }
            },
            {
                "uom.column.name",
                new Dictionary<string, string>
                {
                    { "en", "Name" },
                    { "ur", "نام" }
                }
            },
            {
                "uom.column.symbol",
                new Dictionary<string, string>
                {
                    { "en", "Symbol" },
                    { "ur", "علامت" }
                }
            },
            {
                "uom.column.type",
                new Dictionary<string, string>
                {
                    { "en", "Type" },
                    { "ur", "قسم" }
                }
            },
            {
                "uom.column.category",
                new Dictionary<string, string>
                {
                    { "en", "Category" },
                    { "ur", "زمرہ" }
                }
            },
            {
                "uom.column.base_uom",
                new Dictionary<string, string>
                {
                    { "en", "Base UOM" },
                    { "ur", "بنیادی UOM" }
                }
            },
            {
                "uom.column.conversion",
                new Dictionary<string, string>
                {
                    { "en", "Conversion" },
                    { "ur", "تبدیلی" }
                }
            },
            {
                "uom.column.created",
                new Dictionary<string, string>
                {
                    { "en", "Created" },
                    { "ur", "تخلیق شدہ" }
                }
            },
            {
                "uom.column.status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حیثیت" }
                }
            }
        };

        await SeedTranslationCategory("UOM Management", uomTranslations, localizationService);
    }

    private async Task SeedStoreTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var storeTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "store.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Store Management" },
                    { "ur", "دکان کا انتظام" }
                }
            },
            {
                "store.add_store",
                new Dictionary<string, string>
                {
                    { "en", "Add Store" },
                    { "ur", "دکان شامل کریں" }
                }
            },
            {
                "store.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search stores..." },
                    { "ur", "دکانیں تلاش کریں..." }
                }
            },
            {
                "store.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            },
            {
                "store.show_all",
                new Dictionary<string, string>
                {
                    { "en", "Show All" },
                    { "ur", "سب دکھائیں" }
                }
            },
            {
                "store.loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading stores..." },
                    { "ur", "دکانیں لوڈ ہو رہی ہیں..." }
                }
            },
            {
                "store.no_data",
                new Dictionary<string, string>
                {
                    { "en", "No stores found" },
                    { "ur", "کوئی دکان نہیں ملی" }
                }
            },
            {
                "store.no_data_hint",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Store' to create your first store" },
                    { "ur", "اپنی پہلی دکان بنانے کے لیے 'دکان شامل کریں' پر کلک کریں" }
                }
            },
            {
                "store.items_count",
                new Dictionary<string, string>
                {
                    { "en", "stores" },
                    { "ur", "دکانیں" }
                }
            },
            {
                "store.column.name",
                new Dictionary<string, string>
                {
                    { "en", "Name" },
                    { "ur", "نام" }
                }
            },
            {
                "store.column.address",
                new Dictionary<string, string>
                {
                    { "en", "Address" },
                    { "ur", "پتہ" }
                }
            },
            {
                "store.column.phone",
                new Dictionary<string, string>
                {
                    { "en", "Phone" },
                    { "ur", "فون" }
                }
            },
            {
                "store.column.email",
                new Dictionary<string, string>
                {
                    { "en", "Email" },
                    { "ur", "ای میل" }
                }
            },
            {
                "store.column.manager",
                new Dictionary<string, string>
                {
                    { "en", "Manager" },
                    { "ur", "منیجر" }
                }
            },
            {
                "store.column.default",
                new Dictionary<string, string>
                {
                    { "en", "Default" },
                    { "ur", "ڈیفالٹ" }
                }
            },
            {
                "store.column.status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حیثیت" }
                }
            },
            {
                "store.column.discounts",
                new Dictionary<string, string>
                {
                    { "en", "Discounts" },
                    { "ur", "رعایتیں" }
                }
            },
            {
                "store.default",
                new Dictionary<string, string>
                {
                    { "en", "Default" },
                    { "ur", "ڈیفالٹ" }
                }
            },
            {
                "store.set_as_default",
                new Dictionary<string, string>
                {
                    { "en", "Set as Default" },
                    { "ur", "ڈیفالٹ کے طور پر سیٹ کریں" }
                }
            }
        };

        await SeedTranslationCategory("Store Management", storeTranslations, localizationService);
    }

    private async Task SeedTranslationCategory(string category, Dictionary<string, Dictionary<string, string>> translations, IDatabaseLocalizationService localizationService)
    {
        Console.WriteLine($"🔧 [LanguageSeedingService] Seeding {category} translations...");
        
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
        
        Console.WriteLine($"✅ [LanguageSeedingService] {category} translations seeded successfully");
    }

    private async Task SeedDiscountTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var discountTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "discount.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Discount Management" },
                    { "ur", "رعایت کا انتظام" }
                }
            },
            {
                "discount.add_new",
                new Dictionary<string, string>
                {
                    { "en", "Add New Discount" },
                    { "ur", "نئی رعایت شامل کریں" }
                }
            },
            {
                "discount.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search discounts..." },
                    { "ur", "رعایتیں تلاش کریں..." }
                }
            },
            {
                "discount.loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading discounts..." },
                    { "ur", "رعایتیں لوڈ ہو رہی ہیں..." }
                }
            },
            {
                "discount.no_data",
                new Dictionary<string, string>
                {
                    { "en", "No discounts found" },
                    { "ur", "کوئی رعایت نہیں ملی" }
                }
            },
            {
                "discount.no_data_hint",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add New Discount' to create your first discount" },
                    { "ur", "اپنی پہلی رعایت بنانے کے لیے 'نئی رعایت شامل کریں' پر کلک کریں" }
                }
            },
            {
                "discount.items_count",
                new Dictionary<string, string>
                {
                    { "en", "discounts" },
                    { "ur", "رعایتیں" }
                }
            },
            {
                "discount.active_only",
                new Dictionary<string, string>
                {
                    { "en", "Active Only" },
                    { "ur", "صرف فعال" }
                }
            },
            {
                "discount.clear_filters",
                new Dictionary<string, string>
                {
                    { "en", "Clear Filters" },
                    { "ur", "فلٹرز صاف کریں" }
                }
            },
            {
                "discount.column.name",
                new Dictionary<string, string>
                {
                    { "en", "Discount Name" },
                    { "ur", "رعایت کا نام" }
                }
            },
            {
                "discount.column.code",
                new Dictionary<string, string>
                {
                    { "en", "Discount Code" },
                    { "ur", "رعایت کا کوڈ" }
                }
            },
            {
                "discount.column.type",
                new Dictionary<string, string>
                {
                    { "en", "Type" },
                    { "ur", "قسم" }
                }
            },
            {
                "discount.column.value",
                new Dictionary<string, string>
                {
                    { "en", "Value" },
                    { "ur", "قیمت" }
                }
            },
            {
                "discount.column.start_date",
                new Dictionary<string, string>
                {
                    { "en", "Start Date" },
                    { "ur", "شروع کی تاریخ" }
                }
            },
            {
                "discount.column.end_date",
                new Dictionary<string, string>
                {
                    { "en", "End Date" },
                    { "ur", "آخری تاریخ" }
                }
            },
            {
                "discount.column.status",
                new Dictionary<string, string>
                {
                    { "en", "Status" },
                    { "ur", "حالت" }
                }
            },
            {
                "discount.edit",
                new Dictionary<string, string>
                {
                    { "en", "Edit Discount" },
                    { "ur", "رعایت میں ترمیم کریں" }
                }
            },
            {
                "discount.delete",
                new Dictionary<string, string>
                {
                    { "en", "Delete Discount" },
                    { "ur", "رعایت حذف کریں" }
                }
            },
            {
                "discount.delete_confirmation",
                new Dictionary<string, string>
                {
                    { "en", "Are you sure you want to delete this discount?" },
                    { "ur", "کیا آپ واقعی اس رعایت کو حذف کرنا چاہتے ہیں؟" }
                }
            },
            {
                "discount.success_added",
                new Dictionary<string, string>
                {
                    { "en", "Discount added successfully" },
                    { "ur", "رعایت کامیابی سے شامل ہو گئی" }
                }
            },
            {
                "discount.success_updated",
                new Dictionary<string, string>
                {
                    { "en", "Discount updated successfully" },
                    { "ur", "رعایت کامیابی سے اپ ڈیٹ ہو گئی" }
                }
            },
            {
                "discount.success_deleted",
                new Dictionary<string, string>
                {
                    { "en", "Discount deleted successfully" },
                    { "ur", "رعایت کامیابی سے حذف ہو گئی" }
                }
            }
        };

        await SeedTranslationCategory("Discount Management", discountTranslations, localizationService);
    }

    private async Task SeedCurrencyTranslationsAsync(IDatabaseLocalizationService localizationService)
    {
        var currencyTranslations = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "currency.page_title",
                new Dictionary<string, string>
                {
                    { "en", "Currency Management" },
                    { "ur", "کرنسی کا انتظام" }
                }
            },
            {
                "currency.add_currency",
                new Dictionary<string, string>
                {
                    { "en", "Add Currency" },
                    { "ur", "کرنسی شامل کریں" }
                }
            },
            {
                "currency.search_placeholder",
                new Dictionary<string, string>
                {
                    { "en", "Search currencies..." },
                    { "ur", "کرنسی تلاش کریں..." }
                }
            },
            {
                "currency.loading",
                new Dictionary<string, string>
                {
                    { "en", "Loading currencies..." },
                    { "ur", "کرنسی لوڈ ہو رہی ہیں..." }
                }
            },
            {
                "currency.no_data",
                new Dictionary<string, string>
                {
                    { "en", "No currencies found" },
                    { "ur", "کوئی کرنسی نہیں ملی" }
                }
            },
            {
                "currency.no_data_hint",
                new Dictionary<string, string>
                {
                    { "en", "Click 'Add Currency' to create your first currency" },
                    { "ur", "اپنی پہلی کرنسی بنانے کے لیے 'کرنسی شامل کریں' پر کلک کریں" }
                }
            },
            {
                "currency.items_count",
                new Dictionary<string, string>
                {
                    { "en", "currencies" },
                    { "ur", "کرنسیاں" }
                }
            },
            {
                "currency.column.name",
                new Dictionary<string, string>
                {
                    { "en", "Currency Name" },
                    { "ur", "کرنسی کا نام" }
                }
            },
            {
                "currency.column.code",
                new Dictionary<string, string>
                {
                    { "en", "Code" },
                    { "ur", "کوڈ" }
                }
            },
            {
                "currency.column.symbol",
                new Dictionary<string, string>
                {
                    { "en", "Symbol" },
                    { "ur", "علامت" }
                }
            },
            {
                "currency.column.exchange_rate",
                new Dictionary<string, string>
                {
                    { "en", "Exchange Rate" },
                    { "ur", "تبادلے کی شرح" }
                }
            },
            {
                "currency.column.is_default",
                new Dictionary<string, string>
                {
                    { "en", "Default" },
                    { "ur", "ڈیفالٹ" }
                }
            },
            {
                "currency.column.actions",
                new Dictionary<string, string>
                {
                    { "en", "Actions" },
                    { "ur", "اعمال" }
                }
            }
        };

        await SeedTranslationCategory("Currency Management", currencyTranslations, localizationService);
    }
}
