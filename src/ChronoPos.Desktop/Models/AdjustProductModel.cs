using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChronoPos.Desktop.Models
{
    public class AdjustProductModel : INotifyPropertyChanged
    {
        private string _productName = string.Empty;
        private string _currentStock = "0";
        private string _adjustmentType = "Increase";
        private string _quantity = "0";
        private string _reason = "Stock count correction";

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged();
            }
        }

        public string CurrentStock
        {
            get => _currentStock;
            set
            {
                _currentStock = value;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
