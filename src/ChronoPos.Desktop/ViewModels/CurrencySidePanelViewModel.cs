using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;

namespace ChronoPos.Desktop.ViewModels;

public partial class CurrencySidePanelViewModel : ObservableObject
{
    private readonly ICurrencyService _currencyService;
    private readonly Action<bool> _onSaved;
    private readonly Action _onCancelled;
    private CurrencyDto? _originalCurrency;
    private bool _isEditMode;

    [ObservableProperty]
    private string currencyName = string.Empty;

    [ObservableProperty]
    private string currencyCode = string.Empty;

    [ObservableProperty]
    private string symbol = string.Empty;

    [ObservableProperty]
    private string? imagePath;

    [ObservableProperty]
    private BitmapImage? imagePreview;

    [ObservableProperty]
    private bool hasImage = false;

    [ObservableProperty]
    private string exchangeRate = "1.0000";

    [ObservableProperty]
    private bool isDefault = false;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string formTitle = "Add Currency";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool canSave = true;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }
    public IRelayCommand SelectImageCommand { get; }
    public IRelayCommand RemoveImageCommand { get; }

    // Constructor for adding a new currency
    public CurrencySidePanelViewModel(
        ICurrencyService currencyService,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));
        
        _isEditMode = false;
        _originalCurrency = null;
        
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Close);
        CloseCommand = new RelayCommand(Close);
        SelectImageCommand = new RelayCommand(SelectImage);
        RemoveImageCommand = new RelayCommand(RemoveImage);
    }

    // Constructor for editing an existing currency
    public CurrencySidePanelViewModel(
        ICurrencyService currencyService,
        CurrencyDto originalCurrency,
        Action<bool> onSaved,
        Action onCancelled) : this(currencyService, onSaved, onCancelled)
    {
        _isEditMode = true;
        _originalCurrency = originalCurrency ?? throw new ArgumentNullException(nameof(originalCurrency));
        
        FormTitle = "Edit Currency";
        SaveButtonText = "Update";
        
        LoadForEdit(originalCurrency);
    }

    public void LoadForEdit(CurrencyDto currency)
    {
        CurrencyName = currency.CurrencyName;
        CurrencyCode = currency.CurrencyCode;
        Symbol = currency.Symbol;
        ExchangeRate = currency.ExchangeRate.ToString("F4", CultureInfo.InvariantCulture);
        IsDefault = currency.IsDefault;
        FormTitle = "Edit Currency";
        SaveButtonText = "Update";
        
        // Load image if available
        if (!string.IsNullOrEmpty(currency.ImagePath))
        {
            ImagePath = currency.ImagePath;
            LoadImagePreview(currency.ImagePath);
        }
    }

    private async Task SaveAsync()
    {
        if (!ValidateForm()) return;

        try
        {
            IsLoading = true;
            CanSave = false;
            ValidationMessage = string.Empty;

            if (_isEditMode && _originalCurrency != null)
            {
                await UpdateCurrency();
            }
            else
            {
                await CreateCurrency();
            }

            ValidationMessage = _isEditMode ? "Currency updated successfully!" : "Currency created successfully!";
            
            // Delay before closing to show success message
            await Task.Delay(1000);
            
            _onSaved.Invoke(true);
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error: {ex.Message}";
            CanSave = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateCurrency()
    {
        var createDto = new CreateCurrencyDto
        {
            CurrencyName = CurrencyName.Trim(),
            CurrencyCode = CurrencyCode.Trim().ToUpperInvariant(),
            Symbol = Symbol.Trim(),
            ImagePath = ImagePath,
            ExchangeRate = decimal.Parse(ExchangeRate, CultureInfo.InvariantCulture),
            IsDefault = IsDefault
        };

        await _currencyService.CreateAsync(createDto);
    }

    private async Task UpdateCurrency()
    {
        if (_originalCurrency == null) return;

        var updateDto = new UpdateCurrencyDto
        {
            CurrencyName = CurrencyName.Trim(),
            CurrencyCode = CurrencyCode.Trim().ToUpperInvariant(),
            Symbol = Symbol.Trim(),
            ImagePath = ImagePath,
            ExchangeRate = decimal.Parse(ExchangeRate, CultureInfo.InvariantCulture),
            IsDefault = IsDefault
        };

        await _currencyService.UpdateAsync(_originalCurrency.Id, updateDto);
    }

    private bool ValidateForm()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(CurrencyName))
        {
            ValidationMessage = "Currency name is required.";
            return false;
        }

        if (CurrencyName.Trim().Length < 2)
        {
            ValidationMessage = "Currency name must be at least 2 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(CurrencyCode))
        {
            ValidationMessage = "Currency code is required.";
            return false;
        }

        if (CurrencyCode.Trim().Length < 2 || CurrencyCode.Trim().Length > 10)
        {
            ValidationMessage = "Currency code must be between 2 and 10 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Symbol))
        {
            ValidationMessage = "Currency symbol is required.";
            return false;
        }

        if (Symbol.Trim().Length > 5)
        {
            ValidationMessage = "Currency symbol cannot exceed 5 characters.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ExchangeRate))
        {
            ValidationMessage = "Exchange rate is required.";
            return false;
        }

        if (!decimal.TryParse(ExchangeRate, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal rate))
        {
            ValidationMessage = "Exchange rate must be a valid decimal number.";
            return false;
        }

        if (rate <= 0)
        {
            ValidationMessage = "Exchange rate must be greater than zero.";
            return false;
        }

        if (rate < 0.0001m || rate > 999999.9999m)
        {
            ValidationMessage = "Exchange rate must be between 0.0001 and 999999.9999.";
            return false;
        }

        return true;
    }

    private void Close()
    {
        _onCancelled.Invoke();
    }

    private void SelectImage()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Currency Logo",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif|All Files|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                
                // Check file size (2MB limit)
                if (fileInfo.Length > 2 * 1024 * 1024)
                {
                    ValidationMessage = "Image file size must be less than 2MB.";
                    return;
                }

                // Get app data folder for storing images
                var appDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ChronoPos",
                    "CurrencyImages");
                
                // Create directory if it doesn't exist
                if (!Directory.Exists(appDataFolder))
                {
                    Directory.CreateDirectory(appDataFolder);
                }
                
                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(openFileDialog.FileName)}";
                var destinationPath = Path.Combine(appDataFolder, fileName);
                
                // Copy file to app data folder
                File.Copy(openFileDialog.FileName, destinationPath, true);
                
                // Store the path
                ImagePath = destinationPath;
                
                // Create preview
                LoadImagePreview(destinationPath);
                
                ValidationMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading image: {ex.Message}";
        }
    }

    private void RemoveImage()
    {
        // Delete the image file if it exists
        if (!string.IsNullOrEmpty(ImagePath) && File.Exists(ImagePath))
        {
            try
            {
                File.Delete(ImagePath);
            }
            catch
            {
                // Ignore deletion errors
            }
        }
        
        ImagePath = null;
        ImagePreview = null;
        HasImage = false;
        ValidationMessage = string.Empty;
    }

    private void LoadImagePreview(string imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                ImagePreview = null;
                HasImage = false;
                return;
            }

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(imagePath, UriKind.Absolute);
            image.EndInit();
            image.Freeze();

            ImagePreview = image;
            HasImage = true;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error displaying image: {ex.Message}";
            ImagePreview = null;
            HasImage = false;
        }
    }
}
