using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using ChronoPos.Desktop.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IDiscountService _discountService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CategoryViewModel> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDatabaseLocalizationService _localizationService;
        private readonly Action? _navigateBack;

        // Cache for discount information to avoid repeated service calls
        private Dictionary<int, string> _discountCache = new();

        [ObservableProperty]
        private ObservableCollection<CategoryHierarchyItem> hierarchicalCategories = new();

        [ObservableProperty]
        private string searchText = string.Empty;

        [ObservableProperty]
        private bool isSidePanelVisible;

        [ObservableProperty]
        private CategorySidePanelViewModel? sidePanelViewModel;

        [ObservableProperty]
        private string statusMessage = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private int totalCategories;

        // Permission Properties
        [ObservableProperty]
        private bool canCreateCategory = false;

        [ObservableProperty]
        private bool canEditCategory = false;

        [ObservableProperty]
        private bool canDeleteCategory = false;

        [ObservableProperty]
        private bool canImportCategory = false;

        [ObservableProperty]
        private bool canExportCategory = false;

        // Localized Text Properties
        [ObservableProperty]
        private string pageTitle = "Categories";

        [ObservableProperty]
        private string searchPlaceholder = "Search categories...";

        [ObservableProperty]
        private string refreshButtonText = "Refresh";

        [ObservableProperty]
        private string importButtonText = "Import";

        [ObservableProperty]
        private string exportButtonText = "Export";

        [ObservableProperty]
        private string addCategoryButtonText = "Add Category";

        [ObservableProperty]
        private string columnName = "Name";

        [ObservableProperty]
        private string columnArabicName = "Arabic Name";

        [ObservableProperty]
        private string columnDescription = "Description";

        [ObservableProperty]
        private string columnProducts = "Products";

        [ObservableProperty]
        private string columnDiscounts = "Discounts";

        [ObservableProperty]
        private string columnStatus = "Status";

        [ObservableProperty]
        private string columnActions = "Actions";

        [ObservableProperty]
        private string editButtonText = "Edit";

        [ObservableProperty]
        private string deleteButtonText = "Delete";

        [ObservableProperty]
        private string addSubCategoryButtonText = "Add Subcategory";

        [ObservableProperty]
        private string noCategoriesFoundText = "No categories found";

        [ObservableProperty]
        private string noCategoriesMessageText = "Click 'Add Category' to create your first category";

        [ObservableProperty]
        private string categoriesCountText = "categories";

        [ObservableProperty]
        private string activeText = "Active";

        [ObservableProperty]
        private string inactiveText = "Inactive";

        public CategoryViewModel(
            IProductService productService,
            IDiscountService discountService,
            IServiceProvider serviceProvider,
            ILogger<CategoryViewModel> logger,
            ICurrentUserService currentUserService,
            IDatabaseLocalizationService localizationService,
            Action? navigateBack = null)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _navigateBack = navigateBack;

            // Initialize permissions
            InitializePermissions();

            // Initialize commands
            BackCommand = new RelayCommand(GoBack);
            AddCategoryCommand = new RelayCommand(ShowAddCategoryPanel);
            EditCategoryCommand = new RelayCommand<CategoryHierarchyItem?>(ShowEditCategoryPanel);
            DeleteCategoryCommand = new AsyncRelayCommand<CategoryHierarchyItem?>(DeleteCategoryAsync);
            AddSubCategoryCommand = new RelayCommand<CategoryHierarchyItem?>(ShowAddSubCategoryPanel);
            FilterCommand = new AsyncRelayCommand(FilterCategoriesAsync);
            RefreshDataCommand = new AsyncRelayCommand(LoadCategoriesAsync);
            ImportCommand = new AsyncRelayCommand(ImportAsync);
            ExportCommand = new AsyncRelayCommand(ExportAsync);

            // Subscribe to property changes for search
            PropertyChanged += OnPropertyChanged;

            // Load localized texts
            _ = LoadLocalizedTextsAsync();

            // Load initial data
            _ = LoadCategoriesAsync();
        }

        #region Commands

        public RelayCommand BackCommand { get; }
        public RelayCommand AddCategoryCommand { get; }
        public RelayCommand<CategoryHierarchyItem?> EditCategoryCommand { get; }
        public IAsyncRelayCommand<CategoryHierarchyItem?> DeleteCategoryCommand { get; }
        public RelayCommand<CategoryHierarchyItem?> AddSubCategoryCommand { get; }
        public IAsyncRelayCommand FilterCommand { get; }
        public IAsyncRelayCommand RefreshDataCommand { get; }
        public IAsyncRelayCommand ImportCommand { get; }
        public IAsyncRelayCommand ExportCommand { get; }

        #endregion

        #region Private Methods

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SearchText))
            {
                _ = FilterCategoriesAsync();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading categories...";

                // Load discount cache first
                await LoadDiscountCacheAsync();

                var categories = await _productService.GetAllCategoriesAsync();
                var hierarchicalItems = BuildHierarchy(categories.ToList());

                HierarchicalCategories.Clear();
                foreach (var item in hierarchicalItems)
                {
                    HierarchicalCategories.Add(item);
                }

                TotalCategories = categories.Count();
                StatusMessage = $"Loaded {TotalCategories} categories";

                _logger?.LogInformation("Categories loaded successfully: {Count} categories", TotalCategories);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading categories");
                StatusMessage = "Error loading categories";
                
                var errorDialog = new MessageDialog(
                    "Loading Error",
                    "Failed to load categories. Please try again.",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FilterCategoriesAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadCategoriesAsync();
                    return;
                }

                IsLoading = true;
                StatusMessage = "Filtering categories...";

                var allCategories = await _productService.GetAllCategoriesAsync();
                
                // First, find all categories that match the search directly
                var directMatches = allCategories.Where(c => 
                    c.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(c.Description) && c.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(c.NameArabic) && c.NameArabic.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                // Then, include parent categories of matching children
                var categoriesToInclude = new HashSet<CategoryDto>();
                
                foreach (var match in directMatches)
                {
                    categoriesToInclude.Add(match);
                    
                    // Add all ancestors
                    var currentCategory = match;
                    while (currentCategory?.ParentCategoryId != null)
                    {
                        var parent = allCategories.FirstOrDefault(c => c.Id == currentCategory.ParentCategoryId);
                        if (parent != null)
                        {
                            categoriesToInclude.Add(parent);
                            currentCategory = parent;
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    // Add all descendants
                    AddAllDescendants(match, allCategories.ToList(), categoriesToInclude);
                }

                var filteredCategories = categoriesToInclude.ToList();
                var hierarchicalItems = BuildHierarchy(filteredCategories);

                HierarchicalCategories.Clear();
                foreach (var item in hierarchicalItems)
                {
                    HierarchicalCategories.Add(item);
                }

                StatusMessage = $"Found {directMatches.Count} matching categories";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error filtering categories with search text: {SearchText}", SearchText);
                StatusMessage = "Error filtering categories";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddAllDescendants(CategoryDto category, List<CategoryDto> allCategories, HashSet<CategoryDto> includeSet)
        {
            var children = allCategories.Where(c => c.ParentCategoryId == category.Id).ToList();
            foreach (var child in children)
            {
                includeSet.Add(child);
                AddAllDescendants(child, allCategories, includeSet);
            }
        }

        private List<CategoryHierarchyItem> BuildHierarchy(List<CategoryDto> categories)
        {
            var hierarchyItems = categories.Select(c => new CategoryHierarchyItem 
            { 
                Category = c,
                SubCategories = new ObservableCollection<CategoryHierarchyItem>(),
                DiscountPills = new ObservableCollection<string>(BuildDiscountPills(c))
            }).ToList();

            var rootItems = new List<CategoryHierarchyItem>();

            foreach (var item in hierarchyItems)
            {
                if (item.Category?.ParentCategoryId == null)
                {
                    rootItems.Add(item);
                }
                else
                {
                    var parent = hierarchyItems.FirstOrDefault(h => h.Category?.Id == item.Category.ParentCategoryId);
                    parent?.SubCategories.Add(item);
                }
            }

            return rootItems;
        }

        private List<string> BuildDiscountPills(CategoryDto category)
        {
            if (category?.SelectedDiscountIds == null || !category.SelectedDiscountIds.Any())
                return new List<string>();

            try
            {
                var discountPills = new List<string>();
                
                foreach (var discountId in category.SelectedDiscountIds.Take(5)) // Show up to 5 discount pills
                {
                    if (_discountCache.TryGetValue(discountId, out var discountText))
                    {
                        discountPills.Add(discountText);
                    }
                    else
                    {
                        discountPills.Add($"Discount#{discountId}");
                    }
                }

                if (category.SelectedDiscountIds.Count > 5)
                {
                    discountPills.Add($"+{category.SelectedDiscountIds.Count - 5} more");
                }

                return discountPills;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error building discount pills for category {CategoryId}", category.Id);
                return new List<string> { "Discounts available" };
            }
        }

        private async Task LoadDiscountCacheAsync()
        {
            try
            {
                var discounts = await _discountService.GetActiveDiscountsAsync();
                _discountCache.Clear();
                
                foreach (var discount in discounts)
                {
                    var displayText = $"{discount.DiscountName} {discount.FormattedDiscountValue}";
                    _discountCache[discount.Id] = displayText;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error loading discount cache");
            }
        }

        private void ShowAddCategoryPanel()
        {
            try
            {
                OpenCategorySidePanel();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening add category panel");
                
                var errorDialog = new MessageDialog(
                    "Error",
                    "Failed to open add category panel. Please try again.",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
        }

        private void ShowEditCategoryPanel(CategoryHierarchyItem? categoryItem)
        {
            if (categoryItem?.Category == null)
                return;

            try
            {
                OpenCategorySidePanel(categoryItem.Category);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening edit category panel for category ID: {CategoryId}", categoryItem.Category.Id);
                
                var errorDialog = new MessageDialog(
                    "Error",
                    "Failed to open edit category panel. Please try again.",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
        }

        private void ShowAddSubCategoryPanel(CategoryHierarchyItem? parentCategoryItem)
        {
            if (parentCategoryItem?.Category == null)
                return;

            try
            {
                OpenCategorySidePanel(parentCategory: parentCategoryItem.Category);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening add subcategory panel for parent ID: {ParentId}", parentCategoryItem.Category.Id);
                
                var errorDialog = new MessageDialog(
                    "Error",
                    "Failed to open add subcategory panel. Please try again.",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
        }

        private async Task DeleteCategoryAsync(CategoryHierarchyItem? categoryItem)
        {
            if (categoryItem?.Category == null)
                return;

            try
            {
                var hasSubCategories = categoryItem.SubCategories?.Any() == true;

                string message;
                string title;
                
                if (hasSubCategories)
                {
                    title = "Delete Category with Subcategories";
                    message = $"Category '{categoryItem.Category.Name}' has subcategories.\n\n" +
                             "Deleting it will also remove all subcategories.\n\n" +
                             "Are you sure you want to proceed?";
                }
                else
                {
                    title = "Delete Category";
                    message = $"Are you sure you want to delete category '{categoryItem.Category.Name}'?\n\n" +
                             "This action cannot be undone.";
                }

                var dialog = new ConfirmationDialog(
                    title,
                    message,
                    ConfirmationDialog.DialogType.Danger,
                    "Delete",
                    "Cancel");
                    
                var result = dialog.ShowDialog();
                if (result != true)
                    return;

                IsLoading = true;
                StatusMessage = "Deleting category...";

                await _productService.DeleteCategoryAsync(categoryItem.Category.Id);
                await LoadCategoriesAsync();

                StatusMessage = $"Category '{categoryItem.Category.Name}' deleted successfully";
                _logger?.LogInformation("Category deleted successfully: {CategoryName} (ID: {CategoryId})", 
                    categoryItem.Category.Name, categoryItem.Category.Id);
                    
                var successDialog = new MessageDialog(
                    "Success",
                    $"Category '{categoryItem.Category.Name}' has been deleted successfully.",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting category: {CategoryId}", categoryItem.Category.Id);
                
                var errorDialog = new MessageDialog(
                    "Delete Error",
                    $"Failed to delete category.\n\n{ex.Message}",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void OpenCategorySidePanel(CategoryDto? categoryToEdit = null, CategoryDto? parentCategory = null)
        {
            try
            {
                var sidePanelVm = new CategorySidePanelViewModel(
                    _productService,
                    _discountService,
                    categoryToEdit,
                    parentCategory,
                    OnCategorySaved,
                    CloseSidePanel);

                SidePanelViewModel = sidePanelVm;
                IsSidePanelVisible = true;

                if (categoryToEdit != null)
                {
                    StatusMessage = $"Edit category '{categoryToEdit.Name}'...";
                }
                else if (parentCategory != null)
                {
                    StatusMessage = $"Add subcategory under '{parentCategory.Name}'...";
                }
                else
                {
                    StatusMessage = "Add new category...";
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error opening category side panel");
                throw;
            }
        }

        private void CloseSidePanel()
        {
            IsSidePanelVisible = false;
            SidePanelViewModel = null;
            StatusMessage = "Category editing cancelled.";
        }

        private void OnCategorySaved(bool success)
        {
            if (success)
            {
                CloseSidePanel();
                _ = LoadCategoriesAsync();
            }
        }

        private void GoBack()
        {
            if (IsSidePanelVisible)
            {
                CloseSidePanel();
                return;
            }

            _navigateBack?.Invoke();
        }

        private void InitializePermissions()
        {
            try
            {
                CanCreateCategory = _currentUserService.HasPermission(ScreenNames.CATEGORY, TypeMatrix.CREATE);
                CanEditCategory = _currentUserService.HasPermission(ScreenNames.CATEGORY, TypeMatrix.UPDATE);
                CanDeleteCategory = _currentUserService.HasPermission(ScreenNames.CATEGORY, TypeMatrix.DELETE);
                CanImportCategory = _currentUserService.HasPermission(ScreenNames.CATEGORY, TypeMatrix.IMPORT);
                CanExportCategory = _currentUserService.HasPermission(ScreenNames.CATEGORY, TypeMatrix.EXPORT);
            }
            catch (Exception)
            {
                // Fail-secure: all permissions default to false
                CanCreateCategory = false;
                CanEditCategory = false;
                CanDeleteCategory = false;
                CanImportCategory = false;
                CanExportCategory = false;
            }
        }

        private async Task LoadLocalizedTextsAsync()
        {
            try
            {
                PageTitle = await _localizationService.GetTranslationAsync("category.page_title") ?? "Categories";
                SearchPlaceholder = await _localizationService.GetTranslationAsync("category.search_placeholder") ?? "Search categories...";
                RefreshButtonText = await _localizationService.GetTranslationAsync("common.refresh") ?? "Refresh";
                ImportButtonText = await _localizationService.GetTranslationAsync("common.import") ?? "Import";
                ExportButtonText = await _localizationService.GetTranslationAsync("common.export") ?? "Export";
                AddCategoryButtonText = await _localizationService.GetTranslationAsync("category.add_category") ?? "Add Category";
                
                // Column headers
                ColumnName = await _localizationService.GetTranslationAsync("category.column.name") ?? "Name";
                ColumnArabicName = await _localizationService.GetTranslationAsync("category.column.arabic_name") ?? "Arabic Name";
                ColumnDescription = await _localizationService.GetTranslationAsync("category.column.description") ?? "Description";
                ColumnProducts = await _localizationService.GetTranslationAsync("category.column.products") ?? "Products";
                ColumnDiscounts = await _localizationService.GetTranslationAsync("category.column.discounts") ?? "Discounts";
                ColumnStatus = await _localizationService.GetTranslationAsync("category.column.status") ?? "Status";
                ColumnActions = await _localizationService.GetTranslationAsync("category.column.actions") ?? "Actions";
                
                // Action buttons
                EditButtonText = await _localizationService.GetTranslationAsync("common.edit") ?? "Edit";
                DeleteButtonText = await _localizationService.GetTranslationAsync("common.delete") ?? "Delete";
                AddSubCategoryButtonText = await _localizationService.GetTranslationAsync("category.add_subcategory") ?? "Add Subcategory";
                
                // Messages
                NoCategoriesFoundText = await _localizationService.GetTranslationAsync("category.no_categories_found") ?? "No categories found";
                NoCategoriesMessageText = await _localizationService.GetTranslationAsync("category.no_categories_message") ?? "Click 'Add Category' to create your first category";
                CategoriesCountText = await _localizationService.GetTranslationAsync("category.categories_count") ?? "categories";
                
                // Status text
                ActiveText = await _localizationService.GetTranslationAsync("common.active") ?? "Active";
                InactiveText = await _localizationService.GetTranslationAsync("common.inactive") ?? "Inactive";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading localized texts for CategoryViewModel");
            }
        }

        public async Task RefreshTranslationsAsync()
        {
            await LoadLocalizedTextsAsync();
        }

        private async Task ExportAsync()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    FileName = $"Categories_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    DefaultExt = ".csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Exporting categories...";

                    var allCategories = await _productService.GetAllCategoriesAsync();
                    var csv = new StringBuilder();
                    csv.AppendLine("Name,NameArabic,Description,ParentCategoryName,DisplayOrder,IsActive");

                    foreach (var category in allCategories)
                    {
                        // Add visual indentation for subcategories to make hierarchy clear
                        var displayName = string.IsNullOrEmpty(category.ParentCategoryName) 
                            ? category.Name 
                            : $"  ↳ {category.Name}";
                            
                        csv.AppendLine($"\"{displayName}\"," +
                                     $"\"{category.NameArabic ?? ""}\"," +
                                     $"\"{category.Description ?? ""}\"," +
                                     $"\"{category.ParentCategoryName ?? ""}\"," +
                                     $"{category.DisplayOrder}," +
                                     $"{category.IsActive}");
                    }

                    await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                    StatusMessage = $"Exported {allCategories.Count()} categories successfully";
                    
                    var successDialog = new MessageDialog(
                        "Export Successful",
                        $"Successfully exported {allCategories.Count()} categories to:\n\n{saveFileDialog.FileName}",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error exporting categories: {ex.Message}";
                
                var errorDialog = new MessageDialog(
                    "Export Error",
                    $"An error occurred while exporting categories:\n\n{ex.Message}",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
                
                _logger?.LogError(ex, "Error exporting categories");
            }
            finally
            {
                IsLoading = false;
            }
        }

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
                        FileName = "Categories_Template.csv",
                        DefaultExt = ".csv"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var templateCsv = new StringBuilder();
                        templateCsv.AppendLine("Name,NameArabic,Description,ParentCategoryName,DisplayOrder,IsActive");
                        templateCsv.AppendLine("Electronics,الإلكترونيات,Electronic devices and accessories,,1,true");
                        templateCsv.AppendLine("  ↳ Laptops,اللابتوب,Portable computers,Electronics,1,true");
                        templateCsv.AppendLine("  ↳ Smartphones,الهواتف الذكية,Mobile phones,Electronics,2,true");
                        templateCsv.AppendLine("Clothing,الملابس,Apparel and fashion,,2,true");
                        templateCsv.AppendLine("  ↳ Men's Wear,ملابس رجالية,Men's clothing,Clothing,1,true");
                        templateCsv.AppendLine("  ↳ Women's Wear,ملابس نسائية,Women's clothing,Clothing,2,true");

                        await File.WriteAllTextAsync(saveFileDialog.FileName, templateCsv.ToString());
                        
                        var successDialog = new MessageDialog(
                            "Template Downloaded",
                            $"Template downloaded successfully to:\n\n{saveFileDialog.FileName}\n\n" +
                            "📝 Template Instructions:\n" +
                            "• ParentCategoryName: Leave empty for main categories\n" +
                            "• For subcategories: Enter the exact parent category name\n" +
                            "• Use '  ↳ ' prefix for subcategories (optional, for visual clarity)\n" +
                            "• DisplayOrder: Number to control display sequence\n\n" +
                            "Please fill in your data and use the Import function again to upload it.",
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
                    StatusMessage = "Importing categories...";

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

                    // First pass: Load existing categories to resolve parent names
                    var existingCategories = await _productService.GetAllCategoriesAsync();
                    var categoryLookup = existingCategories.ToDictionary(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase);

                    // Second pass: Import in two phases - parents first, then children
                    var parentCategories = new List<(int lineNumber, CategoryDto category)>();
                    var childCategories = new List<(int lineNumber, CategoryDto category, string parentName)>();

                    // Parse all lines first
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
                                errors.AppendLine($"Line {i + 1}: Invalid format (expected 6 columns: Name,NameArabic,Description,ParentCategoryName,DisplayOrder,IsActive)");
                                continue;
                            }

                            // Clean up the name (remove visual indentation markers)
                            var name = values[0].Trim('"').Replace("↳", "").Trim();
                            var parentName = values[3].Trim('"');

                            var categoryDto = new CategoryDto
                            {
                                Name = name,
                                NameArabic = values[1].Trim('"'),
                                Description = values[2].Trim('"'),
                                DisplayOrder = int.Parse(values[4]),
                                IsActive = bool.Parse(values[5])
                            };

                            // Separate parent and child categories
                            if (string.IsNullOrWhiteSpace(parentName))
                            {
                                parentCategories.Add((i + 1, categoryDto));
                            }
                            else
                            {
                                childCategories.Add((i + 1, categoryDto, parentName));
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            var errorMessage = $"Parse error - {ex.Message}";
                            
                            // Include inner exception details if available
                            if (ex.InnerException != null)
                            {
                                errorMessage += $" | Inner: {ex.InnerException.Message}";
                                
                                if (ex.InnerException.InnerException != null)
                                {
                                    errorMessage += $" | Details: {ex.InnerException.InnerException.Message}";
                                }
                            }
                            
                            errors.AppendLine($"Line {i + 1}: {errorMessage}");
                        }
                    }

                    // Phase 1: Create parent categories first
                    foreach (var (lineNumber, categoryDto) in parentCategories)
                    {
                        try
                        {
                            var created = await _productService.CreateCategoryAsync(categoryDto);
                            if (created != null)
                            {
                                categoryLookup[created.Name] = created.Id;
                                successCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            var errorMessage = ex.Message;
                            
                            // Include inner exception details if available
                            if (ex.InnerException != null)
                            {
                                errorMessage += $" | Inner: {ex.InnerException.Message}";
                                
                                if (ex.InnerException.InnerException != null)
                                {
                                    errorMessage += $" | Details: {ex.InnerException.InnerException.Message}";
                                }
                            }
                            
                            errors.AppendLine($"Line {lineNumber}: {errorMessage}");
                        }
                    }

                    // Phase 2: Create child categories with resolved parent IDs
                    foreach (var (lineNumber, categoryDto, parentName) in childCategories)
                    {
                        try
                        {
                            if (categoryLookup.TryGetValue(parentName, out int parentId))
                            {
                                categoryDto.ParentCategoryId = parentId;
                                await _productService.CreateCategoryAsync(categoryDto);
                                successCount++;
                            }
                            else
                            {
                                errorCount++;
                                errors.AppendLine($"Line {lineNumber}: Parent category '{parentName}' not found. Make sure parent categories are created first.");
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            var errorMessage = ex.Message;
                            
                            // Include inner exception details if available
                            if (ex.InnerException != null)
                            {
                                errorMessage += $" | Inner: {ex.InnerException.Message}";
                                
                                if (ex.InnerException.InnerException != null)
                                {
                                    errorMessage += $" | Details: {ex.InnerException.InnerException.Message}";
                                }
                            }
                            
                            errors.AppendLine($"Line {lineNumber}: {errorMessage}");
                        }
                    }

                    await LoadCategoriesAsync();

                    var message = $"Import completed:\n\n✓ {successCount} categories imported successfully";
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
                StatusMessage = $"Error importing categories: {ex.Message}";
                
                var errorDialog = new MessageDialog(
                    "Import Error",
                    $"An error occurred while importing categories:\n\n{ex.Message}",
                    MessageDialog.MessageType.Error);
                errorDialog.ShowDialog();
                
                _logger?.LogError(ex, "Error importing categories");
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

        #endregion
    }

    public partial class CategoryHierarchyItem : ObservableObject
    {
        [ObservableProperty]
        private CategoryDto? category;

        [ObservableProperty]
        private ObservableCollection<CategoryHierarchyItem> subCategories = new();

        [ObservableProperty]
        private bool isExpanded = true;

        [ObservableProperty]
        private ObservableCollection<string> discountPills = new();

        public string Name => Category?.Name ?? string.Empty;
        public string Description => Category?.Description ?? string.Empty;
        public bool HasSubCategories => SubCategories.Any();

        public ICommand ToggleExpandCommand => new RelayCommand(ToggleExpand);

        private void ToggleExpand()
        {
            IsExpanded = !IsExpanded;
        }
    }
}
