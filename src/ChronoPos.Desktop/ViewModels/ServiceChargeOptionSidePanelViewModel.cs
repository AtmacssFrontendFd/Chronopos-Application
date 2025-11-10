using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using System.Collections.ObjectModel;
using System.Windows;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class ServiceChargeOptionSidePanelViewModel : ObservableObject
    {
        private readonly IServiceChargeOptionService _serviceChargeOptionService;
        private readonly IServiceChargeTypeService _serviceChargeTypeService;
        private readonly ICurrentUserService _currentUserService;
        private bool _isEditMode;
        private int _editingOptionId;
        private int? _editingOptionServiceChargeTypeId;

        // Events
        public event EventHandler? ServiceChargeOptionSaved;
        public event EventHandler? CloseRequested;

        #region Observable Properties

        [ObservableProperty]
        private string _formTitle = "Add New Service Charge Option";

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private decimal? _cost;

        [ObservableProperty]
        private decimal? _price;

        [ObservableProperty]
        private ServiceChargeTypeDto? _selectedServiceChargeType;

        [ObservableProperty]
        private bool _status = true;

        [ObservableProperty]
        private ObservableCollection<ServiceChargeTypeDto> _availableServiceChargeTypes = new();

        #endregion

        #region Observable Properties - UI State

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _canSave = true;

        #endregion

        public ServiceChargeOptionSidePanelViewModel(
            IServiceChargeOptionService serviceChargeOptionService,
            IServiceChargeTypeService serviceChargeTypeService,
            ICurrentUserService currentUserService)
        {
            _serviceChargeOptionService = serviceChargeOptionService;
            _serviceChargeTypeService = serviceChargeTypeService;
            _currentUserService = currentUserService;
            _isEditMode = false;
            _editingOptionId = 0;

            FileLogger.Log("🔧 ServiceChargeOptionSidePanelViewModel constructor started (Add mode)");
            _ = LoadDropdownDataAsync();
        }

        public ServiceChargeOptionSidePanelViewModel(
            IServiceChargeOptionService serviceChargeOptionService,
            IServiceChargeTypeService serviceChargeTypeService,
            ICurrentUserService currentUserService,
            ServiceChargeOptionDto option)
        {
            _serviceChargeOptionService = serviceChargeOptionService;
            _serviceChargeTypeService = serviceChargeTypeService;
            _currentUserService = currentUserService;
            _isEditMode = true;
            _editingOptionId = option.Id;

            FileLogger.Log($"🔧 ServiceChargeOptionSidePanelViewModel constructor started (Edit mode) - ID: {option.Id}");
            _ = LoadDropdownDataAsync();
            LoadOptionForEdit(option);
        }

        private void LoadOptionForEdit(ServiceChargeOptionDto option)
        {
            FormTitle = "Edit Service Charge Option";
            Name = option.Name;
            Cost = option.Cost;
            Price = option.Price;
            Status = option.Status;
            
            // Selected values will be set after dropdowns are loaded
            _editingOptionServiceChargeTypeId = option.ServiceChargeTypeId;
        }

        private async Task LoadDropdownDataAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading data...";

                // Load Service Charge Types
                var types = await _serviceChargeTypeService.GetAllAsync();

                await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    AvailableServiceChargeTypes.Clear();
                    foreach (var type in types.Where(t => t.Status == true))
                    {
                        AvailableServiceChargeTypes.Add(type);
                    }

                    // Set default selections for Add mode
                    if (!_isEditMode)
                    {
                        if (AvailableServiceChargeTypes.Any())
                        {
                            SelectedServiceChargeType = AvailableServiceChargeTypes.First();
                        }
                    }
                    else
                    {
                        // Set selections for Edit mode
                        if (_editingOptionServiceChargeTypeId.HasValue)
                        {
                            SelectedServiceChargeType = AvailableServiceChargeTypes
                                .FirstOrDefault(t => t.Id == _editingOptionServiceChargeTypeId.Value);
                        }
                    }
                });

                StatusMessage = "Ready";
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error loading dropdown data: {ex.Message}");
                StatusMessage = $"Error loading data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region Commands

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(Name))
                {
                    new MessageDialog("Validation Error", "Name is required.", MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }

                if (SelectedServiceChargeType == null)
                {
                    new MessageDialog("Validation Error", "Service Charge Type is required.", MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }

                IsLoading = true;
                CanSave = false;
                StatusMessage = _isEditMode ? "Updating service charge option..." : "Creating service charge option...";

                var userId = _currentUserService.CurrentUserId ?? 1; // Fallback to 1 if not available

                if (_isEditMode)
                {
                    // Update existing option
                    var updateDto = new UpdateServiceChargeOptionDto
                    {
                        ServiceChargeTypeId = SelectedServiceChargeType.Id,
                        Name = Name.Trim(),
                        Cost = Cost,
                        Price = Price,
                        LanguageId = null,
                        Status = Status
                    };

                    await _serviceChargeOptionService.UpdateAsync(_editingOptionId, updateDto, userId);
                    FileLogger.Log($"✅ Service charge option updated - ID: {_editingOptionId}");
                }
                else
                {
                    // Create new option
                    var createDto = new CreateServiceChargeOptionDto
                    {
                        ServiceChargeTypeId = SelectedServiceChargeType.Id,
                        Name = Name.Trim(),
                        Cost = Cost,
                        Price = Price,
                        LanguageId = null,
                        Status = Status
                    };

                    var createdOption = await _serviceChargeOptionService.CreateAsync(createDto, userId);
                    FileLogger.Log($"✅ Service charge option created - ID: {createdOption.Id}");
                }

                StatusMessage = _isEditMode ? "Service charge option updated successfully" : "Service charge option created successfully";
                
                // Notify parent
                ServiceChargeOptionSaved?.Invoke(this, EventArgs.Empty);
                
                // Close panel
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error saving service charge option: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
                new MessageDialog("Error", $"Failed to save service charge option: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
            finally
            {
                IsLoading = false;
                CanSave = true;
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
