using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Reservation Side Panel (Add/Edit)
/// </summary>
public partial class ReservationSidePanelViewModel : ObservableObject
{
    private readonly IReservationService _reservationService;
    private readonly IRestaurantTableService _restaurantTableService;
    private readonly ICustomerService _customerService;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly Func<Task> _onSaved;
    private readonly Action _onCancelled;
    private ReservationDto? _originalReservation;
    private bool _isEditMode;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();

    [ObservableProperty]
    private CustomerDto? selectedCustomer;

    [ObservableProperty]
    private ObservableCollection<RestaurantTableDto> tables = new();

    [ObservableProperty]
    private RestaurantTableDto? selectedTable;

    [ObservableProperty]
    private ObservableCollection<PaymentTypeDto> paymentTypes = new();

    [ObservableProperty]
    private PaymentTypeDto? selectedPaymentType;

    [ObservableProperty]
    private DateTime reservationDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan reservationTime = new TimeSpan(12, 0, 0); // Default 12:00 PM

    [ObservableProperty]
    private int numberOfPersons = 2;

    [ObservableProperty]
    private int durationHours = 2;

    [ObservableProperty]
    private decimal depositFee = 0;

    [ObservableProperty]
    private string notes = string.Empty;

    [ObservableProperty]
    private string selectedStatus = "waiting";

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool hasValidationError = false;

    [ObservableProperty]
    private string formTitle = "Add New Reservation";

    [ObservableProperty]
    private string saveButtonText = "Save Reservation";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool canSave = true;

    [ObservableProperty]
    private string customerSearchText = string.Empty;

    [ObservableProperty]
    private bool isConflictCheckRunning = false;

    [ObservableProperty]
    private string conflictMessage = string.Empty;

    [ObservableProperty]
    private bool hasConflict = false;

    #endregion

    #region Collections

    public ObservableCollection<string> StatusOptions { get; } = new()
    {
        "waiting",
        "confirmed",
        "checked_in",
        "completed",
        "cancelled"
    };

    public ObservableCollection<int> DurationOptions { get; } = new()
    {
        1, 2, 3, 4
    };

    public ObservableCollection<TimeSpan> TimeSlots { get; } = new();

    #endregion

    #region Commands

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand CloseCommand { get; }
    public IAsyncRelayCommand CheckConflictCommand { get; }
    public IAsyncRelayCommand SearchCustomersCommand { get; }

    #endregion

    #region Constructors

    // Constructor for adding a new reservation
    public ReservationSidePanelViewModel(
        IReservationService reservationService,
        IRestaurantTableService restaurantTableService,
        ICustomerService customerService,
        IPaymentTypeService paymentTypeService,
        Func<Task> onSaved,
        Action onCancelled,
        int startHour = 10,
        int endHour = 22)
    {
        _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
        _restaurantTableService = restaurantTableService ?? throw new ArgumentNullException(nameof(restaurantTableService));
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
        _onSaved = onSaved ?? throw new ArgumentNullException(nameof(onSaved));
        _onCancelled = onCancelled ?? throw new ArgumentNullException(nameof(onCancelled));

        _isEditMode = false;
        _originalReservation = null;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Close);
        CloseCommand = new RelayCommand(Close);
        CheckConflictCommand = new AsyncRelayCommand(CheckConflictAsync);
        SearchCustomersCommand = new AsyncRelayCommand(SearchCustomersAsync);

        GenerateTimeSlots(startHour, endHour);
        _ = InitializeAsync();
    }

    // Constructor for editing an existing reservation
    public ReservationSidePanelViewModel(
        IReservationService reservationService,
        IRestaurantTableService restaurantTableService,
        ICustomerService customerService,
        IPaymentTypeService paymentTypeService,
        ReservationDto originalReservation,
        Func<Task> onSaved,
        Action onCancelled,
        int startHour = 10,
        int endHour = 22) 
        : this(reservationService, restaurantTableService, customerService, paymentTypeService, onSaved, onCancelled, startHour, endHour)
    {
        _isEditMode = true;
        _originalReservation = originalReservation ?? throw new ArgumentNullException(nameof(originalReservation));

        FormTitle = "Edit Reservation";
        SaveButtonText = "Update Reservation";

        LoadForEdit(originalReservation);
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        try
        {
            IsLoading = true;

            await Task.WhenAll(
                LoadCustomersAsync(),
                LoadTablesAsync(),
                LoadPaymentTypesAsync()
            );
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error initializing form: {ex.Message}";
            HasValidationError = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void GenerateTimeSlots(int startHour = 0, int endHour = 23)
    {
        TimeSlots.Clear();
        for (int hour = startHour; hour <= endHour; hour++)
        {
            TimeSlots.Add(new TimeSpan(hour, 0, 0));
            if (hour < endHour)
            {
                TimeSlots.Add(new TimeSpan(hour, 30, 0));
            }
        }
    }

    private async Task LoadCustomersAsync()
    {
        try
        {
            var allCustomers = await _customerService.GetAllAsync();
            Customers = new ObservableCollection<CustomerDto>(
                allCustomers.Where(c => c.Status == "Active").OrderBy(c => c.DisplayName));
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading customers: {ex.Message}";
        }
    }

    private async Task LoadTablesAsync()
    {
        try
        {
            var allTables = await _restaurantTableService.GetAllAsync();
            Tables = new ObservableCollection<RestaurantTableDto>(
                allTables.OrderBy(t => t.TableNumber));
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading tables: {ex.Message}";
        }
    }

    private async Task LoadPaymentTypesAsync()
    {
        try
        {
            var allPaymentTypes = await _paymentTypeService.GetAllAsync();
            PaymentTypes = new ObservableCollection<PaymentTypeDto>(
                allPaymentTypes.OrderBy(p => p.Name));
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error loading payment types: {ex.Message}";
        }
    }

    private void LoadForEdit(ReservationDto reservation)
    {
        // Set values
        ReservationDate = reservation.ReservationDate;
        ReservationTime = reservation.ReservationTime;
        NumberOfPersons = reservation.NumberOfPersons;
        DurationHours = 2; // Default, can be enhanced
        DepositFee = reservation.DepositFee;
        Notes = reservation.Notes ?? string.Empty;
        SelectedStatus = reservation.Status;

        // Select related entities after they're loaded
        Task.Run(async () =>
        {
            await InitializeAsync();
            
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                SelectedCustomer = Customers.FirstOrDefault(c => c.Id == reservation.CustomerId);
                SelectedTable = Tables.FirstOrDefault(t => t.Id == reservation.TableId);
                SelectedPaymentType = PaymentTypes.FirstOrDefault(p => p.Id == reservation.PaymentTypeId);
            });
        });
    }

    #endregion

    #region Validation

    private bool ValidateForm()
    {
        HasValidationError = false;
        ValidationMessage = string.Empty;

        if (SelectedCustomer == null)
        {
            ValidationMessage = "Please select a customer";
            HasValidationError = true;
            return false;
        }

        if (SelectedTable == null)
        {
            ValidationMessage = "Please select a table";
            HasValidationError = true;
            return false;
        }

        if (ReservationDate.Date < DateTime.Today)
        {
            ValidationMessage = "Reservation date cannot be in the past";
            HasValidationError = true;
            return false;
        }

        if (NumberOfPersons < 1 || NumberOfPersons > 100)
        {
            ValidationMessage = "Number of persons must be between 1 and 100";
            HasValidationError = true;
            return false;
        }

        if (NumberOfPersons > SelectedTable.Capacity)
        {
            ValidationMessage = $"Selected table capacity ({SelectedTable.Capacity}) is less than number of persons ({NumberOfPersons})";
            HasValidationError = true;
            return false;
        }

        if (DurationHours < 1 || DurationHours > 4)
        {
            ValidationMessage = "Duration must be between 1 and 4 hours";
            HasValidationError = true;
            return false;
        }

        if (DepositFee < 0)
        {
            ValidationMessage = "Deposit fee cannot be negative";
            HasValidationError = true;
            return false;
        }

        return true;
    }

    #endregion

    #region Conflict Detection

    private async Task CheckConflictAsync()
    {
        if (SelectedTable == null) return;

        try
        {
            IsConflictCheckRunning = true;
            HasConflict = false;
            ConflictMessage = string.Empty;

            var excludeId = _isEditMode && _originalReservation != null ? _originalReservation.Id : (int?)null;

            var isAvailable = await _reservationService.IsTimeSlotAvailableAsync(
                SelectedTable.Id,
                ReservationDate,
                ReservationTime,
                excludeId);

            if (!isAvailable)
            {
                HasConflict = true;
                ConflictMessage = $"⚠️ Time slot conflict! Table {SelectedTable.TableNumber} is already reserved at this time.";
            }
            else
            {
                ConflictMessage = $"✓ Time slot is available for Table {SelectedTable.TableNumber}";
            }
        }
        catch (Exception ex)
        {
            ConflictMessage = $"Error checking availability: {ex.Message}";
        }
        finally
        {
            IsConflictCheckRunning = false;
        }
    }

    partial void OnSelectedTableChanged(RestaurantTableDto? value)
    {
        _ = CheckConflictAsync();
    }

    partial void OnReservationDateChanged(DateTime value)
    {
        _ = CheckConflictAsync();
    }

    partial void OnReservationTimeChanged(TimeSpan value)
    {
        _ = CheckConflictAsync();
    }

    #endregion

    #region Search

    private async Task SearchCustomersAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerSearchText))
        {
            await LoadCustomersAsync();
            return;
        }

        try
        {
            var allCustomers = await _customerService.GetAllAsync();
            var filtered = allCustomers.Where(c =>
                c.Status == "Active" &&
                (c.DisplayName.Contains(CustomerSearchText, StringComparison.OrdinalIgnoreCase) ||
                 (c.PrimaryMobile != null && c.PrimaryMobile.Contains(CustomerSearchText)))).ToList();

            Customers = new ObservableCollection<CustomerDto>(filtered);
        }
        catch (Exception ex)
        {
            ValidationMessage = $"Error searching customers: {ex.Message}";
        }
    }

    #endregion

    #region Save/Cancel

    private async Task SaveAsync()
    {
        AppLogger.LogInfo("===== SaveAsync invoked - starting reservation save =====", filename: "reservation");
        
        if (!ValidateForm())
        {
            AppLogger.LogWarning("Form validation failed, cannot save", filename: "reservation");
            return;
        }

        AppLogger.LogInfo("Form validation passed", filename: "reservation");

        // Final conflict check before saving
        if (SelectedTable != null)
        {
            AppLogger.LogInfo($"Checking for time slot conflicts - Table: {SelectedTable.TableNumber}, Date: {ReservationDate}, Time: {ReservationTime}", filename: "reservation");
            
            await CheckConflictAsync();
            
            if (HasConflict)
            {
                AppLogger.LogWarning($"Time slot conflict detected: {ConflictMessage}", filename: "reservation");
                
                var result = MessageBox.Show(
                    "There is a time slot conflict. Do you want to save anyway?",
                    "Conflict Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    AppLogger.LogInfo("User cancelled save due to conflict", filename: "reservation");
                    return;
                }
                
                AppLogger.LogInfo("User chose to save despite conflict", filename: "reservation");
            }
            else
            {
                AppLogger.LogInfo("No conflicts detected, proceeding with save", filename: "reservation");
            }
        }

        try
        {
            IsLoading = true;
            CanSave = false;
            ValidationMessage = string.Empty;
            HasValidationError = false;

            if (_isEditMode && _originalReservation != null)
            {
                AppLogger.LogInfo($"Updating existing reservation ID: {_originalReservation.Id}", filename: "reservation");
                await UpdateReservation();
                AppLogger.LogInfo($"Reservation ID {_originalReservation.Id} updated successfully", filename: "reservation");
            }
            else
            {
                AppLogger.LogInfo("Creating new reservation", filename: "reservation");
                await CreateReservation();
                AppLogger.LogInfo("New reservation created successfully", filename: "reservation");
            }

            ValidationMessage = _isEditMode ? "✓ Reservation updated successfully!" : "✓ Reservation created successfully!";

            // Delay before closing to show success message
            await Task.Delay(1000);

            AppLogger.LogInfo("Invoking onSaved callback", filename: "reservation");
            await _onSaved.Invoke();
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to save reservation", ex, filename: "reservation");
            ValidationMessage = $"Error: {ex.Message}";
            HasValidationError = true;
            CanSave = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task CreateReservation()
    {
        var createDto = new CreateReservationDto
        {
            CustomerId = SelectedCustomer!.Id,
            TableId = SelectedTable!.Id,
            NumberOfPersons = NumberOfPersons,
            ReservationDate = ReservationDate,
            ReservationTime = ReservationTime,
            DepositFee = DepositFee,
            PaymentTypeId = SelectedPaymentType?.Id,
            Status = SelectedStatus,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
        };

        await _reservationService.CreateAsync(createDto);
    }

    private async Task UpdateReservation()
    {
        if (_originalReservation == null) return;

        var updateDto = new UpdateReservationDto
        {
            CustomerId = SelectedCustomer!.Id,
            TableId = SelectedTable!.Id,
            NumberOfPersons = NumberOfPersons,
            ReservationDate = ReservationDate,
            ReservationTime = ReservationTime,
            DepositFee = DepositFee,
            PaymentTypeId = SelectedPaymentType?.Id,
            Status = SelectedStatus,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
        };

        await _reservationService.UpdateAsync(_originalReservation.Id, updateDto);
    }

    private void Close()
    {
        _onCancelled.Invoke();
    }

    #endregion
}
