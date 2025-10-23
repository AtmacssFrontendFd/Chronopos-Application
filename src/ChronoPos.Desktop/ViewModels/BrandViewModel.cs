using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels;

public partial class BrandViewModel : ObservableObject
{
    private readonly IBrandService _brandService;
    private readonly ICurrentUserService _currentUserService;
    private readonly Action? _navigateBack;

    [ObservableProperty]
    private ObservableCollection<BrandDto> brands = new();

    [ObservableProperty]
    private BrandDto? selectedBrand;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSidePanelVisible = false;

    [ObservableProperty]
    private BrandSidePanelViewModel? sidePanelViewModel;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool showActiveOnly = false;

    // Permission Properties
    [ObservableProperty]
    private bool canCreateBrand = false;

    [ObservableProperty]
    private bool canEditBrand = false;

    [ObservableProperty]
    private bool canDeleteBrand = false;

    [ObservableProperty]
    private bool canImportBrand = false;

    [ObservableProperty]
    private bool canExportBrand = false;

    private readonly ICollectionView _filteredBrandsView;

    public ICollectionView FilteredBrands => _filteredBrandsView;

    public bool HasBrands => Brands.Count > 0;
    public int TotalBrands => Brands.Count;

    public BrandViewModel(IBrandService brandService, ICurrentUserService currentUserService, Action? navigateBack = null)
    {
        _brandService = brandService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _navigateBack = navigateBack;
        
        // Initialize permissions
        InitializePermissions();
        
        // Initialize filtered view
        _filteredBrandsView = CollectionViewSource.GetDefaultView(Brands);
        _filteredBrandsView.Filter = FilterBrands;

        // Initialize commands
        LoadBrandsCommand = new AsyncRelayCommand(LoadBrandsAsync);
        AddBrandCommand = new RelayCommand(ShowAddBrandPanel);
        EditBrandCommand = new RelayCommand<BrandDto?>(ShowEditBrandPanel);
        DeleteBrandCommand = new AsyncRelayCommand<BrandDto?>(DeleteBrandAsync);
        FilterActiveCommand = new RelayCommand(FilterActive);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        RefreshDataCommand = new AsyncRelayCommand(LoadBrandsAsync);
        ToggleActiveCommand = new AsyncRelayCommand<BrandDto?>(ToggleActiveAsync);
        ViewBrandDetailsCommand = new RelayCommand<BrandDto?>(ViewBrandDetails);
        BackCommand = new RelayCommand(GoBack);
        CloseSidePanelCommand = new RelayCommand(CloseSidePanel);

        // Subscribe to search text changes
        PropertyChanged += OnPropertyChanged;
        
        // Load brands on startup
        _ = LoadBrandsAsync();
    }

    public IAsyncRelayCommand LoadBrandsCommand { get; }
    public RelayCommand AddBrandCommand { get; }
    public RelayCommand<BrandDto?> EditBrandCommand { get; }
    public IAsyncRelayCommand<BrandDto?> DeleteBrandCommand { get; }
    public RelayCommand FilterActiveCommand { get; }
    public RelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand RefreshDataCommand { get; }
    public IAsyncRelayCommand<BrandDto?> ToggleActiveCommand { get; }
    public RelayCommand<BrandDto?> ViewBrandDetailsCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand CloseSidePanelCommand { get; }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SearchText) || e.PropertyName == nameof(ShowActiveOnly))
        {
            _filteredBrandsView.Refresh();
        }
        else if (e.PropertyName == nameof(Brands))
        {
            OnPropertyChanged(nameof(HasBrands));
            OnPropertyChanged(nameof(TotalBrands));
            _filteredBrandsView.Refresh();
        }
    }

    private bool FilterBrands(object obj)
    {
        if (obj is not BrandDto brand) return false;
        
        // Apply active filter
        if (ShowActiveOnly && !brand.IsActive) return false;
        
        // Apply search filter
        if (string.IsNullOrWhiteSpace(SearchText)) return true;
        
        return brand.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
               (!string.IsNullOrEmpty(brand.NameArabic) && brand.NameArabic.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
               (!string.IsNullOrEmpty(brand.Description) && brand.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
    }

    private async Task LoadBrandsAsync()
    {
        IsLoading = true;
        try
        {
            var allBrands = await _brandService.GetAllAsync();
            
            // Clear and repopulate the existing collection to maintain the filtered view
            Brands.Clear();
            foreach (var brand in allBrands)
            {
                Brands.Add(brand);
            }
            
            StatusMessage = $"Loaded {Brands.Count} brands.";
            
            // Refresh the filtered view
            _filteredBrandsView.Refresh();
            OnPropertyChanged(nameof(HasBrands));
            OnPropertyChanged(nameof(TotalBrands));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading brands: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ShowAddBrandPanel()
    {
        SidePanelViewModel = new BrandSidePanelViewModel(
            _brandService,
            OnBrandSaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = "Add new brand...";
    }

    private void ShowEditBrandPanel(BrandDto? brand)
    {
        if (brand == null) return;
        
        SidePanelViewModel = new BrandSidePanelViewModel(
            _brandService,
            brand,
            OnBrandSaved,
            CloseSidePanel);
        IsSidePanelVisible = true;
        StatusMessage = $"Edit brand '{brand.Name}'...";
    }

    private void OnBrandSaved(bool success)
    {
        if (success)
        {
            CloseSidePanel();
            // Reload brands to reflect changes
            _ = LoadBrandsAsync();
        }
    }

    private void CloseSidePanel()
    {
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
        StatusMessage = "Brand editing cancelled.";
    }

    private async Task DeleteBrandAsync(BrandDto? brand)
    {
        if (brand == null) return;
        
        var result = MessageBox.Show(
            $"Are you sure you want to delete the brand '{brand.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
            
        if (result != MessageBoxResult.Yes) return;
        
        try
        {
            var success = await _brandService.DeleteAsync(brand.Id);
            if (success)
            {
                Brands.Remove(brand);
                StatusMessage = $"Brand '{brand.Name}' deleted.";
                _filteredBrandsView.Refresh();
                OnPropertyChanged(nameof(HasBrands));
                OnPropertyChanged(nameof(TotalBrands));
            }
            else
            {
                StatusMessage = $"Failed to delete brand '{brand.Name}'.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting brand: {ex.Message}";
        }
    }

    private async Task ToggleActiveAsync(BrandDto? brand)
    {
        if (brand == null) return;
        
        try
        {
            var updateDto = new UpdateBrandDto
            {
                Name = brand.Name,
                NameArabic = brand.NameArabic,
                Description = brand.Description,
                LogoUrl = brand.LogoUrl,
                IsActive = !brand.IsActive
            };
            
            var updatedBrand = await _brandService.UpdateAsync(brand.Id, updateDto);
            if (updatedBrand != null)
            {
                brand.IsActive = updatedBrand.IsActive;
                StatusMessage = $"Brand '{brand.Name}' {(brand.IsActive ? "activated" : "deactivated")}.";
            }
            else
            {
                StatusMessage = $"Failed to update brand '{brand.Name}'.";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error updating brand: {ex.Message}";
        }
    }

    private void ViewBrandDetails(BrandDto? brand)
    {
        if (brand == null) return;
        StatusMessage = $"Viewing details for brand '{brand.Name}'...";
        // TODO: Implement brand details view
    }

    private void FilterActive()
    {
        ShowActiveOnly = !ShowActiveOnly;
        StatusMessage = ShowActiveOnly ? "Showing active brands only." : "Showing all brands.";
    }

    private void ClearFilters()
    {
        SearchText = string.Empty;
        ShowActiveOnly = false;
        StatusMessage = "Filters cleared.";
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"Brands_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Exporting brands...";

                var csv = new StringBuilder();
                csv.AppendLine("Id,Name,NameArabic,Description,LogoUrl,IsActive");

                foreach (var brand in Brands)
                {
                    csv.AppendLine($"{brand.Id}," +
                                 $"\"{brand.Name}\"," +
                                 $"\"{brand.NameArabic ?? ""}\"," +
                                 $"\"{brand.Description ?? ""}\"," +
                                 $"\"{brand.LogoUrl ?? ""}\"," +
                                 $"{brand.IsActive}");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {Brands.Count} brands successfully";
                MessageBox.Show($"Exported {Brands.Count} brands to:\n{saveFileDialog.FileName}", 
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting brands: {ex.Message}";
            MessageBox.Show($"Error exporting brands: {ex.Message}", "Export Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            // Show dialog with Download Template and Upload File options
            var result = MessageBox.Show(
                "Would you like to download a template first?\n\n" +
                "• Click 'Yes' to download the CSV template\n" +
                "• Click 'No' to upload your file directly\n" +
                "• Click 'Cancel' to exit",
                "Import Brands",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel)
                return;

            if (result == MessageBoxResult.Yes)
            {
                // Download Template
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = "Brands_Template.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var templateCsv = new StringBuilder();
                    templateCsv.AppendLine("Id,Name,NameAr,Description,IsActive");
                    templateCsv.AppendLine("0,Sample Brand,العلامة التجارية النموذجية,Sample brand description,true");
                    templateCsv.AppendLine("0,Nike,نايكي,Sports brand,true");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    MessageBox.Show($"Template downloaded successfully to:\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.", 
                        "Template Downloaded", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                return;
            }

            // Upload File
            var openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Importing brands...";

                var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                if (lines.Length <= 1)
                {
                    MessageBox.Show("The CSV file is empty or contains only headers.", "Import Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int successCount = 0;
                int errorCount = 0;
                var errors = new StringBuilder();

                // Skip header row
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var values = ParseCsvLine(line);
                        if (values.Length < 6)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 6 columns)");
                            continue;
                        }

                        var createDto = new CreateBrandDto
                        {
                            Name = values[1].Trim('"'),
                            NameArabic = string.IsNullOrWhiteSpace(values[2].Trim('"')) ? null : values[2].Trim('"'),
                            Description = string.IsNullOrWhiteSpace(values[3].Trim('"')) ? null : values[3].Trim('"'),
                            LogoUrl = string.IsNullOrWhiteSpace(values[4].Trim('"')) ? null : values[4].Trim('"'),
                            IsActive = bool.Parse(values[5])
                        };

                        await _brandService.CreateAsync(createDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        errors.AppendLine($"Line {i + 1}: {ex.Message}");
                    }
                }

                await LoadBrandsAsync();

                var message = $"Import completed:\n✓ {successCount} brands imported successfully";
                if (errorCount > 0)
                {
                    message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                }

                MessageBox.Show(message, "Import Complete", 
                    MessageBoxButton.OK, errorCount > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
                
                StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing brands: {ex.Message}";
            MessageBox.Show($"Error importing brands: {ex.Message}", "Import Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                currentValue.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString());
        return values.ToArray();
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreateBrand = _currentUserService.HasPermission(ScreenNames.BRAND, TypeMatrix.CREATE);
            CanEditBrand = _currentUserService.HasPermission(ScreenNames.BRAND, TypeMatrix.UPDATE);
            CanDeleteBrand = _currentUserService.HasPermission(ScreenNames.BRAND, TypeMatrix.DELETE);
            CanImportBrand = _currentUserService.HasPermission(ScreenNames.BRAND, TypeMatrix.IMPORT);
            CanExportBrand = _currentUserService.HasPermission(ScreenNames.BRAND, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            // Fail-secure: all permissions default to false
            CanCreateBrand = false;
            CanEditBrand = false;
            CanDeleteBrand = false;
            CanImportBrand = false;
            CanExportBrand = false;
        }
    }

    private void GoBack()
    {
        if (_navigateBack != null)
        {
            _navigateBack.Invoke();
        }
        else
        {
            StatusMessage = "Navigation back not configured.";
        }
    }
}
