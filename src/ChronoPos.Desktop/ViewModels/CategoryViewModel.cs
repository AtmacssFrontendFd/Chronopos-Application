using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using ChronoPos.Desktop.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        private readonly IProductService _productService;
        private readonly IDiscountService _discountService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CategoryViewModel> _logger;
        private readonly ICurrentUserService _currentUserService;
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

        public CategoryViewModel(
            IProductService productService,
            IDiscountService discountService,
            IServiceProvider serviceProvider,
            ILogger<CategoryViewModel> logger,
            ICurrentUserService currentUserService,
            Action? navigateBack = null)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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

            // Subscribe to property changes for search
            PropertyChanged += OnPropertyChanged;

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
                MessageBox.Show("Failed to load categories. Please try again.", "Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Failed to open add category panel. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Failed to open edit category panel. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Failed to open add subcategory panel. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                if (hasSubCategories)
                {
                    message = $"Category '{categoryItem.Category.Name}' has subcategories. Deleting it will also remove all subcategories. Are you sure?";
                }
                else
                {
                    message = $"Are you sure you want to delete category '{categoryItem.Category.Name}'?";
                }

                var result = MessageBox.Show(message, "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes)
                    return;

                IsLoading = true;
                StatusMessage = "Deleting category...";

                await _productService.DeleteCategoryAsync(categoryItem.Category.Id);
                await LoadCategoriesAsync();

                StatusMessage = $"Category '{categoryItem.Category.Name}' deleted successfully";
                _logger?.LogInformation("Category deleted successfully: {CategoryName} (ID: {CategoryId})", 
                    categoryItem.Category.Name, categoryItem.Category.Id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting category: {CategoryId}", categoryItem.Category.Id);
                MessageBox.Show($"Failed to delete category. {ex.Message}", "Delete Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            }
            catch (Exception)
            {
                // Fail-secure: all permissions default to false
                CanCreateCategory = false;
                CanEditCategory = false;
                CanDeleteCategory = false;
            }
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
