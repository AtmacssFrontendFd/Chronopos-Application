using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Domain.Entities;
using System.Collections.ObjectModel;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Settings page
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly IFontService _fontService;
    private readonly ILocalizationService _localizationService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;

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

    public SettingsViewModel(IThemeService themeService, IFontService fontService, 
                            ILocalizationService localizationService, IDatabaseLocalizationService databaseLocalizationService,
                            IColorSchemeService colorSchemeService, 
                            ILayoutDirectionService layoutDirectionService)
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
            
            Console.WriteLine("SettingsViewModel: Initializing settings");
            // Initialize with current settings
            InitializeSettings();
            Console.WriteLine("SettingsViewModel: Settings initialized");
            
            Console.WriteLine("SettingsViewModel: Loading available options");
            LoadAvailableOptions();
            Console.WriteLine("SettingsViewModel: Available options loaded");
            
            Console.WriteLine("SettingsViewModel: Constructor completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SettingsViewModel: Constructor error - {ex.Message}");
            Console.WriteLine($"SettingsViewModel: Constructor stack trace - {ex.StackTrace}");
            throw;
        }
    }

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
        _ = SetDatabaseLanguageAsync(AvailableLanguagesFromDb.FirstOrDefault(l => l.LanguageCode == "en"));
    }

    [RelayCommand]
    private void SetUrduLanguage()
    {
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
    }

    private void InitializeSettings()
    {
        UpdateThemeProperties(_themeService.CurrentTheme);
        UpdateFontProperties(_fontService.CurrentFontSize);
        UpdateLanguageProperties(_localizationService.CurrentLanguage);
        UpdatePrimaryColorProperties(_colorSchemeService.CurrentPrimaryColor);
        UpdateBackgroundColorProperties(_colorSchemeService.CurrentBackgroundColor);
        UpdateLayoutDirectionProperties(_layoutDirectionService.CurrentDirection);
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
}
