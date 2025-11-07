using System.Windows.Controls;
using ChronoPos.Desktop.ViewModels;

namespace ChronoPos.Desktop.Views;

public partial class InventoryReportView : UserControl
{
    public InventoryReportView()
    {
        InitializeComponent();
    }

    private async void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is InventoryReportViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }
}
