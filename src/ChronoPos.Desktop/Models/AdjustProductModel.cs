using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using ChronoPos.Application.DTOs;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.Models
{
    public class AdjustProductModel : INotifyPropertyChanged
    {
        private string _searchText = string.Empty;
        private ProductDto? _selectedProduct;
        private string _productName = string.Empty;
        private decimal _currentStock = 0;
        private decimal _newQuantity = 0;
        private string _adjustmentType = "Increase";
        private string _quantity = "0";
        private string _reason = "Stock count correction";
        private string _reasonText = "Stock count correction";
        private DateTime? _expiryDate;
        private int? _reasonId;

        public string SearchText
        {
            get => _searchText;
            set
            {
                FileLogger.Log($"[AdjustProductModel] SearchText changing: '{_searchText}' → '{value}'");
                _searchText = value;
                OnPropertyChanged();
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

        public int ProductId => SelectedProduct?.Id ?? 0;

        public string? ProductImagePath => SelectedProduct?.ImagePath;

        public bool HasProductImage => !string.IsNullOrEmpty(ProductImagePath);

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

        // For validation
        public bool IsValid => ProductId > 0 && NewQuantity >= 0 && !string.IsNullOrWhiteSpace(ReasonText);

        // Method to reset the form
        public void Reset()
        {
            SearchText = string.Empty;
            SelectedProduct = null;
            ProductName = string.Empty;
            CurrentStock = 0;
            NewQuantity = 0;
            ExpiryDate = null;
            ReasonId = null;
            ReasonText = "Stock count correction";
            AdjustmentType = "Increase";
            Quantity = "0";
            Reason = "Stock count correction";
            
            // Notify UI about image properties
            OnPropertyChanged(nameof(ProductImagePath));
            OnPropertyChanged(nameof(HasProductImage));
        }

        private void UpdateDifferenceDisplay()
        {
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
