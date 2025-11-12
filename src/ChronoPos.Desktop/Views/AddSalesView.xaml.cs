using System.Windows.Controls;
using System.Windows.Input;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for AddSalesView.xaml
/// </summary>
public partial class AddSalesView : UserControl
{
    public AddSalesView()
    {
        InitializeComponent();
        
        // Auto-focus the barcode textbox when the view loads
        Loaded += (s, e) =>
        {
            BarcodeTextBox?.Focus();
        };
    }
    
    /// <summary>
    /// Handle Enter key press in barcode textbox to trigger barcode search
    /// </summary>
    private void BarcodeTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            // Prevent the Enter key from being processed further
            e.Handled = true;
            
            // Get the ViewModel and execute the SearchByBarcode command
            if (DataContext is AddSalesViewModel viewModel)
            {
                if (viewModel.SearchByBarcodeCommand.CanExecute(null))
                {
                    viewModel.SearchByBarcodeCommand.Execute(null);
                }
            }
            
            // Keep focus on the textbox for next scan
            BarcodeTextBox?.Focus();
        }
    }
}
