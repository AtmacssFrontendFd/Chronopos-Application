using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Domain.Entities;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Settings page with module navigation
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly IFontService _fontService;
    private readonly ILocalizationService _localizationService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IZoomService _zoomService;

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private string _selectedTheme = "Light";

    [ObservableProperty]
    private string _selectedFontSize = "Medium";

    [ObservableProperty]
    private string _selectedLanguage = "English";

    [ObservableProperty]
    private ColorOption _selectedPrimaryColor = new();

    [ObservableProperty]
    private ColorOption _selectedBackgroundColor = new();

    [ObservableProperty]
    private string _selectedLayoutDirection = "LeftToRight";

    [ObservableProperty]
    private ZoomLevel _selectedZoomLevel = ZoomLevel.Zoom100;

    [ObservableProperty]
    private string _selectedZoomLevelDisplay = "100% - Normal (Default)";

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private ObservableCollection<Language> _availableLanguagesFromDb = new();

    [ObservableProperty]
    private Language? _selectedLanguageFromDb;

    [ObservableProperty]
    private ObservableCollection<ColorOption> _availablePrimaryColors = new();

    [ObservableProperty]
    private ObservableCollection<ColorOption> _availableBackgroundColors = new();

    [ObservableProperty]
    private ObservableCollection<KeyValuePair<LayoutDirection, string>> _availableLayoutDirections = new();

    [ObservableProperty]
    private ObservableCollection<KeyValuePair<ZoomLevel, string>> _availableZoomLevels = new();

    [ObservableProperty]
    private ObservableCollection<SettingsModuleInfo> _modules = new();

    [ObservableProperty]
    private string _pageTitle = "Settings";

    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private string _currentView = "Modules"; // "Modules", "UserSettings", "ApplicationSettings"

    /// <summary>
    /// Navigation action for module navigation (set by parent)
    /// </summary>
    public Action<string>? NavigateToSettingsModuleAction { get; set; }

    /// <summary>
    /// Navigation action to go back (set by parent)
    /// </summary>
    public Action? NavigateBackAction { get; set; }

    public SettingsViewModel(IThemeService themeService, IFontService fontService, 
                            ILocalizationService localizationService, IDatabaseLocalizationService databaseLocalizationService,
                            IColorSchemeService colorSchemeService, 
                            ILayoutDirectionService layoutDirectionService,
                            IZoomService zoomService)
    {
        Console.WriteLine("SettingsViewModel: Constructor starting");
        
        try
        {
            Console.WriteLine("SettingsViewModel: Validating services");
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            Console.WriteLine("SettingsViewModel: ThemeService validated");
            _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
            Console.WriteLine("SettingsViewModel: FontService validated");
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            Console.WriteLine("SettingsViewModel: LocalizationService validated");
            _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
            Console.WriteLine("SettingsViewModel: DatabaseLocalizationService validated");
            _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
            Console.WriteLine("SettingsViewModel: ColorSchemeService validated");
            _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
            Console.WriteLine("SettingsViewModel: LayoutDirectionService validated");
            _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
            Console.WriteLine("SettingsViewModel: ZoomService validated");
            
            Console.WriteLine("SettingsViewModel: Subscribing to service events");
            // Subscribe to service events
            _themeService.ThemeChanged += OnThemeChanged;
            Console.WriteLine("SettingsViewModel: ThemeChanged event subscribed");
            _fontService.FontChanged += OnFontChanged;
            Console.WriteLine("SettingsViewModel: FontChanged event subscribed");
            _localizationService.LanguageChanged += OnLanguageChanged;
            Console.WriteLine("SettingsViewModel: LanguageChanged event subscribed");
            _databaseLocalizationService.LanguageChanged += OnDatabaseLanguageChanged;
            Console.WriteLine("SettingsViewModel: DatabaseLanguageChanged event subscribed");
            _colorSchemeService.PrimaryColorChanged += OnPrimaryColorChanged;
            Console.WriteLine("SettingsViewModel: PrimaryColorChanged event subscribed");
            _colorSchemeService.BackgroundColorChanged += OnBackgroundColorChanged;
            Console.WriteLine("SettingsViewModel: BackgroundColorChanged event subscribed");
            _layoutDirectionService.DirectionChanged += OnLayoutDirectionChanged;
            Console.WriteLine("SettingsViewModel: DirectionChanged event subscribed");
            _zoomService.ZoomChanged += OnZoomChanged;
            Console.WriteLine("SettingsViewModel: ZoomChanged event subscribed");
            
            Console.WriteLine("SettingsViewModel: Initializing settings");
            // Initialize with current settings - wrapped in try-catch to prevent errors
            SafelyInitializeSettings();
            Console.WriteLine("SettingsViewModel: Settings initialized");
            
            Console.WriteLine("SettingsViewModel: Loading available options");
            SafelyLoadAvailableOptions();
            Console.WriteLine("SettingsViewModel: Available options loaded");
            
            Console.WriteLine("SettingsViewModel: Loading settings modules");
            LoadSettingsModules();
            Console.WriteLine("SettingsViewModel: Settings modules loaded");
            
            Console.WriteLine("SettingsViewModel: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SettingsViewModel: Constructor error - {ex.Message}");
            Console.WriteLine($"SettingsViewModel: Constructor stack trace - {ex.StackTrace}");
            
            // Set safe default values if construction fails
            SetSafeDefaults();
            StatusMessage = "Settings loaded with defaults due to initialization error";
        }
    }

    #region Commands

    [RelayCommand]
    private void NavigateToSettingsModule(string moduleType)
    {
        Console.WriteLine($"Navigating to settings module: {moduleType}");
        CurrentView = moduleType;
        
        if (NavigateToSettingsModuleAction != null)
        {
            NavigateToSettingsModuleAction(moduleType);
        }
    }

    [RelayCommand]
    private void BackToModules()
    {
        CurrentView = "Modules";
    }

    [RelayCommand]
    private void GoBack()
    {
        Console.WriteLine("GoBack command triggered");
        NavigateBackAction?.Invoke();
    }

    [RelayCommand]
    private async Task RefreshModulesAsync()
    {
        LoadSettingsModules();
        await Task.CompletedTask;
    }

    #endregion

    #region Module Loading

    private void LoadSettingsModules()
    {
        try
        {
            Modules.Clear();

            var primaryColorBrush = GetPrimaryColorBrush();
            var buttonBackgroundBrush = GetButtonBackgroundBrush();

            // Add two setting modules
            Modules.Add(new SettingsModuleInfo
            {
                ModuleType = "UserSettings",
                Title = "User Settings",
                Description = "",
                IconBackground = primaryColorBrush,
                ButtonBackground = buttonBackgroundBrush
            });

            Modules.Add(new SettingsModuleInfo
            {
                ModuleType = "ApplicationSettings",
                Title = "Application Settings",
                Description = "",
                IconBackground = primaryColorBrush,
                ButtonBackground = buttonBackgroundBrush
            });

            Console.WriteLine($"Loaded {Modules.Count} settings modules");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading settings modules: {ex.Message}");
        }
    }

    private Brush GetPrimaryColorBrush()
    {
        try
        {
            var primaryColor = _colorSchemeService.CurrentPrimaryColor;
            return new SolidColorBrush(primaryColor.Color);
        }
        catch
        {
            return new SolidColorBrush(Color.FromRgb(225, 175, 35)); // Default gold
        }
    }

    private Brush GetButtonBackgroundBrush()
    {
        try
        {
            return SelectedTheme == "Dark"
                ? new SolidColorBrush(Color.FromRgb(45, 45, 45))
                : new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }
        catch
        {
            return new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }
    }

    #endregion

    [RelayCommand]
    private void ToggleTheme()
    {
        var newTheme = IsDarkTheme ? Theme.Light : Theme.Dark;
        _themeService.ChangeTheme(newTheme);
    }

    [RelayCommand]
    private void SetLightTheme()
    {
        _themeService.ChangeTheme(Theme.Light);
    }

    [RelayCommand]
    private void SetDarkTheme()
    {
        _themeService.ChangeTheme(Theme.Dark);
    }

    [RelayCommand]
    private void SetVerySmallFont()
    {
        _fontService.ChangeFontSize(FontSize.VerySmall);
    }

    [RelayCommand]
    private void SetSmallFont()
    {
        _fontService.ChangeFontSize(FontSize.Small);
    }

    [RelayCommand]
    private void SetMediumFont()
    {
        _fontService.ChangeFontSize(FontSize.Medium);
    }

    [RelayCommand]
    private void SetLargeFont()
    {
        _fontService.ChangeFontSize(FontSize.Large);
        StatusMessage = "Font size changed to Large";
    }

    // Database Language Commands
    [RelayCommand]
    private async Task SetDatabaseLanguageAsync(Language? language)
    {
        if (language != null)
        {
            await _databaseLocalizationService.SetCurrentLanguageAsync(language.LanguageCode);
            SelectedLanguageFromDb = language;
            StatusMessage = $"Language changed to {language.LanguageName}";
            
            // Also update the LocalizationService to coordinate both services
            var supportedLanguage = language.LanguageCode == "ur" ? SupportedLanguage.Urdu : SupportedLanguage.English;
            _localizationService.ChangeLanguage(supportedLanguage);
            
            // Update layout direction based on language
            if (language.IsRtl)
            {
                _layoutDirectionService.ChangeDirection(LayoutDirection.RightToLeft);
            }
            else
            {
                _layoutDirectionService.ChangeDirection(LayoutDirection.LeftToRight);
            }
        }
    }

    // Language Commands
    [RelayCommand]
    private void SetEnglishLanguage()
    {
        // Update LocalizationService first
        _localizationService.ChangeLanguage(SupportedLanguage.English);
        
        // Then update database language
        _ = SetDatabaseLanguageAsync(AvailableLanguagesFromDb.FirstOrDefault(l => l.LanguageCode == "en"));
    }

    [RelayCommand]
    private void SetUrduLanguage()
    {
        // Update LocalizationService first
        _localizationService.ChangeLanguage(SupportedLanguage.Urdu);
        
        // Then update database language
        _ = SetDatabaseLanguageAsync(AvailableLanguagesFromDb.FirstOrDefault(l => l.LanguageCode == "ur"));
    }

    // Color Scheme Commands
    [RelayCommand]
    private void SetPrimaryColor(ColorOption? colorOption)
    {
        if (colorOption != null)
        {
            _colorSchemeService.ChangePrimaryColor(colorOption);
            StatusMessage = $"Primary color changed to {colorOption.DisplayName}";
        }
    }

    [RelayCommand]
    private void SetBackgroundColor(ColorOption? colorOption)
    {
        if (colorOption != null)
        {
            _colorSchemeService.ChangeBackgroundColor(colorOption);
            StatusMessage = $"Background color changed to {colorOption.DisplayName}";
        }
    }

    // Layout Direction Commands
    [RelayCommand]
    private void SetLeftToRightDirection()
    {
        _layoutDirectionService.ChangeDirection(LayoutDirection.LeftToRight);
        StatusMessage = "Layout direction changed to Left-to-Right";
    }

    [RelayCommand]
    private void SetRightToLeftDirection()
    {
        _layoutDirectionService.ChangeDirection(LayoutDirection.RightToLeft);
        StatusMessage = "Layout direction changed to Right-to-Left";
    }

    // Zoom Commands
    [RelayCommand]
    private void SetZoomLevel(ZoomLevel zoomLevel)
    {
        _zoomService.ChangeZoomLevel(zoomLevel);
        StatusMessage = $"Zoom level changed to {(int)zoomLevel}%";
    }

    [RelayCommand]
    private void ZoomIn()
    {
        _zoomService.ZoomIn();
        StatusMessage = $"Zoomed in to {_zoomService.CurrentZoomPercentage}%";
    }

    [RelayCommand]
    private void ZoomOut()
    {
        _zoomService.ZoomOut();
        StatusMessage = $"Zoomed out to {_zoomService.CurrentZoomPercentage}%";
    }

    [RelayCommand]
    private void ResetZoom()
    {
        _zoomService.ResetZoom();
        StatusMessage = "Zoom reset to 100%";
    }

    // Save All Settings Command
    [RelayCommand]
    private async Task SaveAllSettingsAsync()
    {
        try
        {
            StatusMessage = "Saving settings...";
            
            // Force save all settings
            Properties.Settings.Default.Save();
            
            await Task.Delay(500); // Show saving message briefly
            
            StatusMessage = "All settings saved successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving settings: {ex.Message}";
        }
    }

    private void OnThemeChanged(Theme newTheme)
    {
        UpdateThemeProperties(newTheme);
    }

    private void OnFontChanged(FontSize newFontSize)
    {
        UpdateFontProperties(newFontSize);
    }

    private void OnLanguageChanged(SupportedLanguage newLanguage)
    {
        UpdateLanguageProperties(newLanguage);
    }

    private void OnDatabaseLanguageChanged(object? sender, string newLanguageCode)
    {
        // Update selected language based on code
        var language = AvailableLanguagesFromDb.FirstOrDefault(l => l.LanguageCode == newLanguageCode);
        if (language != null)
        {
            SelectedLanguageFromDb = language;
        }
    }

    private void OnPrimaryColorChanged(ColorOption newColor)
    {
        UpdatePrimaryColorProperties(newColor);
    }

    private void OnBackgroundColorChanged(ColorOption newColor)
    {
        UpdateBackgroundColorProperties(newColor);
    }

    private void OnLayoutDirectionChanged(LayoutDirection newDirection)
    {
        UpdateLayoutDirectionProperties(newDirection);
        CurrentFlowDirection = newDirection == LayoutDirection.RightToLeft
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        
        // Reload modules with updated direction
        LoadSettingsModules();
    }

    private void OnZoomChanged(ZoomLevel newZoomLevel)
    {
        UpdateZoomProperties(newZoomLevel);
    }

    private void SetSafeDefaults()
    {
        try
        {
            // Set safe default values
            IsDarkTheme = false;
            SelectedTheme = "Light";
            SelectedFontSize = "Medium";
            SelectedLanguage = "English";
            SelectedLayoutDirection = "LeftToRight";
            SelectedZoomLevel = ZoomLevel.Zoom100;
            SelectedZoomLevelDisplay = "100% - Normal (Default)";
            StatusMessage = "Settings loaded with safe defaults";
            
            // Initialize collections with safe defaults
            AvailableLanguagesFromDb.Clear();
            AvailablePrimaryColors.Clear();
            AvailableBackgroundColors.Clear();
            AvailableLayoutDirections.Clear();
            AvailableZoomLevels.Clear();
            
            // Add basic defaults
            AvailableZoomLevels.Add(new KeyValuePair<ZoomLevel, string>(ZoomLevel.Zoom100, "100% - Normal (Default)"));
            
            Console.WriteLine("Safe defaults set successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error setting safe defaults: {ex.Message}");
        }
    }

    private void SafelyInitializeSettings()
    {
        try
        {
            InitializeSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in InitializeSettings: {ex.Message}");
            SetSafeDefaults();
        }
    }

    private void SafelyLoadAvailableOptions()
    {
        try
        {
            LoadAvailableOptions();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadAvailableOptions: {ex.Message}");
            LoadSafeAvailableOptions();
        }
    }

    private void LoadSafeAvailableOptions()
    {
        try
        {
            Console.WriteLine("Loading safe available options...");
            
            // Load primary colors safely
            try
            {
                AvailablePrimaryColors.Clear();
                var primaryColors = _colorSchemeService?.GetAvailablePrimaryColors() ?? new List<ColorOption>();
                foreach (var color in primaryColors)
                {
                    AvailablePrimaryColors.Add(color);
                }
                
                if (AvailablePrimaryColors.Count == 0)
                {
                    // Add a default color if none available
                    AvailablePrimaryColors.Add(new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#E1AF23" });
                }
                
                SelectedPrimaryColor = AvailablePrimaryColors.FirstOrDefault() ?? new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#E1AF23" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading primary colors: {ex.Message}");
                SelectedPrimaryColor = new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#E1AF23" };
            }

            // Load background colors safely
            try
            {
                AvailableBackgroundColors.Clear();
                var backgroundColors = _colorSchemeService?.GetAvailableBackgroundColors() ?? new List<ColorOption>();
                foreach (var color in backgroundColors)
                {
                    AvailableBackgroundColors.Add(color);
                }
                
                if (AvailableBackgroundColors.Count == 0)
                {
                    // Add a default color if none available
                    AvailableBackgroundColors.Add(new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#FFFFFF" });
                }
                
                SelectedBackgroundColor = AvailableBackgroundColors.FirstOrDefault() ?? new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#FFFFFF" };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading background colors: {ex.Message}");
                SelectedBackgroundColor = new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#FFFFFF" };
            }

            // Load layout directions safely
            try
            {
                AvailableLayoutDirections.Clear();
                var layoutDirections = _layoutDirectionService?.GetAvailableDirections() ?? new List<KeyValuePair<LayoutDirection, string>>();
                foreach (var direction in layoutDirections)
                {
                    AvailableLayoutDirections.Add(direction);
                }
                
                if (AvailableLayoutDirections.Count == 0)
                {
                    // Add defaults if none available
                    AvailableLayoutDirections.Add(new KeyValuePair<LayoutDirection, string>(LayoutDirection.LeftToRight, "Left to Right (LTR)"));
                    AvailableLayoutDirections.Add(new KeyValuePair<LayoutDirection, string>(LayoutDirection.RightToLeft, "Right to Left (RTL)"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading layout directions: {ex.Message}");
            }

            // Load zoom levels safely
            try
            {
                AvailableZoomLevels.Clear();
                var zoomLevels = _zoomService?.GetAvailableZoomLevels() ?? new List<KeyValuePair<ZoomLevel, string>>();
                foreach (var zoomLevel in zoomLevels)
                {
                    AvailableZoomLevels.Add(zoomLevel);
                }
                
                if (AvailableZoomLevels.Count == 0)
                {
                    // Add default zoom levels if none available
                    AvailableZoomLevels.Add(new KeyValuePair<ZoomLevel, string>(ZoomLevel.Zoom50, "50% - Very Small"));
                    AvailableZoomLevels.Add(new KeyValuePair<ZoomLevel, string>(ZoomLevel.Zoom100, "100% - Normal (Default)"));
                    AvailableZoomLevels.Add(new KeyValuePair<ZoomLevel, string>(ZoomLevel.Zoom150, "150% - Maximum"));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading zoom levels: {ex.Message}");
            }

            // Load database languages asynchronously but safely
            _ = Task.Run(async () =>
            {
                try
                {
                    Console.WriteLine("Loading database languages asynchronously...");
                    var languages = await _databaseLocalizationService.GetAvailableLanguagesAsync();
                    
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        AvailableLanguagesFromDb.Clear();
                        foreach (var language in languages)
                        {
                            AvailableLanguagesFromDb.Add(language);
                        }

                        // Set current language
                        if (AvailableLanguagesFromDb.Count > 0)
                        {
                            var currentLanguage = _databaseLocalizationService.GetCurrentLanguageCode();
                            SelectedLanguageFromDb = AvailableLanguagesFromDb.FirstOrDefault(l => l.LanguageCode == currentLanguage) 
                                                   ?? AvailableLanguagesFromDb.FirstOrDefault();
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading database languages: {ex.Message}");
                    
                    // Add default languages if database loading fails
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        AvailableLanguagesFromDb.Clear();
                        AvailableLanguagesFromDb.Add(new ChronoPos.Domain.Entities.Language 
                        { 
                            Id = 1, 
                            LanguageName = "English", 
                            LanguageCode = "en", 
                            IsRtl = false 
                        });
                        AvailableLanguagesFromDb.Add(new ChronoPos.Domain.Entities.Language 
                        { 
                            Id = 2, 
                            LanguageName = "اردو", 
                            LanguageCode = "ur", 
                            IsRtl = true 
                        });
                        
                        SelectedLanguageFromDb = AvailableLanguagesFromDb.FirstOrDefault();
                    });
                }
            });

            Console.WriteLine("Safe available options loaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoadSafeAvailableOptions: {ex.Message}");
        }
    }

    private void InitializeSettings()
    {
        try
        {
            UpdateThemeProperties(_themeService.CurrentTheme);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating theme properties: {ex.Message}");
            SelectedTheme = "Light";
            IsDarkTheme = false;
        }
        
        try
        {
            UpdateFontProperties(_fontService.CurrentFontSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating font properties: {ex.Message}");
            SelectedFontSize = "Medium";
        }
        
        try
        {
            UpdateLanguageProperties(_localizationService.CurrentLanguage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating language properties: {ex.Message}");
            SelectedLanguage = "English";
        }
        
        try
        {
            UpdatePrimaryColorProperties(_colorSchemeService.CurrentPrimaryColor);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating primary color properties: {ex.Message}");
            SelectedPrimaryColor = new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#E1AF23" };
        }
        
        try
        {
            UpdateBackgroundColorProperties(_colorSchemeService.CurrentBackgroundColor);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating background color properties: {ex.Message}");
            SelectedBackgroundColor = new ColorOption { Name = "Default", DisplayName = "Default", HexValue = "#FFFFFF" };
        }
        
        try
        {
            UpdateLayoutDirectionProperties(_layoutDirectionService.CurrentDirection);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating layout direction properties: {ex.Message}");
            SelectedLayoutDirection = "LeftToRight";
        }
        
        try
        {
            UpdateZoomProperties(_zoomService.CurrentZoomLevel);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating zoom properties: {ex.Message}");
            SelectedZoomLevel = ZoomLevel.Zoom100;
            SelectedZoomLevelDisplay = "100% - Normal (Default)";
        }
    }

    private async void LoadAvailableOptions()
    {
        // Load available languages from database
        try
        {
            var languages = await _databaseLocalizationService.GetAvailableLanguagesAsync();
            AvailableLanguagesFromDb.Clear();
            foreach (var language in languages)
            {
                AvailableLanguagesFromDb.Add(language);
            }

            // Set current language
            var currentLanguage = await _databaseLocalizationService.GetCurrentLanguageAsync();
            SelectedLanguageFromDb = currentLanguage;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading languages from database: {ex.Message}");
        }

        // Load available primary colors
        AvailablePrimaryColors.Clear();
        foreach (var color in _colorSchemeService.GetAvailablePrimaryColors())
        {
            AvailablePrimaryColors.Add(color);
        }

        // Load available background colors
        AvailableBackgroundColors.Clear();
        foreach (var color in _colorSchemeService.GetAvailableBackgroundColors())
        {
            AvailableBackgroundColors.Add(color);
        }

        // Load available layout directions
        AvailableLayoutDirections.Clear();
        foreach (var direction in _layoutDirectionService.GetAvailableDirections())
        {
            AvailableLayoutDirections.Add(direction);
        }

        // Load available zoom levels
        AvailableZoomLevels.Clear();
        foreach (var zoomLevel in _zoomService.GetAvailableZoomLevels())
        {
            AvailableZoomLevels.Add(zoomLevel);
        }
    }

    private void UpdateThemeProperties(Theme theme)
    {
        IsDarkTheme = theme == Theme.Dark;
        SelectedTheme = theme.ToString();
    }

    private void UpdateFontProperties(FontSize fontSize)
    {
        SelectedFontSize = fontSize.ToString();
    }

    private void UpdateLanguageProperties(SupportedLanguage language)
    {
        SelectedLanguage = language.ToString();
    }

    private void UpdatePrimaryColorProperties(ColorOption color)
    {
        SelectedPrimaryColor = color;
    }

    private void UpdateBackgroundColorProperties(ColorOption color)
    {
        SelectedBackgroundColor = color;
    }

    private void UpdateLayoutDirectionProperties(LayoutDirection direction)
    {
        SelectedLayoutDirection = direction.ToString();
    }

    private void UpdateZoomProperties(ZoomLevel zoomLevel)
    {
        SelectedZoomLevel = zoomLevel;
        var zoomOption = AvailableZoomLevels.FirstOrDefault(z => z.Key == zoomLevel);
        SelectedZoomLevelDisplay = zoomOption.Value ?? $"{(int)zoomLevel}%";
    }

    /// <summary>
    /// Called when SelectedZoomLevel property changes
    /// This enables the ComboBox to actually change the zoom level
    /// </summary>
    partial void OnSelectedZoomLevelChanged(ZoomLevel value)
    {
        try
        {
            Console.WriteLine($"SelectedZoomLevel changed to: {value} ({(int)value}%)");
            
            // Apply the zoom level change through the zoom service
            _zoomService?.ChangeZoomLevel(value);
            
            // Update the display text
            var zoomOption = AvailableZoomLevels.FirstOrDefault(z => z.Key == value);
            SelectedZoomLevelDisplay = zoomOption.Value ?? $"{(int)value}%";
            
            StatusMessage = $"Zoom level changed to {(int)value}%";
            
            Console.WriteLine($"Zoom level successfully applied: {value} ({(int)value}%)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error changing zoom level: {ex.Message}");
            StatusMessage = $"Error changing zoom level: {ex.Message}";
        }
    }
}

/// <summary>
/// Model class for settings module information
/// </summary>
public partial class SettingsModuleInfo : ObservableObject
{
    [ObservableProperty]
    private string _moduleType = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private Brush _iconBackground = new SolidColorBrush(Colors.Blue);

    [ObservableProperty]
    private Brush _buttonBackground = new SolidColorBrush(Colors.White);
}
