using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ChronoPos.Desktop.ViewModels;

public partial class CategorySidePanelViewModel : ObservableObject
{
    private readonly IProductService _productService;
    private readonly IDiscountService _discountService;
    private readonly Action<bool> _onSaved;
    private readonly Action _onCancelled;
    private CategoryDto? _originalCategory;
    private CategoryDto? _parentCategory;
    private bool _isEditMode;

    [ObservableProperty]
    private string name = string.Empty;

    [ObservableProperty]
    private string nameArabic = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private bool isActive = true;

    [ObservableProperty]
    private CategoryDto? selectedParentCategory;

    [ObservableProperty]
    private ObservableCollection<CategoryDto> availableParentCategories = new();

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private string formTitle = "Add Category";

    [ObservableProperty]
    private string saveButtonText = "Save";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool canSave = true;

    [ObservableProperty]
    private bool showParentSelection = true;

    // Discount-related properties
    [ObservableProperty]
    private ObservableCollection<DiscountDto> availableDiscounts = new();

    [ObservableProperty]
    private ObservableCollection<DiscountDto> selectedDiscounts = new();

    [ObservableProperty]
    private DiscountDto? selectedDiscount;

    [ObservableProperty]
    private string discountsLabel = "Available Discounts";

    [ObservableProperty]
    private string selectedDiscountsLabel = "Applied Discounts";

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }
    public IAsyncRelayCommand AddSelectedDiscountCommand { get; }
    public IRelayCommand<DiscountDto> RemoveDiscountCommand { get; }

    // Constructor for new category
    public CategorySidePanelViewModel(
        IProductService productService,
        IDiscountService discountService,
        Action<bool> onSaved,
        Action onCancelled)
        : this(productService, discountService, null, null, onSaved, onCancelled)
    {
    }

    // Constructor for editing existing category
    public CategorySidePanelViewModel(
        IProductService productService,
        IDiscountService discountService,
        CategoryDto? categoryToEdit,
        Action<bool> onSaved,
        Action onCancelled)
        : this(productService, discountService, categoryToEdit, null, onSaved, onCancelled)
    {
    }

    // Main constructor
    public CategorySidePanelViewModel(
        IProductService productService,
        IDiscountService discountService,
        CategoryDto? categoryToEdit,
        CategoryDto? parentCategory,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));
        _originalCategory = categoryToEdit;
        _parentCategory = parentCategory;
        _isEditMode = categoryToEdit != null;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanExecuteSave);
        CancelCommand = new RelayCommand(Cancel);
        CloseCommand = new RelayCommand(Cancel);
        AddSelectedDiscountCommand = new AsyncRelayCommand(AddSelectedDiscountAsync);
        RemoveDiscountCommand = new RelayCommand<DiscountDto>(RemoveDiscount);

        // Initialize form
        InitializeForm();
        
        // Load parent categories and discounts
        _ = LoadParentCategoriesAsync();
        _ = LoadDiscountsAsync();
    }

    private void InitializeForm()
    {
        if (_isEditMode && _originalCategory != null)
        {
            // Edit mode
            Name = _originalCategory.Name;
            NameArabic = _originalCategory.NameArabic;
            Description = _originalCategory.Description;
            IsActive = _originalCategory.IsActive;
            FormTitle = $"Edit Category - {_originalCategory.Name}";
            SaveButtonText = "Update";
            
            // Load existing discounts
            _ = LoadExistingDiscountsAsync(_originalCategory.Id);
        }
        else if (_parentCategory != null)
        {
            // Adding subcategory
            FormTitle = $"Add Subcategory under {_parentCategory.Name}";
            SaveButtonText = "Add";
            ShowParentSelection = false;
        }
        else
        {
            // Adding new root category
            FormTitle = "Add Category";
            SaveButtonText = "Add";
        }

        PropertyChanged += OnPropertyChanged;
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Name))
        {
            ValidateForm();
            SaveCommand.NotifyCanExecuteChanged();
        }
    }

    private async Task LoadParentCategoriesAsync()
    {
        try
        {
            IsLoading = true;
            var allCategories = await _productService.GetAllCategoriesAsync();
            
            // Filter out the current category and its descendants to prevent circular references
            var availableParents = allCategories.Where(c => 
                !_isEditMode || 
                (c.Id != _originalCategory!.Id && !IsDescendantOf(c, _originalCategory!.Id))
            ).ToList();

            AvailableParentCategories.Clear();
            
            // Add "No Parent" option
            AvailableParentCategories.Add(new CategoryDto { Id = 0, Name = "No Parent (Root Category)" });
            
            foreach (var category in availableParents.OrderBy(c => c.Name))
            {
                AvailableParentCategories.Add(category);
            }

            // Set selected parent
            if (_parentCategory != null)
            {
                SelectedParentCategory = AvailableParentCategories.FirstOrDefault(c => c.Id == _parentCategory.Id);
            }
            else if (_isEditMode && _originalCategory?.ParentCategoryId != null)
            {
                SelectedParentCategory = AvailableParentCategories.FirstOrDefault(c => c.Id == _originalCategory.ParentCategoryId);
            }
            else
            {
                SelectedParentCategory = AvailableParentCategories.FirstOrDefault(); // "No Parent"
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading parent categories: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool IsDescendantOf(CategoryDto category, int ancestorId)
    {
        // Simple check - in a real implementation, you'd need to traverse the hierarchy
        return category.ParentCategoryId == ancestorId;
    }

    private void ValidateForm()
    {
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Name))
        {
            ValidationMessage = "Category name is required.";
            CanSave = false;
            return;
        }

        if (Name.Length > 100)
        {
            ValidationMessage = "Category name cannot exceed 100 characters.";
            CanSave = false;
            return;
        }

        if (!string.IsNullOrWhiteSpace(Description) && Description.Length > 500)
        {
            ValidationMessage = "Description cannot exceed 500 characters.";
            CanSave = false;
            return;
        }

        CanSave = true;
    }

    private bool CanExecuteSave()
    {
        return CanSave && !IsLoading && !string.IsNullOrWhiteSpace(Name);
    }

    private async Task SaveAsync()
    {
        try
        {
            FileLogger.LogSeparator("CATEGORY SAVE OPERATION START");
            FileLogger.Log($"Category Save - Edit Mode: {_isEditMode}");
            FileLogger.Log($"Category Save - Name: '{Name}'");
            FileLogger.Log($"Category Save - NameArabic: '{NameArabic}'");
            FileLogger.Log($"Category Save - Description: '{Description}'");
            FileLogger.Log($"Category Save - IsActive: {IsActive}");
            FileLogger.Log($"Category Save - ParentCategory: {SelectedParentCategory?.Name ?? "None"}");
            FileLogger.Log($"Category Save - Selected Discounts Count: {SelectedDiscounts.Count}");
            FileLogger.Log($"Category Save - Selected Discount IDs: [{string.Join(", ", SelectedDiscounts.Select(d => d.Id))}]");

            ValidateForm();
            if (!CanSave)
            {
                FileLogger.Log("Category Save - Validation failed, cannot save");
                return;
            }

            IsLoading = true;
            ValidationMessage = string.Empty;

            var categoryDto = new CategoryDto
            {
                Id = _isEditMode ? _originalCategory!.Id : 0,
                Name = Name.Trim(),
                NameArabic = NameArabic?.Trim() ?? string.Empty,
                Description = Description?.Trim() ?? string.Empty,
                IsActive = IsActive,
                ParentCategoryId = SelectedParentCategory?.Id == 0 ? null : SelectedParentCategory?.Id,
                SelectedDiscountIds = SelectedDiscounts.Select(d => d.Id).ToList()
            };

            if (_parentCategory != null)
            {
                categoryDto.ParentCategoryId = _parentCategory.Id;
            }

            FileLogger.Log($"Category DTO Created - ID: {categoryDto.Id}");
            FileLogger.Log($"Category DTO Created - NameArabic: '{categoryDto.NameArabic}'");
            FileLogger.Log($"Category DTO Created - SelectedDiscountIds: [{string.Join(", ", categoryDto.SelectedDiscountIds)}]");

            CategoryDto? savedCategory;
            if (_isEditMode)
            {
                FileLogger.Log($"Calling UpdateCategoryAsync for ID: {categoryDto.Id}");
                savedCategory = await _productService.UpdateCategoryAsync(categoryDto);
            }
            else
            {
                FileLogger.Log("Calling CreateCategoryAsync");
                savedCategory = await _productService.CreateCategoryAsync(categoryDto);
            }

            if (savedCategory != null)
            {
                FileLogger.Log($"Category saved successfully - ID: {savedCategory.Id}");
                FileLogger.Log($"Saved Category NameArabic: '{savedCategory.NameArabic}'");
                FileLogger.Log($"Saved Category SelectedDiscountIds: [{string.Join(", ", savedCategory.SelectedDiscountIds ?? new List<int>())}]");
                _onSaved(true);
            }
            else
            {
                FileLogger.Log("Category save returned null - operation failed");
                ValidationMessage = "Failed to save category. Please try again.";
                _onSaved(false);
            }
        }
        catch (Exception ex)
        {
            FileLogger.Log($"ERROR in Category Save: {ex.Message}");
            FileLogger.Log($"ERROR Stack Trace: {ex.StackTrace}");
            ValidationMessage = $"Error saving category: {ex.Message}";
            _onSaved(false);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Cancel()
    {
        _onCancelled();
    }

    #region Discount Management

    private async Task LoadDiscountsAsync()
    {
        try
        {
            var discounts = await _discountService.GetActiveDiscountsAsync();
            AvailableDiscounts.Clear();
            
            foreach (var discount in discounts)
            {
                AvailableDiscounts.Add(discount);
            }

            FilterAvailableDiscounts();
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading discounts: {ex.Message}";
        }
    }

    private Task AddSelectedDiscountAsync()
    {
        try
        {
            if (SelectedDiscount == null)
                return Task.CompletedTask;

            // Check if discount is already selected
            if (SelectedDiscounts.Any(d => d.Id == SelectedDiscount.Id))
                return Task.CompletedTask;

            SelectedDiscounts.Add(SelectedDiscount);
            FilterAvailableDiscounts();

            // Clear selection
            SelectedDiscount = null;
            
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error adding discount: {ex.Message}";
            return Task.CompletedTask;
        }
    }

    private void RemoveDiscount(DiscountDto? discount)
    {
        if (discount == null)
            return;

        SelectedDiscounts.Remove(discount);
        FilterAvailableDiscounts();
    }

    private void FilterAvailableDiscounts()
    {
        try
        {
            var allDiscounts = AvailableDiscounts.ToList();
            AvailableDiscounts.Clear();

            foreach (var discount in allDiscounts)
            {
                // Only show discounts that are not already selected
                if (!SelectedDiscounts.Any(sd => sd.Id == discount.Id))
                {
                    AvailableDiscounts.Add(discount);
                }
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error filtering discounts: {ex.Message}";
        }
    }

    private async Task LoadExistingDiscountsAsync(int categoryId)
    {
        try
        {
            // Load the full category data including discounts and translations
            var categoryData = await _productService.GetCategoryByIdAsync(categoryId);
            if (categoryData == null) return;

            // Update the form with the loaded data including translations
            if (!string.IsNullOrWhiteSpace(categoryData.NameArabic))
            {
                NameArabic = categoryData.NameArabic;
            }

            // Load all available discounts first
            await LoadDiscountsAsync();

            // Now populate the selected discounts based on the loaded data
            if (categoryData.SelectedDiscountIds.Any())
            {
                SelectedDiscounts.Clear();
                
                foreach (var discountId in categoryData.SelectedDiscountIds)
                {
                    // Find the discount from available discounts
                    var discount = AvailableDiscounts.FirstOrDefault(d => d.Id == discountId);
                    if (discount != null)
                    {
                        SelectedDiscounts.Add(discount);
                    }
                }

                // Update available discounts to remove selected ones
                FilterAvailableDiscounts();
            }
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading existing category data: {ex.Message}";
        }
    }

    #endregion
}