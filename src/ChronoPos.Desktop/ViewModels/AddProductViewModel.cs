using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel.DataAnnotations;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for adding new products with comprehensive form validation
/// </summary>
public partial class AddProductViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly Action? _navigateBack;

    #region Observable Properties

    [ObservableProperty]
    private string code = string.Empty;

    [ObservableProperty]
    private int plu = 0;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private decimal price = 0;

    [ObservableProperty]
    private decimal cost = 0;

    [ObservableProperty]
    private decimal lastPurchasePrice = 0;

    [ObservableProperty]
    private decimal markup = 0;

    [ObservableProperty]
    private int? categoryId;

    [ObservableProperty]
    private int selectedUnitOfMeasurementId = 1; // Default to "Pieces"

    [ObservableProperty]
    private UnitOfMeasurementDto? selectedUnitOfMeasurement;

    [ObservableProperty]
    private bool isTaxInclusivePrice = true;

    [ObservableProperty]
    private decimal excise = 0;

    [ObservableProperty]
    private bool isDiscountAllowed = true;

    [ObservableProperty]
    private decimal maxDiscount = 100;

    [ObservableProperty]
    private bool isPriceChangeAllowed = true;

    [ObservableProperty]
    private bool isUsingSerialNumbers = false;

    [ObservableProperty]
    private bool isManufactureRequired = false;

    [ObservableProperty]
    private bool isService = false;

    [ObservableProperty]
    private bool isUsingDefaultQuantity = true;

    // Stock Control Properties (Enhanced)
    [ObservableProperty]
    private bool isStockTracked = true;

    [ObservableProperty]
    private bool allowNegativeStock = false;

    [ObservableProperty]
    private decimal initialStock = 0;

    [ObservableProperty]
    private decimal minimumStock = 0;

    [ObservableProperty]
    private decimal maximumStock = 0;

    [ObservableProperty]
    private decimal reorderLevel = 0;

    [ObservableProperty]
    private decimal reorderQuantity = 0;

    [ObservableProperty]
    private decimal averageCost = 0;

    [ObservableProperty]
    private int selectedStoreId = 1; // Default to store 1

    // Computed Properties for Stock Control
    public bool IsStockFieldsEnabled => IsStockTracked;
    public bool IsSerialNumbersEnabled => IsStockTracked && IsUsingSerialNumbers;

    // Validation Properties
    [ObservableProperty]
    private Dictionary<string, string> stockValidationErrors = new();

    [ObservableProperty]
    private int? ageRestriction;

    [ObservableProperty]
    private string color = "#FFC107";

    [ObservableProperty]
    private string imagePath = string.Empty;

    [ObservableProperty]
    private bool isEnabled = true;

    [ObservableProperty]
    private ObservableCollection<BarcodeItemViewModel> barcodes = new();

    [ObservableProperty]
    private ObservableCollection<string> comments = new();

    [ObservableProperty]
    private ObservableCollection<CategoryDto> categories = new();

    [ObservableProperty]
    private ObservableCollection<UnitOfMeasurementDto> unitsOfMeasurement = new();

    [ObservableProperty]
    private ObservableCollection<string> availableTaxes = new();

    [ObservableProperty]
    private ObservableCollection<StoreDto> availableStores = new();

    [ObservableProperty]
    private string newBarcode = string.Empty;

    [ObservableProperty]
    private string newComment = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string statusMessage = "Ready to create new product";

    [ObservableProperty]
    private bool hasValidationErrors = false;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string barcodeValidationMessage = string.Empty;

    [ObservableProperty]
    private bool hasBarcodeValidationError = false;

    // Navigation Properties for Sidebar
    [ObservableProperty]
    private string currentSection = "ProductInfo";

    // Category panel properties
    [ObservableProperty]
    private bool isCategoryPanelOpen = false;

    [ObservableProperty]
    private CategoryDto currentCategory = new();

    [ObservableProperty]
    private bool isCategoryEditMode = false;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> parentCategories = new();

    #endregion

    #region Barcode Management Classes

    public class BarcodeItemViewModel : ObservableObject
    {
        private string _value = string.Empty;
        private bool _isNew = true;
        private bool _isDeleted = false;

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        public bool IsDeleted
        {
            get => _isDeleted;
            set => SetProperty(ref _isDeleted, value);
        }

        public object? Id { get; set; }
    }

    #endregion

    #region Validation Classes

    public class ValidationResult
    {
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public bool IsValid => !Errors.Any();
    }

    #endregion

    #region Validation Properties

    public List<string> ValidationErrors { get; private set; } = new();

    #endregion

    #region Stock Validation Methods

    partial void OnIsStockTrackedChanged(bool value)
    {
        ValidateStockLevels();
        OnPropertyChanged(nameof(IsStockFieldsEnabled));
    }

    partial void OnInitialStockChanged(decimal value)
    {
        ValidateStockLevels();
    }

    partial void OnMinimumStockChanged(decimal value)
    {
        ValidateStockLevels();
    }

    partial void OnMaximumStockChanged(decimal value)
    {
        ValidateStockLevels();
    }

    partial void OnReorderLevelChanged(decimal value)
    {
        ValidateStockLevels();
    }

    partial void OnIsUsingSerialNumbersChanged(bool value)
    {
        ValidateStockLevels();
        OnPropertyChanged(nameof(IsSerialNumbersEnabled));
        
        if (value && InitialStock > 0)
        {
            InitialStock = 0;
            StatusMessage = "Initial stock set to 0 for serial number tracking";
        }
    }

    private void ValidateStockLevels()
    {
        StockValidationErrors.Clear();
        
        if (IsStockTracked)
        {
            // Validate minimum vs maximum
            if (MaximumStock > 0 && MinimumStock > MaximumStock)
            {
                StockValidationErrors["MinimumStock"] = "Minimum stock cannot exceed maximum stock";
            }
            
            // Validate reorder level
            if (ReorderLevel > 0 && MinimumStock > 0 && ReorderLevel < MinimumStock)
            {
                StockValidationErrors["ReorderLevel"] = "Reorder level should be at or above minimum stock";
            }
            
            // Validate initial stock
            if (InitialStock < 0)
            {
                StockValidationErrors["InitialStock"] = "Initial stock cannot be negative";
            }
            
            // Validate costs
            if (InitialStock > 0 && AverageCost <= 0)
            {
                StockValidationErrors["AverageCost"] = "Average cost should be specified when setting initial stock";
            }
            
            // Validate reorder quantity
            if (ReorderLevel > 0 && ReorderQuantity <= 0)
            {
                StockValidationErrors["ReorderQuantity"] = "Reorder quantity should be specified when reorder level is set";
            }
            
            // Serial number validation
            if (IsUsingSerialNumbers && InitialStock > 0)
            {
                StockValidationErrors["SerialNumbers"] = "Serial numbers must be entered individually. Initial stock will be set to 0.";
            }
        }
        
        OnPropertyChanged(nameof(StockValidationErrors));
        OnPropertyChanged(nameof(HasStockValidationErrors));
    }

    public bool HasStockValidationErrors => StockValidationErrors.Any();

    #endregion

    #region Constructor

    public AddProductViewModel(IProductService productService, Action? navigateBack = null)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _navigateBack = navigateBack;
        
        // Initialize with default values
        Code = GenerateNextCode();
        Name = "Test Product"; // Debug: Set a default name to see if binding works
        IsTaxInclusivePrice = true;
        IsEnabled = true;
        IsDiscountAllowed = true;
        MaxDiscount = 100;
        SelectedUnitOfMeasurementId = 1; // Default to "Pieces"
        Color = "#FFC107";
        
        _ = InitializeAsync();
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading data...";

            await LoadCategoriesAsync();
            await LoadStoresAsync();
            await LoadUnitsOfMeasurementAsync();

            StatusMessage = "Ready to create new product";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            MessageBox.Show($"Failed to load data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadCategoriesAsync()
    {
        var categoryList = await _productService.GetAllCategoriesAsync();
        Categories.Clear();
        ParentCategories.Clear();
        
        // Add "No Parent" option for parent categories
        ParentCategories.Add(new CategoryDto { Id = 0, Name = "No Parent Category", Description = "Top Level Category" });
        
        foreach (var category in categoryList)
        {
            Categories.Add(category);
            ParentCategories.Add(category);
        }
    }

    private async Task LoadStoresAsync()
    {
        // For now, add default stores. In a real app, this would come from a store service
        AvailableStores.Clear();
        AvailableStores.Add(new StoreDto { Id = 1, Name = "Main Store", IsDefault = true, IsActive = true });
        AvailableStores.Add(new StoreDto { Id = 2, Name = "Branch Store", IsDefault = false, IsActive = true });
        
        // Set default store
        SelectedStoreId = AvailableStores.FirstOrDefault(s => s.IsDefault)?.Id ?? 1;
        
        await Task.CompletedTask; // Placeholder for async operation
    }

    private async Task LoadUnitsOfMeasurementAsync()
    {
        var uomList = await _productService.GetAllUnitsOfMeasurementAsync();
        UnitsOfMeasurement.Clear();
        
        foreach (var uom in uomList)
        {
            UnitsOfMeasurement.Add(uom);
        }
        
        // Set default UOM to "Pieces" if available
        SelectedUnitOfMeasurement = UnitsOfMeasurement.FirstOrDefault(u => u.Id == 1) ?? UnitsOfMeasurement.FirstOrDefault();
        if (SelectedUnitOfMeasurement != null)
        {
            SelectedUnitOfMeasurementId = SelectedUnitOfMeasurement.Id;
        }
    }

    private string GenerateNextCode()
    {
        // Generate a simple auto-incrementing code
        // In a real application, this would query the database for the next available code
        return $"PROD{DateTime.Now:yyyyMMddHHmmss}";
    }

    #endregion

    #region Commands

    [RelayCommand]
    private void AddBarcode()
    {
        if (!CanAddBarcode())
        {
            return;
        }

        var trimmedBarcode = NewBarcode.Trim();
        var validation = ValidateBarcode(trimmedBarcode);
        
        if (!validation.IsValid)
        {
            BarcodeValidationMessage = string.Join(Environment.NewLine, validation.Errors);
            HasBarcodeValidationError = true;
            return;
        }

        var barcodeItem = new BarcodeItemViewModel 
        { 
            Value = trimmedBarcode,
            IsNew = true 
        };
        
        Barcodes.Add(barcodeItem);
        NewBarcode = string.Empty;
        BarcodeValidationMessage = string.Empty;
        HasBarcodeValidationError = false;
        
        StatusMessage = $"Barcode '{trimmedBarcode}' added successfully";
    }

    [RelayCommand]
    private void RemoveBarcode(BarcodeItemViewModel? barcode)
    {
        if (barcode != null)
        {
            Barcodes.Remove(barcode);
            StatusMessage = $"Barcode '{barcode.Value}' removed";
        }
    }

    [RelayCommand]
    private void GenerateBarcode()
    {
        try
        {
            // Generate different types of barcodes based on preference
            var generatedBarcode = GenerateUniqueBarcode();
            
            if (!string.IsNullOrEmpty(generatedBarcode))
            {
                // Check if it already exists
                if (!Barcodes.Any(b => b.Value.Equals(generatedBarcode, StringComparison.OrdinalIgnoreCase)))
                {
                    // Set the generated barcode in the input field so user can see it before adding
                    NewBarcode = generatedBarcode;
                    StatusMessage = $"Barcode '{generatedBarcode}' generated - click Add to include it";
                }
                else
                {
                    // Try again with a different pattern
                    GenerateBarcode();
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error generating barcode: {ex.Message}";
        }
    }

    private bool CanAddBarcode()
    {
        if (string.IsNullOrWhiteSpace(NewBarcode))
        {
            BarcodeValidationMessage = "Barcode cannot be empty";
            HasBarcodeValidationError = true;
            return false;
        }

        var trimmedBarcode = NewBarcode.Trim();
        if (Barcodes.Any(b => b.Value.Equals(trimmedBarcode, StringComparison.OrdinalIgnoreCase)))
        {
            BarcodeValidationMessage = "Barcode already exists in the list";
            HasBarcodeValidationError = true;
            return false;
        }

        BarcodeValidationMessage = string.Empty;
        HasBarcodeValidationError = false;
        return true;
    }

    private ValidationResult ValidateBarcode(string barcodeValue)
    {
        var result = new ValidationResult();
        
        // Check if empty
        if (string.IsNullOrWhiteSpace(barcodeValue))
        {
            result.Errors.Add("Barcode cannot be empty");
            return result;
        }

        // Check length (typical barcode lengths)
        if (barcodeValue.Length < 4 || barcodeValue.Length > 50)
        {
            result.Errors.Add("Barcode length should be between 4-50 characters");
        }

        // Check for valid characters (alphanumeric, hyphens, spaces)
        if (!IsValidBarcodeFormat(barcodeValue))
        {
            result.Errors.Add("Barcode contains invalid characters. Only letters, numbers, hyphens, and spaces are allowed");
        }

        return result;
    }

    private bool IsValidBarcodeFormat(string barcode)
    {
        // Allow alphanumeric characters, hyphens, and spaces
        return Regex.IsMatch(barcode, @"^[a-zA-Z0-9\-\s]+$");
    }

    [RelayCommand]
    private void AddComment()
    {
        if (!string.IsNullOrWhiteSpace(NewComment))
        {
            Comments.Add(NewComment);
            NewComment = string.Empty;
            StatusMessage = "Comment added successfully";
        }
    }

    [RelayCommand]
    private void RemoveComment(string comment)
    {
        if (!string.IsNullOrEmpty(comment))
        {
            Comments.Remove(comment);
            StatusMessage = "Comment removed";
        }
    }

    [RelayCommand]
    private void ChooseImage()
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Product Image",
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp;*.gif)|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var selectedFile = openFileDialog.FileName;
                
                // Validate file size (max 5MB)
                var fileInfo = new FileInfo(selectedFile);
                if (fileInfo.Length > 5 * 1024 * 1024)
                {
                    MessageBox.Show("Image file size cannot exceed 5MB. Please choose a smaller image.", 
                        "File Too Large", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Copy file to application images directory
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChronoPos", "Images");
                Directory.CreateDirectory(appDataPath);

                var fileName = $"product_{Code}_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(selectedFile)}";
                var destinationPath = Path.Combine(appDataPath, fileName);

                File.Copy(selectedFile, destinationPath, true);
                ImagePath = destinationPath;
                
                StatusMessage = "Product image updated successfully";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error selecting image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void RemoveImage()
    {
        ImagePath = string.Empty;
        StatusMessage = "Product image removed";
    }

    [RelayCommand]
    private async Task SaveProduct()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Validating product data...";

            if (!ValidateForm())
            {
                StatusMessage = "Please fix validation errors before saving";
                return;
            }

            StatusMessage = "Saving product...";

            var productDto = CreateProductDto();
            var savedProduct = await _productService.CreateProductAsync(productDto);

            StatusMessage = "Product saved successfully!";
            MessageBox.Show("Product created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Navigate back to product management
            _navigateBack?.Invoke();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving product: {ex.Message}";
            MessageBox.Show($"Failed to save product: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        var result = MessageBox.Show("Are you sure you want to cancel? All unsaved changes will be lost.", 
            "Confirm Cancel", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Navigate back to product management
            _navigateBack?.Invoke();
        }
    }

    [RelayCommand]
    private void ResetForm()
    {
        Code = GenerateNextCode();
        Name = string.Empty;
        Description = string.Empty;
        Price = 0;
        Cost = 0;
        Markup = 0;
        CategoryId = null;
        SelectedUnitOfMeasurementId = 1; // Reset to "Pieces"
        SelectedUnitOfMeasurement = UnitsOfMeasurement.FirstOrDefault(u => u.Id == 1) ?? UnitsOfMeasurement.FirstOrDefault();
        IsTaxInclusivePrice = true;
        Excise = 0;
        IsDiscountAllowed = true;
        MaxDiscount = 100;
        IsPriceChangeAllowed = true;
        IsUsingSerialNumbers = false;
        IsService = false;
        AgeRestriction = null;
        Color = "#FFC107";
        ImagePath = string.Empty;
        IsEnabled = true;
        
        Barcodes.Clear();
        Comments.Clear();
        NewBarcode = string.Empty;
        NewComment = string.Empty;
        
        ValidationErrors.Clear();
        HasValidationErrors = false;
        ValidationMessage = string.Empty;
        BarcodeValidationMessage = string.Empty;
        HasBarcodeValidationError = false;
        
        StatusMessage = "Form reset - ready for new product";
    }

    #endregion

    #region Category Panel Commands

    [RelayCommand]
    private void OpenAddCategoryPanel()
    {
        CurrentCategory = new CategoryDto { IsActive = true, DisplayOrder = 0 };
        IsCategoryEditMode = false;
        IsCategoryPanelOpen = true;
    }

    [RelayCommand]
    private void CloseCategoryPanel()
    {
        IsCategoryPanelOpen = false;
        CurrentCategory = new CategoryDto();
    }

    [RelayCommand]
    private async Task SaveCategory()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Saving category...";

            if (!ValidateCategoryForm())
            {
                StatusMessage = "Please fix category validation errors";
                return;
            }

            CategoryDto savedCategory;
            if (IsCategoryEditMode)
            {
                savedCategory = await _productService.UpdateCategoryAsync(CurrentCategory);
                StatusMessage = "Category updated successfully";
            }
            else
            {
                savedCategory = await _productService.CreateCategoryAsync(CurrentCategory);
                StatusMessage = "Category created successfully";
            }

            // Refresh categories list
            await LoadCategoriesAsync();
            
            // Select the newly created/updated category
            CategoryId = savedCategory.Id;

            // Close the panel
            IsCategoryPanelOpen = false;
            
            MessageBox.Show("Category saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error saving category: {ex.Message}";
            MessageBox.Show($"Failed to save category: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool ValidateCategoryForm()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(CurrentCategory.Name))
            errors.Add("Category name is required");
        
        if (CurrentCategory.Name.Length > 100)
            errors.Add("Category name cannot exceed 100 characters");

        if (!string.IsNullOrWhiteSpace(CurrentCategory.Description) && CurrentCategory.Description.Length > 500)
            errors.Add("Category description cannot exceed 500 characters");

        if (!string.IsNullOrWhiteSpace(CurrentCategory.NameArabic) && CurrentCategory.NameArabic.Length > 100)
            errors.Add("Category name (Arabic) cannot exceed 100 characters");

        if (CurrentCategory.DisplayOrder < 0)
            errors.Add("Display order cannot be negative");

        if (errors.Any())
        {
            ValidationMessage = string.Join(Environment.NewLine, errors);
            HasValidationErrors = true;
            return false;
        }

        HasValidationErrors = false;
        return true;
    }

    #endregion

    #region Validation

    private bool ValidateForm()
    {
        ValidationErrors.Clear();

        // Required field validation
        if (string.IsNullOrWhiteSpace(Code))
            ValidationErrors.Add("Product code is required");
        else if (Code.Length > 50)
            ValidationErrors.Add("Product code cannot exceed 50 characters");

        if (string.IsNullOrWhiteSpace(Name))
            ValidationErrors.Add("Product name is required");
        else if (Name.Length > 100)
            ValidationErrors.Add("Product name cannot exceed 100 characters");

        if (Price < 0)
            ValidationErrors.Add("Price cannot be negative");

        if (Cost < 0)
            ValidationErrors.Add("Cost cannot be negative");

        if (MaxDiscount < 0 || MaxDiscount > 100)
            ValidationErrors.Add("Max discount must be between 0 and 100");

        if (AgeRestriction.HasValue && (AgeRestriction < 0 || AgeRestriction > 150))
            ValidationErrors.Add("Age restriction must be between 0 and 150");

        // Business rule validation
        if (Cost > 0 && Price > 0 && Price < Cost)
            ValidationErrors.Add("Warning: Selling price is lower than cost price");

        // Update validation status
        HasValidationErrors = ValidationErrors.Any();
        ValidationMessage = HasValidationErrors 
            ? string.Join(Environment.NewLine, ValidationErrors)
            : "All validations passed";

        return !HasValidationErrors;
    }

    #endregion

    #region Helper Methods

    private ProductDto CreateProductDto()
    {
        // Calculate markup if both cost and price are provided
        var calculatedMarkup = Markup;
        if (Cost > 0 && Price > Cost)
        {
            calculatedMarkup = ((Price - Cost) / Cost) * 100;
        }

        var productDto = new ProductDto
        {
            Name = Name,
            Description = Description ?? string.Empty,
            SKU = Code,
            Price = Price,
            CategoryId = CategoryId ?? 1, // Default category if none selected
            StockQuantity = 0, // New products start with 0 stock
            IsActive = IsEnabled,
            CostPrice = Cost,
            Markup = calculatedMarkup,
            ImagePath = ImagePath,
            Color = Color,
            // Stock Control Properties
            IsStockTracked = IsStockTracked,
            AllowNegativeStock = AllowNegativeStock,
            IsUsingSerialNumbers = IsUsingSerialNumbers,
            InitialStock = IsUsingSerialNumbers ? 0 : InitialStock, // Serial number products start with 0
            MinimumStock = MinimumStock,
            MaximumStock = MaximumStock,
            ReorderLevel = ReorderLevel,
            ReorderQuantity = ReorderQuantity,
            AverageCost = AverageCost,
            LastCost = AverageCost, // Set last cost same as average cost initially
            // UOM Properties
            UnitOfMeasurementId = SelectedUnitOfMeasurementId > 0 ? SelectedUnitOfMeasurementId : 1, // Default to "Pieces"
            UnitOfMeasurementName = SelectedUnitOfMeasurement?.Name ?? "Pieces",
            UnitOfMeasurementAbbreviation = SelectedUnitOfMeasurement?.Abbreviation ?? "pcs",
            SelectedStoreId = SelectedStoreId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return productDto;
    }

    private string GenerateUniqueBarcode()
    {
        var random = new Random();
        var barcodeTypes = new string[] { "EAN13", "CODE128", "CUSTOM" };
        var selectedType = barcodeTypes[random.Next(barcodeTypes.Length)];

        return selectedType switch
        {
            "EAN13" => GenerateEAN13Barcode(),
            "CODE128" => GenerateCode128Barcode(),
            "CUSTOM" => GenerateCustomBarcode(),
            _ => GenerateCustomBarcode()
        };
    }

    private string GenerateEAN13Barcode()
    {
        // Generate EAN-13 format: 13 digits
        var random = new Random();
        var digits = new int[12]; // First 12 digits, 13th is check digit
        
        // Country code (2-3 digits) - using 123 as example
        digits[0] = 1;
        digits[1] = 2;
        digits[2] = 3;
        
        // Manufacturer code and product code (9 digits)
        for (int i = 3; i < 12; i++)
        {
            digits[i] = random.Next(0, 10);
        }
        
        // Calculate check digit
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            sum += digits[i] * (i % 2 == 0 ? 1 : 3);
        }
        int checkDigit = (10 - (sum % 10)) % 10;
        
        return string.Join("", digits) + checkDigit;
    }

    private string GenerateCode128Barcode()
    {
        // Generate CODE-128 format: Alphanumeric
        var random = new Random();
        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var length = random.Next(8, 16); // Variable length
        
        var barcode = new System.Text.StringBuilder();
        for (int i = 0; i < length; i++)
        {
            barcode.Append(chars[random.Next(chars.Length)]);
        }
        
        return barcode.ToString();
    }

    private string GenerateCustomBarcode()
    {
        // Generate custom format based on product info
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var productPrefix = !string.IsNullOrEmpty(Name) ? Name.Substring(0, Math.Min(3, Name.Length)).ToUpper() : "PRD";
        var random = new Random().Next(100, 999);
        
        return $"{productPrefix}{timestamp}{random}";
    }

    #endregion

    #region Property Change Handlers

    partial void OnPriceChanged(decimal value)
    {
        if (Cost > 0 && value > Cost)
        {
            Markup = ((value - Cost) / Cost) * 100;
        }
        ValidateForm();
    }

    partial void OnCostChanged(decimal value)
    {
        if (Price > 0 && value > 0 && Price > value)
        {
            Markup = ((Price - value) / value) * 100;
        }
        ValidateForm();
    }

    partial void OnCodeChanged(string value)
    {
        ValidateForm();
    }

    partial void OnNameChanged(string value)
    {
        ValidateForm();
    }

    #endregion
}
