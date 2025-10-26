using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Helpers;
using ChronoPos.Desktop.Models;
using ChronoPos.Desktop.Views;
using ChronoPos.Desktop.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for Restaurant Reservation Timeline View
/// </summary>
public partial class ReservationTimelineViewModel : ObservableObject
{
    private readonly IReservationService _reservationService;
    private readonly IRestaurantTableService _restaurantTableService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPaymentTypeService _paymentTypeService;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<string> floors = new();

    [ObservableProperty]
    private string? selectedFloor;

    [ObservableProperty]
    private DateTime selectedDate = DateTime.Today;

    [ObservableProperty]
    private string selectedDateFilter = "Today"; // For dropdown selection

    [ObservableProperty]
    private ObservableCollection<RestaurantTableDto> tables = new();

    [ObservableProperty]
    private ObservableCollection<TableGridRow> tableGridRows = new();

    [ObservableProperty]
    private ObservableCollection<TimeSlotHeader> timeSlots = new();

    [ObservableProperty]
    private ObservableCollection<ReservationDto> allReservations = new();

    [ObservableProperty]
    private ReservationDto? selectedReservation;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isSidePanelVisible = false;

    [ObservableProperty]
    private ReservationSidePanelViewModel? sidePanelViewModel;

    [ObservableProperty]
    private bool isTableSidePanelVisible = false;

    [ObservableProperty]
    private RestaurantTableSidePanelViewModel? tableSidePanelViewModel;

    [ObservableProperty]
    private bool isDetailsPopupVisible = false;

    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private int gridStartHour = TimeSlotGridHelper.DefaultStartHour;

    [ObservableProperty]
    private int gridEndHour = TimeSlotGridHelper.DefaultEndHour;

    // Available hours for selection (0-23)
    public ObservableCollection<int> AvailableHours { get; } = new(Enumerable.Range(0, 24));

    // Date filter options
    public List<string> DateFilterOptions { get; } = new() { "Yesterday", "Today", "Tomorrow" };

    // Permission Properties
    [ObservableProperty]
    private bool canCreateReservation = false;

    [ObservableProperty]
    private bool canEditReservation = false;

    [ObservableProperty]
    private bool canDeleteReservation = false;

    #endregion

    #region Computed Properties

    public bool HasTables => Tables.Count > 0;
    public bool HasFloors => Floors.Count > 0;
    public int TotalReservations => AllReservations.Count;
    public string SelectedDateFormatted => SelectedDate.ToString("dddd, MMMM dd, yyyy");

    #endregion

    #region Constructor

    public ReservationTimelineViewModel(
        IReservationService reservationService,
        IRestaurantTableService restaurantTableService,
        ICustomerService customerService,
        ICurrentUserService currentUserService,
        IPaymentTypeService paymentTypeService)
    {
        try
        {
            AppLogger.LogInfo("ReservationTimelineViewModel constructor started", filename: "reservation");
            
            _reservationService = reservationService ?? throw new ArgumentNullException(nameof(reservationService));
            _restaurantTableService = restaurantTableService ?? throw new ArgumentNullException(nameof(restaurantTableService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));

            AppLogger.LogInfo("All services injected successfully", filename: "reservation");

            InitializePermissions();
            AppLogger.LogInfo("Permissions initialized", filename: "reservation");
            
            GenerateTimeSlots();
            AppLogger.LogInfo("Time slots generated", filename: "reservation");
            
            _ = InitializeAsync();
            AppLogger.LogInfo("Async initialization started", filename: "reservation");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Constructor failed", ex, filename: "reservation");
            throw;
        }
    }

    #endregion

    #region Property Changed Handlers

    partial void OnGridStartHourChanged(int value)
    {
        AppLogger.LogInfo($"Grid start hour changed to: {value}", filename: "reservation");
        
        // Ensure start hour is before end hour
        if (value >= GridEndHour)
        {
            AppLogger.LogInfo($"Start hour {value} >= End hour {GridEndHour}, adjusting end hour", filename: "reservation");
            // Set end hour to at least 1 hour after start, or max 23
            GridEndHour = Math.Min(value + 1, 23);
            AppLogger.LogInfo($"End hour adjusted to: {GridEndHour}", filename: "reservation");
            return; // Don't regenerate yet, OnGridEndHourChanged will be called
        }
        
        GenerateTimeSlots();
        _ = LoadReservationsAsync();
    }

    partial void OnGridEndHourChanged(int value)
    {
        AppLogger.LogInfo($"Grid end hour changed to: {value}", filename: "reservation");
        
        // Ensure end hour is after start hour
        if (value <= GridStartHour)
        {
            AppLogger.LogInfo($"End hour {value} <= Start hour {GridStartHour}, adjusting start hour", filename: "reservation");
            // Set start hour to at least 1 hour before end, or min 0
            GridStartHour = Math.Max(value - 1, 0);
            AppLogger.LogInfo($"Start hour adjusted to: {GridStartHour}", filename: "reservation");
            return; // Don't regenerate yet, OnGridStartHourChanged will be called
        }
        
        GenerateTimeSlots();
        _ = LoadReservationsAsync();
    }

    partial void OnSelectedFloorChanged(string? value)
    {
        AppLogger.LogInfo($"===== Floor tab clicked: '{value}' =====", filename: "reservation");
        AppLogger.LogInfo($"SelectedFloor property is now: '{value}'", filename: "reservation");
        // When floor tab changes, reload tables and reservations with the NEW floor value
        _ = LoadTablesAsync(value);
        _ = LoadReservationsAsync();
    }

    partial void OnSelectedDateFilterChanged(string value)
    {
        AppLogger.LogInfo($"Date filter dropdown changed to: {value}", filename: "reservation");
        switch (value)
        {
            case "Yesterday":
                SelectedDate = DateTime.Today.AddDays(-1);
                break;
            case "Today":
                SelectedDate = DateTime.Today;
                break;
            case "Tomorrow":
                SelectedDate = DateTime.Today.AddDays(1);
                break;
        }
        _ = LoadReservationsAsync();
    }

    #endregion

    #region Initialization

    private async Task InitializeAsync()
    {
        try
        {
            AppLogger.LogInfo("InitializeAsync started", filename: "reservation");
            IsLoading = true;
            StatusMessage = "Loading reservation data...";

            AppLogger.LogInfo("Loading floors...", filename: "reservation");
            await LoadFloorsAsync();
            AppLogger.LogInfo($"Floors loaded: {Floors.Count}", filename: "reservation");
            
            AppLogger.LogInfo("Loading tables...", filename: "reservation");
            await LoadTablesAsync();
            AppLogger.LogInfo($"Tables loaded: {Tables.Count}", filename: "reservation");
            
            AppLogger.LogInfo("Loading reservations...", filename: "reservation");
            await LoadReservationsAsync();
            AppLogger.LogInfo($"Reservations loaded: {AllReservations.Count}", filename: "reservation");
            
            StatusMessage = $"Loaded {TotalReservations} reservations";
            AppLogger.LogInfo("InitializeAsync completed successfully", filename: "reservation");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("InitializeAsync failed", ex, filename: "reservation");
            StatusMessage = $"Error initializing: {ex.Message}";
            new MessageDialog("Error", $"Failed to initialize reservation timeline: {ex.Message}", 
                MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void InitializePermissions()
    {
        try
        {
            AppLogger.LogInfo("InitializePermissions started", filename: "reservation");
            // Check for reservation management permissions
            CanCreateReservation = _currentUserService.HasAnyScreenPermission("Reservation");
            CanEditReservation = _currentUserService.HasAnyScreenPermission("Reservation");
            CanDeleteReservation = _currentUserService.HasAnyScreenPermission("Reservation");
            
            // If no specific reservation screen, grant permissions by default for testing
            if (!CanCreateReservation)
            {
                CanCreateReservation = true;
                CanEditReservation = true;
                CanDeleteReservation = true;
            }
            AppLogger.LogInfo($"Permissions: Create={CanCreateReservation}, Edit={CanEditReservation}, Delete={CanDeleteReservation}", filename: "reservation");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("InitializePermissions failed", ex, filename: "reservation");
        }
    }

    private void GenerateTimeSlots()
    {
        var slots = TimeSlotGridHelper.GenerateTimeSlots(GridStartHour, GridEndHour);
        TimeSlots = new ObservableCollection<TimeSlotHeader>(slots);
    }

    #endregion

    #region Data Loading

    private async Task LoadFloorsAsync()
    {
        try
        {
            AppLogger.LogInfo("LoadFloorsAsync started", filename: "reservation");
            
            var locations = await _restaurantTableService.GetDistinctLocationsAsync();
            AppLogger.LogInfo($"Retrieved {locations.Count()} distinct locations from service", filename: "reservation");
            
            Floors.Clear();
            
            // Add "All Locations" option
            Floors.Add("All Locations");
            
            foreach (var location in locations)
            {
                Floors.Add(location);
                AppLogger.LogInfo($"Added floor: {location}", filename: "reservation");
            }

            // Select first floor by default
            if (Floors.Count > 0)
            {
                SelectedFloor = Floors[0];
                AppLogger.LogInfo($"Selected default floor: {SelectedFloor}", filename: "reservation");
            }
            else
            {
                AppLogger.LogWarning("No floors available", filename: "reservation");
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("LoadFloorsAsync failed", ex, filename: "reservation");
            StatusMessage = $"Error loading floors: {ex.Message}";
        }
    }

    private async Task LoadTablesAsync(string? floorLocation = null)
    {
        try
        {
            // Use parameter if provided, otherwise use property
            var locationToUse = floorLocation ?? SelectedFloor;
            
            AppLogger.LogInfo($"===== LoadTablesAsync started =====", filename: "reservation");
            AppLogger.LogInfo($"Parameter floor: '{floorLocation}', SelectedFloor property: '{SelectedFloor}', Using: '{locationToUse}'", filename: "reservation");
            
            IEnumerable<RestaurantTableDto> tables;

            if (string.IsNullOrEmpty(locationToUse) || locationToUse == "All Locations")
            {
                AppLogger.LogInfo("Loading all tables (All Locations selected)", filename: "reservation");
                tables = await _restaurantTableService.GetAllAsync();
            }
            else
            {
                AppLogger.LogInfo($"Loading tables FILTERED by location: '{locationToUse}'", filename: "reservation");
                tables = await _restaurantTableService.GetTablesByLocationAsync(locationToUse);
            }

            AppLogger.LogInfo($"Service returned {tables.Count()} tables", filename: "reservation");
            
            // Log first few tables to verify filtering
            var tableList = tables.Take(3).ToList();
            foreach (var table in tableList)
            {
                AppLogger.LogInfo($"  Table: {table.TableNumber}, Location: {table.Location}", filename: "reservation");
            }

            AppLogger.LogInfo($"Setting Tables property with {tables.Count()} tables", filename: "reservation");
            Tables = new ObservableCollection<RestaurantTableDto>(
                tables.OrderBy(t => t.TableNumber));
            AppLogger.LogInfo($"Tables property set. Collection count: {Tables.Count}", filename: "reservation");

            OnPropertyChanged(nameof(HasTables));
            AppLogger.LogInfo($"HasTables property changed. Value: {HasTables}", filename: "reservation");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("LoadTablesAsync failed", ex, filename: "reservation");
            StatusMessage = $"Error loading tables: {ex.Message}";
        }
    }

    private async Task LoadReservationsAsync()
    {
        try
        {
            AppLogger.LogInfo("===== LoadReservationsAsync started =====", filename: "reservation");
            IsLoading = true;
            StatusMessage = "Loading reservations...";

            // Don't load tables here - they should be loaded separately when needed
            // await LoadTablesAsync(); // REMOVED - causes double loading

            AppLogger.LogInfo($"===== Getting reservations from service =====", filename: "reservation");
            AppLogger.LogInfo($"Current SelectedFloor property value: '{SelectedFloor}'", filename: "reservation");
            
            var floor = SelectedFloor == "All Locations" ? null : SelectedFloor;
            AppLogger.LogInfo($"Converted floor parameter for service call: '{floor ?? "NULL (All Locations)"}'", filename: "reservation");
            AppLogger.LogInfo($"Getting reservations for date: {SelectedDate:yyyy-MM-dd}, floor: {floor ?? "All Locations"}", filename: "reservation");
            
            var reservations = await _reservationService.GetReservationsForTimelineAsync(
                SelectedDate, 
                floor);

            AppLogger.LogInfo($"Retrieved {reservations.Count()} reservations from service", filename: "reservation");

            AllReservations = new ObservableCollection<ReservationDto>(reservations);
            
            AppLogger.LogInfo("Building grid rows...", filename: "reservation");
            BuildGridRows();
            AppLogger.LogInfo($"Grid rows built: {TableGridRows.Count}", filename: "reservation");
            
            OnPropertyChanged(nameof(TotalReservations));
            StatusMessage = $"Loaded {TotalReservations} reservations for {SelectedDateFormatted}";
            AppLogger.LogInfo("LoadReservationsAsync completed successfully", filename: "reservation");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("LoadReservationsAsync failed", ex, filename: "reservation");
            StatusMessage = $"Error loading reservations: {ex.Message}";
            new MessageDialog("Error", $"Failed to load reservations: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void BuildGridRows()
    {
        AppLogger.LogInfo($"===== BuildGridRows started =====", filename: "reservation");
        AppLogger.LogInfo($"Tables.Count: {Tables.Count}, AllReservations.Count: {AllReservations.Count}", filename: "reservation");
        
        var rows = new List<TableGridRow>();
        int rowIndex = 0;

        foreach (var table in Tables)
        {
            var tableReservations = AllReservations
                .Where(r => r.TableId == table.Id)
                .Select(r => TimeSlotGridHelper.ToGridItem(r, GridStartHour))
                .ToList();

            AppLogger.LogInfo($"Table {table.TableNumber} (ID: {table.Id}): Found {tableReservations.Count} reservations", filename: "reservation");
            if (tableReservations.Any())
            {
                AppLogger.LogInfo($"  First reservation: Customer={AllReservations.FirstOrDefault(r => r.TableId == table.Id)?.CustomerName}, Time={AllReservations.FirstOrDefault(r => r.TableId == table.Id)?.ReservationDateTime}", filename: "reservation");
            }

            var row = new TableGridRow
            {
                Table = table,
                Reservations = tableReservations,
                RowIndex = rowIndex++
            };

            rows.Add(row);
        }

        AppLogger.LogInfo($"Setting TableGridRows with {rows.Count} rows...", filename: "reservation");
        AppLogger.LogInfo($"Total reservations across all rows: {rows.Sum(r => r.Reservations.Count)}", filename: "reservation");
        TableGridRows = new ObservableCollection<TableGridRow>(rows);
        AppLogger.LogInfo($"TableGridRows set successfully. Collection count: {TableGridRows.Count}", filename: "reservation");
    }
    
    // Partial method to track TableGridRows property changes
    partial void OnTableGridRowsChanged(ObservableCollection<TableGridRow> value)
    {
        AppLogger.LogInfo($"===== OnTableGridRowsChanged triggered =====", filename: "reservation");
        AppLogger.LogInfo($"New TableGridRows count: {value?.Count ?? 0}", filename: "reservation");
        AppLogger.LogInfo($"HasTables property will be: {Tables?.Any() == true}", filename: "reservation");
        
        if (value != null && value.Count > 0)
        {
            AppLogger.LogInfo($"First row - Table: {value[0].Table?.TableNumber ?? "null"}, Reservations: {value[0].Reservations?.Count ?? 0}", filename: "reservation");
        }
    }
    
    // Partial method to track Tables property changes
    partial void OnTablesChanged(ObservableCollection<RestaurantTableDto> value)
    {
        AppLogger.LogInfo($"===== OnTablesChanged triggered =====", filename: "reservation");
        AppLogger.LogInfo($"New Tables count: {value?.Count ?? 0}", filename: "reservation");
        AppLogger.LogInfo($"HasTables computed property: {value?.Any() == true}", filename: "reservation");
        
        if (value != null && value.Count > 0)
        {
            AppLogger.LogInfo($"First table: {value[0].TableNumber} (ID: {value[0].Id}), Location: {value[0].Location}", filename: "reservation");
        }
    }

    #endregion

    #region Commands

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadReservationsAsync();
    }

    [RelayCommand]
    private void PreviousDay()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        _ = LoadReservationsAsync();
    }

    [RelayCommand]
    private void NextDay()
    {
        SelectedDate = SelectedDate.AddDays(1);
        _ = LoadReservationsAsync();
    }

    [RelayCommand]
    private void Today()
    {
        SelectedDate = DateTime.Today;
        _ = LoadReservationsAsync();
    }

    [RelayCommand]
    private void Yesterday()
    {
        SelectedDate = DateTime.Today.AddDays(-1);
        _ = LoadReservationsAsync();
    }

    [RelayCommand]
    private void Tomorrow()
    {
        SelectedDate = DateTime.Today.AddDays(1);
        _ = LoadReservationsAsync();
    }

    [RelayCommand]
    private void PreviousWeek()
    {
        SelectedDate = SelectedDate.AddDays(-7);
        _ = LoadReservationsAsync();
    }

    [RelayCommand]
    private void SelectDate()
    {
        // Date picker will handle the selection
        _ = LoadReservationsAsync();
    }

    [RelayCommand]
    private async Task ReloadData()
    {
        AppLogger.LogInfo("ReloadData command invoked", filename: "reservation");
        await LoadFloorsAsync();
        await LoadTablesAsync();
        await LoadReservationsAsync();
        StatusMessage = "Data refreshed successfully";
    }

    [RelayCommand]
    private void AddReservation()
    {
        AppLogger.LogInfo("===== AddReservation command invoked =====", filename: "reservation");

        if (!CanCreateReservation)
        {
            AppLogger.LogWarning("User attempted to open Add Reservation but lacks permission", filename: "reservation");
            new MessageDialog("Permission Denied", "You don't have permission to create reservations.", MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        StatusMessage = "Opening add reservation form...";

        try
        {
            AppLogger.LogInfo("Creating side panel view model for Add Reservation", filename: "reservation");

            // Create side panel ViewModel for adding
            SidePanelViewModel = new ReservationSidePanelViewModel(
                _reservationService,
                _restaurantTableService,
                _customerService,
                _paymentTypeService,
                OnReservationSavedAsync,
                OnSidePanelCancelled,
                GridStartHour,
                GridEndHour);

            AppLogger.LogInfo("Side panel ViewModel created successfully", filename: "reservation");
            
            IsSidePanelVisible = true;
            AppLogger.LogInfo("Side panel opened (IsSidePanelVisible = true)", filename: "reservation");
            
            StatusMessage = "Ready to add reservation";
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to open Add Reservation side panel", ex, filename: "reservation");
            new MessageDialog("Error", $"Failed to open reservation form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private void EditReservation(ReservationDto? reservation)
    {
        AppLogger.LogInfo($"===== EditReservation command invoked for reservation ID: {reservation?.Id} =====", filename: "reservation");

        if (!CanEditReservation)
        {
            AppLogger.LogWarning("User attempted to edit reservation but lacks permission", filename: "reservation");
            new MessageDialog("Permission Denied", "You don't have permission to edit reservations.", MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        if (reservation == null)
        {
            AppLogger.LogWarning("EditReservation called with null reservation", filename: "reservation");
            return;
        }

        StatusMessage = $"Opening edit reservation form for #{reservation.Id}...";

        try
        {
            AppLogger.LogInfo($"Creating side panel view model for Edit Reservation (ID: {reservation.Id})", filename: "reservation");

            // Create side panel ViewModel for editing
            SidePanelViewModel = new ReservationSidePanelViewModel(
                _reservationService,
                _restaurantTableService,
                _customerService,
                _paymentTypeService,
                reservation,
                OnReservationSavedAsync,
                OnSidePanelCancelled,
                GridStartHour,
                GridEndHour);

            AppLogger.LogInfo("Side panel ViewModel created successfully", filename: "reservation");
            
            IsSidePanelVisible = true;
            AppLogger.LogInfo("Side panel opened (IsSidePanelVisible = true)", filename: "reservation");
            
            StatusMessage = $"Editing reservation #{reservation.Id}";
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Failed to open Edit Reservation side panel for ID {reservation.Id}", ex, filename: "reservation");
            new MessageDialog("Error", $"Failed to open reservation form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private void AddTable()
    {
        AppLogger.LogInfo("===== AddTable command invoked =====", filename: "reservation");

        StatusMessage = "Opening add table form...";

        try
        {
            AppLogger.LogInfo("Creating table side panel view model for Add Table", filename: "reservation");

            TableSidePanelViewModel = new RestaurantTableSidePanelViewModel(
                _restaurantTableService,
                OnTableSaved,
                OnTableSidePanelCancelled);

            AppLogger.LogInfo("Table side panel ViewModel created successfully", filename: "reservation");
            
            IsTableSidePanelVisible = true;
            AppLogger.LogInfo("Table side panel opened (IsTableSidePanelVisible = true)", filename: "reservation");
            
            StatusMessage = "Ready to add table";
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to open Add Table side panel", ex, filename: "reservation");
            new MessageDialog("Error", $"Failed to open table form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private void EditTable(RestaurantTableDto? table)
    {
        AppLogger.LogInfo($"===== EditTable command invoked for table: {table?.TableNumber} =====", filename: "reservation");

        if (table == null)
        {
            AppLogger.LogWarning("EditTable called with null table", filename: "reservation");
            return;
        }

        StatusMessage = $"Opening edit table form for {table.TableNumber}...";

        try
        {
            AppLogger.LogInfo($"Creating table side panel view model for Edit Table ({table.TableNumber})", filename: "reservation");

            TableSidePanelViewModel = new RestaurantTableSidePanelViewModel(
                _restaurantTableService,
                table,
                OnTableSaved,
                OnTableSidePanelCancelled);

            AppLogger.LogInfo("Table side panel ViewModel created successfully", filename: "reservation");
            
            IsTableSidePanelVisible = true;
            AppLogger.LogInfo("Table side panel opened (IsTableSidePanelVisible = true)", filename: "reservation");
            
            StatusMessage = $"Editing table {table.TableNumber}";
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Failed to open Edit Table side panel for {table.TableNumber}", ex, filename: "reservation");
            new MessageDialog("Error", $"Failed to open table form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private void ViewReservationDetails(ReservationDto? reservation)
    {
        AppLogger.LogInfo($"===== ViewReservationDetails invoked, reservation: {reservation?.Id} =====", filename: "reservation");
        
        if (reservation == null)
        {
            AppLogger.LogWarning("ViewReservationDetails called with null reservation", filename: "reservation");
            return;
        }

        StatusMessage = $"Viewing reservation #{reservation.Id}...";
        AppLogger.LogInfo($"Opening reservation details popup for #{reservation.Id}", filename: "reservation");

        try
        {
            var popup = new ReservationDetailsPopup(
                reservation,
                EditReservation,
                OnConfirmFromPopup,
                OnCheckInFromPopup,
                OnCompleteFromPopup,
                OnCancelFromPopup);

            popup.Owner = System.Windows.Application.Current.MainWindow;
            AppLogger.LogInfo("Showing reservation details popup dialog", filename: "reservation");
            popup.ShowDialog();
            AppLogger.LogInfo("Reservation details popup closed", filename: "reservation");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Failed to open reservation details popup", ex, filename: "reservation");
            new MessageDialog("Error", $"Failed to open reservation details: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private void CloseDetailsPopup()
    {
        IsDetailsPopupVisible = false;
        SelectedReservation = null;
    }

    [RelayCommand]
    private void CloseSidePanel()
    {
        AppLogger.LogInfo("===== CloseSidePanel invoked =====", filename: "reservation");
        IsSidePanelVisible = false;
        SidePanelViewModel = null;
        SelectedReservation = null;
        StatusMessage = "Reservation form closed";
        AppLogger.LogInfo("Side panel closed", filename: "reservation");
    }

    #region Callbacks

    private void OnSidePanelCancelled()
    {
        AppLogger.LogInfo("Side panel cancelled by user", filename: "reservation");
        CloseSidePanel();
        StatusMessage = "Reservation form cancelled";
    }

    private async Task OnConfirmFromPopup(ReservationDto reservation)
    {
        await ConfirmReservationAsync(reservation);
    }

    private async Task OnCheckInFromPopup(ReservationDto reservation)
    {
        await CheckInReservationAsync(reservation);
    }

    private async Task OnCompleteFromPopup(ReservationDto reservation)
    {
        await CompleteReservationAsync(reservation);
    }

    private async Task OnCancelFromPopup(ReservationDto reservation)
    {
        await CancelReservationAsync(reservation);
    }

    private void OnTableSaved(bool success)
    {
        if (success)
        {
            AppLogger.LogInfo("Table saved successfully, reloading floors, tables, and reservations", filename: "reservation");
            CloseTableSidePanel();
            _ = LoadFloorsAsync(); // Reload floors to include new location
            _ = LoadTablesAsync(); // Reload tables to show the new/updated table
            _ = LoadReservationsAsync(); // Reload reservations
            StatusMessage = "Table saved successfully";
        }
    }

    private void OnTableSidePanelCancelled()
    {
        AppLogger.LogInfo("Table side panel cancelled by user", filename: "reservation");
        CloseTableSidePanel();
        StatusMessage = "Table form cancelled";
    }

    private void CloseTableSidePanel()
    {
        AppLogger.LogInfo("===== CloseTableSidePanel invoked =====", filename: "reservation");
        IsTableSidePanelVisible = false;
        TableSidePanelViewModel = null;
        StatusMessage = "Table form closed";
        AppLogger.LogInfo("Table side panel closed", filename: "reservation");
    }

    #endregion

    [RelayCommand]
    private async Task ChangeFloorAsync(string? floor)
    {
        if (!string.IsNullOrEmpty(floor))
        {
            AppLogger.LogInfo($"Changing floor to: {floor}", filename: "reservation");
            SelectedFloor = floor;
        }
        await LoadReservationsAsync();
    }

    [RelayCommand]
    private async Task DeleteReservationAsync(ReservationDto? reservation)
    {
        if (!CanDeleteReservation)
        {
            new MessageDialog("Permission Denied", "You don't have permission to delete reservations.", MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        if (reservation == null) return;

        var result = new ConfirmationDialog(
            "Confirm Delete",
            $"Are you sure you want to delete the reservation for {reservation.CustomerName} on {reservation.ReservationDateTime}?",
            ConfirmationDialog.DialogType.Warning).ShowDialog();

        if (result == true)
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting reservation #{reservation.Id}...";

                var success = await _reservationService.DeleteAsync(reservation.Id);
                
                if (success)
                {
                    StatusMessage = "Reservation deleted successfully";
                    await LoadReservationsAsync();
                    
                    new MessageDialog("Success", "Reservation deleted successfully.", MessageDialog.MessageType.Success).ShowDialog();
                }
                else
                {
                    StatusMessage = "Failed to delete reservation";
                    new MessageDialog("Error", "Failed to delete reservation. Please try again.", MessageDialog.MessageType.Error).ShowDialog();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                new MessageDialog("Error", $"Error deleting reservation: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task ConfirmReservationAsync(ReservationDto? reservation)
    {
        if (reservation == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = $"Confirming reservation #{reservation.Id}...";

            var success = await _reservationService.ConfirmReservationAsync(reservation.Id);
            
            if (success)
            {
                StatusMessage = "Reservation confirmed";
                await LoadReservationsAsync();
                CloseDetailsPopup();
            }
            else
            {
                new MessageDialog("Error", "Failed to confirm reservation.", MessageDialog.MessageType.Error).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            new MessageDialog("Error", $"Error confirming reservation: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CheckInReservationAsync(ReservationDto? reservation)
    {
        if (reservation == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = $"Checking in reservation #{reservation.Id}...";

            var success = await _reservationService.CheckInReservationAsync(reservation.Id);
            
            if (success)
            {
                StatusMessage = "Reservation checked in";
                await LoadReservationsAsync();
                CloseDetailsPopup();
            }
            else
            {
                new MessageDialog("Error", "Failed to check in reservation.", MessageDialog.MessageType.Error).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            new MessageDialog("Error", $"Error checking in reservation: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CancelReservationAsync(ReservationDto? reservation)
    {
        if (reservation == null) return;

        var result = new ConfirmationDialog(
            "Confirm Cancellation",
            $"Are you sure you want to cancel this reservation?",
            ConfirmationDialog.DialogType.Warning).ShowDialog();

        if (result == true)
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Cancelling reservation #{reservation.Id}...";

                var success = await _reservationService.CancelReservationAsync(reservation.Id);
                
                if (success)
                {
                    StatusMessage = "Reservation cancelled";
                    await LoadReservationsAsync();
                    CloseDetailsPopup();
                }
                else
                {
                    new MessageDialog("Error", "Failed to cancel reservation.", MessageDialog.MessageType.Error).ShowDialog();
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                new MessageDialog("Error", $"Error cancelling reservation: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task CompleteReservationAsync(ReservationDto? reservation)
    {
        if (reservation == null) return;

        try
        {
            IsLoading = true;
            StatusMessage = $"Completing reservation #{reservation.Id}...";

            var success = await _reservationService.CompleteReservationAsync(reservation.Id);
            
            if (success)
            {
                StatusMessage = "Reservation completed";
                await LoadReservationsAsync();
                CloseDetailsPopup();
            }
            else
            {
                new MessageDialog("Error", "Failed to complete reservation.", MessageDialog.MessageType.Error).ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            new MessageDialog("Error", $"Error completing reservation: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Called when a reservation is saved from the side panel
    /// </summary>
    public async Task OnReservationSavedAsync()
    {
        AppLogger.LogInfo("===== OnReservationSavedAsync invoked - reservation was saved =====", filename: "reservation");
        
        CloseSidePanel();
        StatusMessage = "Reservation saved successfully. Reloading data...";
        
        AppLogger.LogInfo("Reloading reservations after save", filename: "reservation");
        await LoadReservationsAsync();
        
        AppLogger.LogInfo("Reservations reloaded successfully", filename: "reservation");
    }

    #endregion
}
