using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for ProductManagementView.xaml
/// </summary>
public partial class ProductManagementView : UserControl
{
    public ProductManagementView()
    {
        InitializeComponent();
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
