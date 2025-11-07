using ChronoPos.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for SalesReportView.xaml
/// </summary>
public partial class SalesReportView : UserControl
{
    public SalesReportView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is SalesReportViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
