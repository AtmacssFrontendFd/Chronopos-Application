using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Services;
using ChronoPos.Domain.Enums;

namespace ChronoPos.Desktop.Models
{
    public class AdjustProductModel : INotifyPropertyChanged
    {
        private string _searchText = string.Empty;
        private ProductDto? _selectedProduct;
        private StockAdjustmentSearchItemDto? _selectedSearchItem;
        private StockAdjustmentMode _adjustmentMode = StockAdjustmentMode.Product;
        private string _productName = string.Empty;
        private decimal _currentStock = 0;
        private decimal _newQuantity = 0;
        private decimal _changeAmount = 0;
        private string _changeAmountText = "0";
        private bool _isIncrement = true;
        private string _adjustmentType = "Increase";
        private string _quantity = "0";
        private string _reason = "Stock count correction";
        private string _reasonText = "Stock count correction";
        private DateTime? _expiryDate;
        private int? _reasonId;
        private List<ProductBatchDto> _availableBatches = new();
        private ProductBatchDto? _selectedBatch;

        public StockAdjustmentMode AdjustmentMode
        {
            get => _adjustmentMode;
            set
            {
                FileLogger.Log($"[AdjustProductModel] AdjustmentMode changing: '{_adjustmentMode}' → '{value}'");
                _adjustmentMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProductMode));
                OnPropertyChanged(nameof(IsProductUnitMode));
                OnPropertyChanged(nameof(ModeDisplayText));
            }
        }

        public bool IsProductMode => AdjustmentMode == StockAdjustmentMode.Product;
        public bool IsProductUnitMode => AdjustmentMode == StockAdjustmentMode.ProductUnit;
        public string ModeDisplayText => AdjustmentMode == StockAdjustmentMode.Product ? "Product" : "Product Unit";

        public string SearchText
        {
            get => _searchText;
            set
            {
                FileLogger.Log($"[AdjustProductModel] SearchText changing: '{_searchText}' → '{value}' (IsUpdatingFromSelection: {_isUpdatingFromSelection})");
                
                // Add stack trace if SearchText is being cleared unexpectedly
                if (string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(_searchText))
                {
                    FileLogger.Log($"[AdjustProductModel] WARNING: SearchText being cleared! Current: '{_searchText}' → Empty");
                    FileLogger.Log($"[AdjustProductModel] Stack trace: {Environment.StackTrace}");
                }
                
                // If we're updating from selection, only update the backing field
                if (_isUpdatingFromSelection)
                {
                    _searchText = value;
                    return; // Don't trigger PropertyChanged during selection updates
                }
                
                _searchText = value;
                OnPropertyChanged();
            }
        }

        private bool _isUpdatingFromSelection = false;
        public bool IsUpdatingFromSelection => _isUpdatingFromSelection;
        
        public StockAdjustmentSearchItemDto? SelectedSearchItem
        {
            get => _selectedSearchItem;
            set
            {
                FileLogger.Log($"[AdjustProductModel] SelectedSearchItem changing: '{_selectedSearchItem?.Name ?? "NULL"}' → '{value?.Name ?? "NULL"}'");
                
                // Log the stack trace to see what's causing the deselection
                if (value == null && _selectedSearchItem != null)
                {
                    FileLogger.Log($"[AdjustProductModel] WARNING: SelectedSearchItem being cleared! Stack trace:");
                    FileLogger.Log($"[AdjustProductModel] {Environment.StackTrace}");
                }
                
                _selectedSearchItem = value;
                OnPropertyChanged();
                
                if (value != null)
                {
                    _isUpdatingFromSelection = true;
                    try
                    {
                        // CRITICAL: Update SearchText to match selection display text to prevent conflicts
                        var expectedText = value.SearchDisplayText;
                        if (_searchText != expectedText)
                        {
                            _searchText = expectedText;
                            FileLogger.Log($"[AdjustProductModel] SearchText synchronized to: '{expectedText}'");
                        }
                        
                        ProductName = value.Name;
                        CurrentStock = value.CurrentQuantity;
                        OnPropertyChanged(nameof(HasProductImage));
                        OnPropertyChanged(nameof(ProductImagePath));
                        OnPropertyChanged(nameof(UnitDisplayText));
                        // Also update ProductId property notification since validation depends on it
                        OnPropertyChanged(nameof(ProductId));
                        
                        // Notify SearchText change after other updates
                        OnPropertyChanged(nameof(SearchText));
                    }
                    finally
                    {
                        _isUpdatingFromSelection = false;
                    }
                }
                else if (_isUpdatingFromSelection)
                {
                    // Don't allow clearing during updates
                    FileLogger.Log("[AdjustProductModel] Preventing selection clear during update");
                    return;
                }
            }
        }

        public ProductDto? SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                FileLogger.Log($"[AdjustProductModel] SelectedProduct changing: '{_selectedProduct?.Name ?? "NULL"}' (ID: {_selectedProduct?.Id ?? 0}) → '{value?.Name ?? "NULL"}' (ID: {value?.Id ?? 0})");
                _selectedProduct = value;
                if (value != null)
                {
                    ProductName = value.Name;
                    FileLogger.Log($"[AdjustProductModel] ProductName updated to: '{value.Name}'");
                    // CurrentStock will be loaded separately
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasProductImage));
                OnPropertyChanged(nameof(ProductImagePath));
                FileLogger.Log($"[AdjustProductModel] SelectedProduct change complete");
            }
        }

        public int ProductId => SelectedSearchItem?.ProductId ?? SelectedProduct?.Id ?? 0;

        public string? ProductImagePath => SelectedSearchItem?.ImagePath ?? SelectedProduct?.ImagePath;

        public bool HasProductImage => !string.IsNullOrEmpty(ProductImagePath);

        public string UnitDisplayText => SelectedSearchItem?.Mode == StockAdjustmentMode.ProductUnit 
            ? $"{SelectedSearchItem.UnitName} ({SelectedSearchItem.QtyInUnit})"
            : "N/A";

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged();
            }
        }

        public decimal CurrentStock
        {
            get => _currentStock;
            set
            {
                _currentStock = value;
                OnPropertyChanged();
                CalculateNewQuantity();
                UpdateDifferenceDisplay();
            }
        }

        public decimal NewQuantity
        {
            get => _newQuantity;
            set
            {
                _newQuantity = value;
                OnPropertyChanged();
                UpdateDifferenceDisplay();
            }
        }

        public decimal ChangeAmount
        {
            get => _changeAmount;
            set
            {
                AppLogger.Log("AdjustProductModel", $"ChangeAmount set to: {value}, IsIncrement: {IsIncrement}");
                _changeAmount = value;
                _changeAmountText = value.ToString();
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChangeAmountText));
                CalculateNewQuantity();
                UpdateDifferenceDisplay();
            }
        }

        public string ChangeAmountText
        {
            get => _changeAmountText;
            set
            {
                AppLogger.Log("AdjustProductModel", $"ChangeAmountText set to: '{value}'");
                _changeAmountText = value;
                
                // Try to parse the text to decimal
                if (string.IsNullOrEmpty(value))
                {
                    _changeAmount = 0;
                }
                else if (decimal.TryParse(value, out var parsedValue))
                {
                    _changeAmount = Math.Max(0, parsedValue); // Ensure non-negative
                }
                else
                {
                    // Invalid input, keep the previous value but update the text
                    _changeAmountText = _changeAmount.ToString();
                }
                
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChangeAmount));
                CalculateNewQuantity();
                UpdateDifferenceDisplay();
            }
        }

        public bool IsIncrement
        {
            get => _isIncrement;
            set
            {
                _isIncrement = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDecrement));
                OnPropertyChanged(nameof(OperationText));
                CalculateNewQuantity();
                UpdateDifferenceDisplay();
            }
        }

        public bool IsDecrement
        {
            get => !_isIncrement;
            set => IsIncrement = !value;
        }

        public string OperationText => IsIncrement ? "Increment (+)" : "Decrement (−)";

        public string DifferenceText
        {
            get
            {
                var difference = NewQuantity - CurrentStock;
                if (difference == 0)
                    return "No change";
                else if (difference > 0)
                    return $"Increase by {difference:F2} units";
                else
                    return $"Decrease by {Math.Abs(difference):F2} units";
            }
        }

        public Brush DifferenceColor
        {
            get
            {
                var difference = NewQuantity - CurrentStock;
                if (difference == 0)
                    return Brushes.Gray;
                else if (difference > 0)
                    return Brushes.Green;
                else
                    return Brushes.Red;
            }
        }

        public decimal DifferenceQuantity => NewQuantity - CurrentStock;

        public DateTime? ExpiryDate
        {
            get => _expiryDate;
            set
            {
                _expiryDate = value;
                OnPropertyChanged();
            }
        }

        public int? ReasonId
        {
            get => _reasonId;
            set
            {
                _reasonId = value;
                OnPropertyChanged();
            }
        }

        public string AdjustmentType
        {
            get => _adjustmentType;
            set
            {
                _adjustmentType = value;
                OnPropertyChanged();
            }
        }

        public string Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        public string Reason
        {
            get => _reason;
            set
            {
                _reason = value;
                OnPropertyChanged();
            }
        }

        public string ReasonText
        {
            get => _reasonText;
            set
            {
                _reasonText = value;
                OnPropertyChanged();
            }
        }

        public List<ProductBatchDto> AvailableBatches
        {
            get => _availableBatches;
            set
            {
                _availableBatches = value;
                OnPropertyChanged();
            }
        }

        public ProductBatchDto? SelectedBatch
        {
            get => _selectedBatch;
            set
            {
                _selectedBatch = value;
                OnPropertyChanged();
            }
        }

        // For validation
        public bool IsValid => HasValidSelection && ChangeAmount >= 0 && NewQuantity >= 0 && !string.IsNullOrWhiteSpace(ReasonText);
        
        // Check if we have a valid product/product unit selection
        public bool HasValidSelection => SelectedSearchItem != null && ProductId > 0;

        // Method to reset the form
        public void Reset()
        {
            SearchText = string.Empty;
            SelectedProduct = null;
            SelectedSearchItem = null;
            ProductName = string.Empty;
            CurrentStock = 0;
            NewQuantity = 0;
            ChangeAmount = 0;
            IsIncrement = true;
            ExpiryDate = null;
            ReasonId = null;
            ReasonText = "Stock count correction";
            AdjustmentType = "Increase";
            Quantity = "0";
            Reason = "Stock count correction";
            AdjustmentMode = StockAdjustmentMode.Product;
            AvailableBatches = new List<ProductBatchDto>();
            SelectedBatch = null;
            
            // Notify UI about image properties
            OnPropertyChanged(nameof(ProductImagePath));
            OnPropertyChanged(nameof(HasProductImage));
            OnPropertyChanged(nameof(UnitDisplayText));
        }

        private void CalculateNewQuantity()
        {
            if (IsIncrement)
            {
                _newQuantity = CurrentStock + ChangeAmount;
            }
            else
            {
                _newQuantity = CurrentStock - ChangeAmount;
            }
            AppLogger.Log("AdjustProductModel", $"CalculateNewQuantity - CurrentStock: {CurrentStock}, ChangeAmount: {ChangeAmount}, IsIncrement: {IsIncrement}, NewQuantity: {_newQuantity}");
            OnPropertyChanged(nameof(NewQuantity));
        }

        private void UpdateDifferenceDisplay()
        {
            var difference = NewQuantity - CurrentStock;
            AppLogger.Log("AdjustProductModel", $"UpdateDifferenceDisplay - NewQuantity: {NewQuantity}, CurrentStock: {CurrentStock}, Difference: {difference}");
            OnPropertyChanged(nameof(DifferenceText));
            OnPropertyChanged(nameof(DifferenceColor));
            OnPropertyChanged(nameof(DifferenceQuantity));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
