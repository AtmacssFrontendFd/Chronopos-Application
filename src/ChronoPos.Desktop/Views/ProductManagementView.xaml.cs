using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ChronoPos.Desktop.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for ProductManagementView.xaml
/// </summary>
public partial class ProductManagementView : UserControl
{
    public ProductManagementView()
    {
        InitializeComponent();
        Loaded += ProductManagementView_Loaded;
        Unloaded += ProductManagementView_Unloaded;
        
        // Add mouse event handlers to ensure ScrollViewer gets focus for mouse wheel scrolling
        MouseEnter += ProductManagementView_MouseEnter;
        PreviewMouseWheel += ProductManagementView_PreviewMouseWheel;
    }

    private void ProductManagementView_MouseEnter(object sender, MouseEventArgs e)
    {
        // Give focus to the main ScrollViewer when mouse enters the view
        MainScrollViewer.Focus();
    }

    private void ProductManagementView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // Ensure the main ScrollViewer handles mouse wheel events
        if (!MainScrollViewer.IsFocused)
        {
            MainScrollViewer.Focus();
        }
        
        // Let the ScrollViewer handle the mouse wheel event
        if (MainScrollViewer.IsMouseOver)
        {
            // Scroll the main ScrollViewer
            if (e.Delta > 0)
            {
                MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset - 50);
            }
            else
            {
                MainScrollViewer.ScrollToVerticalOffset(MainScrollViewer.VerticalOffset + 50);
            }
            e.Handled = true;
        }
    }

    private async void ProductManagementView_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductManagementViewModel viewModel)
        {
            viewModel.CategoryScrollRequested += OnCategoryScrollRequested;
            
            // Force refresh data when view loads to ensure products are displayed
            // This fixes the issue where products don't load on first view open
            if (viewModel.RefreshDataCommand.CanExecute(null))
            {
                await viewModel.RefreshDataCommand.ExecuteAsync(null);
            }
        }
        
        // Set initial focus to enable mouse wheel scrolling
        MainScrollViewer.Focus();
    }

    private void ProductManagementView_Unloaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ProductManagementViewModel viewModel)
        {
            viewModel.CategoryScrollRequested -= OnCategoryScrollRequested;
        }
    }

    private void OnCategoryScrollRequested(string direction)
    {
        const double scrollAmount = 200; // Pixels to scroll
        
        switch (direction)
        {
            case "Left":
                CategoryScrollViewer.ScrollToHorizontalOffset(CategoryScrollViewer.HorizontalOffset - scrollAmount);
                break;
            case "Right":
                CategoryScrollViewer.ScrollToHorizontalOffset(CategoryScrollViewer.HorizontalOffset + scrollAmount);
                break;
        }
    }

    private void SearchTypeDropdown_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // Toggle the popup visibility
        SearchTypePopup.IsOpen = !SearchTypePopup.IsOpen;
    }

    private void SearchTypeOption_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is Button button && DataContext is ProductManagementViewModel viewModel)
        {
            // Set the selected search type
            viewModel.SelectedSearchType = button.Content.ToString() ?? "Product Name";
            
            // Close the popup
            SearchTypePopup.IsOpen = false;
            
            // Focus back to the search box
            SearchBox.Focus();
        }
    }
}
