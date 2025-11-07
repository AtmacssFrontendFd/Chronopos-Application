using ChronoPos.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for ReportsHubView.xaml
/// </summary>
public partial class ReportsHubView : UserControl
{
    public ReportsHubView()
    {
        InitializeComponent();
    }

    private void SalesReportCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ReportsHubViewModel viewModel)
        {
            viewModel.NavigateToSalesReportCommand.Execute(null);
        }
    }

    private void InventoryReportCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ReportsHubViewModel viewModel)
        {
            viewModel.NavigateToInventoryReportCommand.Execute(null);
        }
    }

    private void CustomerReportCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ReportsHubViewModel viewModel)
        {
            viewModel.NavigateToCustomerReportCommand.Execute(null);
        }
    }
}
