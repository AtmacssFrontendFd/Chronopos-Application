using ChronoPos.Application.DTOs;
using System.Windows;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Interaction logic for ReservationDetailsPopup.xaml
/// </summary>
public partial class ReservationDetailsPopup : Window
{
    public ReservationDetailsPopup(ReservationDto reservation, 
        Action<ReservationDto> onEdit,
        Func<ReservationDto, Task> onConfirm,
        Func<ReservationDto, Task> onCheckIn,
        Func<ReservationDto, Task> onComplete,
        Func<ReservationDto, Task> onCancel)
    {
        InitializeComponent();
        DataContext = new ReservationDetailsViewModel(reservation, onEdit, onConfirm, onCheckIn, onComplete, onCancel, Close);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
