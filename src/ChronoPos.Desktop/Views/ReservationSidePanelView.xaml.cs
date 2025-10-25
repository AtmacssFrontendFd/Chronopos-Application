using ChronoPos.Desktop.ViewModels;
using System.Windows.Controls;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for ReservationSidePanelView.xaml
/// </summary>
public partial class ReservationSidePanelView : UserControl
{
    public ReservationSidePanelView()
    {
        InitializeComponent();
    }

    public ReservationSidePanelView(ReservationSidePanelViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
