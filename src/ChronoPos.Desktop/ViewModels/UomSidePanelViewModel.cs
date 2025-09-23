using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Services;
using System.Collections.ObjectModel;
using System.Windows;
using InfrastructureServices = ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the UOM sidepanel form with validation and CRUD operations
/// </summary>
public partial class UomSidePanelViewModel : ObservableObject, IDisposable
{
    #region Fields
    
    private readonly IUomService _uomService;
    private readonly IThemeService _themeService;
    private readonly ILocalizationService _localizationService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _databaseLocalizationService;
    
    private readonly Action<bool> _onSaved;
    private readonly Action _onCancelled;
    
    private UnitOfMeasurementDto? _originalUom;
    private bool _isEditMode;
    
    #endregion

    #region Observable Properties

    [ObservableProperty]
    private string formTitle = "Add Unit of Measurement";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private string loadingMessage = "Loading...";

    [ObservableProperty]
    private bool canSave = true;

    // Form Fields
    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string nameAr = string.Empty;

    [ObservableProperty]
    private string symbol = string.Empty;

    [ObservableProperty]
    private string type = "Base";

    [ObservableProperty]
    private string categoryTitle = string.Empty;

    [ObservableProperty]
    private long? baseUomId;

    [ObservableProperty]
    private decimal conversionFactor = 1;

    [ObservableProperty]
    private bool isActive = true;

    // Validation Properties
    [ObservableProperty]
    private string nameError = string.Empty;

    [ObservableProperty]
    private string symbolError = string.Empty;

    [ObservableProperty]
    private string typeError = string.Empty;

    [ObservableProperty]
    private string categoryError = string.Empty;

    [ObservableProperty]
    private string baseUomError = string.Empty;

    [ObservableProperty]
    private string conversionFactorError = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private string validationSummary = string.Empty;

    // Dropdown Data
    [ObservableProperty]
    private ObservableCollection<DropdownItem> uomTypes = new();

    [ObservableProperty]
    private ObservableCollection<DropdownItem> categories = new();

    [ObservableProperty]
    private ObservableCollection<DropdownItem> baseUnits = new();

    // Settings Properties
    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    // Computed Properties
    public bool HasNameError => !string.IsNullOrEmpty(NameError);
    public bool HasSymbolError => !string.IsNullOrEmpty(SymbolError);
    public bool HasTypeError => !string.IsNullOrEmpty(TypeError);
    public bool HasCategoryError => !string.IsNullOrEmpty(CategoryError);
    public bool HasBaseUomError => !string.IsNullOrEmpty(BaseUomError);
    public bool HasConversionFactorError => !string.IsNullOrEmpty(ConversionFactorError);
    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);
    public bool HasValidationErrors => HasNameError || HasSymbolError || HasTypeError || HasCategoryError || HasBaseUomError || HasConversionFactorError;
    
    public bool IsDerivedType => Type == "Derived";
    public bool ShowConversionExample => IsDerivedType && BaseUomId.HasValue && ConversionFactor > 0;

    public string ConversionExample
    {
        get
        {
            if (!ShowConversionExample) return string.Empty;
            
            var baseUnit = BaseUnits.FirstOrDefault(b => b.Value.ToString() == BaseUomId.ToString());
            if (baseUnit == null) return string.Empty;
            
            return $"1 {Symbol} = {ConversionFactor} {baseUnit.Display}";
        }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor for adding a new UOM
    /// </summary>
    public UomSidePanelViewModel(
        IUomService uomService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ILayoutDirectionService layoutDirectionService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _uomService = uomService ?? throw new ArgumentNullException(nameof(uomService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));
        
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));
        
        _isEditMode = false;
        _originalUom = null;
        
        InitializeSettings();
        InitializeDropdowns();
        
        // Subscribe to property changes for validation
        PropertyChanged += OnPropertyChanged;
    }

    /// <summary>
    /// Constructor for editing an existing UOM
    /// </summary>
    public UomSidePanelViewModel(
        IUomService uomService,
        IThemeService themeService,
        ILocalizationService localizationService,
        ILayoutDirectionService layoutDirectionService,
        InfrastructureServices.IDatabaseLocalizationService databaseLocalizationService,
        UnitOfMeasurementDto originalUom,
        Action<bool> onSaved,
        Action onCancelled) : this(uomService, themeService, localizationService, layoutDirectionService, databaseLocalizationService, onSaved, onCancelled)
    {
        _isEditMode = true;
        _originalUom = originalUom ?? throw new ArgumentNullException(nameof(originalUom));
        
        FormTitle = "Edit Unit of Measurement";
        SaveButtonText = "Update";
        
        LoadUomForEditing();
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task Save()
    {
        if (!ValidateForm()) return;

        try
        {
            IsLoading = true;
            LoadingMessage = _isEditMode ? "Updating unit of measurement..." : "Creating unit of measurement...";
            CanSave = false;

            if (_isEditMode && _originalUom != null)
            {
                await UpdateUom();
            }
            else
            {
                await CreateUom();
            }

            StatusMessage = _isEditMode ? "Unit updated successfully!" : "Unit created successfully!";
            
            // Delay before closing to show success message
            await Task.Delay(1000);
            
            _onSaved.Invoke(true);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            CanSave = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _onCancelled.Invoke();
    }

    [RelayCommand]
    private void Close()
    {
        _onCancelled.Invoke();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initialize current settings
    /// </summary>
    private void InitializeSettings()
    {
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
    }

    /// <summary>
    /// Initialize dropdown data
    /// </summary>
    private async void InitializeDropdowns()
    {
        try
        {
            // UOM Types
            UomTypes.Clear();
            UomTypes.Add(new DropdownItem { Value = "Base", Display = "Base Unit" });
            UomTypes.Add(new DropdownItem { Value = "Derived", Display = "Derived Unit" });

            // Categories
            Categories.Clear();
            Categories.Add(new DropdownItem { Value = "Weight", Display = "Weight" });
            Categories.Add(new DropdownItem { Value = "Length", Display = "Length" });
            Categories.Add(new DropdownItem { Value = "Volume", Display = "Volume" });
            Categories.Add(new DropdownItem { Value = "Area", Display = "Area" });
            Categories.Add(new DropdownItem { Value = "Time", Display = "Time" });
            Categories.Add(new DropdownItem { Value = "Quantity", Display = "Quantity/Count" });
            Categories.Add(new DropdownItem { Value = "Temperature", Display = "Temperature" });
            Categories.Add(new DropdownItem { Value = "Other", Display = "Other" });

            // Load base units from service
            await LoadBaseUnits();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading dropdown data: {ex.Message}";
        }
    }

    /// <summary>
    /// Load base units for derived unit selection
    /// </summary>
    private async Task LoadBaseUnits()
    {
        try
        {
            var allUoms = await _uomService.GetAllAsync();
            var baseUoms = allUoms.Where(u => u.Type == "Base" && u.IsActive).ToList();

            BaseUnits.Clear();
            foreach (var uom in baseUoms)
            {
                BaseUnits.Add(new DropdownItem 
                { 
                    Value = uom.Id, 
                    Display = $"{uom.Name} ({uom.Abbreviation})" 
                });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading base units: {ex.Message}";
        }
    }

    /// <summary>
    /// Load UOM data for editing
    /// </summary>
    private void LoadUomForEditing()
    {
        if (_originalUom == null) return;

        Name = _originalUom.Name;
        NameAr = _originalUom.Name ?? string.Empty; // Using Name as NameAr equivalent
        Symbol = _originalUom.Abbreviation ?? string.Empty;
        Type = _originalUom.Type;
        CategoryTitle = _originalUom.CategoryTitle ?? string.Empty;
        BaseUomId = _originalUom.BaseUomId;
        ConversionFactor = _originalUom.ConversionFactor ?? 1;
        IsActive = _originalUom.IsActive;
    }

    /// <summary>
    /// Create a new UOM
    /// </summary>
    private async Task CreateUom()
    {
        var createDto = new CreateUomDto
        {
            Name = Name.Trim(),
            // For now, we'll use the Name field for both Name and NameAr
            Abbreviation = Symbol.Trim(),
            Type = Type,
            CategoryTitle = CategoryTitle,
            BaseUomId = Type == "Derived" ? BaseUomId : null,
            ConversionFactor = Type == "Derived" ? ConversionFactor : null,
            IsActive = IsActive
        };

        await _uomService.CreateAsync(createDto);
    }

    /// <summary>
    /// Update an existing UOM
    /// </summary>
    private async Task UpdateUom()
    {
        if (_originalUom == null) return;

        var updateDto = new UpdateUomDto
        {
            Name = Name.Trim(),
            // For now, we'll use the Name field for both Name and NameAr
            Abbreviation = Symbol.Trim(),
            Type = Type,
            CategoryTitle = CategoryTitle,
            BaseUomId = Type == "Derived" ? BaseUomId : null,
            ConversionFactor = Type == "Derived" ? ConversionFactor : null,
            IsActive = IsActive
        };

        await _uomService.UpdateAsync(_originalUom.Id, updateDto);
    }

    /// <summary>
    /// Validate the form
    /// </summary>
    private bool ValidateForm()
    {
        bool isValid = true;
        var errors = new List<string>();

        // Clear previous errors
        NameError = string.Empty;
        SymbolError = string.Empty;
        TypeError = string.Empty;
        CategoryError = string.Empty;
        BaseUomError = string.Empty;
        ConversionFactorError = string.Empty;

        // Name validation
        if (string.IsNullOrWhiteSpace(Name))
        {
            NameError = "Name is required";
            errors.Add("Name");
            isValid = false;
        }
        else if (Name.Length < 2)
        {
            NameError = "Name must be at least 2 characters";
            errors.Add("Name");
            isValid = false;
        }

        // Symbol validation
        if (string.IsNullOrWhiteSpace(Symbol))
        {
            SymbolError = "Symbol is required";
            errors.Add("Symbol");
            isValid = false;
        }
        else if (Symbol.Length < 1 || Symbol.Length > 10)
        {
            SymbolError = "Symbol must be 1-10 characters";
            errors.Add("Symbol");
            isValid = false;
        }

        // Type validation
        if (string.IsNullOrWhiteSpace(Type))
        {
            TypeError = "Type is required";
            errors.Add("Type");
            isValid = false;
        }

        // Category validation
        if (string.IsNullOrWhiteSpace(CategoryTitle))
        {
            CategoryError = "Category is required";
            errors.Add("Category");
            isValid = false;
        }

        // Derived unit specific validations
        if (Type == "Derived")
        {
            if (!BaseUomId.HasValue)
            {
                BaseUomError = "Base unit is required for derived units";
                errors.Add("Base Unit");
                isValid = false;
            }

            if (ConversionFactor <= 0)
            {
                ConversionFactorError = "Conversion factor must be greater than 0";
                errors.Add("Conversion Factor");
                isValid = false;
            }
        }

        // Update validation summary
        ValidationSummary = errors.Count > 0 ? string.Join(", ", errors) : string.Empty;

        // Notify computed properties
        OnPropertyChanged(nameof(HasNameError));
        OnPropertyChanged(nameof(HasSymbolError));
        OnPropertyChanged(nameof(HasTypeError));
        OnPropertyChanged(nameof(HasCategoryError));
        OnPropertyChanged(nameof(HasBaseUomError));
        OnPropertyChanged(nameof(HasConversionFactorError));
        OnPropertyChanged(nameof(HasValidationErrors));

        return isValid;
    }

    /// <summary>
    /// Handle property changes for real-time validation and UI updates
    /// </summary>
    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(Type):
                OnPropertyChanged(nameof(IsDerivedType));
                OnPropertyChanged(nameof(ShowConversionExample));
                if (Type == "Base")
                {
                    BaseUomId = null;
                    ConversionFactor = 1;
                }
                break;

            case nameof(BaseUomId):
            case nameof(ConversionFactor):
                OnPropertyChanged(nameof(ShowConversionExample));
                OnPropertyChanged(nameof(ConversionExample));
                break;

            case nameof(Name):
                if (!string.IsNullOrWhiteSpace(NameError) && !string.IsNullOrWhiteSpace(Name))
                {
                    NameError = string.Empty;
                    OnPropertyChanged(nameof(HasNameError));
                }
                break;

            case nameof(Symbol):
                if (!string.IsNullOrWhiteSpace(SymbolError) && !string.IsNullOrWhiteSpace(Symbol))
                {
                    SymbolError = string.Empty;
                    OnPropertyChanged(nameof(HasSymbolError));
                }
                break;
        }
    }

    #endregion

    #region Dispose

    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
        GC.SuppressFinalize(this);
    }

    #endregion
}

/// <summary>
/// Helper class for dropdown items
/// </summary>
public class DropdownItem
{
    public object Value { get; set; } = null!;
    public string Display { get; set; } = string.Empty;
}