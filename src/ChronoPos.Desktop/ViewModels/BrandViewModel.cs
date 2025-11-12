using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Infrastructure.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Data;
using System.Windows;
using Microsoft.Win32;
using ChronoPos.Desktop.Views.Dialogs;

namespace ChronoPos.Desktop.ViewModels;

public partial class BrandViewModel : ObservableObject
{
    private readonly IBrandService _brandService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDatabaseLocalizationService _localizationService;
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

    // Localized Text Properties
    [ObservableProperty]
    private string pageTitle = "Brands";

    [ObservableProperty]
    private string searchPlaceholder = "Search brands...";

    [ObservableProperty]
    private string refreshButtonText = "Refresh";

    [ObservableProperty]
    private string importButtonText = "Import";

    [ObservableProperty]
    private string exportButtonText = "Export";

    [ObservableProperty]
    private string addBrandButtonText = "Add Brand";

    [ObservableProperty]
    private string activeOnlyButtonText = "Active Only";

    [ObservableProperty]
    private string showAllButtonText = "Show All";

    [ObservableProperty]
    private string clearFiltersButtonText = "Clear Filters";

    [ObservableProperty]
    private string columnName = "Name";

    [ObservableProperty]
    private string columnArabicName = "Arabic Name";

    [ObservableProperty]
    private string columnDescription = "Description";

    [ObservableProperty]
    private string columnProducts = "Products";

    [ObservableProperty]
    private string columnCreated = "Created";

    [ObservableProperty]
    private string columnStatus = "Status";

    [ObservableProperty]
    private string columnActive = "Active";

    [ObservableProperty]
    private string columnActions = "Actions";

    [ObservableProperty]
    private string editButtonText = "Edit";

    [ObservableProperty]
    private string deleteButtonText = "Delete";

    [ObservableProperty]
    private string noBrandsFoundText = "No brands found";

    [ObservableProperty]
    private string noBrandsMessageText = "Click 'Add Brand' to create your first brand";

    [ObservableProperty]
    private string brandsCountText = "brands";

    [ObservableProperty]
    private string activeText = "Active";

    [ObservableProperty]
    private string inactiveText = "Inactive";

    private readonly ICollectionView _filteredBrandsView;

    public ICollectionView FilteredBrands => _filteredBrandsView;

    public bool HasBrands => Brands.Count > 0;
    public int TotalBrands => Brands.Count;

    public BrandViewModel(IBrandService brandService, ICurrentUserService currentUserService, IDatabaseLocalizationService localizationService, Action? navigateBack = null)
    {
        _brandService = brandService;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
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
        
        // Load localized texts
        _ = LoadLocalizedTextsAsync();
        
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
        
        var dialog = new ConfirmationDialog(
            "Delete Brand",
            $"Are you sure you want to delete the brand '{brand.Name}'?\n\nThis action cannot be undone.",
            ConfirmationDialog.DialogType.Danger,
            "Delete",
            "Cancel");
            
        var result = dialog.ShowDialog();
        if (result != true) return;
        
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
                
                var successDialog = new MessageDialog(
                    "Success",
                    $"Brand '{brand.Name}' has been deleted successfully.",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            else
            {
                StatusMessage = $"Failed to delete brand '{brand.Name}'.";
                var errorDialog = new MessageDialog(
                    "Delete Failed",
                    $"Failed to delete brand '{brand.Name}'. Please try again.",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error deleting brand: {ex.Message}";
            var errorDialog = new MessageDialog(
                "Error",
                $"An error occurred while deleting the brand:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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
                csv.AppendLine("Name,NameArabic,Description,LogoUrl,IsActive");

                foreach (var brand in Brands)
                {
                    csv.AppendLine($"\"{brand.Name}\"," +
                                 $"\"{brand.NameArabic ?? ""}\"," +
                                 $"\"{brand.Description ?? ""}\"," +
                                 $"\"{brand.LogoUrl ?? ""}\"," +
                                 $"{brand.IsActive}");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {Brands.Count} brands successfully";
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {Brands.Count} brands to:\n\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting brands: {ex.Message}";
            var errorDialog = new MessageDialog(
                "Export Error",
                $"An error occurred while exporting brands:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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
            // Show custom import dialog
            var importDialog = new ImportDialog();
            var dialogResult = importDialog.ShowDialog();

            if (dialogResult != true || importDialog.SelectedAction == ImportDialog.ImportAction.None)
                return;

            if (importDialog.SelectedAction == ImportDialog.ImportAction.DownloadTemplate)
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
                    templateCsv.AppendLine("Name,NameArabic,Description,LogoUrl,IsActive");
                    templateCsv.AppendLine("Sample Brand,العلامة التجارية النموذجية,Sample brand description,,true");
                    templateCsv.AppendLine("Nike,نايكي,Sports brand,,true");

                    await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                    
                    var successDialog = new MessageDialog(
                        "Template Downloaded",
                        $"Template downloaded successfully to:\n\n{saveFileDialog.FileName}\n\nPlease fill in your data and use the Import function again to upload it.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
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
                    var warningDialog = new MessageDialog(
                        "Import Error",
                        "The CSV file is empty or contains only headers.",
                        MessageDialog.MessageType.Warning);
                    warningDialog.ShowDialog();
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
                        if (values.Length < 5)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 5 columns)");
                            continue;
                        }

                        var createDto = new CreateBrandDto
                        {
                            Name = values[0].Trim('"'),
                            NameArabic = string.IsNullOrWhiteSpace(values[1].Trim('"')) ? null : values[1].Trim('"'),
                            Description = string.IsNullOrWhiteSpace(values[2].Trim('"')) ? null : values[2].Trim('"'),
                            LogoUrl = string.IsNullOrWhiteSpace(values[3].Trim('"')) ? null : values[3].Trim('"'),
                            IsActive = bool.Parse(values[4])
                        };

                        await _brandService.CreateAsync(createDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        var errorMessage = ex.Message;
                        
                        // Include inner exception details if available
                        if (ex.InnerException != null)
                        {
                            errorMessage += $" | Inner: {ex.InnerException.Message}";
                            
                            // Go deeper if there's another inner exception
                            if (ex.InnerException.InnerException != null)
                            {
                                errorMessage += $" | Details: {ex.InnerException.InnerException.Message}";
                            }
                        }
                        
                        errors.AppendLine($"Line {i + 1}: {errorMessage}");
                    }
                }

                await LoadBrandsAsync();

                var message = $"Import completed:\n\n✓ {successCount} brands imported successfully";
                if (errorCount > 0)
                {
                    message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                }

                var resultDialog = new MessageDialog(
                    "Import Complete",
                    message,
                    errorCount > 0 ? MessageDialog.MessageType.Warning : MessageDialog.MessageType.Success);
                resultDialog.ShowDialog();
                
                StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing brands: {ex.Message}";
            var errorDialog = new MessageDialog(
                "Import Error",
                $"An error occurred while importing brands:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
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

    private async Task LoadLocalizedTextsAsync()
    {
        try
        {
            PageTitle = await _localizationService.GetTranslationAsync("brand.page_title") ?? "Brands";
            SearchPlaceholder = await _localizationService.GetTranslationAsync("brand.search_placeholder") ?? "Search brands...";
            RefreshButtonText = await _localizationService.GetTranslationAsync("common.refresh") ?? "Refresh";
            ImportButtonText = await _localizationService.GetTranslationAsync("common.import") ?? "Import";
            ExportButtonText = await _localizationService.GetTranslationAsync("common.export") ?? "Export";
            AddBrandButtonText = await _localizationService.GetTranslationAsync("brand.add_brand") ?? "Add Brand";
            ActiveOnlyButtonText = await _localizationService.GetTranslationAsync("brand.active_only") ?? "Active Only";
            ShowAllButtonText = await _localizationService.GetTranslationAsync("brand.show_all") ?? "Show All";
            ClearFiltersButtonText = await _localizationService.GetTranslationAsync("common.clear_filters") ?? "Clear Filters";
            
            // Column headers
            ColumnName = await _localizationService.GetTranslationAsync("brand.column.name") ?? "Name";
            ColumnArabicName = await _localizationService.GetTranslationAsync("brand.column.arabic_name") ?? "Arabic Name";
            ColumnDescription = await _localizationService.GetTranslationAsync("brand.column.description") ?? "Description";
            ColumnProducts = await _localizationService.GetTranslationAsync("brand.column.products") ?? "Products";
            ColumnCreated = await _localizationService.GetTranslationAsync("brand.column.created") ?? "Created";
            ColumnStatus = await _localizationService.GetTranslationAsync("brand.column.status") ?? "Status";
            ColumnActive = await _localizationService.GetTranslationAsync("brand.column.active") ?? "Active";
            ColumnActions = await _localizationService.GetTranslationAsync("brand.column.actions") ?? "Actions";
            
            // Action buttons
            EditButtonText = await _localizationService.GetTranslationAsync("common.edit") ?? "Edit";
            DeleteButtonText = await _localizationService.GetTranslationAsync("common.delete") ?? "Delete";
            
            // Messages
            NoBrandsFoundText = await _localizationService.GetTranslationAsync("brand.no_brands_found") ?? "No brands found";
            NoBrandsMessageText = await _localizationService.GetTranslationAsync("brand.no_brands_message") ?? "Click 'Add Brand' to create your first brand";
            BrandsCountText = await _localizationService.GetTranslationAsync("brand.brands_count") ?? "brands";
            
            // Status text
            ActiveText = await _localizationService.GetTranslationAsync("common.active") ?? "Active";
            InactiveText = await _localizationService.GetTranslationAsync("common.inactive") ?? "Inactive";
        }
        catch (Exception ex)
        {
            // Fallback to default English texts if translation fails
            Console.WriteLine($"Error loading localized texts: {ex.Message}");
        }
    }

    public async Task RefreshTranslationsAsync()
    {
        await LoadLocalizedTextsAsync();
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
