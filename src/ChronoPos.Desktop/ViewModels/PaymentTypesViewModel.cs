using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Infrastructure.Services;
using Microsoft.Win32;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for managing payment types
/// </summary>
public partial class PaymentTypesViewModel : ObservableObject
{
    #region Private Fields

    private readonly IPaymentTypeService _paymentTypeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IThemeService _themeService;
    private readonly IZoomService _zoomService;
    private readonly ILocalizationService _localizationService;
    private readonly IColorSchemeService _colorSchemeService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly IFontService _fontService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;

    #endregion

    #region Observable Properties

    /// <summary>
    /// Collection of payment types
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<PaymentTypeDto> _paymentTypes = new();

    /// <summary>
    /// Currently selected payment type
    /// </summary>
    [ObservableProperty]
    private PaymentTypeDto? _selectedPaymentType;

    /// <summary>
    /// Loading indicator
    /// </summary>
    [ObservableProperty]
    private bool _isLoading = false;

    /// <summary>
    /// Status message
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = string.Empty;

    /// <summary>
    /// Whether to show only active payment types
    /// </summary>
    [ObservableProperty]
    private bool _showActiveOnly = true;

    /// <summary>
    /// Search text for filtering payment types
    /// </summary>
    [ObservableProperty]
    private string _searchText = string.Empty;

    /// <summary>
    /// Whether the side panel is visible
    /// </summary>
    [ObservableProperty]
    private bool _isSidePanelVisible = false;

    /// <summary>
    /// Current flow direction for UI
    /// </summary>
    [ObservableProperty]
    private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

    [ObservableProperty]
    private bool canCreatePaymentType = false;

    [ObservableProperty]
    private bool canEditPaymentType = false;

    [ObservableProperty]
    private bool canDeletePaymentType = false;

    [ObservableProperty]
    private bool canImportPaymentType = false;

    [ObservableProperty]
    private bool canExportPaymentType = false;

    /// <summary>
    /// Side panel view model
    /// </summary>
    [ObservableProperty]
    private PaymentTypeSidePanelViewModel _sidePanelViewModel;

    #endregion

    #region Computed Properties

    /// <summary>
    /// Filtered collection of payment types based on search and active filter
    /// </summary>
    public ObservableCollection<PaymentTypeDto> FilteredPaymentTypes
    {
        get
        {
            var filtered = PaymentTypes.AsEnumerable();

            // Apply active filter
            if (ShowActiveOnly)
            {
                filtered = filtered.Where(p => p.Status);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                filtered = filtered.Where(p => 
                    p.Name.ToLower().Contains(searchLower) ||
                    p.PaymentCode.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(p.NameAr) && p.NameAr.ToLower().Contains(searchLower)));
            }

            return new ObservableCollection<PaymentTypeDto>(filtered);
        }
    }

    /// <summary>
    /// Whether there are any payment types
    /// </summary>
    public bool HasPaymentTypes => PaymentTypes.Count > 0;

    #endregion

    #region Commands

    /// <summary>
    /// Back navigation action (set by parent)
    /// </summary>
    public Action? GoBackAction { get; set; }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor with all required services
    /// </summary>
    public PaymentTypesViewModel(
        IPaymentTypeService paymentTypeService,
        ICurrentUserService currentUserService,
        IThemeService themeService,
        IZoomService zoomService,
        ILocalizationService localizationService,
        IColorSchemeService colorSchemeService,
        ILayoutDirectionService layoutDirectionService,
        IFontService fontService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _zoomService = zoomService ?? throw new ArgumentNullException(nameof(zoomService));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _colorSchemeService = colorSchemeService ?? throw new ArgumentNullException(nameof(colorSchemeService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _fontService = fontService ?? throw new ArgumentNullException(nameof(fontService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));

        InitializePermissions();

        // Initialize side panel view model
        _sidePanelViewModel = new PaymentTypeSidePanelViewModel(paymentTypeService, layoutDirectionService);
        _sidePanelViewModel.OnSave += OnPaymentTypeSaved;
        _sidePanelViewModel.OnCancel += OnSidePanelCancelRequested;
        _sidePanelViewModel.OnClose += OnSidePanelCancelRequested;

        // Initialize with current values
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
            ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

        // Load payment types
        _ = Task.Run(LoadPaymentTypesAsync);
    }

    #endregion

    #region Command Implementations

    /// <summary>
    /// Command to go back to previous screen
    /// </summary>
    [RelayCommand]
    private void GoBack()
    {
        GoBackAction?.Invoke();
    }

    /// <summary>
    /// Command to refresh payment types
    /// </summary>
    [RelayCommand]
    private async Task Refresh()
    {
        await LoadPaymentTypesAsync();
    }

    /// <summary>
    /// Command to add new payment type
    /// </summary>
    [RelayCommand]
    private void AddNew()
    {
        SidePanelViewModel.ResetForNew();
        IsSidePanelVisible = true;
    }

    /// <summary>
    /// Command to edit a payment type
    /// </summary>
    [RelayCommand]
    private void Edit(PaymentTypeDto paymentType)
    {
        if (paymentType != null)
        {
            SidePanelViewModel.LoadPaymentType(paymentType);
            IsSidePanelVisible = true;
        }
    }

    /// <summary>
    /// Command to delete a payment type
    /// </summary>
    [RelayCommand]
    private async Task Delete(PaymentTypeDto paymentType)
    {
        if (paymentType != null)
        {
            var confirmDialog = new ConfirmationDialog(
                "Delete Payment Type",
                $"Are you sure you want to delete the payment type '{paymentType.Name}'?\n\nThis action cannot be undone.",
                ConfirmationDialog.DialogType.Danger);
            
            var result = confirmDialog.ShowDialog();
            if (result == true)
            {
                try
                {
                    IsLoading = true;
                    StatusMessage = "Deleting payment type...";
                    
                    await _paymentTypeService.DeleteAsync(paymentType.Id, 1); // Assuming user ID 1 for now
                    
                    StatusMessage = "Payment type deleted successfully";
                    await LoadPaymentTypesAsync();
                    
                    var successDialog = new MessageDialog(
                        "Success",
                        "Payment type deleted successfully.",
                        MessageDialog.MessageType.Success);
                    successDialog.ShowDialog();
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error deleting payment type: {ex.Message}";
                    
                    var errorDialog = new MessageDialog(
                        "Delete Error",
                        $"An error occurred while deleting the payment type:\n\n{ex.Message}",
                        MessageDialog.MessageType.Error);
                    errorDialog.ShowDialog();
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }
    }

    /// <summary>
    /// Command to toggle active filter
    /// </summary>
    [RelayCommand]
    private void ToggleActiveFilter()
    {
        ShowActiveOnly = !ShowActiveOnly;
        OnPropertyChanged(nameof(FilteredPaymentTypes));
    }

    /// <summary>
    /// Command to clear search
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
    }

    /// <summary>
    /// Command to export payment types to CSV
    /// </summary>
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"PaymentTypes_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsLoading = true;
                StatusMessage = "Exporting payment types...";

                var csv = new StringBuilder();
                csv.AppendLine("Name,PaymentCode,NameAr,Status,ChangeAllowed,CustomerRequired,MarkTransactionAsPaid,ShortcutKey,IsRefundable,IsSplitAllowed");

                foreach (var paymentType in PaymentTypes)
                {
                    csv.AppendLine($"\"{paymentType.Name}\"," +
                                 $"\"{paymentType.PaymentCode}\"," +
                                 $"\"{paymentType.NameAr ?? ""}\"," +
                                 $"{paymentType.Status}," +
                                 $"{paymentType.ChangeAllowed}," +
                                 $"{paymentType.CustomerRequired}," +
                                 $"{paymentType.MarkTransactionAsPaid}," +
                                 $"\"{paymentType.ShortcutKey ?? ""}\"," +
                                 $"{paymentType.IsRefundable}," +
                                 $"{paymentType.IsSplitAllowed}");
                }

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                StatusMessage = $"Exported {PaymentTypes.Count} payment types successfully";
                
                var successDialog = new MessageDialog(
                    "Export Successful",
                    $"Successfully exported {PaymentTypes.Count} payment types to:\n\n{saveFileDialog.FileName}",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error exporting payment types: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Export Error",
                $"An error occurred while exporting payment types:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to import payment types from CSV
    /// </summary>
    [RelayCommand]
    private async Task ImportAsync()
    {
        try
        {
            // Show custom import dialog
            var importDialog = new ImportDialog();
            var dialogResult = importDialog.ShowDialog();
            
            if (dialogResult != true)
                return;

            if (importDialog.SelectedAction == ImportDialog.ImportAction.DownloadTemplate)
            {
                // Download Template
                await DownloadTemplateAsync();
                return;
            }
            else if (importDialog.SelectedAction == ImportDialog.ImportAction.UploadFile)
            {
                // Upload File
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = ".csv"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    IsLoading = true;
                    StatusMessage = "Importing payment types...";

                    var lines = await File.ReadAllLinesAsync(openFileDialog.FileName);
                    if (lines.Length <= 1)
                    {
                        var warningDialog = new MessageDialog(
                            "Import Error",
                            "The CSV file is empty or contains only headers.",
                            MessageDialog.MessageType.Warning);
                        warningDialog.ShowDialog();
                        IsLoading = false;
                        return;
                    }

                int successCount = 0;
                int errorCount = 0;
                var errors = new StringBuilder();

                // Skip header row
                for (int i = 1; i < lines.Length; i++)
                {
                    try
                    {
                        var line = lines[i];
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        var values = ParseCsvLine(line);
                        if (values.Length < 10)
                        {
                            errorCount++;
                            errors.AppendLine($"Line {i + 1}: Invalid format (expected 10 columns)");
                            continue;
                        }

                        var createDto = new CreatePaymentTypeDto
                        {
                            Name = values[0].Trim('"'),
                            PaymentCode = values[1].Trim('"'),
                            NameAr = string.IsNullOrWhiteSpace(values[2].Trim('"')) ? null : values[2].Trim('"'),
                            Status = bool.Parse(values[3]),
                            ChangeAllowed = bool.Parse(values[4]),
                            CustomerRequired = bool.Parse(values[5]),
                            MarkTransactionAsPaid = bool.Parse(values[6]),
                            ShortcutKey = string.IsNullOrWhiteSpace(values[7].Trim('"')) ? null : values[7].Trim('"'),
                            IsRefundable = bool.Parse(values[8]),
                            IsSplitAllowed = bool.Parse(values[9]),
                            CreatedBy = 1 // TODO: Get from current user
                        };

                        await _paymentTypeService.CreateAsync(createDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        var errorMessage = ex.Message;
                        
                        // Include inner exception details if available
                        if (ex.InnerException != null)
                        {
                            errorMessage += $" | Inner: {ex.InnerException.Message}";
                            
                            // Go deeper if there's another inner exception
                            if (ex.InnerException.InnerException != null)
                            {
                                errorMessage += $" | Details: {ex.InnerException.InnerException.Message}";
                            }
                        }
                        
                        errors.AppendLine($"Line {i + 1}: {errorMessage}");
                    }
                }

                    await LoadPaymentTypesAsync();

                    var message = $"Import completed:\n\n✓ {successCount} payment types imported successfully";
                    if (errorCount > 0)
                    {
                        message += $"\n✗ {errorCount} errors occurred\n\nErrors:\n{errors}";
                    }

                    var resultDialog = new MessageDialog(
                        "Import Complete",
                        message,
                        errorCount > 0 ? MessageDialog.MessageType.Warning : MessageDialog.MessageType.Success);
                    resultDialog.ShowDialog();
                    
                    StatusMessage = $"Import completed: {successCount} successful, {errorCount} errors";
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error importing payment types: {ex.Message}";
            
            var errorDialog = new MessageDialog(
                "Import Error",
                $"An error occurred while importing payment types:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Download CSV template for payment types
    /// </summary>
    private async Task DownloadTemplateAsync()
    {
        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = "PaymentTypes_Template.csv",
                DefaultExt = ".csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var csv = new StringBuilder();
                csv.AppendLine("Name,PaymentCode,NameAr,Status,ChangeAllowed,CustomerRequired,MarkTransactionAsPaid,ShortcutKey,IsRefundable,IsSplitAllowed");
                csv.AppendLine("\"Cash\",\"CASH\",\"نقد\",True,True,False,True,\"F1\",True,True");
                csv.AppendLine("\"Credit Card\",\"CC\",\"بطاقة الائتمان\",True,False,False,True,\"F2\",False,False");
                csv.AppendLine("\"Bank Transfer\",\"BANK\",\"تحويل بنكي\",True,False,True,False,\"\",True,False");

                await File.WriteAllTextAsync(saveFileDialog.FileName, csv.ToString());
                
                var successDialog = new MessageDialog(
                    "Template Downloaded",
                    $"Template downloaded successfully to:\n\n{saveFileDialog.FileName}\n\nYou can now fill in your data and import it.",
                    MessageDialog.MessageType.Success);
                successDialog.ShowDialog();
            }
        }
        catch (Exception ex)
        {
            var errorDialog = new MessageDialog(
                "Download Error",
                $"An error occurred while downloading template:\n\n{ex.Message}",
                MessageDialog.MessageType.Error);
            errorDialog.ShowDialog();
        }
    }

    /// <summary>
    /// Parse CSV line handling quoted values
    /// </summary>
    private string[] ParseCsvLine(string line)
    {
        var values = new List<string>();
        var currentValue = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                currentValue.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                values.Add(currentValue.ToString());
                currentValue.Clear();
            }
            else
            {
                currentValue.Append(c);
            }
        }

        values.Add(currentValue.ToString());
        return values.ToArray();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Load all payment types from the service
    /// </summary>
    private async Task LoadPaymentTypesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Loading payment types...";
            
            var paymentTypes = await _paymentTypeService.GetAllAsync();
            
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                PaymentTypes.Clear();
                foreach (var paymentType in paymentTypes)
                {
                    PaymentTypes.Add(paymentType);
                }
            });
            
            // Force refresh of filtered collection
            OnPropertyChanged(nameof(FilteredPaymentTypes));
            OnPropertyChanged(nameof(HasPaymentTypes));
            
            StatusMessage = $"Loaded {paymentTypes.Count()} payment types";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading payment types: {ex.Message}";
            MessageBox.Show($"Error loading payment types: {ex.Message}", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Handle payment type saved event from side panel
    /// </summary>
    private async void OnPaymentTypeSaved()
    {
        IsSidePanelVisible = false;
        await LoadPaymentTypesAsync();
    }

    /// <summary>
    /// Handle cancel request from side panel
    /// </summary>
    private void OnSidePanelCancelRequested()
    {
        IsSidePanelVisible = false;
    }

    #endregion

    #region Property Changed Handlers

    /// <summary>
    /// Handle changes to ShowActiveOnly property
    /// </summary>
    partial void OnShowActiveOnlyChanged(bool value)
    {
        OnPropertyChanged(nameof(FilteredPaymentTypes));
    }

    /// <summary>
    /// Handle changes to SearchText property
    /// </summary>
    partial void OnSearchTextChanged(string value)
    {
        OnPropertyChanged(nameof(FilteredPaymentTypes));
    }

    private void InitializePermissions()
    {
        try
        {
            CanCreatePaymentType = _currentUserService.HasPermission(ScreenNames.PAYMENT_TYPES, TypeMatrix.CREATE);
            CanEditPaymentType = _currentUserService.HasPermission(ScreenNames.PAYMENT_TYPES, TypeMatrix.UPDATE);
            CanDeletePaymentType = _currentUserService.HasPermission(ScreenNames.PAYMENT_TYPES, TypeMatrix.DELETE);
            CanImportPaymentType = _currentUserService.HasPermission(ScreenNames.PAYMENT_TYPES, TypeMatrix.IMPORT);
            CanExportPaymentType = _currentUserService.HasPermission(ScreenNames.PAYMENT_TYPES, TypeMatrix.EXPORT);
        }
        catch (Exception)
        {
            CanCreatePaymentType = false;
            CanEditPaymentType = false;
            CanDeletePaymentType = false;
            CanImportPaymentType = false;
            CanExportPaymentType = false;
        }
    }

    #endregion
}