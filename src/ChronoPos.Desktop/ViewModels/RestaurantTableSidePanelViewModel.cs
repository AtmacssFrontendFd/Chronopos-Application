using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Views.Dialogs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels;

public partial class RestaurantTableSidePanelViewModel : ObservableObject
{
    private readonly IRestaurantTableService _restaurantTableService;
    private readonly Action<bool> _onSaved;
    private readonly Action _onCancelled;

    #region Properties

    [ObservableProperty]
    private int? tableId;

    [ObservableProperty]
    private string tableNumber = string.Empty;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private int capacity = 2;

    [ObservableProperty]
    private string status = "available";

    [ObservableProperty]
    private bool isEditMode = false;

    [ObservableProperty]
    private string title = "Add New Table";

    [ObservableProperty]
    private bool isLoading = false;

    [ObservableProperty]
    private bool isSaving = false;

    [ObservableProperty]
    private string validationMessage = string.Empty;

    [ObservableProperty]
    private bool hasValidationError = false;

    // Status Options
    public ObservableCollection<string> StatusOptions { get; } = new()
    {
        "available",
        "occupied",
        "reserved",
        "maintenance"
    };

    // Location Options (dynamically loaded from existing tables)
    public ObservableCollection<string> LocationOptions { get; } = new();

    // Commands
    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    #endregion

    #region Constructors

    // Constructor for Add mode
    public RestaurantTableSidePanelViewModel(
        IRestaurantTableService restaurantTableService,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _restaurantTableService = restaurantTableService;
        _onSaved = onSaved;
        _onCancelled = onCancelled;

        IsEditMode = false;
        Title = "Add New Table";
        
        // Set default values
        TableNumber = "";
        Location = "Ground Floor";
        Capacity = 2;
        Status = "available";

        // Initialize commands
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Cancel);

        // Load locations from existing tables
        _ = LoadLocationsAsync();

        AppLogger.LogInfo("RestaurantTableSidePanelViewModel created in Add mode", filename: "reservation");
        AppLogger.LogInfo($"Default values set - TableNumber: '{TableNumber}', Location: '{Location}', Capacity: {Capacity}, Status: '{Status}'", filename: "reservation");
    }

    // Constructor for Edit mode
    public RestaurantTableSidePanelViewModel(
        IRestaurantTableService restaurantTableService,
        RestaurantTableDto table,
        Action<bool> onSaved,
        Action onCancelled)
    {
        _restaurantTableService = restaurantTableService;
        _onSaved = onSaved;
        _onCancelled = onCancelled;

        IsEditMode = true;
        Title = $"Edit Table {table.TableNumber}";

        // Populate fields
        TableId = table.Id;
        TableNumber = table.TableNumber;
        Location = table.Location ?? string.Empty;
        Capacity = table.Capacity;
        Status = table.Status ?? "available";

        // Initialize commands
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        CancelCommand = new RelayCommand(Cancel);

        // Load locations from existing tables
        _ = LoadLocationsAsync();

        AppLogger.LogInfo($"RestaurantTableSidePanelViewModel created in Edit mode for table: {table.TableNumber}", filename: "reservation");
    }

    #endregion

    #region Commands

    private async Task SaveAsync()
    {
        AppLogger.LogInfo("===== SaveAsync invoked =====", filename: "reservation");
        AppLogger.LogInfo($"IsEditMode: {IsEditMode}, TableId: {TableId}, TableNumber: '{TableNumber}', Location: '{Location}', Capacity: {Capacity}, Status: '{Status}'", filename: "reservation");

        // Validate
        if (!ValidateInput())
        {
            AppLogger.LogWarning("Validation failed", filename: "reservation");
            AppLogger.LogWarning($"TableNumber empty: {string.IsNullOrWhiteSpace(TableNumber)}, Location empty: {string.IsNullOrWhiteSpace(Location)}, Capacity: {Capacity}", filename: "reservation");
            return;
        }

        AppLogger.LogInfo("Validation passed, proceeding with save", filename: "reservation");
        IsSaving = true;
        HasValidationError = false;
        ValidationMessage = string.Empty;

        try
        {
            if (IsEditMode && TableId.HasValue)
            {
                // Update existing table
                AppLogger.LogInfo($"Updating table ID: {TableId}", filename: "reservation");

                var updateDto = new UpdateRestaurantTableDto
                {
                    TableNumber = TableNumber.Trim(),
                    Location = Location.Trim(),
                    Capacity = Capacity,
                    Status = Status
                };

                var updated = await _restaurantTableService.UpdateAsync(TableId.Value, updateDto);

                if (updated != null)
                {
                    AppLogger.LogInfo("Table updated successfully", filename: "reservation");
                    new MessageDialog("Success", "Table updated successfully!", MessageDialog.MessageType.Success).ShowDialog();
                    _onSaved?.Invoke(true);
                }
                else
                {
                    AppLogger.LogError("Failed to update table", filename: "reservation");
                    HasValidationError = true;
                    ValidationMessage = "Failed to update table. Please try again.";
                }
            }
            else
            {
                // Create new table
                AppLogger.LogInfo("Creating new table", filename: "reservation");

                var createDto = new CreateRestaurantTableDto
                {
                    TableNumber = TableNumber.Trim(),
                    Location = Location.Trim(),
                    Capacity = Capacity,
                    Status = Status
                };

                var created = await _restaurantTableService.CreateAsync(createDto);

                if (created != null)
                {
                    AppLogger.LogInfo("Table created successfully", filename: "reservation");
                    new MessageDialog("Success", "Table created successfully!", MessageDialog.MessageType.Success).ShowDialog();
                    _onSaved?.Invoke(true);
                }
                else
                {
                    AppLogger.LogError("Failed to create table", filename: "reservation");
                    HasValidationError = true;
                    ValidationMessage = "Failed to create table. Please try again.";
                }
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Error saving table", ex, filename: "reservation");
            HasValidationError = true;
            ValidationMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void Cancel()
    {
        AppLogger.LogInfo("Cancel invoked", filename: "reservation");
        _onCancelled?.Invoke();
    }

    #endregion

    #region Validation

    private bool ValidateInput()
    {
        AppLogger.LogInfo("===== ValidateInput called =====", filename: "reservation");
        AppLogger.LogInfo($"TableNumber: '{TableNumber}' (Empty: {string.IsNullOrWhiteSpace(TableNumber)})", filename: "reservation");
        AppLogger.LogInfo($"Location: '{Location}' (Empty: {string.IsNullOrWhiteSpace(Location)})", filename: "reservation");
        AppLogger.LogInfo($"Capacity: {Capacity} (Valid: {Capacity > 0 && Capacity <= 50})", filename: "reservation");

        HasValidationError = false;
        ValidationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(TableNumber))
        {
            HasValidationError = true;
            ValidationMessage = "Table number is required.";
            AppLogger.LogWarning("Validation failed: TableNumber is empty", filename: "reservation");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Location))
        {
            HasValidationError = true;
            ValidationMessage = "Location is required.";
            AppLogger.LogWarning("Validation failed: Location is empty", filename: "reservation");
            return false;
        }

        if (Capacity <= 0)
        {
            HasValidationError = true;
            ValidationMessage = "Capacity must be greater than 0.";
            AppLogger.LogWarning($"Validation failed: Capacity {Capacity} <= 0", filename: "reservation");
            return false;
        }

        if (Capacity > 50)
        {
            HasValidationError = true;
            ValidationMessage = "Capacity cannot exceed 50.";
            AppLogger.LogWarning($"Validation failed: Capacity {Capacity} > 50", filename: "reservation");
            return false;
        }

        AppLogger.LogInfo("Validation passed successfully", filename: "reservation");
        return true;
    }

    private async Task LoadLocationsAsync()
    {
        try
        {
            AppLogger.LogInfo("Loading locations from existing tables...", filename: "reservation");
            
            // Get distinct locations from existing tables
            var locations = await _restaurantTableService.GetDistinctLocationsAsync();
            
            LocationOptions.Clear();
            
            foreach (var location in locations)
            {
                if (!string.IsNullOrWhiteSpace(location))
                {
                    LocationOptions.Add(location);
                    AppLogger.LogInfo($"Added location: {location}", filename: "reservation");
                }
            }

            AppLogger.LogInfo($"Loaded {LocationOptions.Count} distinct locations", filename: "reservation");
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Failed to load locations", ex, filename: "reservation");
            // Add some default locations if loading fails
            LocationOptions.Add("Ground Floor");
            LocationOptions.Add("First Floor");
        }
    }

    #endregion
}
