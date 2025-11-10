using ChronoPos.Desktop.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ChronoPos.Desktop.Views;

public partial class CustomerReportView : UserControl
{
    private CustomerReportViewModel? _viewModel;

    public CustomerReportView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CustomerReportViewModel viewModel)
        {
            _viewModel = viewModel;
            await viewModel.InitializeAsync();
        }
    }
}
