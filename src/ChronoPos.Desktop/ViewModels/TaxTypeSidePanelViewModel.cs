using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel for the Tax Type side panel
    /// </summary>
    public partial class TaxTypeSidePanelViewModel : ObservableObject
    {
        #region Private Fields
        private readonly ITaxTypeService _taxTypeService;
        private readonly ILayoutDirectionService _layoutDirectionService;
        #endregion

        #region Properties

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private decimal _value = 0;

        [ObservableProperty]
        private bool _isPercentage = true;

        [ObservableProperty]
        private bool _appliesToBuying = false;

        [ObservableProperty]
        private bool _appliesToSelling = true;

        [ObservableProperty]
        private bool _includedInPrice = false;

        [ObservableProperty]
        private int _calculationOrder = 1;

        [ObservableProperty]
        private bool _status = true;

        [ObservableProperty]
        private string _formTitle = "Add Tax Type";

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
        public TaxTypeSidePanelViewModel(
            ITaxTypeService taxTypeService,
            ILayoutDirectionService layoutDirectionService)
        {
            _taxTypeService = taxTypeService ?? throw new ArgumentNullException(nameof(taxTypeService));
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

                if (Value < 0)
                {
                    ValidationMessage = "Value must be non-negative";
                    return;
                }

                if (!AppliesToBuying && !AppliesToSelling)
                {
                    ValidationMessage = "Tax type must apply to at least buying or selling";
                    return;
                }

                if (Id == 0)
                {
                    var createDto = new TaxTypeDto
                    {
                        Id = 0,
                        Name = Name.Trim(),
                        Description = Description?.Trim(),
                        Value = Value,
                        IsPercentage = IsPercentage,
                        IncludedInPrice = IncludedInPrice,
                        AppliesToBuying = AppliesToBuying,
                        AppliesToSelling = AppliesToSelling,
                        CalculationOrder = CalculationOrder,
                        IsActive = Status
                    };

                    await _taxTypeService.CreateTaxTypeAsync(createDto);
                }
                else
                {
                    var updateDto = new TaxTypeDto
                    {
                        Id = Id,
                        Name = Name.Trim(),
                        Description = Description?.Trim(),
                        Value = Value,
                        IsPercentage = IsPercentage,
                        IncludedInPrice = IncludedInPrice,
                        AppliesToBuying = AppliesToBuying,
                        AppliesToSelling = AppliesToSelling,
                        CalculationOrder = CalculationOrder,
                        IsActive = Status
                    };

                    await _taxTypeService.UpdateTaxTypeAsync(updateDto);
                }

                OnSave?.Invoke();
            }
            catch (Exception ex)
            {
                ValidationMessage = $"Error saving tax type: {ex.Message}";
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
        /// Load tax type data for editing
        /// </summary>
        public void LoadTaxType(TaxTypeDto taxType)
        {
            if (taxType != null)
            {
                Id = taxType.Id;
                Name = taxType.Name ?? string.Empty;
                Description = taxType.Description ?? string.Empty;
                Value = taxType.Value;
                IsPercentage = taxType.IsPercentage;
                IncludedInPrice = taxType.IncludedInPrice;
                AppliesToBuying = taxType.AppliesToBuying;
                AppliesToSelling = taxType.AppliesToSelling;
                CalculationOrder = taxType.CalculationOrder;
                Status = taxType.IsActive;
                FormTitle = "Edit Tax Type";
                SaveButtonText = "Update";
            }
        }

        /// <summary>
        /// Reset form for new tax type
        /// </summary>
        public void ResetForNew()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            Value = 0;
            IsPercentage = true;
            IncludedInPrice = false;
            AppliesToBuying = false;
            AppliesToSelling = true;
            CalculationOrder = 1;
            Status = true;
            FormTitle = "Add Tax Type";
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