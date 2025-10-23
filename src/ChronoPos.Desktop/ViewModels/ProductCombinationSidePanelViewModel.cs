using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ChronoPos.Desktop.ViewModels;

public partial class ProductCombinationSidePanelViewModel : ObservableObject
{
    private readonly IProductCombinationItemService _combinationService;
    private readonly IProductUnitService _productUnitService;
    private readonly IProductAttributeService _attributeService;
    private readonly Func<bool, Task> _onSaved;
    private readonly Action _onCancelled;

    [ObservableProperty]
    private ObservableCollection<ProductUnitDto> productUnits = new();

    [ObservableProperty]
    private ObservableCollection<ProductAttributeDto> attributes = new();

    [ObservableProperty]
    private ObservableCollection<ProductAttributeValueDto> attributeValues = new();

    [ObservableProperty]
    private ObservableCollection<ProductAttributeValueDto> selectedAttributeValues = new();

    [ObservableProperty]
    private ProductUnitDto? selectedProductUnit;

    [ObservableProperty]
    private ProductAttributeDto? selectedAttribute;

    [ObservableProperty]
    private ProductAttributeValueDto? selectedAttributeValue;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string formTitle = "Add Product Combination";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool canSave = true;

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }
    public IRelayCommand AddAttributeValueCommand { get; }
    public IRelayCommand<ProductAttributeValueDto?> RemoveAttributeValueCommand { get; }

    public ProductCombinationSidePanelViewModel(
        IProductCombinationItemService combinationService,
        IProductUnitService productUnitService,
        IProductAttributeService attributeService,
        Func<bool, Task> onSaved,
        Action onCancelled)
    {
        _combinationService = combinationService ?? throw new ArgumentNullException(nameof(combinationService));
        _productUnitService = productUnitService ?? throw new ArgumentNullException(nameof(productUnitService));
        _attributeService = attributeService ?? throw new ArgumentNullException(nameof(attributeService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));
        
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Close);
        CloseCommand = new RelayCommand(Close);
        AddAttributeValueCommand = new RelayCommand(AddAttributeValue);
        RemoveAttributeValueCommand = new RelayCommand<ProductAttributeValueDto?>(RemoveAttributeValue);

        // Subscribe to property changes
        PropertyChanged += OnPropertyChanged;

        // Load data
        _ = LoadDataAsync();
    }

    private async void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedAttribute))
        {
            await LoadAttributeValuesAsync();
        }
        else if (e.PropertyName == nameof(SelectedProductUnit) || 
                 e.PropertyName == nameof(SelectedAttributeValues))
        {
            ValidateForm();
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            
            // Load both product units and attributes
            var productUnitsResult = await _productUnitService.GetAllAsync();
            var attributesResult = await _attributeService.GetAllAttributesAsync();

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Load Product Units
                ProductUnits.Clear();
                foreach (var productUnit in productUnitsResult)
                {
                    ProductUnits.Add(productUnit);
                }

                // Load Attributes
                Attributes.Clear();
                foreach (var attr in attributesResult)
                {
                    Attributes.Add(attr);
                }
            });
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadAttributeValuesAsync()
    {
        if (SelectedAttribute == null) return;

        try
        {
            var values = await _attributeService.GetValuesByAttributeIdAsync(SelectedAttribute.Id);
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                AttributeValues.Clear();
                foreach (var value in values)
                {
                    AttributeValues.Add(value);
                }
            });
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading attribute values: {ex.Message}";
        }
    }

    private void AddAttributeValue()
    {
        if (SelectedAttributeValue == null || 
            SelectedAttributeValues.Any(v => v.Id == SelectedAttributeValue.Id))
            return;

        SelectedAttributeValues.Add(SelectedAttributeValue);
        SelectedAttributeValue = null;
        ValidateForm();
    }

    private void RemoveAttributeValue(ProductAttributeValueDto? value)
    {
        if (value == null) return;
        
        SelectedAttributeValues.Remove(value);
        ValidateForm();
    }

    private void ValidateForm()
    {
        var errors = new List<string>();

        if (SelectedProductUnit == null)
            errors.Add("Please select a product unit");

        if (!SelectedAttributeValues.Any())
            errors.Add("Please select at least one attribute value");

        ValidationMessage = string.Join(Environment.NewLine, errors);
        CanSave = !errors.Any() && !IsLoading;
    }

    private async Task SaveAsync()
    {
        try
        {
            ValidateForm();
            if (!CanSave) return;

            IsLoading = true;
            ValidationMessage = "Saving combinations...";

            var successCount = 0;
            var totalCount = SelectedAttributeValues.Count;

            foreach (var attributeValue in SelectedAttributeValues)
            {
                try
                {
                    // Check if combination already exists
                    var existingCombinations = await _combinationService.GetCombinationItemsByProductUnitIdAsync(SelectedProductUnit!.Id);
                    var exists = existingCombinations.Any(c => c.AttributeValueId == attributeValue.Id);

                    if (!exists)
                    {
                        var createDto = new CreateProductCombinationItemDto
                        {
                            ProductUnitId = SelectedProductUnit.Id,
                            AttributeValueId = attributeValue.Id
                        };

                        await _combinationService.CreateCombinationItemAsync(createDto);
                        successCount++;
                    }
                }
                catch (Exception ex)
                {
                    // Log individual error but continue with other combinations
                    System.Diagnostics.Debug.WriteLine($"Error creating combination: {ex.Message}");
                }
            }

            if (successCount > 0)
            {
                ValidationMessage = $"Successfully created {successCount} of {totalCount} combinations";
                await Task.Delay(1000); // Show success message briefly
                await _onSaved(true);
            }
            else
            {
                ValidationMessage = "No new combinations were created (they may already exist)";
                await Task.Delay(2000); // Show message longer
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error saving combinations: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Close()
    {
        _onCancelled();
    }

    public void LoadForEdit(ProductCombinationItemDto combination)
    {
        FormTitle = "Edit Product Combination";
        SaveButtonText = "Update";
        
        // Set the selected values based on the combination to edit
        SelectedProductUnit = ProductUnits.FirstOrDefault(u => u.Id == combination.ProductUnitId);
        
        // Load the attribute and its values, then select the appropriate ones
        // This would require additional logic to find the attribute from the attribute value
        // For now, we'll just focus on the add functionality
    }

    /// <summary>
    /// Loads existing combinations for a product unit for editing
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Task</returns>
    public async Task LoadExistingCombinationsAsync(int productUnitId)
    {
        try
        {
            FormTitle = "Edit Product Combinations";
            SaveButtonText = "Update";

            System.Diagnostics.Debug.WriteLine($"Loading existing combinations for ProductUnit ID: {productUnitId}");

            // Get existing combinations for this product unit
            var existingCombinations = await _combinationService.GetCombinationItemsByProductUnitIdAsync(productUnitId);
            
            System.Diagnostics.Debug.WriteLine($"Found {existingCombinations.Count()} existing combinations");

            // Update UI on the UI thread
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                // First, ensure the ProductUnit is selected in the dropdown
                var targetProductUnit = ProductUnits.FirstOrDefault(pu => pu.Id == productUnitId);
                if (targetProductUnit != null)
                {
                    SelectedProductUnit = targetProductUnit;
                    System.Diagnostics.Debug.WriteLine($"Selected ProductUnit: {targetProductUnit.DisplayName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"ProductUnit with ID {productUnitId} not found in ProductUnits collection");
                }

                // Clear any previously selected values
                SelectedAttributeValues.Clear();
                
                // Do NOT pre-select any attribute - leave it empty
                SelectedAttribute = null;
                
                // Clear the attribute values dropdown since no attribute is selected
                AttributeValues.Clear();

                if (existingCombinations.Any())
                {
                    // Get all unique attribute values from the combinations and add them to SelectedAttributeValues
                    var allAttributes = await _attributeService.GetAllAttributesAsync();
                    
                    foreach (var combination in existingCombinations)
                    {
                        System.Diagnostics.Debug.WriteLine($"Processing combination with AttributeValue ID: {combination.AttributeValueId}");
                        
                        // Find which attribute this value belongs to
                        foreach (var attr in allAttributes)
                        {
                            var attrValues = await _attributeService.GetValuesByAttributeIdAsync(attr.Id);
                            var matchingValue = attrValues.FirstOrDefault(av => av.Id == combination.AttributeValueId);
                            
                            if (matchingValue != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"Found AttributeValue: {matchingValue.Value} in attribute: {attr.Name}");
                                
                                // Add to selected values if not already present
                                if (!SelectedAttributeValues.Any(sav => sav.Id == matchingValue.Id))
                                {
                                    SelectedAttributeValues.Add(matchingValue);
                                    System.Diagnostics.Debug.WriteLine($"Added to SelectedAttributeValues: {matchingValue.Value}");
                                }
                                break; // Found the value, no need to check other attributes
                            }
                        }
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Total selected attribute values for editing: {SelectedAttributeValues.Count}");
                    
                    // Debug: List all values in SelectedAttributeValues
                    foreach (var val in SelectedAttributeValues)
                    {
                        System.Diagnostics.Debug.WriteLine($"Selected Value: {val.Value} (ID: {val.Id})");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            // Handle error
            System.Diagnostics.Debug.WriteLine($"Error loading existing combinations: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}