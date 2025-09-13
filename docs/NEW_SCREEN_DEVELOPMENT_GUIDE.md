# ChronoPos New Screen Development Guide

## Overview
This comprehensive guide provides step-by-step instructions for developers to create new screens/pages in the ChronoPos Desktop application that are fully aligned with the advanced settings system. The application follows the MVVM (Model-View-ViewModel) pattern with dependency injection, database-driven multi-language support, dynamic theming, customizable color schemes, layout direction control (RTL/LTR), and modern font management.

## Architecture Overview

### Project Structure
```
src/ChronoPos.Desktop/
├── Views/              # XAML UserControls for UI
├── ViewModels/         # ViewModels with business logic
├── Services/           # Application services (Theme, Language, Color, Layout)
├── Themes/             # Theme resource dictionaries (Light/Dark)
├── Fonts/              # Poppins font files
├── Properties/         # Application settings persistence
└── Converters/         # Value converters for data binding

src/ChronoPos.Infrastructure/
├── Services/           # DatabaseLocalizationService, LanguageManager
├── Repositories/       # Data access layer
└── Data/              # Database entities and context

Database Entities:
├── Language           # Language definitions (English, Urdu)
├── LanguageKeyword    # Translatable keywords/labels
└── LabelTranslation   # Actual translations per language
```

### Technologies Used
- **WPF** - Windows Presentation Foundation for UI
- **MVVM** - Model-View-ViewModel pattern using CommunityToolkit.Mvvm
- **Dependency Injection** - Microsoft.Extensions.DependencyInjection
- **Entity Framework Core** - Data access with SQLite database
- **Database-Driven Localization** - Multi-language support with extensible keywords
- **Dynamic Theming** - Resource dictionary switching (Light/Dark themes)
- **Color Scheme Customization** - User-customizable primary/background colors
- **Layout Direction Control** - RTL/LTR support for different languages
- **Font Management** - Poppins font family with scalable font sizes
- **Settings Persistence** - Comprehensive user preferences storage

## Step-by-Step Guide to Create a Settings-Aligned New Screen

### Step 1: Create the ViewModel with Settings Integration

Create a new ViewModel class in `ViewModels/` folder that integrates with all settings services:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for [ScreenName] management with full settings integration
/// </summary>
public partial class [ScreenName]ViewModel : ObservableObject
{
    private readonly I[ServiceName] _service;
    private readonly DatabaseLocalizationService _localizationService;
    private readonly ThemeService _themeService;
    private readonly ColorSchemeService _colorSchemeService;
    private readonly LayoutDirectionService _layoutDirectionService;
    private readonly ZoomService _zoomService;

    [ObservableProperty]
    private string _title = "[Screen Title]";

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private ZoomLevel _currentZoomLevel = ZoomLevel.Zoom100;

    public [ScreenName]ViewModel(
        I[ServiceName] service,
        DatabaseLocalizationService localizationService,
        ThemeService themeService,
        ColorSchemeService colorSchemeService,
        LayoutDirectionService layoutDirectionService,
        ZoomService zoomService)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));

        // Subscribe to settings changes
        _layoutDirectionService.LayoutDirectionChanged += OnLayoutDirectionChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _zoomService.ZoomChanged += OnZoomChanged;

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        // NO MORE per-screen translation seeding!
        // Translations are already seeded at application startup
        await LoadTranslationsAsync();
        await LoadDataAsync();
        UpdateLayoutDirection();
        UpdateZoomLevel();
    }

    private async Task LoadTranslationsAsync()
    {
        try
        {
            // Simply load translations - they're already seeded at startup!
            Title = await _localizationService.GetTranslationAsync("[screen_name]_title") ?? "[Screen Title]";
            StatusMessage = await _localizationService.GetTranslationAsync("[screen_name]_status_ready") ?? "Ready";
            SearchButtonText = await _localizationService.GetTranslationAsync("[screen_name]_search_button") ?? "🔍 Search";
            AddNewButtonText = await _localizationService.GetTranslationAsync("[screen_name]_add_new_button") ?? "➕ Add New";
            DataOverviewText = await _localizationService.GetTranslationAsync("[screen_name]_data_overview") ?? "Data Overview";
            NameColumnHeader = await _localizationService.GetTranslationAsync("[screen_name]_column_name") ?? "Name";
            StatusColumnHeader = await _localizationService.GetTranslationAsync("[screen_name]_column_status") ?? "Status";
            DateColumnHeader = await _localizationService.GetTranslationAsync("[screen_name]_column_date") ?? "Date";
            StatusLabelText = await _localizationService.GetTranslationAsync("[screen_name]_status_label") ?? "Status:";
            SearchPlaceholder = await _localizationService.GetTranslationAsync("[screen_name]_search_placeholder") ?? "Search...";
            RefreshTooltip = await _localizationService.GetTranslationAsync("[screen_name]_refresh_tooltip") ?? "Refresh";
        }
        catch (Exception ex)
        {
            // Fallback to default values if translation fails
            Title = "[Screen Title]";
            StatusMessage = "Ready";
            // Other fallbacks are already set as default property values
            System.Diagnostics.Debug.WriteLine($"Translation loading failed: {ex.Message}");
        }
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Reload translations when language changes
        _ = Task.Run(LoadTranslationsAsync);
    }

    private void OnLayoutDirectionChanged(object? sender, FlowDirection direction)
    {
        CurrentFlowDirection = direction;
    }

    private void OnZoomChanged(object? sender, ZoomLevel zoomLevel)
    {
        CurrentZoomLevel = zoomLevel;
        // UI elements will automatically scale through dynamic resources
        // Additional zoom-specific logic can be added here if needed
    }

    private void UpdateLayoutDirection()
    {
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection;
    }

    private void UpdateZoomLevel()
    {
        CurrentZoomLevel = _zoomService.CurrentZoomLevel;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = await _localizationService.GetTranslationAsync("status_loading") ?? "Loading...";
            
            // Load your data here
            // Example: Items = await _service.GetAllAsync();
            
            StatusMessage = await _localizationService.GetTranslationAsync("status_loaded") ?? "Data loaded successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = await _localizationService.GetTranslationAsync("status_error") ?? $"Error: {ex.Message}";
            throw new InvalidOperationException($"Failed to load data: {ex.Message}", ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            StatusMessage = await _localizationService.GetTranslationAsync("status_saving") ?? "Saving...";
            // Implement save logic
            StatusMessage = await _localizationService.GetTranslationAsync("status_saved") ?? "Saved successfully";
        }
        catch (Exception ex)
        {
            StatusMessage = await _localizationService.GetTranslationAsync("status_save_failed") ?? $"Save failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            StatusMessage = await _localizationService.GetTranslationAsync("status_search_empty") ?? "Enter search terms";
            return;
        }

        StatusMessage = await _localizationService.GetTranslationAsync("status_searching") ?? $"Searching for: {SearchText}";
        // Implement search logic
    }

    // Helper method to get translations with fallback
    private async Task<string> GetTranslationAsync(string key, string fallback)
    {
        try
        {
            return await _localizationService.GetTranslationAsync(key) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    // Cleanup
    public void Dispose()
    {
        _layoutDirectionService.LayoutDirectionChanged -= OnLayoutDirectionChanged;
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _zoomService.ZoomChanged -= OnZoomChanged;
    }
}
```
```

### Step 2: Create the View (XAML) with Full Settings Support

Create a new UserControl in `Views/` folder that supports all settings features:

**[ScreenName]View.xaml:**
```xml
<UserControl x:Class="ChronoPos.Desktop.Views.[ScreenName]View"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" 
             xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
             mc:Ignorable="d" 
             d:DesignHeight="600" d:DesignWidth="800"
             FlowDirection="{Binding CurrentFlowDirection}"
             Background="{DynamicResource CardBackground}">
    
    <ScrollViewer VerticalScrollBarVisibility="Auto" 
                  HorizontalScrollBarVisibility="Disabled">
        <StackPanel Margin="20">
            
            <!-- Page Header with Multi-language Support -->
            <Border Background="{DynamicResource SurfaceBackground}" 
                    CornerRadius="8" 
                    Padding="20" 
                    Margin="0,0,0,20"
                    BorderBrush="{DynamicResource BorderLight}"
                    BorderThickness="1">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    
                    <TextBlock Grid.Column="0"
                               Text="{Binding Title}" 
                               FontSize="{DynamicResource FontSizeLarge}" 
                               FontWeight="Bold" 
                               FontFamily="{DynamicResource PoppinsFontFallback}"
                               Foreground="{DynamicResource TextPrimary}"
                               VerticalAlignment="Center"
                               TextAlignment="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToTextAlignmentConverter}}"/>
                    
                    <!-- Refresh Button with Translated Text -->
                    <Button Grid.Column="1"
                            Content="🔄"
                            ToolTip="{Binding RefreshTooltip}"
                            Style="{DynamicResource ModernButtonStyle}"
                            FontFamily="{DynamicResource PoppinsFontFallback}"
                            FontSize="{DynamicResource FontSizeMedium}"
                            Width="40" Height="40"
                            Command="{Binding LoadDataCommand}"/>
                </Grid>
            </Border>

            <!-- Action Section with RTL/LTR Support -->
            <Border Background="{DynamicResource SurfaceBackground}" 
                    CornerRadius="8" 
                    Padding="20" 
                    Margin="0,0,0,20"
                    BorderBrush="{DynamicResource BorderLight}"
                    BorderThickness="1">
                <Grid>
                    <Grid.ColumnDefinitions>
                        <ColumnDefinition Width="*"/>
                        <ColumnDefinition Width="15"/>
                        <ColumnDefinition Width="Auto"/>
                        <ColumnDefinition Width="15"/>
                        <ColumnDefinition Width="Auto"/>
                    </Grid.ColumnDefinitions>
                    
                    <!-- Search Box with Placeholder Translation -->
                    <TextBox Grid.Column="0"
                             Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}"
                             Style="{DynamicResource ModernTextBoxStyle}"
                             FontFamily="{DynamicResource PoppinsFontFallback}"
                             FontSize="{DynamicResource FontSizeSmall}"
                             VerticalAlignment="Center"
                             Tag="{Binding SearchPlaceholder}"/>
                    
                    <!-- Search Button with Translated Content -->
                    <Button Grid.Column="2"
                            Content="{Binding SearchButtonText}"
                            Style="{DynamicResource ModernButtonStyle}"
                            FontFamily="{DynamicResource PoppinsFontFallback}"
                            FontSize="{DynamicResource FontSizeSmall}"
                            Command="{Binding SearchCommand}"/>
                    
                    <!-- Add New Button with Translated Content -->
                    <Button Grid.Column="4"
                            Content="{Binding AddNewButtonText}"
                            Style="{DynamicResource PrimaryButtonStyle}"
                            FontFamily="{DynamicResource PoppinsFontFallback}"
                            FontSize="{DynamicResource FontSizeSmall}"
                            Command="{Binding AddNewCommand}"/>
                </Grid>
            </Border>

            <!-- Main Content Section with Theme and Language Support -->
            <Border Background="{DynamicResource SurfaceBackground}" 
                    CornerRadius="8" 
                    Padding="20"
                    Margin="0,0,0,20"
                    BorderBrush="{DynamicResource BorderLight}"
                    BorderThickness="1">
                
                <Grid>
                    <Grid.RowDefinitions>
                        <RowDefinition Height="Auto"/>
                        <RowDefinition Height="*"/>
                    </Grid.RowDefinitions>
                    
                    <!-- Loading Indicator with Custom Colors -->
                    <ProgressBar Grid.Row="0"
                                 Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}"
                                 IsIndeterminate="True"
                                 Height="4"
                                 Margin="0,0,0,20"
                                 Background="{DynamicResource CardBackground}"
                                 Foreground="{DynamicResource Primary}"/>
                    
                    <!-- Content Area -->
                    <StackPanel Grid.Row="1"
                                Visibility="{Binding IsLoading, Converter={StaticResource InverseBooleanToVisibilityConverter}}">
                        
                        <!-- Section Header with Translation -->
                        <TextBlock Text="{Binding DataOverviewText}" 
                                   FontSize="{DynamicResource FontSizeMedium}" 
                                   FontWeight="SemiBold" 
                                   FontFamily="{DynamicResource PoppinsFontFallback}"
                                   Foreground="{DynamicResource TextPrimary}"
                                   TextAlignment="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToTextAlignmentConverter}}"
                                   Margin="0,0,0,15"/>
                        
                        <!-- Data Grid with Full Theme and RTL Support -->
                        <DataGrid Background="{DynamicResource CardBackground}"
                                  Foreground="{DynamicResource TextPrimary}"
                                  BorderBrush="{DynamicResource BorderLight}"
                                  GridLinesVisibility="Horizontal"
                                  HorizontalGridLinesBrush="{DynamicResource BorderLight}"
                                  VerticalGridLinesBrush="Transparent"
                                  HeadersVisibility="Column"
                                  AutoGenerateColumns="False"
                                  CanUserResizeRows="False"
                                  CanUserAddRows="False"
                                  SelectionMode="Single"
                                  FontFamily="{DynamicResource PoppinsFontFallback}"
                                  FontSize="{DynamicResource FontSizeSmall}"
                                  FlowDirection="{Binding CurrentFlowDirection}"
                                  Height="300">
                            
                            <!-- Column Headers with Theme Support -->
                            <DataGrid.ColumnHeaderStyle>
                                <Style TargetType="DataGridColumnHeader">
                                    <Setter Property="Background" Value="{DynamicResource SurfaceBackground}"/>
                                    <Setter Property="Foreground" Value="{DynamicResource TextPrimary}"/>
                                    <Setter Property="FontWeight" Value="SemiBold"/>
                                    <Setter Property="FontFamily" Value="{DynamicResource PoppinsFontFallback}"/>
                                    <Setter Property="FontSize" Value="{DynamicResource FontSizeSmall}"/>
                                    <Setter Property="Padding" Value="10,8"/>
                                    <Setter Property="BorderBrush" Value="{DynamicResource BorderLight}"/>
                                    <Setter Property="BorderThickness" Value="0,0,1,1"/>
                                    <Setter Property="HorizontalContentAlignment" Value="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToHorizontalAlignmentConverter}}"/>
                                </Style>
                            </DataGrid.ColumnHeaderStyle>
                            
                            <!-- Row Style with Hover and Selection States -->
                            <DataGrid.RowStyle>
                                <Style TargetType="DataGridRow">
                                    <Setter Property="Background" Value="{DynamicResource CardBackground}"/>
                                    <Setter Property="Foreground" Value="{DynamicResource TextPrimary}"/>
                                    <Style.Triggers>
                                        <Trigger Property="IsMouseOver" Value="True">
                                            <Setter Property="Background" Value="{DynamicResource OverlayBackground}"/>
                                        </Trigger>
                                        <Trigger Property="IsSelected" Value="True">
                                            <Setter Property="Background" Value="{DynamicResource Primary}"/>
                                            <Setter Property="Foreground" Value="White"/>
                                        </Trigger>
                                    </Style.Triggers>
                                </Style>
                            </DataGrid.RowStyle>
                            
                            <!-- Cell Style with RTL Support -->
                            <DataGrid.CellStyle>
                                <Style TargetType="DataGridCell">
                                    <Setter Property="BorderThickness" Value="0"/>
                                    <Setter Property="Padding" Value="10,8"/>
                                    <Setter Property="FontFamily" Value="{DynamicResource PoppinsFontFallback}"/>
                                    <Setter Property="FontSize" Value="{DynamicResource FontSizeSmall}"/>
                                    <Setter Property="HorizontalContentAlignment" Value="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToHorizontalAlignmentConverter}}"/>
                                </Style>
                            </DataGrid.CellStyle>
                            
                            <!-- Columns with Translated Headers -->
                            <DataGrid.Columns>
                                <DataGridTextColumn Header="{Binding NameColumnHeader}" 
                                                    Binding="{Binding Name}" 
                                                    Width="*"/>
                                <DataGridTextColumn Header="{Binding StatusColumnHeader}" 
                                                    Binding="{Binding Status}" 
                                                    Width="Auto"/>
                                <DataGridTextColumn Header="{Binding DateColumnHeader}" 
                                                    Binding="{Binding Date, StringFormat='{}{0:MMM dd, yyyy}'}" 
                                                    Width="Auto"/>
                            </DataGrid.Columns>
                        </DataGrid>
                        
                    </StackPanel>
                </Grid>
            </Border>

            <!-- Status Section with Dynamic Styling -->
            <Border Background="{DynamicResource SurfaceBackground}" 
                    CornerRadius="8" 
                    Padding="15"
                    BorderBrush="{DynamicResource BorderLight}"
                    BorderThickness="1">
                <StackPanel Orientation="Horizontal">
                    <!-- Status Icon -->
                    <Ellipse Width="8" Height="8"
                             Fill="{DynamicResource Success}"
                             VerticalAlignment="Center"
                             Margin="0,0,10,0"/>
                    
                    <!-- Status Label -->
                    <TextBlock Text="{Binding StatusLabelText}" 
                               FontSize="{DynamicResource FontSizeSmall}" 
                               FontFamily="{DynamicResource PoppinsFontFallback}"
                               Foreground="{DynamicResource TextSecondary}"
                               VerticalAlignment="Center"/>
                    
                    <!-- Status Message -->
                    <TextBlock Text="{Binding StatusMessage}" 
                               FontSize="{DynamicResource FontSizeSmall}" 
                               FontWeight="SemiBold"
                               FontFamily="{DynamicResource PoppinsFontFallback}"
                               Foreground="{DynamicResource Primary}"
                               VerticalAlignment="Center"
                               Margin="5,0,0,0"/>
                </StackPanel>
            </Border>

        </StackPanel>
    </ScrollViewer>
</UserControl>
```

**[ScreenName]View.xaml.cs:**
```csharp
using System.Windows.Controls;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for [ScreenName]View.xaml
/// </summary>
public partial class [ScreenName]View : UserControl
{
    public [ScreenName]View()
    {
        InitializeComponent();
    }
}
```

### Step 3: Register Dependencies with All Services

Update `App.xaml.cs` to register your new ViewModel with all required services:

```csharp
// In the ConfigureServices method, add your ViewModel:
services.AddTransient<[ScreenName]ViewModel>();

// Ensure all settings services are registered:
services.AddSingleton<ThemeService>();
services.AddSingleton<ColorSchemeService>();
services.AddSingleton<LayoutDirectionService>();
services.AddSingleton<ZoomService>();
services.AddSingleton<DatabaseLocalizationService>();

// If you need a new business service, also register it:
services.AddScoped<I[ServiceName], [ServiceName]>();
```

### Step 4: Add Your Screen's Translations to Central Seeding Service

**IMPORTANT**: With the new centralized language system, you no longer need to add keywords during screen initialization. Instead, add your translations to the `LanguageSeedingService`.

1. **Open `LanguageSeedingService.cs`** and add your screen's translation method:

```csharp
private async Task Seed[ScreenName]TranslationsAsync()
{
    var keywordTranslations = new Dictionary<string, Dictionary<string, string>>
    {
        {
            "[screen_name]_title",
            new Dictionary<string, string>
            {
                { "en", "[Screen Title]" },
                { "ur", "[اردو میں اسکرین کا عنوان]" }
            }
        },
        {
            "[screen_name]_search_button",
            new Dictionary<string, string>
            {
                { "en", "🔍 Search" },
                { "ur", "🔍 تلاش کریں" }
            }
        },
        {
            "[screen_name]_add_new_button",
            new Dictionary<string, string>
            {
                { "en", "➕ Add New" },
                { "ur", "➕ نیا شامل کریں" }
            }
        },
        {
            "[screen_name]_data_overview",
            new Dictionary<string, string>
            {
                { "en", "Data Overview" },
                { "ur", "ڈیٹا کا جائزہ" }
            }
        },
        {
            "[screen_name]_column_name",
            new Dictionary<string, string>
            {
                { "en", "Name" },
                { "ur", "نام" }
            }
        },
        {
            "[screen_name]_column_status",
            new Dictionary<string, string>
            {
                { "en", "Status" },
                { "ur", "حالت" }
            }
        },
        {
            "[screen_name]_column_date",
            new Dictionary<string, string>
            {
                { "en", "Date" },
                { "ur", "تاریخ" }
            }
        },
        {
            "[screen_name]_status_label",
            new Dictionary<string, string>
            {
                { "en", "Status:" },
                { "ur", "حالت:" }
            }
        },
        {
            "[screen_name]_search_placeholder",
            new Dictionary<string, string>
            {
                { "en", "Search..." },
                { "ur", "تلاش کریں..." }
            }
        },
        {
            "[screen_name]_refresh_tooltip",
            new Dictionary<string, string>
            {
                { "en", "Refresh" },
                { "ur", "ریفریش" }
            }
        },
        // Status messages
        {
            "[screen_name]_status_loading",
            new Dictionary<string, string>
            {
                { "en", "Loading..." },
                { "ur", "لوڈ کر رہا ہے..." }
            }
        },
        {
            "[screen_name]_status_loaded",
            new Dictionary<string, string>
            {
                { "en", "Data loaded successfully" },
                { "ur", "ڈیٹا کامیابی سے لوڈ ہوا" }
            }
        },
        {
            "[screen_name]_status_saving",
            new Dictionary<string, string>
            {
                { "en", "Saving..." },
                { "ur", "محفوظ کر رہا ہے..." }
            }
        },
        {
            "[screen_name]_status_saved",
            new Dictionary<string, string>
            {
                { "en", "Saved successfully" },
                { "ur", "کامیابی سے محفوظ ہوا" }
            }
        },
        {
            "[screen_name]_status_error",
            new Dictionary<string, string>
            {
                { "en", "An error occurred" },
                { "ur", "ایک خرابی ہوئی" }
            }
        }
    };

    // Seed translations using the centralized category approach
    await SeedTranslationCategoryAsync("[ScreenName]", keywordTranslations);
}
```

2. **Add your method to the main seeding process** in `SeedAllTranslationsAsync()`:

```csharp
private async Task SeedAllTranslationsAsync()
{
    try
    {
        FileLogger.LogSeparator("LANGUAGE SEEDING STARTED");
        FileLogger.Log("🌐 Starting comprehensive language seeding for all screens");

        // Seed navigation translations
        await SeedNavigationTranslationsAsync();

        // Seed common UI translations
        await SeedCommonTranslationsAsync();

        // Seed screen-specific translations
        await SeedAddProductTranslationsAsync();
        await SeedStockManagementTranslationsAsync();
        await SeedProductManagementTranslationsAsync();
        await SeedSettingsTranslationsAsync();
        
        // ADD YOUR SCREEN HERE:
        await Seed[ScreenName]TranslationsAsync();

        FileLogger.Log("✅ All language translations seeded successfully");
        FileLogger.LogSeparator("LANGUAGE SEEDING COMPLETED");
    }
    catch (Exception ex)
    {
        FileLogger.Log($"❌ ERROR during language seeding: {ex.Message}");
        FileLogger.Log($"❌ Stack trace: {ex.StackTrace}");
        throw;
    }
}
```

3. **Translation Key Naming Best Practices**:

```csharp
// Use consistent prefixes for organization:
"[screen_name]_title"              // Main page title
"[screen_name]_subtitle"           // Page subtitle
"[screen_name]_button_action"      // Action buttons (add, edit, delete, etc.)
"[screen_name]_label_field"        // Form field labels
"[screen_name]_column_name"        // Data grid column headers
"[screen_name]_status_state"       // Status messages (loading, saved, error)
"[screen_name]_placeholder_field"  // Input placeholders
"[screen_name]_tooltip_element"    // Tooltips and help text
"[screen_name]_message_type"       // User messages and confirmations
```

4. **Comprehensive Translation Categories for Your Screen**:

```csharp
// Example comprehensive translation set for a new screen:
private async Task SeedInventoryManagementTranslationsAsync()
{
    var translations = new Dictionary<string, Dictionary<string, string>>
    {
        // Page Structure
        { "inventory_title", new() { { "en", "Inventory Management" }, { "ur", "انوینٹری کا انتظام" } } },
        { "inventory_subtitle", new() { { "en", "Manage your product inventory" }, { "ur", "اپنی پروڈکٹ انوینٹری کا انتظام کریں" } } },
        
        // Action Buttons
        { "inventory_add_product", new() { { "en", "➕ Add Product" }, { "ur", "➕ پروڈکٹ شامل کریں" } } },
        { "inventory_edit_product", new() { { "en", "✏️ Edit" }, { "ur", "✏️ ترمیم" } } },
        { "inventory_delete_product", new() { { "en", "🗑️ Delete" }, { "ur", "🗑️ ڈیلیٹ" } } },
        { "inventory_view_details", new() { { "en", "👁️ View Details" }, { "ur", "👁️ تفصیلات دیکھیں" } } },
        { "inventory_search_products", new() { { "en", "🔍 Search Products" }, { "ur", "🔍 پروڈکٹس تلاش کریں" } } },
        { "inventory_filter_category", new() { { "en", "🏷️ Filter by Category" }, { "ur", "🏷️ کیٹگری کے ذریعے فلٹر کریں" } } },
        { "inventory_export_data", new() { { "en", "📊 Export" }, { "ur", "📊 ایکسپورٹ" } } },
        { "inventory_refresh_data", new() { { "en", "🔄 Refresh" }, { "ur", "🔄 ریفریش" } } },
        
        // Form Labels
        { "inventory_label_name", new() { { "en", "Product Name:" }, { "ur", "پروڈکٹ کا نام:" } } },
        { "inventory_label_sku", new() { { "en", "SKU:" }, { "ur", "SKU:" } } },
        { "inventory_label_category", new() { { "en", "Category:" }, { "ur", "کیٹگری:" } } },
        { "inventory_label_quantity", new() { { "en", "Quantity:" }, { "ur", "مقدار:" } } },
        { "inventory_label_price", new() { { "en", "Price:" }, { "ur", "قیمت:" } } },
        { "inventory_label_cost", new() { { "en", "Cost:" }, { "ur", "لاگت:" } } },
        { "inventory_label_supplier", new() { { "en", "Supplier:" }, { "ur", "سپلائر:" } } },
        
        // Column Headers
        { "inventory_column_name", new() { { "en", "Product Name" }, { "ur", "پروڈکٹ کا نام" } } },
        { "inventory_column_sku", new() { { "en", "SKU" }, { "ur", "SKU" } } },
        { "inventory_column_category", new() { { "en", "Category" }, { "ur", "کیٹگری" } } },
        { "inventory_column_quantity", new() { { "en", "Qty" }, { "ur", "مقدار" } } },
        { "inventory_column_price", new() { { "en", "Price" }, { "ur", "قیمت" } } },
        { "inventory_column_value", new() { { "en", "Total Value" }, { "ur", "کل قیمت" } } },
        { "inventory_column_status", new() { { "en", "Status" }, { "ur", "حالت" } } },
        { "inventory_column_actions", new() { { "en", "Actions" }, { "ur", "عمل" } } },
        
        // Status Messages
        { "inventory_status_loading", new() { { "en", "Loading inventory..." }, { "ur", "انوینٹری لوڈ کر رہا ہے..." } } },
        { "inventory_status_loaded", new() { { "en", "Inventory loaded successfully" }, { "ur", "انوینٹری کامیابی سے لوڈ ہوئی" } } },
        { "inventory_status_saving", new() { { "en", "Saving product..." }, { "ur", "پروڈکٹ محفوظ کر رہا ہے..." } } },
        { "inventory_status_saved", new() { { "en", "Product saved successfully" }, { "ur", "پروڈکٹ کامیابی سے محفوظ ہوا" } } },
        { "inventory_status_deleting", new() { { "en", "Deleting product..." }, { "ur", "پروڈکٹ ڈیلیٹ کر رہا ہے..." } } },
        { "inventory_status_deleted", new() { { "en", "Product deleted successfully" }, { "ur", "پروڈکٹ کامیابی سے ڈیلیٹ ہوا" } } },
        { "inventory_status_error", new() { { "en", "An error occurred" }, { "ur", "ایک خرابی ہوئی" } } },
        { "inventory_status_search_empty", new() { { "en", "Enter search terms" }, { "ur", "تلاش کے الفاظ درج کریں" } } },
        { "inventory_status_searching", new() { { "en", "Searching..." }, { "ur", "تلاش کر رہا ہے..." } } },
        { "inventory_status_no_results", new() { { "en", "No products found" }, { "ur", "کوئی پروڈکٹ نہیں ملا" } } },
        
        // Placeholders
        { "inventory_placeholder_search", new() { { "en", "Search products..." }, { "ur", "پروڈکٹس تلاش کریں..." } } },
        { "inventory_placeholder_name", new() { { "en", "Enter product name" }, { "ur", "پروڈکٹ کا نام درج کریں" } } },
        { "inventory_placeholder_sku", new() { { "en", "Enter SKU" }, { "ur", "SKU درج کریں" } } },
        { "inventory_placeholder_quantity", new() { { "en", "Enter quantity" }, { "ur", "مقدار درج کریں" } } },
        
        // Tooltips
        { "inventory_tooltip_add", new() { { "en", "Add new product to inventory" }, { "ur", "انوینٹری میں نیا پروڈکٹ شامل کریں" } } },
        { "inventory_tooltip_edit", new() { { "en", "Edit product details" }, { "ur", "پروڈکٹ کی تفصیلات میں ترمیم کریں" } } },
        { "inventory_tooltip_delete", new() { { "en", "Delete product from inventory" }, { "ur", "انوینٹری سے پروڈکٹ ڈیلیٹ کریں" } } },
        { "inventory_tooltip_refresh", new() { { "en", "Refresh inventory data" }, { "ur", "انوینٹری کا ڈیٹا ریفریش کریں" } } },
        
        // Confirmations
        { "inventory_confirm_delete", new() { { "en", "Are you sure you want to delete this product?" }, { "ur", "کیا آپ اس پروڈکٹ کو ڈیلیٹ کرنا چاہتے ہیں؟" } } },
        { "inventory_confirm_save", new() { { "en", "Save product changes?" }, { "ur", "پروڈکٹ کی تبدیلیاں محفوظ کریں؟" } } }
    };

    await SeedTranslationCategoryAsync("Inventory", translations);
}
```

**Benefits of the New Centralized Approach:**

✅ **Simplified Development**: No per-screen keyword management  
✅ **Better Performance**: All translations loaded once at startup  
✅ **Consistency**: Same translation availability across all screens  
✅ **Easier Maintenance**: Central location for all translation updates  
✅ **Faster Navigation**: No database operations during screen switches  

**What You NO LONGER Need to Do:**

❌ ~~Call `EnsureTranslationKeywordsAsync()` in ViewModel~~  
❌ ~~Add keyword seeding to screen initialization~~  
❌ ~~Manage per-screen translation databases~~  
❌ ~~Handle translation loading failures during navigation~~
```

### Step 5: Add Navigation to MainWindow with Settings Integration

Update `MainWindowViewModel.cs` to add navigation to your new screen:

```csharp
[RelayCommand]
private async void Show[ScreenName]()
{
    try
    {
        SelectedPage = "[ScreenName]";  // For button highlighting
        
        // Get translated page title
        CurrentPageTitle = await _localizationService.GetTranslationAsync("[screen_name]_title") ?? "[Screen Display Name]";
        
        // Get translated status message
        StatusMessage = await _localizationService.GetTranslationAsync("page_loaded") ?? "[Screen Display Name] loaded";
        
        // Create and configure the view
        var view = new [ScreenName]View();
        var viewModel = _serviceProvider.GetRequiredService<[ScreenName]ViewModel>();
        view.DataContext = viewModel;
        
        CurrentView = view;
    }
    catch (Exception ex)
    {
        StatusMessage = $"Error loading [ScreenName]: {ex.Message}";
    }
}
```

Update `MainWindow.xaml` to add a navigation button with proper selection highlighting and translation:

```xml
<Button Height="40" 
        HorizontalAlignment="Stretch" 
        Margin="0,0,0,5"
        Command="{Binding Show[ScreenName]Command}"
        FlowDirection="{Binding CurrentFlowDirection}">
    
    <!-- Button Content with Icon and Translated Text -->
    <StackPanel Orientation="Horizontal">
        <TextBlock Text="📋" 
                   FontSize="16" 
                   VerticalAlignment="Center" 
                   Margin="0,0,8,0"/>
        <TextBlock Text="{Binding [ScreenName]ButtonText}" 
                   FontFamily="{DynamicResource PoppinsFontFallback}"
                   FontSize="{DynamicResource FontSizeSmall}"
                   VerticalAlignment="Center"/>
    </StackPanel>
    
    <!-- Dynamic Button Style with Theme Support -->
    <Button.Style>
        <Style TargetType="Button" BasedOn="{StaticResource ModernButtonStyle}">
            <Setter Property="Background" Value="{DynamicResource SurfaceBackground}"/>
            <Setter Property="Foreground" Value="{DynamicResource TextPrimary}"/>
            <Setter Property="BorderBrush" Value="{DynamicResource BorderLight}"/>
            <Style.Triggers>
                <!-- Hover state -->
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="{DynamicResource OverlayBackground}"/>
                </Trigger>
                
                <!-- Selected state -->
                <DataTrigger Binding="{Binding SelectedPage}" Value="[ScreenName]">
                    <Setter Property="Background" Value="{DynamicResource Primary}"/>
                    <Setter Property="Foreground" Value="White"/>
                    <Setter Property="BorderBrush" Value="{DynamicResource Primary}"/>
                </DataTrigger>
            </Style.Triggers>
        </Style>
    </Button.Style>
</Button>
```

## 🎨 Complete Settings Integration Guide

### 🌐 Centralized Language System (Updated 2025)

#### NEW: Application-Level Translation Seeding

ChronoPos now uses a **centralized language seeding system** that loads all translations at application startup. This eliminates per-screen seeding and ensures consistent language support across all screens.

**Key Benefits:**
- ✅ **No per-screen keyword seeding required** - All translations loaded at startup
- ✅ **Faster screen loading** - No database operations during navigation
- ✅ **Consistent language availability** - All screens have same translation coverage
- ✅ **Simplified development** - Focus on UI binding, not translation management

#### How the New System Works

1. **Application Startup (`App.xaml.cs`)**:
   ```csharp
   // Language seeding happens once during app startup
   protected override async void OnStartup(StartupEventArgs e)
   {
       base.OnStartup(e);
       
       // Seed all translations for all screens at startup
       var languageSeedingService = _serviceProvider.GetRequiredService<LanguageSeedingService>();
       await languageSeedingService.SeedLanguageTranslationsAsync();
       
       // Show main window
       var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
       mainWindow.Show();
   }
   ```

2. **Centralized Translation Management (`LanguageSeedingService.cs`)**:
   ```csharp
   public class LanguageSeedingService
   {
       public async Task SeedLanguageTranslationsAsync()
       {
           // Seeds ALL translations for ALL screens in one operation
           await SeedAllTranslationsAsync();
       }
       
       private async Task SeedAllTranslationsAsync()
       {
           // Navigation translations
           await SeedNavigationTranslationsAsync();
           
           // Common UI translations
           await SeedCommonTranslationsAsync();
           
           // Screen-specific translations
           await SeedAddProductTranslationsAsync();
           await SeedInventoryTranslationsAsync();
           await SeedSettingsTranslationsAsync();
           // Add your screen here: await SeedYourScreenTranslationsAsync();
       }
   }
   ```

#### Implementing Multi-Language Support in Your Screen

**Step 1: Add Translation Properties to ViewModel**
```csharp
public partial class [ScreenName]ViewModel : ObservableObject
{
    private readonly DatabaseLocalizationService _databaseLocalizationService;

    [ObservableProperty]
    private string _title = "[Screen Title]";

    [ObservableProperty]
    private string _searchButtonText = "🔍 Search";

    [ObservableProperty]
    private string _addNewButtonText = "➕ Add New";

    [ObservableProperty]
    private string _dataOverviewText = "Data Overview";

    [ObservableProperty]
    private string _nameColumnHeader = "Name";

    [ObservableProperty]
    private string _statusColumnHeader = "Status";

    [ObservableProperty]
    private string _dateColumnHeader = "Date";

    [ObservableProperty]
    private string _statusLabelText = "Status:";

    [ObservableProperty]
    private string _searchPlaceholder = "Search...";

    [ObservableProperty]
    private string _refreshTooltip = "Refresh";

    public [ScreenName]ViewModel(DatabaseLocalizationService databaseLocalizationService)
    {
        _databaseLocalizationService = databaseLocalizationService;
        
        // Subscribe to language changes for real-time updates
        _databaseLocalizationService.LanguageChanged += OnLanguageChanged;
        
        // Load translations (they're already seeded at startup)
        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadTranslationsAsync();
    }

    private async Task LoadTranslationsAsync()
    {
        try
        {
            // Simply load translations - no seeding required!
            Title = await _databaseLocalizationService.GetTranslationAsync("[screen_name]_title") ?? "[Screen Title]";
            SearchButtonText = await _databaseLocalizationService.GetTranslationAsync("search_button") ?? "🔍 Search";
            AddNewButtonText = await _databaseLocalizationService.GetTranslationAsync("add_new_button") ?? "➕ Add New";
            DataOverviewText = await _databaseLocalizationService.GetTranslationAsync("data_overview") ?? "Data Overview";
            NameColumnHeader = await _databaseLocalizationService.GetTranslationAsync("column_name") ?? "Name";
            StatusColumnHeader = await _databaseLocalizationService.GetTranslationAsync("column_status") ?? "Status";
            DateColumnHeader = await _databaseLocalizationService.GetTranslationAsync("column_date") ?? "Date";
            StatusLabelText = await _databaseLocalizationService.GetTranslationAsync("status_label") ?? "Status:";
            SearchPlaceholder = await _databaseLocalizationService.GetTranslationAsync("search_placeholder") ?? "Search...";
            RefreshTooltip = await _databaseLocalizationService.GetTranslationAsync("refresh_tooltip") ?? "Refresh";
        }
        catch (Exception ex)
        {
            // Log error and use fallback values
            System.Diagnostics.Debug.WriteLine($"Translation loading error: {ex.Message}");
            // Fallback values are already set as default values
        }
    }

    private async void OnLanguageChanged(object? sender, EventArgs e)
    {
        // Reload translations when language changes
        await LoadTranslationsAsync();
    }

    public void Dispose()
    {
        _databaseLocalizationService.LanguageChanged -= OnLanguageChanged;
    }
}
```

**Step 2: Add Your Screen's Translations to LanguageSeedingService**
```csharp
// In LanguageSeedingService.cs, add your screen's translation method:

private async Task SeedYourScreenTranslationsAsync()
{
    var translations = new Dictionary<string, Dictionary<string, string>>
    {
        // Screen-specific translations
        { "[screen_name]_title", new() { { "en", "Your Screen Title" }, { "ur", "آپ کی اسکرین کا عنوان" } } },
        { "[screen_name]_subtitle", new() { { "en", "Manage your data" }, { "ur", "اپنا ڈیٹا منظم کریں" } } },
        
        // Action buttons specific to your screen
        { "[screen_name]_add_button", new() { { "en", "➕ Add New Item" }, { "ur", "➕ نیا آئٹم شامل کریں" } } },
        { "[screen_name]_edit_button", new() { { "en", "✏️ Edit Item" }, { "ur", "✏️ آئٹم میں ترمیم کریں" } } },
        { "[screen_name]_delete_button", new() { { "en", "🗑️ Delete Item" }, { "ur", "🗑️ آئٹم کو ڈیلیٹ کریں" } } },
        
        // Status messages specific to your screen
        { "[screen_name]_loading", new() { { "en", "Loading data..." }, { "ur", "ڈیٹا لوڈ کر رہا ہے..." } } },
        { "[screen_name]_saved", new() { { "en", "Data saved successfully" }, { "ur", "ڈیٹا کامیابی سے محفوظ ہوگیا" } } },
        { "[screen_name]_error", new() { { "en", "An error occurred" }, { "ur", "ایک خرابی ہوئی" } } },
        
        // Form labels specific to your screen
        { "[screen_name]_name_label", new() { { "en", "Item Name:" }, { "ur", "آئٹم کا نام:" } } },
        { "[screen_name]_description_label", new() { { "en", "Description:" }, { "ur", "تفصیل:" } } },
        { "[screen_name]_category_label", new() { { "en", "Category:" }, { "ur", "کیٹگری:" } } },
        
        // Column headers for data grids
        { "[screen_name]_column_id", new() { { "en", "ID" }, { "ur", "شناخت" } } },
        { "[screen_name]_column_name", new() { { "en", "Name" }, { "ur", "نام" } } },
        { "[screen_name]_column_status", new() { { "en", "Status" }, { "ur", "حالت" } } },
        { "[screen_name]_column_date", new() { { "en", "Date" }, { "ur", "تاریخ" } } },
        { "[screen_name]_column_actions", new() { { "en", "Actions" }, { "ur", "عمل" } } }
    };

    await SeedTranslationCategoryAsync("YourScreen", translations);
}

// Then add this call to SeedAllTranslationsAsync():
private async Task SeedAllTranslationsAsync()
{
    // ... existing seeding calls ...
    
    // Add your screen translation seeding
    await SeedYourScreenTranslationsAsync();
}
```

**Step 3: Register Your Screen Translation Keywords**
```csharp
// NO MORE PER-SCREEN KEYWORD REGISTRATION NEEDED!
// Everything is handled at application startup through LanguageSeedingService

// Old approach (NO LONGER NEEDED):
// public static async Task EnsureScreenKeywordsAsync(DatabaseLocalizationService localizationService)
// {
//     // This is no longer required!
// }

// New approach: Just add translations to LanguageSeedingService and you're done!
```

#### XAML Integration (No Changes Required)

Your XAML remains exactly the same - the binding approach is unchanged:

```xml
<UserControl FlowDirection="{Binding CurrentFlowDirection}">
    <StackPanel>
        <!-- All text elements bind to ViewModel properties -->
        <TextBlock Text="{Binding Title}" FontSize="{DynamicResource FontSizeLarge}"/>
        <Button Content="{Binding SearchButtonText}" Command="{Binding SearchCommand}"/>
        <Button Content="{Binding AddNewButtonText}" Command="{Binding AddNewCommand}"/>
        
        <!-- Data grid with translated headers -->
        <DataGrid>
            <DataGrid.Columns>
                <DataGridTextColumn Header="{Binding NameColumnHeader}" Binding="{Binding Name}"/>
                <DataGridTextColumn Header="{Binding StatusColumnHeader}" Binding="{Binding Status}"/>
                <DataGridTextColumn Header="{Binding DateColumnHeader}" Binding="{Binding Date}"/>
            </DataGrid.Columns>
        </DataGrid>
    </StackPanel>
</UserControl>
```

#### Migration from Old System

If you have existing screens using the old per-screen seeding approach:

1. **Remove keyword seeding from ViewModels**:
   ```csharp
   // REMOVE these calls from your ViewModels:
   // await EnsureKeywordsExistAsync();
   // await AddProductTranslations.EnsureTranslationKeywordsAsync(_databaseLocalizationService);
   ```

2. **Move translations to LanguageSeedingService**:
   ```csharp
   // Move your keyword dictionaries from ViewModels to LanguageSeedingService methods
   ```

3. **Simplify LoadTranslationsAsync**:
   ```csharp
   private async Task LoadTranslationsAsync()
   {
       // Only load translations - no seeding!
       Title = await _databaseLocalizationService.GetTranslationAsync("screen_title") ?? "Default Title";
       // ... other translations
   }
   ```

#### Performance Benefits

**Before (Per-Screen Seeding):**
- Each screen navigation: Database checks + potential insertions
- Slow screen loading due to database operations
- Duplicate keyword checking across screens

**After (Centralized Seeding):**
- Application startup: One-time database seeding
- Fast screen navigation: Only translation retrieval
- No duplicate operations

#### Best Practices for New Screens

1. **Translation Key Naming Convention**:
   ```csharp
   // Use consistent prefixes for your screen
   "[screen_name]_title"          // Main screen title
   "[screen_name]_button_name"    // Action buttons
   "[screen_name]_label_field"    // Form labels
   "[screen_name]_column_name"    // Data grid columns
   "[screen_name]_message_type"   // Status messages
   ```

2. **Fallback Values**:
   ```csharp
   // Always provide meaningful fallbacks
   Title = await _service.GetTranslationAsync("inventory_title") ?? "Inventory Management";
   ```

3. **Language Change Handling**:
   ```csharp
   // Subscribe to language changes for real-time updates
   _databaseLocalizationService.LanguageChanged += OnLanguageChanged;
   ```

4. **Translation Testing**:
   ```csharp
   // Test both languages during development
   // Verify fallbacks work when translations are missing
   // Check RTL layout with Urdu translations
   ```
```

### Theme System Integration

#### Essential Theme Resources for Any Screen

Always use these `{DynamicResource}` bindings for automatic theme switching:

```xml
<!-- Background Resources -->
Background="{DynamicResource MainBackground}"      <!-- Main app background -->
Background="{DynamicResource CardBackground}"      <!-- Cards and panels -->
Background="{DynamicResource SurfaceBackground}"   <!-- Input controls, surfaces -->
Background="{DynamicResource OverlayBackground}"   <!-- Overlays, hover states -->

<!-- Text Color Resources -->
Foreground="{DynamicResource TextPrimary}"         <!-- Main text -->
Foreground="{DynamicResource TextSecondary}"       <!-- Secondary text -->
Foreground="{DynamicResource TextDisabled}"        <!-- Disabled text -->

<!-- Border Resources -->
BorderBrush="{DynamicResource BorderLight}"        <!-- Standard borders -->
BorderBrush="{DynamicResource BorderMuted}"        <!-- Subtle borders -->

<!-- Accent Color Resources -->
Foreground="{DynamicResource Primary}"             <!-- Primary brand color -->
Background="{DynamicResource Primary}"             <!-- Primary backgrounds -->
Foreground="{DynamicResource Success}"             <!-- Success states (Green) -->
Foreground="{DynamicResource Error}"               <!-- Error states (Red) -->
Foreground="{DynamicResource Warning}"             <!-- Warning states (Orange) -->
Foreground="{DynamicResource Info}"                <!-- Info states (Blue) -->
```

### Color Scheme System

#### Using Customizable Colors

The color scheme system allows users to customize the primary and background colors. Your screen automatically inherits these changes through dynamic resources:

```xml
<!-- These automatically update when user changes color scheme -->
<Button Background="{DynamicResource Primary}"         <!-- Updates with user's primary color -->
        Foreground="White"/>

<Border Background="{DynamicResource CardBackground}"  <!-- Updates with user's background theme -->
        BorderBrush="{DynamicResource Primary}"/>       <!-- Updates with user's primary color -->

<!-- Status indicators that respect color customization -->
<TextBlock Foreground="{DynamicResource Success}"/>    <!-- Always readable regardless of theme -->
<TextBlock Foreground="{DynamicResource Error}"/>      <!-- Always readable regardless of theme -->
```

### Layout Direction (RTL/LTR) Support

#### Implementing Proper RTL Support

1. **Set FlowDirection on Root Elements:**
```xml
<UserControl FlowDirection="{Binding CurrentFlowDirection}">
    <!-- All child elements inherit this direction -->
</UserControl>
```

2. **Use Direction-Aware Text Alignment:**
```xml
<!-- Create a converter for automatic text alignment -->
TextAlignment="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToTextAlignmentConverter}}"

<!-- Bind alignment in data grid columns -->
HorizontalContentAlignment="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToHorizontalAlignmentConverter}}"
```

3. **Handle Icons and Visual Elements:**
```xml
<!-- Some icons may need to flip for RTL -->
<TextBlock Text="→" 
           RenderTransformOrigin="0.5,0.5">
    <TextBlock.RenderTransform>
        <ScaleTransform ScaleX="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToScaleConverter}}"/>
    </TextBlock.RenderTransform>
</TextBlock>
```

### Font System Implementation

#### Responsive Font Sizing with Zoom Integration

The font system automatically scales with user zoom preferences and font size settings:

```xml
<!-- Available font sizes (automatically scaled based on user preference and zoom level) -->
FontSize="{DynamicResource FontSizeVerySmall}"    <!-- 13-20px range depending on zoom -->
FontSize="{DynamicResource FontSizeSmall}"        <!-- 15-22px range depending on zoom -->
FontSize="{DynamicResource FontSizeMedium}"       <!-- 17-26px range depending on zoom -->
FontSize="{DynamicResource FontSizeLarge}"        <!-- 19-29px range depending on zoom -->
FontSize="{DynamicResource FontSizeXLarge}"       <!-- 22-33px range depending on zoom -->
FontSize="{DynamicResource FontSizeXXLarge}"      <!-- 26-39px range depending on zoom -->

<!-- Always pair with Poppins font family -->
FontFamily="{DynamicResource PoppinsFontFallback}"

<!-- Complete text element example with zoom support -->
<TextBlock Text="Responsive Text"
           FontFamily="{DynamicResource PoppinsFontFallback}"
           FontSize="{DynamicResource FontSizeMedium}"
           Foreground="{DynamicResource TextPrimary}"/>
```

#### Font Weight Guidelines with Zoom Considerations

```xml
FontWeight="Light"      <!-- For less important text - scales with zoom -->
FontWeight="Normal"     <!-- Default text weight - scales with zoom -->
FontWeight="Medium"     <!-- Slightly emphasized text - scales with zoom -->
FontWeight="SemiBold"   <!-- Section headers, labels - scales with zoom -->
FontWeight="Bold"       <!-- Page titles, important headers - scales with zoom -->
```

## 🔍 Zoom Level System Integration

### Understanding the Zoom System

ChronoPos includes a comprehensive zoom system that allows users to scale the entire UI from 50% to 150%:

- **Zoom50** (50%) - Very Small UI elements
- **Zoom60** (60%) - Small UI elements  
- **Zoom70** (70%) - Smaller UI elements
- **Zoom80** (80%) - Small Normal UI elements
- **Zoom90** (90%) - Almost Normal UI elements
- **Zoom100** (100%) - Normal (Default) UI elements
- **Zoom110** (110%) - Slightly Large UI elements
- **Zoom120** (120%) - Large UI elements
- **Zoom130** (130%) - Larger UI elements
- **Zoom140** (140%) - Very Large UI elements
- **Zoom150** (150%) - Maximum UI elements

### Zoom-Aware Resource Usage

#### Layout Resources that Scale with Zoom

```xml
<!-- Spacing and sizing that automatically scales with zoom level -->
Margin="{DynamicResource DefaultMarginThickness}"      <!-- 10px base, scales 5-15px -->
Padding="{DynamicResource DefaultPaddingThickness}"    <!-- 15px base, scales 7.5-22.5px -->
Height="{DynamicResource ButtonHeight}"                <!-- 45px base, scales 22.5-67.5px -->
MinWidth="{DynamicResource ButtonMinWidth}"            <!-- 80px base, scales 40-120px -->

<!-- Corner radius that scales with zoom -->
CornerRadius="{DynamicResource BorderCornerRadius}"    <!-- 8px base, scales 4-12px -->

<!-- Icon sizes that scale with zoom -->
Width="{DynamicResource IconSize}"                     <!-- 16px base, scales 8-24px -->
Height="{DynamicResource IconSize}"                    <!-- 16px base, scales 8-24px -->
```

#### Card and Panel Padding with Zoom

```xml
<!-- Card containers with zoom-aware padding -->
<Border Background="{DynamicResource CardBackground}" 
        CornerRadius="{DynamicResource CardCornerRadius}"
        Padding="{DynamicResource CardPaddingThickness}"      <!-- 20px base, scales 10-30px -->
        Margin="{DynamicResource DefaultMarginThickness}"     <!-- 10px base, scales 5-15px -->
        BorderBrush="{DynamicResource BorderLight}"
        BorderThickness="1">
    
    <!-- Content automatically scales with container -->
    
</Border>
```

### ViewModel Integration with Zoom Service

```csharp
public partial class [ScreenName]ViewModel : ObservableObject
{
    private readonly ZoomService _zoomService;
    
    [ObservableProperty]
    private ZoomLevel _currentZoomLevel = ZoomLevel.Zoom100;
    
    [ObservableProperty]
    private double _currentZoomScale = 1.0;
    
    [ObservableProperty]
    private int _currentZoomPercentage = 100;

    public [ScreenName]ViewModel(ZoomService zoomService, /* other services */)
    {
        _zoomService = zoomService;
        
        // Subscribe to zoom changes
        _zoomService.ZoomChanged += OnZoomChanged;
        
        // Initialize current zoom state
        UpdateZoomLevel();
    }

    private void OnZoomChanged(object? sender, ZoomLevel zoomLevel)
    {
        CurrentZoomLevel = zoomLevel;
        CurrentZoomScale = _zoomService.CurrentZoomScale;
        CurrentZoomPercentage = _zoomService.CurrentZoomPercentage;
        
        // Optional: Custom zoom handling for specific elements
        HandleCustomZoomLogic();
    }

    private void UpdateZoomLevel()
    {
        CurrentZoomLevel = _zoomService.CurrentZoomLevel;
        CurrentZoomScale = _zoomService.CurrentZoomScale;
        CurrentZoomPercentage = _zoomService.CurrentZoomPercentage;
    }
    
    private void HandleCustomZoomLogic()
    {
        // Example: Adjust data grid column widths based on zoom
        // Example: Show/hide certain UI elements based on available space
        // Example: Adjust image quality based on zoom level
    }

    public void Dispose()
    {
        _zoomService.ZoomChanged -= OnZoomChanged;
        // Other cleanup
    }
}
```

### XAML Components with Zoom-Awareness

#### Buttons with Zoom Scaling

```xml
<!-- Navigation buttons that scale properly with zoom -->
<Button Content="🏠 Dashboard"
        Style="{DynamicResource NavigationButtonStyle}"     <!-- Includes zoom-aware sizing -->
        FontFamily="{DynamicResource PoppinsFontFallback}"
        FontSize="{DynamicResource FontSizeMedium}"         <!-- Scales 17-26px with zoom -->
        Height="{DynamicResource ButtonHeight}"             <!-- Scales 45-67.5px with zoom -->
        Margin="{DynamicResource DefaultMarginThickness}"   <!-- Scales margins appropriately -->
        Command="{Binding NavigateCommand}"/>
```

#### Input Controls with Zoom Support

```xml
<!-- Text inputs that scale with zoom level -->
<TextBox Text="{Binding InputValue}"
         Style="{DynamicResource ModernTextBoxStyle}"
         FontFamily="{DynamicResource PoppinsFontFallback}"
         FontSize="{DynamicResource FontSizeSmall}"         <!-- Scales 15-22px with zoom -->
         Padding="{DynamicResource DefaultPaddingThickness}" <!-- Scales 15-22.5px with zoom -->
         Margin="{DynamicResource DefaultMarginThickness}"   <!-- Scales 10-15px with zoom -->
         MinHeight="{DynamicResource ButtonHeight}"/>        <!-- Consistent with button heights -->
```

#### Data Grids with Zoom Integration

```xml
<DataGrid ItemsSource="{Binding Items}"
          FontFamily="{DynamicResource PoppinsFontFallback}"
          FontSize="{DynamicResource FontSizeSmall}"         <!-- Text scales with zoom -->
          Background="{DynamicResource CardBackground}"
          Foreground="{DynamicResource TextPrimary}"
          BorderBrush="{DynamicResource BorderLight}"
          RowHeight="{DynamicResource ButtonHeight}">         <!-- Row height scales with zoom -->
    
    <DataGrid.ColumnHeaderStyle>
        <Style TargetType="DataGridColumnHeader">
            <Setter Property="FontFamily" Value="{DynamicResource PoppinsFontFallback}"/>
            <Setter Property="FontSize" Value="{DynamicResource FontSizeSmall}"/>
            <Setter Property="Padding" Value="{DynamicResource DefaultPaddingThickness}"/>
            <Setter Property="MinHeight" Value="{DynamicResource ButtonHeight}"/>
            <!-- All properties scale with zoom automatically -->
        </Style>
    </DataGrid.ColumnHeaderStyle>
    
    <DataGrid.CellStyle>
        <Style TargetType="DataGridCell">
            <Setter Property="FontFamily" Value="{DynamicResource PoppinsFontFallback}"/>
            <Setter Property="FontSize" Value="{DynamicResource FontSizeSmall}"/>
            <Setter Property="Padding" Value="{DynamicResource DefaultPaddingThickness}"/>
            <!-- Cells automatically scale with zoom -->
        </Style>
    </DataGrid.CellStyle>
</DataGrid>
```

#### Icons and Images with Zoom Scaling

```xml
<!-- Icons that scale appropriately with zoom level -->
<TextBlock Text="🔍"
           FontSize="{DynamicResource IconSize}"             <!-- Scales 16-24px with zoom -->
           VerticalAlignment="Center"
           Margin="{DynamicResource DefaultMarginThickness}"/>

<!-- Custom icons with zoom-aware sizing -->
<Image Source="icon.png"
       Width="{DynamicResource IconSize}"                   <!-- Scales with zoom -->
       Height="{DynamicResource IconSize}"                  <!-- Maintains aspect ratio -->
       Margin="{DynamicResource DefaultMarginThickness}"/>
```

### Advanced Zoom Considerations

#### Responsive Layout with Zoom

```csharp
// ViewModel method to handle zoom-based layout changes
private void HandleCustomZoomLogic()
{
    // Adjust layout based on zoom level
    switch (CurrentZoomLevel)
    {
        case ZoomLevel.Zoom50:
        case ZoomLevel.Zoom60:
        case ZoomLevel.Zoom70:
            // Compact layout for small zoom levels
            IsCompactMode = true;
            ShowDetailedInfo = false;
            break;
            
        case ZoomLevel.Zoom80:
        case ZoomLevel.Zoom90:
        case ZoomLevel.Zoom100:
            // Normal layout
            IsCompactMode = false;
            ShowDetailedInfo = true;
            break;
            
        case ZoomLevel.Zoom110:
        case ZoomLevel.Zoom120:
        case ZoomLevel.Zoom130:
        case ZoomLevel.Zoom140:
        case ZoomLevel.Zoom150:
            // Expanded layout for large zoom levels
            IsCompactMode = false;
            ShowDetailedInfo = true;
            ShowExtraDetails = true;
            break;
    }
}
```

#### Conditional UI Elements Based on Zoom

```xml
<!-- Show detailed information only at higher zoom levels -->
<StackPanel Visibility="{Binding ShowDetailedInfo, Converter={StaticResource BooleanToVisibilityConverter}}">
    <TextBlock Text="{Binding DetailedDescription}"
               FontFamily="{DynamicResource PoppinsFontFallback}"
               FontSize="{DynamicResource FontSizeSmall}"
               Foreground="{DynamicResource TextSecondary}"
               Margin="{DynamicResource DefaultMarginThickness}"/>
</StackPanel>

<!-- Compact mode for smaller zoom levels -->
<Grid Visibility="{Binding IsCompactMode, Converter={StaticResource BooleanToVisibilityConverter}}">
    <!-- Simplified UI for compact mode -->
</Grid>
```

### Testing Zoom Integration

#### Zoom Testing Checklist for New Screens

- [ ] **50% Zoom**: Verify all text is readable and UI elements are usable
- [ ] **75% Zoom**: Check that layout remains functional and attractive
- [ ] **100% Zoom**: Confirm default appearance matches design specifications
- [ ] **125% Zoom**: Ensure larger elements don't break layout
- [ ] **150% Zoom**: Verify maximum zoom maintains usability

#### Zoom-Specific Test Cases

```csharp
// Test scenarios for zoom functionality
1. Load screen at 100% zoom - verify normal appearance
2. Change to 50% zoom - verify UI scales down appropriately
3. Change to 150% zoom - verify UI scales up without breaking
4. Navigate between screens with different zoom levels - verify consistency
5. Input text at various zoom levels - verify text entry works correctly
6. Test data grids with scrolling at different zoom levels
7. Verify button click targets scale appropriately with zoom
8. Test form validation messages at different zoom levels
```

### Pre-built Component Styles

#### Button Styles with Full Settings Support

```xml
<!-- Standard button (adapts to all themes and colors) -->
<Button Content="{Binding ButtonText}" 
        Style="{DynamicResource ModernButtonStyle}"
        FontFamily="{DynamicResource PoppinsFontFallback}"
        FontSize="{DynamicResource FontSizeSmall}"/>

<!-- Primary action button (uses customizable primary color) -->
<Button Content="{Binding PrimaryButtonText}" 
        Style="{DynamicResource PrimaryButtonStyle}"
        FontFamily="{DynamicResource PoppinsFontFallback}"
        FontSize="{DynamicResource FontSizeSmall}"/>
```

#### Input Controls with Settings Alignment

```xml
<!-- Text input with theme and direction support -->
<TextBox Text="{Binding InputValue}"
         Style="{DynamicResource ModernTextBoxStyle}"
         FontFamily="{DynamicResource PoppinsFontFallback}"
         FontSize="{DynamicResource FontSizeSmall}"
         FlowDirection="{Binding CurrentFlowDirection}"
         TextAlignment="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToTextAlignmentConverter}}"/>

<!-- ComboBox with full settings integration -->
<ComboBox SelectedItem="{Binding SelectedOption}"
          Background="{DynamicResource CardBackground}"
          Foreground="{DynamicResource TextPrimary}"
          BorderBrush="{DynamicResource BorderLight}"
          FontFamily="{DynamicResource PoppinsFontFallback}"
          FontSize="{DynamicResource FontSizeSmall}"
          FlowDirection="{Binding CurrentFlowDirection}"/>
```

## Settings-Aware Data Binding Best Practices

### ViewModel Properties with Settings Integration

Use `[ObservableProperty]` attribute with settings-aware initialization:

```csharp
[ObservableProperty]
private string _title = "Default Title";

[ObservableProperty]
private bool _isLoading = false;

[ObservableProperty]
private ObservableCollection<ItemViewModel> _items = new();

[ObservableProperty]
private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

// Settings-aware property updates
private async void OnLanguageChanged(object? sender, EventArgs e)
{
    await LoadTranslationsAsync();
    OnPropertyChanged(nameof(Title));
    OnPropertyChanged(nameof(SearchButtonText));
    // Update other translated properties
}

private void OnLayoutDirectionChanged(object? sender, FlowDirection direction)
{
    CurrentFlowDirection = direction;
}
```

### Commands with Translated Status Updates

```csharp
[RelayCommand]
private async Task SaveAsync()
{
    try 
    {
        IsLoading = true;
        StatusMessage = await _localizationService.GetTranslationAsync("status_saving") ?? "Saving...";
        
        await _service.SaveAsync();
        
        StatusMessage = await _localizationService.GetTranslationAsync("status_saved") ?? "Saved successfully";
    }
    catch (Exception ex)
    {
        StatusMessage = await _localizationService.GetTranslationAsync("status_save_failed") ?? $"Save failed: {ex.Message}";
    }
    finally 
    {
        IsLoading = false;
    }
}
```

### Async Operations with Cancellation and Translation

```csharp
private CancellationTokenSource? _cancellationTokenSource;

[RelayCommand]
private async Task LoadDataAsync()
{
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();
    
    try
    {
        IsLoading = true;
        StatusMessage = await _localizationService.GetTranslationAsync("status_loading") ?? "Loading...";
        
        var data = await _service.GetDataAsync(_cancellationTokenSource.Token);
        Items.Clear();
        foreach (var item in data)
        {
            Items.Add(new ItemViewModel(item));
        }
        
        StatusMessage = await _localizationService.GetTranslationAsync("status_loaded") ?? "Data loaded successfully";
    }
    catch (OperationCanceledException)
    {
        StatusMessage = await _localizationService.GetTranslationAsync("status_cancelled") ?? "Operation cancelled";
    }
    catch (Exception ex)
    {
        StatusMessage = await _localizationService.GetTranslationAsync("status_error") ?? $"Error: {ex.Message}";
    }
    finally
    {
        IsLoading = false;
    }
}
```

## Common UI Patterns with Full Settings Support

### Card Layout Pattern with RTL and Theme Support

```xml
<Border Background="{DynamicResource SurfaceBackground}" 
        CornerRadius="8" 
        Padding="20" 
        Margin="0,0,0,20"
        BorderBrush="{DynamicResource BorderLight}"
        BorderThickness="1"
        FlowDirection="{Binding CurrentFlowDirection}">
    
    <StackPanel>
        <TextBlock Text="{Binding CardTitle}" 
                   FontSize="{DynamicResource FontSizeMedium}" 
                   FontWeight="SemiBold" 
                   FontFamily="{DynamicResource PoppinsFontFallback}"
                   Foreground="{DynamicResource TextPrimary}"
                   TextAlignment="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToTextAlignmentConverter}}"
                   Margin="0,0,0,15"/>
        
        <!-- Card content with settings support -->
        
    </StackPanel>
</Border>
```

### Loading State Pattern with Translation

```xml
<Grid>
    <!-- Loading overlay with translated text -->
    <Border Background="{DynamicResource OverlayBackground}"
            Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}">
        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
            <ProgressBar IsIndeterminate="True" 
                         Width="200" 
                         Height="4"
                         Foreground="{DynamicResource Primary}"/>
            <TextBlock Text="{Binding LoadingText}" 
                       FontFamily="{DynamicResource PoppinsFontFallback}"
                       FontSize="{DynamicResource FontSizeSmall}"
                       Foreground="{DynamicResource TextPrimary}"
                       Margin="0,10,0,0"
                       HorizontalAlignment="Center"/>
        </StackPanel>
    </Border>
    
    <!-- Main content -->
    <ScrollViewer Visibility="{Binding IsLoading, Converter={StaticResource InverseBooleanToVisibilityConverter}}"
                  FlowDirection="{Binding CurrentFlowDirection}">
        <!-- Your content here -->
    </ScrollViewer>
</Grid>
```

### Status Message Pattern with Color Scheme Integration

```xml
<Border Background="{DynamicResource SurfaceBackground}" 
        CornerRadius="4" 
        Padding="10,8"
        BorderBrush="{DynamicResource BorderLight}"
        BorderThickness="1">
    <StackPanel Orientation="Horizontal">
        <!-- Status indicator using theme colors -->
        <Ellipse Width="8" Height="8"
                 Fill="{DynamicResource Success}"
                 VerticalAlignment="Center"
                 Margin="0,0,8,0"/>
        <TextBlock Text="{Binding StatusMessage}" 
                   FontFamily="{DynamicResource PoppinsFontFallback}"
                   FontSize="{DynamicResource FontSizeSmall}"
                   Foreground="{DynamicResource TextPrimary}"
                   VerticalAlignment="Center"/>
    </StackPanel>
</Border>
```

### Data Grid with Complete Settings Integration

```xml
<DataGrid ItemsSource="{Binding Items}"
          SelectedItem="{Binding SelectedItem}"
          AutoGenerateColumns="False"
          Background="{DynamicResource CardBackground}"
          Foreground="{DynamicResource TextPrimary}"
          BorderBrush="{DynamicResource BorderLight}"
          GridLinesVisibility="Horizontal"
          HorizontalGridLinesBrush="{DynamicResource BorderLight}"
          CanUserAddRows="False"
          FontFamily="{DynamicResource PoppinsFontFallback}"
          FontSize="{DynamicResource FontSizeSmall}"
          FlowDirection="{Binding CurrentFlowDirection}">
    
    <DataGrid.ColumnHeaderStyle>
        <Style TargetType="DataGridColumnHeader">
            <Setter Property="Background" Value="{DynamicResource SurfaceBackground}"/>
            <Setter Property="Foreground" Value="{DynamicResource TextPrimary}"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="FontFamily" Value="{DynamicResource PoppinsFontFallback}"/>
            <Setter Property="FontSize" Value="{DynamicResource FontSizeSmall}"/>
            <Setter Property="Padding" Value="12,8"/>
            <Setter Property="HorizontalContentAlignment" Value="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToHorizontalAlignmentConverter}}"/>
        </Style>
    </DataGrid.ColumnHeaderStyle>
    
    <DataGrid.RowStyle>
        <Style TargetType="DataGridRow">
            <Setter Property="Background" Value="{DynamicResource CardBackground}"/>
            <Style.Triggers>
                <Trigger Property="IsSelected" Value="True">
                    <Setter Property="Background" Value="{DynamicResource Primary}"/>
                    <Setter Property="Foreground" Value="White"/>
                </Trigger>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="{DynamicResource OverlayBackground}"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </DataGrid.RowStyle>
    
    <DataGrid.CellStyle>
        <Style TargetType="DataGridCell">
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Padding" Value="10,8"/>
            <Setter Property="FontFamily" Value="{DynamicResource PoppinsFontFallback}"/>
            <Setter Property="FontSize" Value="{DynamicResource FontSizeSmall}"/>
            <Setter Property="HorizontalContentAlignment" Value="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToHorizontalAlignmentConverter}}"/>
        </Style>
    </DataGrid.CellStyle>
    
    <!-- Columns with translated headers -->
    <DataGrid.Columns>
        <DataGridTextColumn Header="{Binding NameColumnHeader}" 
                            Binding="{Binding Name}" 
                            Width="*"/>
        <DataGridTextColumn Header="{Binding StatusColumnHeader}" 
                            Binding="{Binding Status}" 
                            Width="Auto"/>
        <DataGridTextColumn Header="{Binding DateColumnHeader}" 
                            Binding="{Binding Date, StringFormat='{}{0:MMM dd, yyyy}'}" 
                            Width="Auto"/>
    </DataGrid.Columns>
</DataGrid>
```

## Testing Your Settings-Aligned New Screen

### 1. Build and Run with Validation
```bash
# Build the solution
dotnet build

# Run the application
dotnet run --project src\ChronoPos.Desktop\ChronoPos.Desktop.csproj
```

### 2. Comprehensive Settings Testing Checklist

#### Theme System Testing
- [ ] **Light Theme**: Switch to light theme - verify all elements use proper light colors
- [ ] **Dark Theme**: Switch to dark theme - verify all elements use proper dark colors
- [ ] **Text Readability**: Check all text is readable in both themes
- [ ] **Button States**: Verify hover, pressed, and selected states work in both themes
- [ ] **Border Visibility**: Ensure borders are visible but not too prominent in both themes
- [ ] **Loading Indicators**: Test progress bars and loading states in both themes

#### Color Scheme Testing
- [ ] **Primary Color Change**: Change primary color and verify all accent elements update
- [ ] **Background Color Variants**: Test different background color schemes
- [ ] **Button Color Updates**: Verify primary buttons use the new primary color
- [ ] **Selection Highlighting**: Check that selected items use the new primary color
- [ ] **Status Colors**: Ensure success, error, warning colors remain readable with new schemes

#### Language System Testing
- [ ] **English Language**: Switch to English and verify all text labels are in English
- [ ] **Urdu Language**: Switch to Urdu and verify all text labels are in Urdu
- [ ] **Text Layout**: Check that Urdu text displays correctly (proper font rendering)
- [ ] **Missing Translations**: Verify fallback text appears for any missing translations
- [ ] **Dynamic Updates**: Change language while on your screen - text should update immediately
- [ ] **Special Characters**: Verify Urdu text with special characters displays properly

#### Layout Direction Testing
- [ ] **LTR Mode**: Verify layout flows left-to-right correctly in English
- [ ] **RTL Mode**: Switch to RTL and verify layout flows right-to-left for Urdu
- [ ] **Text Alignment**: Check text aligns properly in both directions
- [ ] **Icon Positioning**: Verify icons position correctly in both directions
- [ ] **Data Grid**: Test grid column headers and cell alignment in both directions
- [ ] **Input Fields**: Check text input direction follows the layout direction

#### Font System Testing
- [ ] **Very Small Font**: Set to very small and verify text is still readable
- [ ] **Small Font**: Test small font size for comfortable reading
- [ ] **Medium Font**: Verify default medium font size looks good
- [ ] **Large Font**: Test large font without breaking layout
- [ ] **Font Consistency**: Check all elements use Poppins font (or fallback)
- [ ] **Font Weights**: Verify different font weights (light, normal, semibold, bold) work correctly

#### Zoom Level System Testing
- [ ] **50% Zoom (Very Small)**: Verify all UI elements scale down and remain usable
- [ ] **60% Zoom (Small)**: Check text readability and button accessibility
- [ ] **70% Zoom (Smaller)**: Verify layout maintains proper proportions
- [ ] **80% Zoom (Small Normal)**: Test form inputs and data grid functionality
- [ ] **90% Zoom (Almost Normal)**: Check navigation and interaction elements
- [ ] **100% Zoom (Normal)**: Verify default appearance matches specifications
- [ ] **110% Zoom (Slightly Large)**: Test larger elements don't break layout
- [ ] **120% Zoom (Large)**: Verify accessibility for users requiring larger UI
- [ ] **130% Zoom (Larger)**: Check advanced layouts handle scaling properly
- [ ] **140% Zoom (Very Large)**: Test maximum usability with large elements
- [ ] **150% Zoom (Maximum)**: Verify UI remains functional at maximum zoom
- [ ] **Zoom Transitions**: Test smooth scaling when changing zoom levels
- [ ] **Cross-Screen Consistency**: Verify zoom applies consistently across all screens
- [ ] **Zoom Persistence**: Check zoom level persists across app restarts

#### Screen-Specific Functionality Testing
- [ ] **Navigation**: Click navigation button - screen loads correctly
- [ ] **Data Loading**: Test data loading with proper loading indicators
- [ ] **Search Function**: Test search functionality with both languages
- [ ] **Form Submission**: Test any forms work correctly in both language modes
- [ ] **Error Handling**: Test error states display translated error messages
- [ ] **Status Updates**: Verify status messages show in the current language

### 3. Advanced Integration Testing

#### Settings Persistence Testing
```csharp
// Test that settings persist across app restarts
1. Change to Dark theme, Urdu language, RTL direction, Large font, Custom colors
2. Close the application
3. Restart the application
4. Verify all settings are restored correctly
5. Navigate to your screen - verify it respects all restored settings
```

#### Multi-Screen Consistency Testing
```csharp
// Test consistency across all screens
1. Set specific settings (e.g., Dark theme, Urdu, RTL, Large font)
2. Navigate between your new screen and existing screens
3. Verify all screens maintain consistent appearance
4. Change settings while on different screens - verify all update correctly
```

#### Performance Testing
```csharp
// Test settings change performance
1. Load your screen with large datasets
2. Change themes rapidly - verify no performance degradation
3. Change languages - verify translation loading is fast
4. Change font sizes - verify layout updates smoothly
```

### 4. Validation Commands

#### Database Verification
```bash
# Check if your keywords were added to database
dotnet run --project src\ChronoPos.Desktop -- --check-translations

# Or use SQL to check directly
# Open database and verify your keywords exist in LanguageKeyword and LabelTranslation tables
```

#### Console Output Monitoring
```bash
# Run with verbose logging to see settings changes
dotnet run --project src\ChronoPos.Desktop --configuration Debug

# Watch for errors in console related to:
# - Missing translations
# - Theme resource not found
# - Font loading issues
# - Layout direction problems
```

## Troubleshooting Common Settings Integration Issues

### Language System Issues

**Problem**: Text not translating when language changes
**Solution**: 
- Verify keywords exist in database using `LanguageManager.AddKeywordWithTranslationsAsync()`
- Check that ViewModel subscribes to `LanguageChanged` event
- Ensure translation loading is called in `OnLanguageChanged`

**Problem**: Fallback text not showing
**Solution**:
- Always provide fallback values in translation calls: `await _localizationService.GetTranslationAsync("key") ?? "Fallback"`
- Set default values for all translated properties

**Problem**: Urdu text not displaying correctly
**Solution**:
- Verify font supports Urdu characters (Poppins or system fallback)
- Check `FlowDirection` is set to `RightToLeft` for Urdu
- Ensure proper Unicode encoding in database

### Theme and Color Scheme Issues

**Problem**: Colors not updating when theme/color scheme changes
**Solution**:
- Use `{DynamicResource}` instead of `{StaticResource}`
- Verify resource keys match theme dictionary definitions
- Check that controls inherit from proper base styles

**Problem**: Custom colors not applying
**Solution**:
- Ensure ColorSchemeService is properly registered and initialized
- Verify custom color resources are being updated in Application.Resources
- Check that your controls bind to the correct color resources

### Layout Direction Issues

**Problem**: RTL layout not working correctly
**Solution**:
- Set `FlowDirection="{Binding CurrentFlowDirection}"` on root control
- Use appropriate text alignment converters
- Verify all child controls inherit FlowDirection

**Problem**: Icons or images not flipping for RTL
**Solution**:
- Use RenderTransform with ScaleTransform for directional icons
- Consider using different icons for RTL languages
- Some icons may not need to flip (numbers, universal symbols)

### Font System Issues

**Problem**: Font sizes not updating
**Solution**:
- Use `{DynamicResource FontSizeXXX}` instead of hardcoded values
- Verify FontService is updating Application.Resources
- Check that all controls bind to font size resources

**Problem**: Poppins font not loading
**Solution**:
- Verify font files are included as Resources in project
- Use `PoppinsFontFallback` which includes system font fallbacks
- Check font resource definitions in theme files

### Performance Issues

**Problem**: Slow language switching
**Solution**:
- Cache translations in ViewModel properties instead of calling service repeatedly
- Use async translation loading to avoid UI blocking
- Consider lazy loading of translations for large screens

**Problem**: Theme switching causes lag
**Solution**:
- Minimize use of complex styles and templates
- Use simpler visual elements where possible
- Consider virtualizing large data collections

## Complete Example: Settings-Integrated Inventory Screen

Here's a complete implementation demonstrating all settings integration:

### InventoryViewModel.cs
```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Desktop.Services;
using System.Collections.ObjectModel;

namespace ChronoPos.Desktop.ViewModels;

public partial class InventoryViewModel : ObservableObject, IDisposable
{
    private readonly DatabaseLocalizationService _localizationService;
    private readonly ThemeService _themeService;
    private readonly ColorSchemeService _colorSchemeService;
    private readonly LayoutDirectionService _layoutDirectionService;
    private readonly ZoomService _zoomService;

    [ObservableProperty] private string _title = "Inventory Management";
    [ObservableProperty] private string _searchButtonText = "🔍 Search";
    [ObservableProperty] private string _addNewButtonText = "➕ Add Product";
    [ObservableProperty] private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;
    [ObservableProperty] private ZoomLevel _currentZoomLevel = ZoomLevel.Zoom100;
    [ObservableProperty] private bool _isLoading = false;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private ObservableCollection<ProductViewModel> _products = new();

    public InventoryViewModel(
        DatabaseLocalizationService localizationService,
        ThemeService themeService,
        ColorSchemeService colorSchemeService,
        LayoutDirectionService layoutDirectionService,
        ZoomService zoomService)
    {
        _localizationService = localizationService;
        _themeService = themeService;
        _colorSchemeService = colorSchemeService;
        _layoutDirectionService = layoutDirectionService;
        _zoomService = zoomService;

        // Subscribe to settings changes
        _layoutDirectionService.LayoutDirectionChanged += OnLayoutDirectionChanged;
        _localizationService.LanguageChanged += OnLanguageChanged;
        _zoomService.ZoomChanged += OnZoomChanged;

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await EnsureKeywordsExistAsync();
        await LoadTranslationsAsync();
        UpdateLayoutDirection();
        UpdateZoomLevel();
        await LoadDataAsync();
    }

    private async Task EnsureKeywordsExistAsync()
    {
        var languageManager = new LanguageManager(_localizationService);
        var keywords = new Dictionary<string, Dictionary<string, string>>
        {
            {
                "inventory_title",
                new Dictionary<string, string>
                {
                    { "en", "Inventory Management" },
                    { "ur", "انوینٹری کا انتظام" }
                }
            },
            {
                "search_products",
                new Dictionary<string, string>
                {
                    { "en", "🔍 Search Products" },
                    { "ur", "🔍 پروڈکٹس تلاش کریں" }
                }
            },
            {
                "add_product",
                new Dictionary<string, string>
                {
                    { "en", "➕ Add Product" },
                    { "ur", "➕ پروڈکٹ شامل کریں" }
                }
            }
        };

        await languageManager.AddMultipleKeywordsAsync(keywords);
    }

    private async Task LoadTranslationsAsync()
    {
        Title = await _localizationService.GetTranslationAsync("inventory_title") ?? "Inventory Management";
        SearchButtonText = await _localizationService.GetTranslationAsync("search_products") ?? "🔍 Search Products";
        AddNewButtonText = await _localizationService.GetTranslationAsync("add_product") ?? "➕ Add Product";
        StatusMessage = await _localizationService.GetTranslationAsync("status_ready") ?? "Ready";
    }

    private void OnLanguageChanged(object? sender, EventArgs e) => _ = LoadTranslationsAsync();
    private void OnLayoutDirectionChanged(object? sender, FlowDirection direction) => CurrentFlowDirection = direction;
    private void OnZoomChanged(object? sender, ZoomLevel zoomLevel) => CurrentZoomLevel = zoomLevel;
    private void UpdateLayoutDirection() => CurrentFlowDirection = _layoutDirectionService.CurrentDirection;
    private void UpdateZoomLevel() => CurrentZoomLevel = _zoomService.CurrentZoomLevel;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;
        StatusMessage = await _localizationService.GetTranslationAsync("status_loading") ?? "Loading...";
        
        // Simulate data loading - scales work automatically with zoom through dynamic resources
        await Task.Delay(1000);
        
        StatusMessage = await _localizationService.GetTranslationAsync("status_loaded") ?? "Products loaded";
        IsLoading = false;
    }

    public void Dispose()
    {
        _layoutDirectionService.LayoutDirectionChanged -= OnLayoutDirectionChanged;
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _zoomService.ZoomChanged -= OnZoomChanged;
    }
}
```

### Register in App.xaml.cs
```csharp
services.AddTransient<InventoryViewModel>();
```

### Add Navigation in MainWindowViewModel.cs
```csharp
[RelayCommand]
private async void ShowInventory()
{
    SelectedPage = "Inventory";
    CurrentPageTitle = await _localizationService.GetTranslationAsync("inventory_title") ?? "Inventory";
    
    var inventoryView = new InventoryView();
    var inventoryViewModel = _serviceProvider.GetRequiredService<InventoryViewModel>();
    inventoryView.DataContext = inventoryViewModel;
    
    CurrentView = inventoryView;
}
```

This comprehensive guide ensures that all new screens you create will be fully integrated with ChronoPos's advanced settings system, providing users with a consistent, customizable, and accessible experience across all screens.

## Quick Reference: Essential Settings Integration Checklist

### ✅ Before Creating Your Screen
- [ ] Plan translation keywords needed for your screen
- [ ] Identify which UI elements need RTL support
- [ ] Consider color scheme and theme dependencies
- [ ] Plan status messages and user feedback text
- [ ] **NEW**: Add your translation method to `LanguageSeedingService.cs`

### ✅ ViewModel Requirements
- [ ] Inject all settings services: `DatabaseLocalizationService`, `ThemeService`, `ColorSchemeService`, `LayoutDirectionService`, `ZoomService`
- [ ] Add `FlowDirection` property for RTL support
- [ ] Add `ZoomLevel` property for zoom level awareness
- [ ] Create translated properties for all user-visible text
- [ ] Subscribe to `LanguageChanged`, `LayoutDirectionChanged`, and `ZoomChanged` events
- [ ] Implement `LoadTranslationsAsync()` method (translation loading only - NO seeding!)
- [ ] **REMOVED**: ~~Add keywords to database using `LanguageManager`~~ (Now handled at startup)
- [ ] **REMOVED**: ~~Call `EnsureTranslationKeywordsAsync()`~~ (No longer needed)
- [ ] Provide fallback values for all translations
- [ ] Handle zoom-specific logic if needed (optional)

### ✅ XAML Requirements
- [ ] Set `FlowDirection="{Binding CurrentFlowDirection}"` on root element
- [ ] Use `{DynamicResource}` for all colors, fonts, and layout dimensions
- [ ] Bind all text to translated properties from ViewModel
- [ ] Use responsive font sizes: `FontSizeVerySmall`, `FontSizeSmall`, `FontSizeMedium`, `FontSizeLarge`
- [ ] Apply `PoppinsFontFallback` font family
- [ ] Use zoom-aware layout resources: `DefaultMarginThickness`, `DefaultPaddingThickness`, `ButtonHeight`
- [ ] Use direction-aware text alignment converters
- [ ] Implement proper theme-aware styles for all controls
- [ ] Ensure all interactive elements scale properly with zoom levels

### ✅ Registration and Navigation
- [ ] Register ViewModel in `App.xaml.cs` dependency injection
- [ ] Add navigation command in `MainWindowViewModel`
- [ ] Create navigation button with translated content
- [ ] Implement proper selection highlighting with `SelectedPage`
- [ ] Add navigation button to `MainWindow.xaml`

### ✅ NEW: Centralized Translation Setup
- [ ] Add translation method to `LanguageSeedingService.cs`
- [ ] Call your translation method in `SeedAllTranslationsAsync()`
- [ ] Use consistent naming convention for translation keys
- [ ] Include comprehensive translations (buttons, labels, status messages, tooltips)
- [ ] Test application startup to ensure translations are seeded properly

### ✅ Translation Keywords Template (For LanguageSeedingService)
```csharp
private async Task Seed[ScreenName]TranslationsAsync()
{
    var screenKeywords = new Dictionary<string, Dictionary<string, string>>
    {
        // Page structure
        { "[screen_name]_title", new() { { "en", "English Title" }, { "ur", "اردو عنوان" } } },
        { "[screen_name]_subtitle", new() { { "en", "Page Description" }, { "ur", "صفحہ کی تفصیل" } } },
        
        // Action buttons
        { "[screen_name]_search_button", new() { { "en", "🔍 Search" }, { "ur", "🔍 تلاش کریں" } } },
        { "[screen_name]_add_new_button", new() { { "en", "➕ Add New" }, { "ur", "➕ نیا شامل کریں" } } },
        { "[screen_name]_save_button", new() { { "en", "💾 Save" }, { "ur", "💾 محفوظ کریں" } } },
        { "[screen_name]_delete_button", new() { { "en", "🗑️ Delete" }, { "ur", "🗑️ ڈیلیٹ کریں" } } },
        { "[screen_name]_edit_button", new() { { "en", "✏️ Edit" }, { "ur", "✏️ ترمیم کریں" } } },
        { "[screen_name]_refresh_button", new() { { "en", "🔄 Refresh" }, { "ur", "🔄 ریفریش کریں" } } },
        
        // Column headers
        { "[screen_name]_column_name", new() { { "en", "Name" }, { "ur", "نام" } } },
        { "[screen_name]_column_status", new() { { "en", "Status" }, { "ur", "حالت" } } },
        { "[screen_name]_column_date", new() { { "en", "Date" }, { "ur", "تاریخ" } } },
        { "[screen_name]_column_amount", new() { { "en", "Amount" }, { "ur", "رقم" } } },
        { "[screen_name]_column_actions", new() { { "en", "Actions" }, { "ur", "عمل" } } },
        
        // Status messages
        { "[screen_name]_status_ready", new() { { "en", "Ready" }, { "ur", "تیار" } } },
        { "[screen_name]_status_loading", new() { { "en", "Loading..." }, { "ur", "لوڈ کر رہا ہے..." } } },
        { "[screen_name]_status_saving", new() { { "en", "Saving..." }, { "ur", "محفوظ کر رہا ہے..." } } },
        { "[screen_name]_status_saved", new() { { "en", "Saved successfully" }, { "ur", "کامیابی سے محفوظ ہوا" } } },
        { "[screen_name]_status_error", new() { { "en", "An error occurred" }, { "ur", "ایک خرابی ہوئی" } } },
        { "[screen_name]_status_deleted", new() { { "en", "Deleted successfully" }, { "ur", "کامیابی سے ڈیلیٹ ہوا" } } },
        
        // Form labels
        { "[screen_name]_label_name", new() { { "en", "Name:" }, { "ur", "نام:" } } },
        { "[screen_name]_label_description", new() { { "en", "Description:" }, { "ur", "تفصیل:" } } },
        { "[screen_name]_label_price", new() { { "en", "Price:" }, { "ur", "قیمت:" } } },
        { "[screen_name]_label_quantity", new() { { "en", "Quantity:" }, { "ur", "مقدار:" } } },
        
        // Placeholders
        { "[screen_name]_placeholder_search", new() { { "en", "Search..." }, { "ur", "تلاش کریں..." } } },
        { "[screen_name]_placeholder_enter_name", new() { { "en", "Enter name" }, { "ur", "نام درج کریں" } } },
        { "[screen_name]_placeholder_enter_amount", new() { { "en", "Enter amount" }, { "ur", "رقم درج کریں" } } },
        
        // Confirmations
        { "[screen_name]_confirm_delete", new() { { "en", "Are you sure you want to delete this item?" }, { "ur", "کیا آپ اس آئٹم کو ڈیلیٹ کرنا چاہتے ہیں؟" } } },
        { "[screen_name]_confirm_save", new() { { "en", "Save changes?" }, { "ur", "تبدیلیاں محفوظ کریں؟" } } },
        
        // Tooltips
        { "[screen_name]_tooltip_refresh", new() { { "en", "Refresh data" }, { "ur", "ڈیٹا ریفریش کریں" } } },
        { "[screen_name]_tooltip_settings", new() { { "en", "Open settings" }, { "ur", "سیٹنگز کھولیں" } } }
    };

    await SeedTranslationCategoryAsync("[ScreenName]", screenKeywords);
}

// Remember to add this method call to SeedAllTranslationsAsync():
// await Seed[ScreenName]TranslationsAsync();
```

### ✅ Essential XAML Template
```xml
<UserControl FlowDirection="{Binding CurrentFlowDirection}"
             Background="{DynamicResource CardBackground}">
    <ScrollViewer>
        <StackPanel Margin="20">
            
            <!-- Header -->
            <Border Background="{DynamicResource SurfaceBackground}" 
                    CornerRadius="8" Padding="20" Margin="0,0,0,20"
                    BorderBrush="{DynamicResource BorderLight}" BorderThickness="1">
                <TextBlock Text="{Binding Title}" 
                           FontSize="{DynamicResource FontSizeLarge}" 
                           FontWeight="Bold" 
                           FontFamily="{DynamicResource PoppinsFontFallback}"
                           Foreground="{DynamicResource TextPrimary}"
                           TextAlignment="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToTextAlignmentConverter}}"/>
            </Border>
            
            <!-- Your content here with proper settings integration -->
            
        </StackPanel>
    </ScrollViewer>
</UserControl>
```

### ✅ Common Resource Bindings
```xml
<!-- Backgrounds -->
Background="{DynamicResource MainBackground}"
Background="{DynamicResource CardBackground}"  
Background="{DynamicResource SurfaceBackground}"

<!-- Text Colors -->
Foreground="{DynamicResource TextPrimary}"
Foreground="{DynamicResource TextSecondary}"

<!-- Fonts -->
FontFamily="{DynamicResource PoppinsFontFallback}"
FontSize="{DynamicResource FontSizeSmall|Medium|Large}"

<!-- Borders -->
BorderBrush="{DynamicResource BorderLight}"

<!-- Accent Colors -->
Foreground="{DynamicResource Primary}"
Foreground="{DynamicResource Success|Error|Warning|Info}"

<!-- RTL Support -->
FlowDirection="{Binding CurrentFlowDirection}"
TextAlignment="{Binding CurrentFlowDirection, Converter={StaticResource FlowDirectionToTextAlignmentConverter}}"
```

By following this comprehensive guide, every new screen you create will seamlessly integrate with ChronoPos's advanced settings system, providing users with a consistent, accessible, and customizable experience that supports multiple languages, themes, color schemes, layout directions, and font preferences.

## 🔄 Migration from Old Language System (Important)

### If You Have Existing Screens Using Per-Screen Seeding

**Step 1: Remove Per-Screen Seeding Code**
```csharp
// REMOVE these from your existing ViewModels:

// ❌ Remove from constructor or initialization:
// await EnsureKeywordsExistAsync();
// await AddProductTranslations.EnsureTranslationKeywordsAsync(_databaseLocalizationService);

// ❌ Remove these methods entirely:
// public static async Task EnsureScreenKeywordsAsync(DatabaseLocalizationService localizationService)
// private async Task EnsureKeywordsExistAsync()

// ❌ Remove these calls from LoadTranslationsAsync:
// await _databaseLocalizationService.EnsureKeywordExistsAsync("keyword", "English", "اردو");
```

**Step 2: Move Translations to LanguageSeedingService**
```csharp
// Move your translation dictionaries from ViewModels to LanguageSeedingService.cs
// Change from per-screen seeding to centralized seeding
```

**Step 3: Update LoadTranslationsAsync**
```csharp
// Simplify your LoadTranslationsAsync method:
private async Task LoadTranslationsAsync()
{
    try
    {
        // Only load translations - no seeding!
        Title = await _databaseLocalizationService.GetTranslationAsync("screen_title") ?? "Default Title";
        SearchButtonText = await _databaseLocalizationService.GetTranslationAsync("search_button") ?? "🔍 Search";
        // ... other translations
    }
    catch (Exception ex)
    {
        // Log error and use fallback values
        System.Diagnostics.Debug.WriteLine($"Translation loading error: {ex.Message}");
    }
}
```

**Step 4: Update Translation Keys (If Needed)**
```csharp
// If your existing keys don't follow the new convention, update them:
// Old: "search_button" 
// New: "[screen_name]_search_button"

// Or keep using existing keys if they're already unique
```

### Benefits After Migration

**Before (Old System):**
- Each screen load: 200-500ms for keyword checking/seeding
- Database operations during navigation
- Potential for duplicate keywords across screens
- Slower screen transitions

**After (New System):**
- Application startup: One-time 2-3 second translation seeding
- Screen load: 10-50ms for translation retrieval only
- No duplicate operations
- Fast screen transitions

### Testing After Migration

1. **Test Application Startup**: Verify all translations load once at startup
2. **Test Screen Navigation**: Confirm fast loading without database operations
3. **Test Language Switching**: Verify all screens update immediately
4. **Test Missing Translations**: Confirm fallback values work correctly

## 🎯 Summary of New Language System

### What's New in 2025
- ✅ **Centralized seeding** via `LanguageSeedingService`
- ✅ **Application startup seeding** in `App.xaml.cs`
- ✅ **No per-screen keyword management**
- ✅ **Faster screen navigation**
- ✅ **Simplified development workflow**

### Development Workflow
1. **Create ViewModel** with translation properties
2. **Add translation method** to `LanguageSeedingService`
3. **Call translation method** in `SeedAllTranslationsAsync()`
4. **Bind XAML** to ViewModel properties
5. **Test** language switching functionality

### Key Files
- `LanguageSeedingService.cs` - Central translation management
- `App.xaml.cs` - Application startup seeding
- `[Screen]ViewModel.cs` - Translation property definitions
- `[Screen]View.xaml` - UI binding to translated properties

## Data Binding Best Practices

### ViewModel Properties
Use `[ObservableProperty]` attribute for automatic property change notification:

```csharp
[ObservableProperty]
private string _title = "Default Title";

// This generates:
// public string Title { get; set; }
// with INotifyPropertyChanged implementation
```

### Commands
Use `[RelayCommand]` for command implementation:

```csharp
[RelayCommand]
private async Task SaveAsync()
{
    // Command implementation
}

// This generates: public IAsyncRelayCommand SaveCommand { get; }
```

### Async Operations
Handle async operations properly in ViewModels:

```csharp
[RelayCommand]
private async Task LoadDataAsync()
{
    try
    {
        IsLoading = true;
        var data = await _service.GetDataAsync();
        Items = data;
    }
    catch (Exception ex)
    {
        // Handle error appropriately
        ErrorMessage = $"Failed to load data: {ex.Message}";
    }
    finally
    {
        IsLoading = false;
    }
}
```

## Common UI Patterns

### Loading States
```xml
<ProgressBar Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityConverter}}"
             IsIndeterminate="True"/>
```

### Error Display
```xml
<TextBlock Text="{Binding ErrorMessage}"
           Foreground="{DynamicResource Error}"
           Visibility="{Binding HasError, Converter={StaticResource BooleanToVisibilityConverter}}"/>
```

### Data Lists
```xml
<DataGrid ItemsSource="{Binding Items}"
          SelectedItem="{Binding SelectedItem}"
          AutoGenerateColumns="False"
          Background="{DynamicResource CardBackground}"
          Foreground="{DynamicResource TextPrimary}">
    <DataGrid.Columns>
        <DataGridTextColumn Header="Name" Binding="{Binding Name}"/>
        <DataGridTextColumn Header="Value" Binding="{Binding Value}"/>
    </DataGrid.Columns>
</DataGrid>
```

## Testing Your New Screen

1. **Build the solution**: `dotnet build`
2. **Run the application**: `dotnet run --project src\ChronoPos.Desktop\ChronoPos.Desktop.csproj`
3. **Test navigation**: Click your new navigation button
4. **Test theme switching**: Switch between light/dark themes to ensure proper styling
5. **Test functionality**: Verify all commands and data operations work correctly

## Troubleshooting Common Issues

### Dependency Injection Errors
- Ensure all services are registered in `App.xaml.cs`
- Check that interfaces and implementations are correctly mapped
- Verify constructor dependencies are properly injected

### Theme Issues
- Use `{DynamicResource}` instead of `{StaticResource}` for theme-aware elements
- Check that resource keys match those defined in theme files
- Ensure the view inherits from the correct base control

### Navigation Issues
- Verify the command is properly bound in MainWindow.xaml
- Check that the ViewModel is registered in DI container
- Ensure the View's DataContext is set correctly

### Data Binding Issues
- Check property names match between View and ViewModel
- Verify `[ObservableProperty]` attributes are used for bindable properties
- Use debugging tools to check binding errors in output window

## Example: Creating a Reports Screen

Here's a complete example of creating a Reports screen:

1. **Create ReportsViewModel.cs**:
```csharp
public partial class ReportsViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Sales Reports";
    
    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        // Implementation
    }
}
```

2. **Create ReportsView.xaml** with proper theme bindings
3. **Register in App.xaml.cs**: `services.AddTransient<ReportsViewModel>();`
4. **Add navigation in MainWindowViewModel.cs**
5. **Add button in MainWindow.xaml**

Following this guide ensures consistency, maintainability, and proper integration with the existing ChronoPos architecture.
