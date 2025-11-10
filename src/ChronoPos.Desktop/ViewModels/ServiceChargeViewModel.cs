using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Constants;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views;
using ChronoPos.Desktop.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class ServiceChargeViewModel : ObservableObject
    {
        private readonly IServiceChargeTypeService _serviceChargeTypeService;
        private readonly IServiceChargeOptionService _serviceChargeOptionService;
        private readonly ICurrentUserService _currentUserService;
        private readonly Action? _navigateBack;

        #region Observable Properties - Service Charge Types

        [ObservableProperty]
        private ObservableCollection<ServiceChargeTypeDto> _serviceChargeTypes = new();

        [ObservableProperty]
        private ObservableCollection<ServiceChargeTypeDto> _filteredServiceChargeTypes = new();

        [ObservableProperty]
        private ServiceChargeTypeDto? _selectedServiceChargeType;

        #endregion

        #region Observable Properties - Service Charge Options

        [ObservableProperty]
        private ObservableCollection<ServiceChargeOptionDto> _serviceChargeOptions = new();

        [ObservableProperty]
        private ObservableCollection<ServiceChargeOptionDto> _filteredServiceChargeOptions = new();

        [ObservableProperty]
        private ServiceChargeOptionDto? _selectedServiceChargeOption;

        #endregion

        #region Observable Properties - UI State

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showActiveOnly = true;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = "Loading...";

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _isSidePanelVisible = false;

        [ObservableProperty]
        private object? _sidePanelContent;

        [ObservableProperty]
        private int _selectedTabIndex = 0; // 0 = Types, 1 = Options

        [ObservableProperty]
        private bool _canCreateServiceCharge = false;

        [ObservableProperty]
        private bool _canEditServiceCharge = false;

        [ObservableProperty]
        private bool _canDeleteServiceCharge = false;

        [ObservableProperty]
        private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

        #endregion

        #region Computed Properties

        public int TotalServiceChargeTypes => ServiceChargeTypes?.Count ?? 0;
        public int TotalServiceChargeOptions => ServiceChargeOptions?.Count ?? 0;
        public bool HasServiceChargeTypes => FilteredServiceChargeTypes != null && FilteredServiceChargeTypes.Count > 0;
        public bool HasServiceChargeOptions => FilteredServiceChargeOptions != null && FilteredServiceChargeOptions.Count > 0;

        #endregion

        public ServiceChargeViewModel(
            IServiceChargeTypeService serviceChargeTypeService,
            IServiceChargeOptionService serviceChargeOptionService,
            ICurrentUserService currentUserService,
            Action? navigateBack = null)
        {
            _serviceChargeTypeService = serviceChargeTypeService ?? throw new ArgumentNullException(nameof(serviceChargeTypeService));
            _serviceChargeOptionService = serviceChargeOptionService ?? throw new ArgumentNullException(nameof(serviceChargeOptionService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _navigateBack = navigateBack;

            FileLogger.Log("🔧 ServiceChargeViewModel constructor started");

            InitializePermissions();
            _ = Task.Run(LoadDataAsync);

            FileLogger.Log("🔧 ServiceChargeViewModel constructor completed");
        }

        #region Property Change Handlers

        partial void OnSearchTextChanged(string value)
        {
            FilterData();
        }

        partial void OnShowActiveOnlyChanged(bool value)
        {
            FilterData();
        }

        partial void OnSelectedTabIndexChanged(int value)
        {
            // When tab changes, close side panel
            IsSidePanelVisible = false;
            SidePanelContent = null;
            StatusMessage = "Ready";
            
            // Reload data for the selected tab
            if (value == 0)
            {
                // Service Charge Types tab selected
                _ = LoadServiceChargeTypesAsync();
            }
            else if (value == 1)
            {
                // Service Charge Options tab selected
                _ = LoadServiceChargeOptionsAsync();
            }
        }

        #endregion

        #region Data Loading

        private async Task LoadDataAsync()
        {
            try
            {
                FileLogger.Log("🔄 LoadDataAsync started");
                IsLoading = true;
                LoadingMessage = "Loading service charges...";
                StatusMessage = "Loading...";

                // Load both types and options
                await Task.WhenAll(
                    LoadServiceChargeTypesAsync(),
                    LoadServiceChargeOptionsAsync()
                );

                StatusMessage = "Ready";
                FileLogger.Log($"✅ LoadDataAsync completed - {ServiceChargeTypes.Count} types, {ServiceChargeOptions.Count} options loaded");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in LoadDataAsync: {ex.Message}");
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadServiceChargeTypesAsync()
        {
            try
            {
                FileLogger.Log("🔄 LoadServiceChargeTypesAsync started");
                var types = await _serviceChargeTypeService.GetAllAsync();
                FileLogger.Log($"🔄 Retrieved {types.Count()} service charge types from service");

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ServiceChargeTypes.Clear();
                    foreach (var type in types)
                    {
                        ServiceChargeTypes.Add(type);
                    }
                    FilterData();
                    OnPropertyChanged(nameof(TotalServiceChargeTypes));
                    OnPropertyChanged(nameof(HasServiceChargeTypes));
                });

                FileLogger.Log($"✅ LoadServiceChargeTypesAsync completed - {types.Count()} types loaded");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in LoadServiceChargeTypesAsync: {ex.Message}");
                throw;
            }
        }

        private async Task LoadServiceChargeOptionsAsync()
        {
            try
            {
                FileLogger.Log("🔄 LoadServiceChargeOptionsAsync started");
                var options = await _serviceChargeOptionService.GetAllAsync();
                FileLogger.Log($"🔄 Retrieved {options.Count()} service charge options from service");

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    ServiceChargeOptions.Clear();
                    foreach (var option in options)
                    {
                        ServiceChargeOptions.Add(option);
                    }
                    FilterData();
                    OnPropertyChanged(nameof(TotalServiceChargeOptions));
                    OnPropertyChanged(nameof(HasServiceChargeOptions));
                });

                FileLogger.Log($"✅ LoadServiceChargeOptionsAsync completed - {options.Count()} options loaded");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in LoadServiceChargeOptionsAsync: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Filtering

        private void FilterData()
        {
            try
            {
                FileLogger.Log($"🔍 FilterData started - Tab: {SelectedTabIndex}, SearchText: '{SearchText}', ShowActiveOnly: {ShowActiveOnly}");

                if (SelectedTabIndex == 0)
                {
                    FilterServiceChargeTypes();
                }
                else
                {
                    FilterServiceChargeOptions();
                }
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in FilterData: {ex.Message}");
            }
        }

        private void FilterServiceChargeTypes()
        {
            try
            {
                var filtered = ServiceChargeTypes.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(t =>
                        t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        t.Code.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(t.ChargeOptionScope) && t.ChargeOptionScope.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                }

                // Apply active filter
                if (ShowActiveOnly)
                {
                    filtered = filtered.Where(t => t.Status == true);
                }

                FilteredServiceChargeTypes.Clear();
                foreach (var type in filtered)
                {
                    FilteredServiceChargeTypes.Add(type);
                }

                OnPropertyChanged(nameof(HasServiceChargeTypes));
                FileLogger.Log($"🔍 FilterServiceChargeTypes completed - {FilteredServiceChargeTypes.Count} filtered from {ServiceChargeTypes.Count} total");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in FilterServiceChargeTypes: {ex.Message}");
            }
        }

        private void FilterServiceChargeOptions()
        {
            try
            {
                var filtered = ServiceChargeOptions.AsEnumerable();

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(o =>
                        o.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (!string.IsNullOrEmpty(o.ServiceChargeTypeName) && o.ServiceChargeTypeName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrEmpty(o.LanguageName) && o.LanguageName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                }

                // Apply active filter
                if (ShowActiveOnly)
                {
                    filtered = filtered.Where(o => o.Status == true);
                }

                FilteredServiceChargeOptions.Clear();
                foreach (var option in filtered)
                {
                    FilteredServiceChargeOptions.Add(option);
                }

                OnPropertyChanged(nameof(HasServiceChargeOptions));
                FileLogger.Log($"🔍 FilterServiceChargeOptions completed - {FilteredServiceChargeOptions.Count} filtered from {ServiceChargeOptions.Count} total");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in FilterServiceChargeOptions: {ex.Message}");
            }
        }

        #endregion

        #region Permissions

        private void InitializePermissions()
        {
            try
            {
                // TODO: Add proper permission checks when permission system is fully implemented
                CanCreateServiceCharge = true; // _currentUserService.HasScreenPermission(ScreenNames.SERVICE_CHARGE, "Can Create");
                CanEditServiceCharge = true; // _currentUserService.HasScreenPermission(ScreenNames.SERVICE_CHARGE, "Can Update");
                CanDeleteServiceCharge = true; // _currentUserService.HasScreenPermission(ScreenNames.SERVICE_CHARGE, "Can Delete");

                FileLogger.Log($"✅ Permissions initialized - Create: {CanCreateServiceCharge}, Edit: {CanEditServiceCharge}, Delete: {CanDeleteServiceCharge}");
            }
            catch (Exception ex)
            {
                CanCreateServiceCharge = false;
                CanEditServiceCharge = false;
                CanDeleteServiceCharge = false;
                FileLogger.Log($"❌ Error initializing permissions: {ex.Message}");
            }
        }

        #endregion

        #region Commands

        [RelayCommand]
        private void AddServiceChargeType()
        {
            if (!CanCreateServiceCharge)
            {
                new MessageDialog("Permission Denied", "You don't have permission to create service charge types.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            try
            {
                FileLogger.Log("📝 AddServiceChargeType - Opening side panel");
                
                // Create the side panel ViewModel
                var sidePanelViewModel = new ServiceChargeTypeSidePanelViewModel(
                    _serviceChargeTypeService,
                    _currentUserService);

                // Subscribe to events
                sidePanelViewModel.ServiceChargeTypeSaved += OnServiceChargeTypeSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;

                // Create the view and set DataContext
                var sidePanelView = new ServiceChargeTypeSidePanelView
                {
                    DataContext = sidePanelViewModel
                };

                // Set side panel content and show
                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;

                StatusMessage = "Adding new service charge type";
                FileLogger.Log("✅ AddServiceChargeType - Side panel opened");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in AddServiceChargeType: {ex.Message}");
                new MessageDialog("Error", $"Failed to open add form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
        }

        [RelayCommand]
        private void AddServiceChargeOption()
        {
            if (!CanCreateServiceCharge)
            {
                new MessageDialog("Permission Denied", "You don't have permission to create service charge options.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            try
            {
                FileLogger.Log("📝 AddServiceChargeOption - Opening side panel");
                
                // Create the side panel ViewModel
                var sidePanelViewModel = new ServiceChargeOptionSidePanelViewModel(
                    _serviceChargeOptionService,
                    _serviceChargeTypeService,
                    _currentUserService);

                // Subscribe to events
                sidePanelViewModel.ServiceChargeOptionSaved += OnServiceChargeOptionSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;

                // Create the view and set DataContext
                var sidePanelView = new ServiceChargeOptionSidePanelView
                {
                    DataContext = sidePanelViewModel
                };

                // Set side panel content and show
                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;

                StatusMessage = "Adding new service charge option";
                FileLogger.Log("✅ AddServiceChargeOption - Side panel opened");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in AddServiceChargeOption: {ex.Message}");
                new MessageDialog("Error", $"Failed to open add form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
        }

        [RelayCommand]
        private void EditServiceChargeType(ServiceChargeTypeDto? type)
        {
            if (type == null) return;

            if (!CanEditServiceCharge)
            {
                new MessageDialog("Permission Denied", "You don't have permission to edit service charge types.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            try
            {
                FileLogger.Log($"✏️ EditServiceChargeType - Opening side panel for '{type.Name}'");
                
                // Create the side panel ViewModel in edit mode
                var sidePanelViewModel = new ServiceChargeTypeSidePanelViewModel(
                    _serviceChargeTypeService,
                    _currentUserService,
                    type);

                // Subscribe to events
                sidePanelViewModel.ServiceChargeTypeSaved += OnServiceChargeTypeSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;

                // Create the view and set DataContext
                var sidePanelView = new ServiceChargeTypeSidePanelView
                {
                    DataContext = sidePanelViewModel
                };

                // Set side panel content and show
                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;

                StatusMessage = $"Editing service charge type '{type.Name}'";
                FileLogger.Log($"✅ EditServiceChargeType - Side panel opened for '{type.Name}'");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in EditServiceChargeType: {ex.Message}");
                new MessageDialog("Error", $"Failed to open edit form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
        }

        [RelayCommand]
        private void EditServiceChargeOption(ServiceChargeOptionDto? option)
        {
            if (option == null) return;

            if (!CanEditServiceCharge)
            {
                new MessageDialog("Permission Denied", "You don't have permission to edit service charge options.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            try
            {
                FileLogger.Log($"✏️ EditServiceChargeOption - Opening side panel for '{option.Name}'");
                
                // Create the side panel ViewModel in edit mode
                var sidePanelViewModel = new ServiceChargeOptionSidePanelViewModel(
                    _serviceChargeOptionService,
                    _serviceChargeTypeService,
                    _currentUserService,
                    option);

                // Subscribe to events
                sidePanelViewModel.ServiceChargeOptionSaved += OnServiceChargeOptionSaved;
                sidePanelViewModel.CloseRequested += OnSidePanelCloseRequested;

                // Create the view and set DataContext
                var sidePanelView = new ServiceChargeOptionSidePanelView
                {
                    DataContext = sidePanelViewModel
                };

                // Set side panel content and show
                SidePanelContent = sidePanelView;
                IsSidePanelVisible = true;

                StatusMessage = $"Editing service charge option '{option.Name}'";
                FileLogger.Log($"✅ EditServiceChargeOption - Side panel opened for '{option.Name}'");
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in EditServiceChargeOption: {ex.Message}");
                new MessageDialog("Error", $"Failed to open edit form: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
        }

        [RelayCommand]
        private async Task DeleteServiceChargeTypeAsync(ServiceChargeTypeDto? type)
        {
            if (type == null) return;

            if (!CanDeleteServiceCharge)
            {
                new MessageDialog("Permission Denied", "You don't have permission to delete service charge types.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            var confirmDialog = new ConfirmationDialog(
                "Delete Service Charge Type",
                $"Are you sure you want to delete the service charge type '{type.Name}'?",
                ConfirmationDialog.DialogType.Warning);

            if (confirmDialog.ShowDialog() == true)
            {
                try
                {
                    // For now, delete without userId since the service method signature may vary
                    // TODO: Update when service methods are finalized
                    await _serviceChargeTypeService.DeleteAsync(type.Id, 1); // Using default userId = 1
                    StatusMessage = $"Service charge type '{type.Name}' deleted successfully";
                    await LoadServiceChargeTypesAsync();
                }
                catch (Exception ex)
                {
                    FileLogger.Log($"❌ Error deleting service charge type: {ex.Message}");
                    new MessageDialog("Error", $"Failed to delete service charge type: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
                }
            }
        }

        [RelayCommand]
        private async Task DeleteServiceChargeOptionAsync(ServiceChargeOptionDto? option)
        {
            if (option == null) return;

            if (!CanDeleteServiceCharge)
            {
                new MessageDialog("Permission Denied", "You don't have permission to delete service charge options.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            var confirmDialog = new ConfirmationDialog(
                "Delete Service Charge Option",
                $"Are you sure you want to delete the service charge option '{option.Name}'?",
                ConfirmationDialog.DialogType.Warning);

            if (confirmDialog.ShowDialog() == true)
            {
                try
                {
                    // For now, delete without userId since the service method signature may vary
                    // TODO: Update when service methods are finalized
                    await _serviceChargeOptionService.DeleteAsync(option.Id, 1); // Using default userId = 1
                    StatusMessage = $"Service charge option '{option.Name}' deleted successfully";
                    await LoadServiceChargeOptionsAsync();
                }
                catch (Exception ex)
                {
                    FileLogger.Log($"❌ Error deleting service charge option: {ex.Message}");
                    new MessageDialog("Error", $"Failed to delete service charge option: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
                }
            }
        }

        [RelayCommand]
        private async Task RefreshDataAsync()
        {
            StatusMessage = "Refreshing data...";
            await LoadDataAsync();
            StatusMessage = "Data refreshed successfully";
        }

        [RelayCommand]
        private void FilterActive()
        {
            ShowActiveOnly = !ShowActiveOnly;
            StatusMessage = ShowActiveOnly ? "Showing active items only" : "Showing all items";
        }

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            ShowActiveOnly = true;
            StatusMessage = "Filters cleared";
        }

        [RelayCommand]
        private void Back()
        {
            _navigateBack?.Invoke();
        }

        #endregion

        #region Event Handlers

        private async void OnServiceChargeTypeSaved(object? sender, EventArgs e)
        {
            try
            {
                FileLogger.Log("✅ ServiceChargeTypeSaved event received");
                StatusMessage = "Service charge type saved successfully";
                
                // Close side panel
                IsSidePanelVisible = false;
                SidePanelContent = null;

                // Reload types list
                await LoadServiceChargeTypesAsync();
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in OnServiceChargeTypeSaved: {ex.Message}");
            }
        }

        private async void OnServiceChargeOptionSaved(object? sender, EventArgs e)
        {
            try
            {
                FileLogger.Log("✅ ServiceChargeOptionSaved event received");
                StatusMessage = "Service charge option saved successfully";
                
                // Close side panel
                IsSidePanelVisible = false;
                SidePanelContent = null;

                // Reload options list
                await LoadServiceChargeOptionsAsync();
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in OnServiceChargeOptionSaved: {ex.Message}");
            }
        }

        private void OnSidePanelCloseRequested(object? sender, EventArgs e)
        {
            try
            {
                FileLogger.Log("🚪 Side panel close requested");
                IsSidePanelVisible = false;
                SidePanelContent = null;
                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error in OnSidePanelCloseRequested: {ex.Message}");
            }
        }

        #endregion
    }
}
