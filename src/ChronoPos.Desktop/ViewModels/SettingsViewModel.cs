using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the Settings page
/// </summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly IFontService _fontService;

    [ObservableProperty]
    private bool _isDarkTheme;

    [ObservableProperty]
    private string _selectedTheme = "Light";

    [ObservableProperty]
    private string _selectedFontSize = "Medium";

    public SettingsViewModel(IThemeService themeService, IFontService fontService)
    {
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        
        // Subscribe to theme changes
        _themeService.ThemeChanged += OnThemeChanged;
        
        // Subscribe to font changes
        _fontService.FontChanged += OnFontChanged;
        
        // Initialize with current theme and font
        UpdateThemeProperties(_themeService.CurrentTheme);
        UpdateFontProperties(_fontService.CurrentFontSize);
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
    }

    private void OnThemeChanged(Theme newTheme)
    {
        UpdateThemeProperties(newTheme);
    }

    private void OnFontChanged(FontSize newFontSize)
    {
        UpdateFontProperties(newFontSize);
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
}
