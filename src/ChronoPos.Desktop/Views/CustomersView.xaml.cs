using System.Windows.Controls;
using System.Windows.Input;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for CustomersView.xaml
/// </summary>
public partial class CustomersView : UserControl
{
    public CustomersView()
    {
        InitializeComponent();
    }

    private void CustomerDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is CustomersViewModel viewModel)
        {
            viewModel.ViewCustomerTransactionsCommand.Execute(null);
        }
    }
}