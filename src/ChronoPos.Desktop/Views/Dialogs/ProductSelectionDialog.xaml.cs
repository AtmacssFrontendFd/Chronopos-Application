using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Services;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Views.Dialogs
{
    /// <summary>
    /// Product Selection Dialog for choosing Product Units, Combinations, Modifiers, and Groups
    /// </summary>
    public partial class ProductSelectionDialog : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProductUnitService _productUnitService;
        private readonly IProductModifierGroupService _modifierGroupService;
        private readonly IProductModifierGroupItemService _modifierGroupItemService;
        private readonly IProductModifierLinkService _modifierLinkService;
        private readonly IProductGroupService _productGroupService;
        private readonly IProductGroupItemService _productGroupItemService;
        private readonly IProductService _productService;
        private readonly IActiveCurrencyService _activeCurrencyService;
        private readonly IProductCombinationItemService _combinationItemService;

        private readonly ProductDto _product;
        private decimal _basePrice;
        private decimal _totalPrice;
        
        // Selected options
        private ProductUnitDto? _selectedProductUnit;
        private List<ProductModifierGroupItemDto> _selectedModifiers = new();
        private ProductGroupDto? _selectedProductGroup;
        
        // Track modifier groups for validation
        private List<ProductModifierGroupDto> _modifierGroups = new();
        
        public ProductSelectionResult? SelectionResult { get; private set; }

        public ProductSelectionDialog(
            IServiceProvider serviceProvider,
            ProductDto product)
        {
            AppLogger.LogSeparator("PRODUCT SELECTION DIALOG CONSTRUCTOR", "product_selection_dialog");
            AppLogger.Log($"Creating dialog for product: {product?.Name ?? "NULL"}", filename: "product_selection_dialog");
            
            InitializeComponent();
            
            _serviceProvider = serviceProvider;
            _product = product;
            _basePrice = product.Price;
            _totalPrice = _basePrice;
            
            AppLogger.Log($"Base price: {_basePrice}", filename: "product_selection_dialog");
            
            try
            {
                // Get services
                AppLogger.Log("Getting required services...", filename: "product_selection_dialog");
                _productUnitService = serviceProvider.GetRequiredService<IProductUnitService>();
                _modifierGroupService = serviceProvider.GetRequiredService<IProductModifierGroupService>();
                _modifierGroupItemService = serviceProvider.GetRequiredService<IProductModifierGroupItemService>();
                _modifierLinkService = serviceProvider.GetRequiredService<IProductModifierLinkService>();
                _productGroupService = serviceProvider.GetRequiredService<IProductGroupService>();
                _productGroupItemService = serviceProvider.GetRequiredService<IProductGroupItemService>();
                _productService = serviceProvider.GetRequiredService<IProductService>();
                _activeCurrencyService = serviceProvider.GetRequiredService<IActiveCurrencyService>();
                _combinationItemService = serviceProvider.GetRequiredService<IProductCombinationItemService>();
                
                AppLogger.Log("All services obtained successfully", filename: "product_selection_dialog");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Error getting services in ProductSelectionDialog constructor", ex, filename: "product_selection_dialog");
                throw;
            }
            
            // Set header
            ProductNameText.Text = product.Name;
            ProductPriceText.Text = $"Base Price: {FormatPrice(_basePrice)}";
            
            AppLogger.Log("Calling LoadProductOptionsAsync...", filename: "product_selection_dialog");
            _ = LoadProductOptionsAsync();
        }

        private async System.Threading.Tasks.Task LoadProductOptionsAsync()
        {
            AppLogger.Log("=== LoadProductOptionsAsync started ===", filename: "product_selection_dialog");
            
            try
            {
                bool hasAnyOptions = false;

                // Load Product Units
                AppLogger.Log($"Loading product units for ProductId={_product.Id}...", filename: "product_selection_dialog");
                var productUnits = await _productUnitService.GetByProductIdAsync(_product.Id);
                AppLogger.Log($"Product units loaded: Count={productUnits?.Count() ?? 0}", filename: "product_selection_dialog");
                
                if (productUnits != null && productUnits.Any())
                {
                    AppLogger.Log("Showing ProductUnitsTab", filename: "product_selection_dialog");
                    ProductUnitsTab.Visibility = Visibility.Visible;
                    hasAnyOptions = true;
                    PopulateProductUnits(productUnits);
                }

                // Load Product Combinations (based on ProductUnits with AttributeValues)
                // Product combinations are linked to product units
                bool hasCombinations = false;
                if (productUnits != null && productUnits.Any())
                {
                    AppLogger.Log("Checking for product combinations...", filename: "product_selection_dialog");
                    var productUnitIds = productUnits.Select(u => u.Id).ToList();
                    var combinationItems = await _combinationItemService.GetCombinationItemsByProductUnitIdsAsync(productUnitIds);
                    hasCombinations = combinationItems != null && combinationItems.Any();
                    AppLogger.Log($"Product combinations found: {hasCombinations}, Count={combinationItems?.Count() ?? 0}", filename: "product_selection_dialog");
                }

                if (hasCombinations)
                {
                    ProductCombinationsTab.Visibility = Visibility.Visible;
                    hasAnyOptions = true;
                    // PopulateProductCombinations would go here
                    // TODO: Implement PopulateProductCombinations
                }

                // Load Product Modifiers
                AppLogger.Log($"Loading modifier links for ProductId={_product.Id}...", filename: "product_selection_dialog");
                var modifierLinks = await _modifierLinkService.GetByProductIdAsync(_product.Id);
                AppLogger.Log($"Modifier links loaded: Count={modifierLinks?.Count() ?? 0}", filename: "product_selection_dialog");
                
                if (modifierLinks != null && modifierLinks.Any())
                {
                    AppLogger.Log("Showing ProductModifiersTab", filename: "product_selection_dialog");
                    ProductModifiersTab.Visibility = Visibility.Visible;
                    hasAnyOptions = true;
                    await PopulateProductModifiersAsync(modifierLinks);
                }

                // Load Product Groups (groups that contain this product)
                // TODO: Implement GetGroupsByProductIdAsync method in IProductGroupService
                // For now, skip product groups
                var productGroups = new List<ProductGroupDto>(); // await _productGroupService.GetGroupsByProductIdAsync(_product.Id);
                if (productGroups != null && productGroups.Any())
                {
                    ProductGroupsTab.Visibility = Visibility.Visible;
                    hasAnyOptions = true;
                    PopulateProductGroups(productGroups);
                }

                // If no options available, this shouldn't happen as we check before opening dialog
                // But handle it gracefully just in case
                AppLogger.Log($"hasAnyOptions: {hasAnyOptions}", filename: "product_selection_dialog");
                
                if (!hasAnyOptions)
                {
                    AppLogger.LogWarning("No options available in dialog - this shouldn't happen", filename: "product_selection_dialog");
                    
                    // No options available, set result and let caller handle
                    SelectionResult = new ProductSelectionResult
                    {
                        Product = _product,
                        ProductUnit = null,
                        SelectedModifiers = new List<ProductModifierGroupItemDto>(),
                        ProductGroup = null,
                        FinalPrice = _basePrice
                    };
                    // Don't try to set DialogResult here - window not shown yet
                    // Just return and dialog will show empty, user can cancel
                    return;
                }

                AppLogger.Log("Options available, selecting first visible tab...", filename: "product_selection_dialog");
                
                // Select first visible tab
                if (ProductUnitsTab.Visibility == Visibility.Visible)
                {
                    AppLogger.Log("Selecting ProductUnitsTab", filename: "product_selection_dialog");
                    OptionsTabControl.SelectedItem = ProductUnitsTab;
                }
                else if (ProductCombinationsTab.Visibility == Visibility.Visible)
                {
                    AppLogger.Log("Selecting ProductCombinationsTab", filename: "product_selection_dialog");
                    OptionsTabControl.SelectedItem = ProductCombinationsTab;
                }
                else if (ProductModifiersTab.Visibility == Visibility.Visible)
                {
                    AppLogger.Log("Selecting ProductModifiersTab", filename: "product_selection_dialog");
                    OptionsTabControl.SelectedItem = ProductModifiersTab;
                }
                else if (ProductGroupsTab.Visibility == Visibility.Visible)
                {
                    AppLogger.Log("Selecting ProductGroupsTab", filename: "product_selection_dialog");
                    OptionsTabControl.SelectedItem = ProductGroupsTab;
                }

                UpdateTotalPrice();
                AppLogger.Log("LoadProductOptionsAsync completed successfully", filename: "product_selection_dialog");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Error loading product options", ex, filename: "product_selection_dialog");
                MessageBox.Show($"Error loading product options: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Product Units

        private void PopulateProductUnits(IEnumerable<ProductUnitDto> productUnits)
        {
            ProductUnitsPanel.Children.Clear();
            
            // Add header text
            var headerText = new TextBlock
            {
                Text = "Select a unit of measurement:",
                FontSize = 14,
                Foreground = (Brush)FindResource("SecondaryText"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            ProductUnitsPanel.Children.Add(headerText);

            foreach (var unit in productUnits)
            {
                var button = new Button
                {
                    Style = (Style)FindResource("ProductUnitItemButton"),
                    Tag = unit
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Unit name and quantity
                var namePanel = new StackPanel();
                var nameText = new TextBlock
                {
                    Text = unit.UnitName ?? "Unknown Unit",
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("PrimaryText")
                };
                var qtyText = new TextBlock
                {
                    Text = $"Quantity: {unit.QtyInUnit}",
                    FontSize = 12,
                    Foreground = (Brush)FindResource("SecondaryText"),
                    Margin = new Thickness(0, 2, 0, 0)
                };
                namePanel.Children.Add(nameText);
                namePanel.Children.Add(qtyText);

                // Price
                var priceText = new TextBlock
                {
                    Text = FormatPrice(unit.PriceOfUnit),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = (Brush)FindResource("ChronoPosGold"),
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(namePanel, 0);
                Grid.SetColumn(priceText, 1);
                grid.Children.Add(namePanel);
                grid.Children.Add(priceText);

                button.Content = grid;
                button.Click += ProductUnitButton_Click;

                ProductUnitsPanel.Children.Add(button);
            }
        }

        private void ProductUnitButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductUnitDto unit)
            {
                _selectedProductUnit = unit;
                _basePrice = unit.PriceOfUnit;
                UpdateTotalPrice();
                
                // Visual feedback
                foreach (var child in ProductUnitsPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.BorderBrush = (Brush)FindResource("BorderMedium");
                        btn.BorderThickness = new Thickness(1);
                    }
                }
                button.BorderBrush = (Brush)FindResource("ChronoPosGold");
                button.BorderThickness = new Thickness(2);
            }
        }

        #endregion

        #region Product Modifiers

        private async System.Threading.Tasks.Task PopulateProductModifiersAsync(IEnumerable<ProductModifierLinkDto> modifierLinks)
        {
            ProductModifiersPanel.Children.Clear();
            _modifierGroups.Clear(); // Reset modifier groups list

            // Add header text
            var headerText = new TextBlock
            {
                Text = "Customize your product:",
                FontSize = 14,
                Foreground = (Brush)FindResource("SecondaryText"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            ProductModifiersPanel.Children.Add(headerText);

            foreach (var link in modifierLinks)
            {
                // Get the modifier group
                var modifierGroup = await _modifierGroupService.GetByIdAsync(link.ModifierGroupId);
                if (modifierGroup == null) continue;
                
                // Store modifier group for validation
                _modifierGroups.Add(modifierGroup);

                // Get the group items
                var groupItems = await _modifierGroupItemService.GetByGroupIdAsync(modifierGroup.Id);
                if (groupItems == null || !groupItems.Any()) continue;

                // Create expander for this modifier group
                var expander = new Expander
                {
                    Header = CreateModifierGroupHeader(modifierGroup),
                    Style = (Style)FindResource("ModifierGroupExpander"),
                    IsExpanded = modifierGroup.Required
                };

                var itemsPanel = new StackPanel { Margin = new Thickness(10) };

                foreach (var item in groupItems)
                {
                    Control itemControl;
                    
                    if (modifierGroup.SelectionType == "Single")
                    {
                        // Use RadioButton for single selection
                        var radioButton = new RadioButton
                        {
                            Content = CreateModifierItemContent(item),
                            Margin = new Thickness(0, 5, 0, 5),
                            Tag = item,
                            GroupName = $"ModifierGroup_{modifierGroup.Id}"
                        };
                        radioButton.Checked += ModifierCheckBox_Changed;
                        radioButton.Unchecked += ModifierCheckBox_Changed;
                        itemControl = radioButton;
                    }
                    else
                    {
                        // Use CheckBox for multiple selection
                        var checkBox = new CheckBox
                        {
                            Content = CreateModifierItemContent(item),
                            Margin = new Thickness(0, 5, 0, 5),
                            Tag = item
                        };
                        checkBox.Checked += ModifierCheckBox_Changed;
                        checkBox.Unchecked += ModifierCheckBox_Changed;
                        itemControl = checkBox;
                    }

                    itemsPanel.Children.Add(itemControl);
                }

                expander.Content = itemsPanel;
                ProductModifiersPanel.Children.Add(expander);
            }
        }

        private object CreateModifierGroupHeader(ProductModifierGroupDto group)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var nameText = new TextBlock
            {
                Text = group.Name,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("PrimaryText")
            };

            panel.Children.Add(nameText);

            if (group.Required)
            {
                var requiredBadge = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                    CornerRadius = new CornerRadius(3),
                    Padding = new Thickness(6, 2, 6, 2),
                    Margin = new Thickness(8, 0, 0, 0)
                };
                var requiredText = new TextBlock
                {
                    Text = "Required",
                    FontSize = 10,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.SemiBold
                };
                requiredBadge.Child = requiredText;
                panel.Children.Add(requiredBadge);
            }

            return panel;
        }

        private object CreateModifierItemContent(ProductModifierGroupItemDto item)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var nameText = new TextBlock
            {
                Text = item.ModifierName ?? "Unknown Modifier",
                FontSize = 13,
                Foreground = (Brush)FindResource("PrimaryText")
            };

            // Use FinalPrice which includes ModifierPrice + PriceAdjustment
            var priceText = new TextBlock
            {
                Text = item.FinalPrice != 0 ? $"+{FormatPrice(item.FinalPrice)}" : "Free",
                FontSize = 12,
                Foreground = (Brush)FindResource("ChronoPosGold"),
                Margin = new Thickness(10, 0, 0, 0)
            };

            Grid.SetColumn(nameText, 0);
            Grid.SetColumn(priceText, 1);
            grid.Children.Add(nameText);
            grid.Children.Add(priceText);

            return grid;
        }

        private void ModifierCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is Control control && control.Tag is ProductModifierGroupItemDto item)
            {
                bool isChecked = false;
                
                if (control is CheckBox checkBox)
                {
                    isChecked = checkBox.IsChecked == true;
                }
                else if (control is RadioButton radioButton)
                {
                    isChecked = radioButton.IsChecked == true;
                }
                
                AppLogger.Log($"Modifier changed: {item.ModifierName}, IsChecked: {isChecked}, PriceAdjustment: {item.PriceAdjustment}", filename: "product_selection_dialog");
                
                if (isChecked)
                {
                    if (!_selectedModifiers.Any(m => m.Id == item.Id))
                    {
                        _selectedModifiers.Add(item);
                        AppLogger.Log($"Modifier added to selection. Total modifiers: {_selectedModifiers.Count}", filename: "product_selection_dialog");
                    }
                }
                else
                {
                    _selectedModifiers.RemoveAll(m => m.Id == item.Id);
                    AppLogger.Log($"Modifier removed from selection. Total modifiers: {_selectedModifiers.Count}", filename: "product_selection_dialog");
                }

                UpdateTotalPrice();
            }
        }

        #endregion

        #region Product Groups

        private void PopulateProductGroups(IEnumerable<ProductGroupDto> productGroups)
        {
            ProductGroupsPanel.Children.Clear();

            // Add header text
            var headerText = new TextBlock
            {
                Text = "Select a product group:",
                FontSize = 14,
                Foreground = (Brush)FindResource("SecondaryText"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            ProductGroupsPanel.Children.Add(headerText);

            foreach (var group in productGroups)
            {
                var button = new Button
                {
                    Style = (Style)FindResource("ProductUnitItemButton"),
                    Tag = group
                };

                var grid = new Grid();
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var namePanel = new StackPanel();
                var nameText = new TextBlock
                {
                    Text = group.Name,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("PrimaryText")
                };
                var descText = new TextBlock
                {
                    Text = group.Description ?? "",
                    FontSize = 12,
                    Foreground = (Brush)FindResource("SecondaryText"),
                    Margin = new Thickness(0, 2, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                namePanel.Children.Add(nameText);
                if (!string.IsNullOrEmpty(group.Description))
                {
                    namePanel.Children.Add(descText);
                }

                Grid.SetColumn(namePanel, 0);
                grid.Children.Add(namePanel);

                button.Content = grid;
                button.Click += ProductGroupButton_Click;

                ProductGroupsPanel.Children.Add(button);
            }
        }

        private void ProductGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductGroupDto group)
            {
                _selectedProductGroup = group;
                
                // Visual feedback
                foreach (var child in ProductGroupsPanel.Children)
                {
                    if (child is Button btn)
                    {
                        btn.BorderBrush = (Brush)FindResource("BorderMedium");
                        btn.BorderThickness = new Thickness(1);
                    }
                }
                button.BorderBrush = (Brush)FindResource("ChronoPosGold");
                button.BorderThickness = new Thickness(2);
            }
        }

        #endregion

        #region Price Calculation

        private void UpdateTotalPrice()
        {
            _totalPrice = _basePrice;

            // Add modifier prices (use FinalPrice which includes base modifier price + adjustment)
            foreach (var modifier in _selectedModifiers)
            {
                AppLogger.Log($"Adding modifier price: {modifier.ModifierName} = {modifier.FinalPrice}", filename: "product_selection_dialog");
                _totalPrice += modifier.FinalPrice;
            }

            AppLogger.Log($"Price calculation: Base={_basePrice}, Modifiers Total={_totalPrice - _basePrice}, Final={_totalPrice}", filename: "product_selection_dialog");
            
            TotalPriceText.Text = FormatPrice(_totalPrice);
            ProductPriceText.Text = $"Base Price: {FormatPrice(_basePrice)}";
        }

        private string FormatPrice(decimal price)
        {
            var convertedPrice = _activeCurrencyService.ConvertFromBaseCurrency(price);
            return _activeCurrencyService.FormatPrice(convertedPrice);
        }

        #endregion

        #region Button Handlers

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate required modifiers
            AppLogger.Log("AddToCart clicked - Validating required modifiers", filename: "product_selection_dialog");
            
            var validationErrors = new List<string>();
            
            foreach (var modifierGroup in _modifierGroups)
            {
                // Get selected modifiers for this group
                var selectedInGroup = _selectedModifiers
                    .Where(m => m.GroupId == modifierGroup.Id)
                    .ToList();
                
                AppLogger.Log($"Modifier Group: {modifierGroup.Name}, Required: {modifierGroup.Required}, MinSelections: {modifierGroup.MinSelections}, MaxSelections: {modifierGroup.MaxSelections}, Selected: {selectedInGroup.Count}", filename: "product_selection_dialog");
                
                // Check if required group has no selections
                if (modifierGroup.Required && selectedInGroup.Count == 0)
                {
                    validationErrors.Add($"'{modifierGroup.Name}' is required. Please make a selection.");
                }
                
                // Check minimum selections
                if (modifierGroup.MinSelections > 0 && selectedInGroup.Count < modifierGroup.MinSelections)
                {
                    validationErrors.Add($"'{modifierGroup.Name}' requires at least {modifierGroup.MinSelections} selection(s). You selected {selectedInGroup.Count}.");
                }
                
                // Check maximum selections
                if (modifierGroup.MaxSelections.HasValue && selectedInGroup.Count > modifierGroup.MaxSelections.Value)
                {
                    validationErrors.Add($"'{modifierGroup.Name}' allows maximum {modifierGroup.MaxSelections.Value} selection(s). You selected {selectedInGroup.Count}.");
                }
            }
            
            if (validationErrors.Any())
            {
                AppLogger.Log($"Validation failed with {validationErrors.Count} error(s)", filename: "product_selection_dialog");
                var errorMessage = "Please fix the following issues before adding to cart:\n\n" + 
                                 string.Join("\n", validationErrors.Select((e, i) => $"{i + 1}. {e}"));
                
                MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            AppLogger.Log("Validation passed - Adding to cart", filename: "product_selection_dialog");

            SelectionResult = new ProductSelectionResult
            {
                Product = _product,
                ProductUnit = _selectedProductUnit,
                SelectedModifiers = _selectedModifiers,
                ProductGroup = _selectedProductGroup,
                FinalPrice = _totalPrice
            };

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        #endregion
    }

    /// <summary>
    /// Result returned from Product Selection Dialog
    /// </summary>
    public class ProductSelectionResult
    {
        public ProductDto Product { get; set; } = null!;
        public ProductUnitDto? ProductUnit { get; set; }
        public List<ProductModifierGroupItemDto> SelectedModifiers { get; set; } = new();
        public ProductGroupDto? ProductGroup { get; set; }
        public decimal FinalPrice { get; set; }
    }
}
