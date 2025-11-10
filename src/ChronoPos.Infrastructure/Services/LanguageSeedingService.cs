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
    // Dashboard translations extracted to a dedicated seeder for maintainability
    await ChronoPos.Infrastructure.Services.DashboardTranslationSeeder.SeedDashboardTranslationsAsync(localizationService);
        // Transaction translations
        await ChronoPos.Infrastructure.Services.TransactionTranslationSeeder.SeedTransactionTranslationsAsync(localizationService);
        // Add Sales translations
        await ChronoPos.Infrastructure.Services.AddSalesTranslationSeeder.SeedAddSalesTranslationsAsync(localizationService);
        // Refund Dialog translations
        await ChronoPos.Infrastructure.Services.RefundDialogTranslationSeeder.SeedRefundDialogTranslationsAsync(localizationService);
        // Product Selection Dialog translations
        await ChronoPos.Infrastructure.Services.ProductSelectionDialogTranslationSeeder.SeedProductSelectionDialogTranslationsAsync(localizationService);
        // Reports Hub translations
        await ChronoPos.Infrastructure.Services.ReportsHubTranslationSeeder.SeedReportsHubTranslationsAsync(localizationService);
        await SeedAddProductTranslationsAsync(localizationService);
        await SeedAddGrnTranslationsAsync(localizationService);
        await SeedProductManagementTranslationsAsync(localizationService);
        await SeedStockManagementTranslationsAsync(localizationService);
        await SeedManagementTranslationsAsync(localizationService);
        await SeedAddOptionsTranslationsAsync(localizationService);
        await SeedSettingsTranslationsAsync(localizationService);
        await SeedSalesTranslationsAsync(localizationService);
        await SeedReportsTranslationsAsync(localizationService);
        
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
            }
        };

        await SeedTranslationCategory("Common", commonTranslations, localizationService);
    }

    // Dashboard translations have been moved to DashboardTranslationSeeder.cs

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
            }
        };

        await SeedTranslationCategory("Others", addOptionsTranslations, localizationService);
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
}
