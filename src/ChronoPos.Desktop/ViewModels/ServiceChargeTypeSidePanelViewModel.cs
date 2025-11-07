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
    public partial class ServiceChargeTypeSidePanelViewModel : ObservableObject
    {
        private readonly IServiceChargeTypeService _serviceChargeTypeService;
        private readonly ICurrentUserService _currentUserService;
        private bool _isEditMode;
        private int _editingTypeId;

        // Events
        public event EventHandler? ServiceChargeTypeSaved;
        public event EventHandler? CloseRequested;

        #region Observable Properties

        [ObservableProperty]
        private string _formTitle = "Add New Service Charge Type";

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _chargeOptionScope = string.Empty;

        [ObservableProperty]
        private bool _isDefault = false;

        [ObservableProperty]
        private bool _status = true;

        [ObservableProperty]
        private ObservableCollection<string> _scopeOptions = new();

        #endregion

        #region Observable Properties - UI State

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        [ObservableProperty]
        private bool _canSave = true;

        #endregion

        public ServiceChargeTypeSidePanelViewModel(
            IServiceChargeTypeService serviceChargeTypeService,
            ICurrentUserService currentUserService)
        {
            _serviceChargeTypeService = serviceChargeTypeService;
            _currentUserService = currentUserService;
            _isEditMode = false;
            _editingTypeId = 0;

            FileLogger.Log("🔧 ServiceChargeTypeSidePanelViewModel constructor started (Add mode)");
            InitializeCollections();
        }

        public ServiceChargeTypeSidePanelViewModel(
            IServiceChargeTypeService serviceChargeTypeService,
            ICurrentUserService currentUserService,
            ServiceChargeTypeDto type)
        {
            _serviceChargeTypeService = serviceChargeTypeService;
            _currentUserService = currentUserService;
            _isEditMode = true;
            _editingTypeId = type.Id;

            FileLogger.Log($"🔧 ServiceChargeTypeSidePanelViewModel constructor started (Edit mode) - ID: {type.Id}");
            InitializeCollections();
            LoadTypeForEdit(type);
        }

        private void InitializeCollections()
        {
            ScopeOptions.Clear();
            ScopeOptions.Add("Global");
            ScopeOptions.Add("Item");
            ScopeOptions.Add("Transaction");
            
            // Set default scope
            if (string.IsNullOrEmpty(ChargeOptionScope))
            {
                ChargeOptionScope = "Global";
            }
        }

        private void LoadTypeForEdit(ServiceChargeTypeDto type)
        {
            FormTitle = "Edit Service Charge Type";
            Code = type.Code;
            Name = type.Name;
            ChargeOptionScope = type.ChargeOptionScope ?? "Global";
            IsDefault = type.IsDefault;
            Status = type.Status;
        }

        #region Commands

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(Code))
                {
                    new MessageDialog("Validation Error", "Code is required.", MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }

                if (string.IsNullOrWhiteSpace(Name))
                {
                    new MessageDialog("Validation Error", "Name is required.", MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }

                IsLoading = true;
                CanSave = false;
                StatusMessage = _isEditMode ? "Updating service charge type..." : "Creating service charge type...";

                var userId = _currentUserService.CurrentUserId ?? 1; // Fallback to 1 if not available

                if (_isEditMode)
                {
                    // Update existing type
                    var updateDto = new UpdateServiceChargeTypeDto
                    {
                        Code = Code.Trim(),
                        Name = Name.Trim(),
                        ChargeOptionScope = ChargeOptionScope,
                        IsDefault = IsDefault,
                        Status = Status
                    };

                    await _serviceChargeTypeService.UpdateAsync(_editingTypeId, updateDto, userId);
                    FileLogger.Log($"✅ Service charge type updated - ID: {_editingTypeId}");
                }
                else
                {
                    // Create new type
                    var createDto = new CreateServiceChargeTypeDto
                    {
                        Code = Code.Trim(),
                        Name = Name.Trim(),
                        ChargeOptionScope = ChargeOptionScope,
                        IsDefault = IsDefault,
                        Status = Status
                    };

                    var createdType = await _serviceChargeTypeService.CreateAsync(createDto, userId);
                    FileLogger.Log($"✅ Service charge type created - ID: {createdType.Id}");
                }

                StatusMessage = _isEditMode ? "Service charge type updated successfully" : "Service charge type created successfully";
                
                // Notify parent
                ServiceChargeTypeSaved?.Invoke(this, EventArgs.Empty);
                
                // Close panel
                CloseRequested?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                FileLogger.Log($"❌ Error saving service charge type: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
                new MessageDialog("Error", $"Failed to save service charge type: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
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
