using ChronoPos.Application.DTOs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ChronoPos.Desktop.Views;

/// <summary>
/// Simple ViewModel for Reservation Details Popup
/// </summary>
public partial class ReservationDetailsViewModel : ObservableObject
{
    private readonly Action<ReservationDto> _onEdit;
    private readonly Func<ReservationDto, Task> _onConfirm;
    private readonly Func<ReservationDto, Task> _onCheckIn;
    private readonly Func<ReservationDto, Task> _onComplete;
    private readonly Func<ReservationDto, Task> _onCancel;
    private readonly Action? _onClose;

    [ObservableProperty]
    private ReservationDto reservation;

    [ObservableProperty]
    private bool canConfirm;

    [ObservableProperty]
    private bool canCheckIn;

    [ObservableProperty]
    private bool canComplete;

    [ObservableProperty]
    private bool canCancelReservation;

    public IRelayCommand EditCommand { get; }
    public IAsyncRelayCommand ConfirmCommand { get; }
    public IAsyncRelayCommand CheckInCommand { get; }
    public IAsyncRelayCommand CompleteCommand { get; }
    public IAsyncRelayCommand CancelReservationCommand { get; }

    public ReservationDetailsViewModel(
        ReservationDto reservation,
        Action<ReservationDto> onEdit,
        Func<ReservationDto, Task> onConfirm,
        Func<ReservationDto, Task> onCheckIn,
        Func<ReservationDto, Task> onComplete,
        Func<ReservationDto, Task> onCancel,
        Action? onClose = null)
    {
        Reservation = reservation ?? throw new ArgumentNullException(nameof(reservation));
        _onEdit = onEdit ?? throw new ArgumentNullException(nameof(onEdit));
        _onConfirm = onConfirm ?? throw new ArgumentNullException(nameof(onConfirm));
        _onCheckIn = onCheckIn ?? throw new ArgumentNullException(nameof(onCheckIn));
        _onComplete = onComplete ?? throw new ArgumentNullException(nameof(onComplete));
        _onCancel = onCancel ?? throw new ArgumentNullException(nameof(onCancel));
        _onClose = onClose;

        EditCommand = new RelayCommand(() => 
        {
            _onEdit(Reservation);
            _onClose?.Invoke(); // Close the popup when editing
        });
        ConfirmCommand = new AsyncRelayCommand(() => _onConfirm(Reservation));
        CheckInCommand = new AsyncRelayCommand(() => _onCheckIn(Reservation));
        CompleteCommand = new AsyncRelayCommand(() => _onComplete(Reservation));
        CancelReservationCommand = new AsyncRelayCommand(() => _onCancel(Reservation));

        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        CanConfirm = Reservation.Status == "waiting";
        CanCheckIn = Reservation.Status == "confirmed";
        CanComplete = Reservation.Status == "checked_in";
        CanCancelReservation = Reservation.Status != "cancelled" && Reservation.Status != "completed";
    }
}
