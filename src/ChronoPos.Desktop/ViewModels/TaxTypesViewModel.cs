using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class TaxTypesViewModel : ObservableObject
{
    private readonly ITaxTypeService _taxTypeService;

    [ObservableProperty]
    private ObservableCollection<TaxTypeDto> _taxTypes = new();

    [ObservableProperty]
    private ObservableCollection<TaxTypeDto> _filteredTaxTypes = new();

    [ObservableProperty]
    private TaxTypeDto _selectedTaxType = new();

    [ObservableProperty]
    private TaxTypeDto _editingTaxType = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _isOverlayVisible = false;

    [ObservableProperty]
    private bool _isEditMode = false;

    [ObservableProperty]
    private bool _showActiveOnly = false;

    /// <summary>
    /// Text for the active filter toggle button
    /// </summary>
    public string ActiveFilterButtonText => ShowActiveOnly ? "Show All" : "Active Only";

    /// <summary>
    /// Title for the overlay form
    /// </summary>
    public string FormTitle => IsEditMode ? "Edit Tax Type" : "Add Tax Type";

    /// <summary>
    /// Action to navigate back (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    public TaxTypesViewModel(ITaxTypeService taxTypeService)
    {
        _taxTypeService = taxTypeService;
        _ = LoadTaxTypesAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterTaxTypes();
    }

    partial void OnShowActiveOnlyChanged(bool value)
    {
        FilterTaxTypes();
        OnPropertyChanged(nameof(ActiveFilterButtonText));
    }

    partial void OnIsEditModeChanged(bool value)
    {
        OnPropertyChanged(nameof(FormTitle));
    }

    private void FilterTaxTypes()
    {
        var filtered = TaxTypes.AsEnumerable();

        if (ShowActiveOnly)
        {
            filtered = filtered.Where(t => t.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(t => 
                t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (t.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        
        FilteredTaxTypes = new ObservableCollection<TaxTypeDto>(filtered);
    }

    [RelayCommand]
    private async Task LoadTaxTypesAsync()
    {
        try
        {
            var taxTypes = await _taxTypeService.GetAllTaxTypesAsync();
            TaxTypes = new ObservableCollection<TaxTypeDto>(taxTypes);
            FilterTaxTypes();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tax types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ShowAddTaxTypeOverlay()
    {
        try
        {
            Console.WriteLine("=== ShowAddTaxTypeOverlay started ===");
            EditingTaxType = new TaxTypeDto
            {
                IsActive = true,
                IsPercentage = true,
                CalculationOrder = 1
            };
            IsEditMode = false;
            IsOverlayVisible = true;
            Console.WriteLine($"ShowAddTaxTypeOverlay executed - IsOverlayVisible: {IsOverlayVisible}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ShowAddTaxTypeOverlay: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ShowEditTaxTypeOverlay(TaxTypeDto? taxType = null)
    {
        var targetTaxType = taxType ?? SelectedTaxType;
        if (targetTaxType?.Id > 0)
        {
            EditingTaxType = new TaxTypeDto
            {
                Id = targetTaxType.Id,
                Name = targetTaxType.Name,
                Description = targetTaxType.Description,
                Value = targetTaxType.Value,
                IsPercentage = targetTaxType.IsPercentage,
                IncludedInPrice = targetTaxType.IncludedInPrice,
                AppliesToBuying = targetTaxType.AppliesToBuying,
                AppliesToSelling = targetTaxType.AppliesToSelling,
                CalculationOrder = targetTaxType.CalculationOrder,
                IsActive = targetTaxType.IsActive
            };
            IsEditMode = true;
            IsOverlayVisible = true;
        }
    }

    [RelayCommand]
    private async Task SaveTaxTypeAsync()
    {
        try
        {
            Console.WriteLine("=== SaveTaxTypeAsync started ===");
            Console.WriteLine($"EditingTaxType.Name: '{EditingTaxType.Name}'");
            Console.WriteLine($"EditingTaxType.Value: {EditingTaxType.Value}");
            Console.WriteLine($"IsEditMode: {IsEditMode}");

            // Basic validation
            if (string.IsNullOrWhiteSpace(EditingTaxType.Name))
            {
                Console.WriteLine("Validation failed: Name is empty");
                MessageBox.Show("Tax type name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditingTaxType.Value < 0)
            {
                Console.WriteLine("Validation failed: Value is negative");
                MessageBox.Show("Tax value cannot be negative.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (EditingTaxType.IsPercentage && EditingTaxType.Value > 100)
            {
                Console.WriteLine("Validation failed: Percentage over 100");
                MessageBox.Show("Percentage value cannot exceed 100%.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Console.WriteLine("Validation passed, calling service...");

            if (IsEditMode)
            {
                await _taxTypeService.UpdateTaxTypeAsync(EditingTaxType);
                MessageBox.Show("Tax type updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                await _taxTypeService.CreateTaxTypeAsync(EditingTaxType);
                MessageBox.Show("Tax type created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Console.WriteLine("Service call completed, closing overlay...");
            CloseOverlay();
            await LoadTaxTypesAsync();
            Console.WriteLine("=== SaveTaxTypeAsync completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ERROR in SaveTaxTypeAsync: {ex.Message} ===");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            MessageBox.Show($"Error saving tax type: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteTaxTypeAsync(TaxTypeDto? taxType = null)
    {
        var targetTaxType = taxType ?? SelectedTaxType;
        if (targetTaxType?.Id > 0)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to delete the tax type '{targetTaxType.Name}'?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await _taxTypeService.DeleteTaxTypeAsync(targetTaxType.Id);
                    MessageBox.Show("Tax type deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadTaxTypesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting tax type: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    [RelayCommand]
    private void CloseOverlay()
    {
        IsOverlayVisible = false;
        EditingTaxType = new TaxTypeDto();
        IsEditMode = false;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadTaxTypesAsync();
    }

    [RelayCommand]
    private void GoBack()
    {
        GoBackAction?.Invoke();
    }

    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
    }

    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }
}