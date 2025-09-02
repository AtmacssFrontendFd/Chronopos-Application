# 🌐 ChronoPos Database-Driven Language System

This document explains the comprehensive language system implemented in ChronoPos that stores all translations in the database, making it easy to add new languages and manage translations.

## 📊 System Overview

The language system consists of:
- **Languages Table**: Stores supported languages (English, Urdu, etc.)
- **Language Keywords Table**: Stores translation keys and descriptions
- **Label Translations Table**: Stores actual translations for each language
- **Database-Driven Service**: Handles all translation operations
- **Easy Extension Tools**: Utilities to add new keywords and translations

## 🗄️ Database Schema

### Languages Table
```sql
CREATE TABLE `language` (
  `id` int PRIMARY KEY,
  `language_name` varchar(255),
  `language_code` varchar(255),
  `is_rtl` boolean,
  `status` varchar(255),
  `created_by` varchar(255),
  `created_at` timestamp,
  `updated_by` varchar(255),
  `updated_at` timestamp
);
```

### Language Keywords Table
```sql
CREATE TABLE `language_keyword` (
  `id` int PRIMARY KEY AUTO_INCREMENT,
  `key` varchar(100) UNIQUE,
  `description` text
);
```

### Label Translations Table
```sql
CREATE TABLE `label_translation` (
  `id` int PRIMARY KEY,
  `language_id` int,
  `translation_key` varchar(255),
  `value` varchar(255),
  `status` varchar(255),
  `created_by` varchar(255),
  `created_at` timestamp
);
```

## 🚀 How to Add New Keywords and Translations

### Method 1: Using the LanguageManager Utility

```csharp
// Get the service from DI container
var dbLocalizationService = serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
var languageManager = new LanguageManager(dbLocalizationService);

// Add a single keyword with translations
var translations = new Dictionary<string, string>
{
    ["en"] = "New Product",
    ["ur"] = "نئی مصنوعات"
};

await languageManager.AddKeywordWithTranslationsAsync(
    "products.new", 
    "New product button", 
    translations
);
```

### Method 2: Adding Multiple Keywords at Once

```csharp
var keywords = new Dictionary<string, (string description, Dictionary<string, string> translations)>
{
    ["dashboard.welcome"] = ("Welcome message", new Dictionary<string, string>
    {
        ["en"] = "Welcome to ChronoPos",
        ["ur"] = "ChronoPos میں خوش آمدید"
    }),
    ["sales.total"] = ("Total amount", new Dictionary<string, string>
    {
        ["en"] = "Total",
        ["ur"] = "کل"
    })
};

await languageManager.AddMultipleKeywordsAsync(keywords);
```

### Method 3: Using the Built-in Tool

```csharp
// Use the LanguageKeywordAdder tool
await LanguageKeywordAdder.AddNewKeywordsAsync(serviceProvider);

// Add common POS keywords
await languageManager.AddCommonPOSKeywordsAsync();

// Add restaurant-specific keywords
await languageManager.AddRestaurantKeywordsAsync();
```

## 🎯 Using Translations in Code

### In ViewModels/Services
```csharp
// Inject the service
private readonly IDatabaseLocalizationService _localizationService;

// Get a translation
var welcomeMessage = await _localizationService.GetTranslationAsync("dashboard.welcome");

// Get translation for specific language
var urduWelcome = await _localizationService.GetTranslationAsync("dashboard.welcome", "ur");
```

### In XAML (Future Enhancement)
```xml
<!-- Using the markup extension -->
<TextBlock Text="{loc:Translate Key='nav.dashboard'}" />

<!-- With fallback -->
<Button Content="{loc:Translate Key='btn.save', FallbackValue='Save'}" />
```

## 📋 Pre-loaded Translation Keys

The system comes with extensive pre-loaded translations:

### Navigation
- `nav.dashboard` - Dashboard
- `nav.products` - Products / مصنوعات
- `nav.customers` - Customers / گاہک
- `nav.sales` - Sales / فروخت
- `nav.settings` - Settings / ترتیبات
- `nav.logout` - Logout / لاگ آؤٹ

### Common Buttons
- `btn.save` - Save / محفوظ کریں
- `btn.cancel` - Cancel / منسوخ
- `btn.edit` - Edit / ترمیم
- `btn.delete` - Delete / حذف
- `btn.add` - Add / شامل کریں
- `btn.search` - Search / تلاش

### Settings Page
- `settings.language` - Language Settings / زبان کی ترتیبات
- `settings.theme` - Theme Settings / تھیم کی ترتیبات
- `settings.color_scheme` - Color Scheme / رنگ سکیم
- `settings.layout_direction` - Layout Direction / لے آؤٹ کی سمت
- `settings.font` - Font Settings / فونٹ کی ترتیبات

### Products
- `products.title` - Products Management / مصنوعات کا انتظام
- `products.name` - Product Name / مصنوع کا نام
- `products.price` - Price / قیمت
- `products.category` - Category / قسم
- `products.stock` - Stock / اسٹاک

### Common Labels
- `label.current` - Current / موجودہ
- `label.ready` - Ready / تیار
- `theme.light` - Light Theme / ہلکا تھیم
- `theme.dark` - Dark Theme / گہرا تھیم

## 🔧 Language Management Features

### Change Language
```csharp
// Set current language
await _localizationService.SetCurrentLanguageAsync("ur"); // Switch to Urdu
await _localizationService.SetCurrentLanguageAsync("en"); // Switch to English

// Get current language
var currentLang = await _localizationService.GetCurrentLanguageAsync();
var isRtl = _localizationService.IsRightToLeft();
```

### Add New Language
```csharp
// First add the language to the database
var newLanguage = new Language
{
    LanguageName = "Arabic",
    LanguageCode = "ar",
    IsRtl = true,
    Status = "Active",
    CreatedBy = "Admin"
};
// Save to database...

// Then add translations for all existing keys
var allEnglishTranslations = await _localizationService.GetAllTranslationsAsync("en");
foreach (var translation in allEnglishTranslations)
{
    await _localizationService.SaveTranslationAsync(
        translation.Key, 
        "TRANSLATE_THIS", // Placeholder - replace with actual translation
        "ar"
    );
}
```

### Get Translation Statistics
```csharp
var languageManager = new LanguageManager(_localizationService);

// Get count of translations per language
var stats = await languageManager.GetTranslationStatsAsync();

// Find missing translations for a language
var missingUrdu = await languageManager.FindMissingTranslationsAsync("ur");
```

## 📊 Monitoring and Maintenance

### Check Translation Coverage
```csharp
await LanguageKeywordAdder.ShowTranslationStatsAsync(serviceProvider);
```

This will show:
- Number of translations per language
- Missing translations for each language
- Overall translation coverage

### Adding Business-Specific Keywords

For different business types, you can add specific keyword sets:

```csharp
// For restaurants
await languageManager.AddRestaurantKeywordsAsync();

// For retail
await languageManager.AddRetailKeywordsAsync();

// For services
await languageManager.AddServiceKeywordsAsync();
```

## 🎨 Integration with UI

The language system integrates seamlessly with:
- **Theme System**: Text colors adapt to light/dark themes
- **Layout Direction**: RTL support for Arabic/Urdu languages
- **Font System**: Font sizes work with all languages
- **Color Schemes**: All text respects the selected color scheme

## 💡 Best Practices

1. **Use Descriptive Keys**: Use hierarchical keys like `products.add_new` instead of `add_product`
2. **Add Descriptions**: Always provide descriptions for translation keys
3. **Consistent Naming**: Follow the pattern `section.action` or `section.label`
4. **Fallback Values**: Always provide English as fallback
5. **Regular Updates**: Keep translations synchronized across all languages

## 🔄 Future Enhancements

- **Translation Memory**: Cache frequently used translations
- **Real-time Updates**: Live language switching without restart
- **Export/Import**: Tools to export translations to Excel/CSV
- **Translation Interface**: Admin panel for managing translations
- **Pluralization**: Support for plural forms in different languages
- **Context-aware**: Different translations based on context

## 📝 Adding Your Own Keywords

To add new keywords for your features:

1. **Identify the feature area** (e.g., inventory, reports, etc.)
2. **Choose descriptive keys** (e.g., `inventory.low_stock`, `reports.monthly`)
3. **Use the LanguageManager utility**:

```csharp
var translations = new Dictionary<string, string>
{
    ["en"] = "Your English Text",
    ["ur"] = "آپ کا اردو متن"
};

await languageManager.AddKeywordWithTranslationsAsync(
    "your.new.key", 
    "Description of what this text is for", 
    translations
);
```

This system makes ChronoPos truly multilingual and easily extensible for any number of languages!
