using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.Views.Dialogs
{
    /// <summary>
    /// Product Group Selection Dialog - Shows all product groups with their items
    /// </summary>
    public partial class ProductGroupSelectionDialog : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IProductGroupItemService _productGroupItemService;
        private readonly IProductService _productService;
        private readonly IProductUnitService _productUnitService;
        private readonly IActiveCurrencyService _activeCurrencyService;

        public ProductGroupDto? SelectedGroup { get; private set; }
        public List<ProductGroupItemWithDetails> GroupItemsWithDetails { get; private set; }

        public ProductGroupSelectionDialog(
            IServiceProvider serviceProvider,
            IEnumerable<ProductGroupDto> productGroups)
        {
            AppLogger.Log("Creating ProductGroupSelectionDialog", filename: "product_group_selection");
            
            InitializeComponent();
            
            _serviceProvider = serviceProvider;
            
            try
            {
                // Get services
                _productGroupItemService = serviceProvider.GetRequiredService<IProductGroupItemService>();
                _productService = serviceProvider.GetRequiredService<IProductService>();
                _productUnitService = serviceProvider.GetRequiredService<IProductUnitService>();
                _activeCurrencyService = serviceProvider.GetRequiredService<IActiveCurrencyService>();
                
                GroupItemsWithDetails = new List<ProductGroupItemWithDetails>();
                
                // Populate groups
                PopulateProductGroups(productGroups);
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Error in ProductGroupSelectionDialog constructor", ex, filename: "product_group_selection");
                throw;
            }
        }

        private void PopulateProductGroups(IEnumerable<ProductGroupDto> productGroups)
        {
            ProductGroupsPanel.Children.Clear();
            
            var headerText = new TextBlock
            {
                Text = "Available Product Groups:",
                FontSize = 14,
                Foreground = (Brush)FindResource("SecondaryText"),
                Margin = new Thickness(0, 0, 0, 15)
            };
            ProductGroupsPanel.Children.Add(headerText);

            foreach (var group in productGroups.Where(g => g.Status == "Active"))
            {
                var card = new Border
                {
                    Style = (Style)FindResource("ProductGroupCard"),
                    Tag = group
                };

                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                // Group Header
                var headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var nameText = new TextBlock
                {
                    Text = group.Name,
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = (Brush)FindResource("PrimaryText")
                };

                var iconText = new TextBlock
                {
                    Text = "→",
                    FontSize = 20,
                    Foreground = (Brush)FindResource("ChronoPosGold"),
                    VerticalAlignment = VerticalAlignment.Center
                };

                Grid.SetColumn(nameText, 0);
                Grid.SetColumn(iconText, 1);
                headerGrid.Children.Add(nameText);
                headerGrid.Children.Add(iconText);

                // Group Description
                var descText = new TextBlock
                {
                    Text = group.Description ?? "No description available",
                    FontSize = 13,
                    Foreground = (Brush)FindResource("SecondaryText"),
                    Margin = new Thickness(0, 8, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };

                Grid.SetRow(headerGrid, 0);
                Grid.SetRow(descText, 1);
                grid.Children.Add(headerGrid);
                grid.Children.Add(descText);

                card.Child = grid;
                card.MouseLeftButtonDown += async (s, e) => await ProductGroupCard_Click(group);
                
                // Hover effect
                card.MouseEnter += (s, e) =>
                {
                    card.BorderBrush = (Brush)FindResource("ChronoPosGold");
                    card.BorderThickness = new Thickness(2);
                };
                card.MouseLeave += (s, e) =>
                {
                    card.BorderBrush = (Brush)FindResource("BorderMedium");
                    card.BorderThickness = new Thickness(1);
                };

                ProductGroupsPanel.Children.Add(card);
            }
        }

        private async System.Threading.Tasks.Task ProductGroupCard_Click(ProductGroupDto group)
        {
            try
            {
                AppLogger.Log($"Product group clicked: {group.Name}", filename: "product_group_selection");
                
                StatusText.Text = $"Loading items from '{group.Name}'...";
                
                // Get all items in this group
                var groupItems = await _productGroupItemService.GetByProductGroupIdAsync(group.Id);
                
                if (groupItems == null || !groupItems.Any())
                {
                    StatusText.Text = "This group has no products";
                    MessageBox.Show($"The group '{group.Name}' has no products.", "Empty Group", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                AppLogger.Log($"Loading details for {groupItems.Count()} items", filename: "product_group_selection");
                
                // Load details for each item
                GroupItemsWithDetails.Clear();
                foreach (var item in groupItems)
                {
                    if (!item.ProductId.HasValue) continue;
                    
                    var product = await _productService.GetProductByIdAsync(item.ProductId.Value);
                    if (product == null) continue;

                    ProductUnitDto? unit = null;
                    if (item.ProductUnitId.HasValue && item.ProductUnitId.Value > 0)
                    {
                        unit = await _productUnitService.GetByIdAsync(item.ProductUnitId.Value);
                    }

                    GroupItemsWithDetails.Add(new ProductGroupItemWithDetails
                    {
                        GroupItem = item,
                        Product = product,
                        ProductUnit = unit
                    });
                }

                var currency = _activeCurrencyService.ActiveCurrency;
                var currencySymbol = currency?.Symbol ?? "AED";

                // Show confirmation with items
                var itemsList = string.Join("\n", GroupItemsWithDetails.Select(i => 
                {
                    var itemName = i.Product.Name;
                    if (i.ProductUnit != null)
                    {
                        itemName += $" ({i.ProductUnit.UnitName})";
                    }
                    var price = i.ProductUnit?.PriceOfUnit ?? i.Product.Price;
                    return $"• {itemName} - {currencySymbol} {price:F2}";
                }));

                var result = MessageBox.Show(
                    $"Add the following items from '{group.Name}' to cart?\n\n{itemsList}",
                    "Confirm Selection",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SelectedGroup = group;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    StatusText.Text = "Selection cancelled. Choose another group or close.";
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"Error loading group details", ex, filename: "product_group_selection");
                MessageBox.Show($"Error loading group details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Error loading group. Try another group.";
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// Helper class to hold product group item with full details
    /// </summary>
    public class ProductGroupItemWithDetails
    {
        public ProductGroupItemDto GroupItem { get; set; } = null!;
        public ProductDto Product { get; set; } = null!;
        public ProductUnitDto? ProductUnit { get; set; }
    }
}
