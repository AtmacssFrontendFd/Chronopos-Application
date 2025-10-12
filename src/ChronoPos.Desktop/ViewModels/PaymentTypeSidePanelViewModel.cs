using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel for the Payment Type side panel
    /// </summary>
    public partial class PaymentTypeSidePanelViewModel : ObservableObject
    {
        #region Private Fields
        private readonly IPaymentTypeService _paymentTypeService;
        private readonly ILayoutDirectionService _layoutDirectionService;
        #endregion

        #region Properties

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _paymentCode = string.Empty;

        [ObservableProperty]
        private string _nameAr = string.Empty;

        [ObservableProperty]
        private bool _status = true;
        
        // Payment Configuration Properties
        [ObservableProperty]
        private bool _changeAllowed = false;

        [ObservableProperty]
        private bool _customerRequired = false;

        [ObservableProperty]
        private bool _markTransactionAsPaid = true;

        [ObservableProperty]
        private string _shortcutKey = string.Empty;

        [ObservableProperty]
        private bool _isRefundable = true;

        [ObservableProperty]
        private bool _isSplitAllowed = true;

        [ObservableProperty]
        private string _formTitle = "Add Payment Type";

        [ObservableProperty]
        private string _saveButtonText = "Save";

        [ObservableProperty]
        private string _validationMessage = string.Empty;

        [ObservableProperty]
        private FlowDirection _currentFlowDirection = FlowDirection.LeftToRight;

        [ObservableProperty]
        private int _id = 0;

        #endregion

        #region Events
        public event Action? OnSave;
        public event Action? OnCancel;
        public event Action? OnClose;
        #endregion

        #region Constructor
        public PaymentTypeSidePanelViewModel(
            IPaymentTypeService paymentTypeService,
            ILayoutDirectionService layoutDirectionService)
        {
            _paymentTypeService = paymentTypeService ?? throw new ArgumentNullException(nameof(paymentTypeService));
            _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));

            // Subscribe to layout direction changes
            _layoutDirectionService.DirectionChanged += OnDirectionChanged;
            CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft 
                ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
        #endregion

        #region Commands

        [RelayCommand]
        private async Task SaveAsync()
        {
            try
            {
                ValidationMessage = string.Empty;

                // Validate required fields
                if (string.IsNullOrWhiteSpace(Name))
                {
                    ValidationMessage = "Name is required";
                    return;
                }

                if (string.IsNullOrWhiteSpace(PaymentCode))
                {
                    ValidationMessage = "Payment Code is required";
                    return;
                }

                if (Id == 0)
                {
                    // Check if name already exists
                    if (await _paymentTypeService.ExistsAsync(Name))
                    {
                        ValidationMessage = "A payment type with this name already exists";
                        return;
                    }

                    // Check if payment code already exists
                    if (await _paymentTypeService.PaymentCodeExistsAsync(PaymentCode))
                    {
                        ValidationMessage = "A payment type with this payment code already exists";
                        return;
                    }

                    var createDto = new CreatePaymentTypeDto
                    {
                        Name = Name.Trim(),
                        PaymentCode = PaymentCode.Trim(),
                        NameAr = NameAr?.Trim(),
                        Status = Status,
                        ChangeAllowed = ChangeAllowed,
                        CustomerRequired = CustomerRequired,
                        MarkTransactionAsPaid = MarkTransactionAsPaid,
                        ShortcutKey = ShortcutKey?.Trim(),
                        IsRefundable = IsRefundable,
                        IsSplitAllowed = IsSplitAllowed,
                        CreatedBy = 1 // TODO: Get from current user context
                    };

                    await _paymentTypeService.CreateAsync(createDto);
                }
                else
                {
                    // Check if name already exists (excluding current item)
                    if (await _paymentTypeService.ExistsAsync(Name, Id))
                    {
                        ValidationMessage = "A payment type with this name already exists";
                        return;
                    }

                    // Check if payment code already exists (excluding current item)
                    if (await _paymentTypeService.PaymentCodeExistsAsync(PaymentCode, Id))
                    {
                        ValidationMessage = "A payment type with this payment code already exists";
                        return;
                    }

                    var updateDto = new UpdatePaymentTypeDto
                    {
                        Id = Id,
                        Name = Name.Trim(),
                        PaymentCode = PaymentCode.Trim(),
                        NameAr = NameAr?.Trim(),
                        Status = Status,
                        ChangeAllowed = ChangeAllowed,
                        CustomerRequired = CustomerRequired,
                        MarkTransactionAsPaid = MarkTransactionAsPaid,
                        ShortcutKey = ShortcutKey?.Trim(),
                        IsRefundable = IsRefundable,
                        IsSplitAllowed = IsSplitAllowed,
                        UpdatedBy = 1 // TODO: Get from current user context
                    };

                    await _paymentTypeService.UpdateAsync(updateDto);
                }

                OnSave?.Invoke();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Error saving payment type: {ex.Message}";
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            OnCancel?.Invoke();
        }

        [RelayCommand]
        private void Close()
        {
            OnClose?.Invoke();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load payment type data for editing
        /// </summary>
        public void LoadPaymentType(PaymentTypeDto paymentType)
        {
            if (paymentType != null)
            {
                Id = paymentType.Id;
                Name = paymentType.Name ?? string.Empty;
                PaymentCode = paymentType.PaymentCode ?? string.Empty;
                NameAr = paymentType.NameAr ?? string.Empty;
                Status = paymentType.Status;
                ChangeAllowed = paymentType.ChangeAllowed;
                CustomerRequired = paymentType.CustomerRequired;
                MarkTransactionAsPaid = paymentType.MarkTransactionAsPaid;
                ShortcutKey = paymentType.ShortcutKey ?? string.Empty;
                IsRefundable = paymentType.IsRefundable;
                IsSplitAllowed = paymentType.IsSplitAllowed;
                FormTitle = "Edit Payment Type";
                SaveButtonText = "Update";
            }
        }

        /// <summary>
        /// Reset form for new payment type
        /// </summary>
        public void ResetForNew()
        {
            Id = 0;
            Name = string.Empty;
            PaymentCode = string.Empty;
            NameAr = string.Empty;
            Status = true;
            ChangeAllowed = false;
            CustomerRequired = false;
            MarkTransactionAsPaid = true;
            ShortcutKey = string.Empty;
            IsRefundable = true;
            IsSplitAllowed = true;
            FormTitle = "Add Payment Type";
            SaveButtonText = "Save";
            ValidationMessage = string.Empty;
        }

        #endregion

        #region Private Methods

        private void OnDirectionChanged(LayoutDirection direction)
        {
            CurrentFlowDirection = direction == LayoutDirection.RightToLeft 
                ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }

        #endregion
    }
}