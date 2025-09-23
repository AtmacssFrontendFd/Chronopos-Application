using ChronoPos.Application.DTOs;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChronoPos.Desktop.ViewModels
{
    public class ProductUnitViewModel : INotifyPropertyChanged
    {
        private int _id;
        private int _productId;
        private int _unitId;
        private UnitOfMeasurementDto? _selectedUnitOfMeasurement;
        private decimal _qtyInUnit;
        private decimal _costOfUnit;
        private decimal _priceOfUnit;
        private bool _discountAllowed;
        private bool _isBase;
        private bool _isNew;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int ProductId
        {
            get => _productId;
            set => SetProperty(ref _productId, value);
        }

        public int UnitId
        {
            get => _unitId;
            set => SetProperty(ref _unitId, value);
        }

        public UnitOfMeasurementDto? SelectedUnitOfMeasurement
        {
            get => _selectedUnitOfMeasurement;
            set
            {
                if (SetProperty(ref _selectedUnitOfMeasurement, value))
                {
                    UnitId = (int)(value?.Id ?? 0);
                }
            }
        }

        public decimal QtyInUnit
        {
            get => _qtyInUnit;
            set => SetProperty(ref _qtyInUnit, value);
        }

        public decimal CostOfUnit
        {
            get => _costOfUnit;
            set => SetProperty(ref _costOfUnit, value);
        }

        public decimal PriceOfUnit
        {
            get => _priceOfUnit;
            set => SetProperty(ref _priceOfUnit, value);
        }

        public bool DiscountAllowed
        {
            get => _discountAllowed;
            set => SetProperty(ref _discountAllowed, value);
        }

        public bool IsBase
        {
            get => _isBase;
            set => SetProperty(ref _isBase, value);
        }

        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}