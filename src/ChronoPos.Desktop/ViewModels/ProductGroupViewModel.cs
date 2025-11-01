using CommunityToolkit.Mvvm.ComponentModel;
using ChronoPos.Application.DTOs;

namespace ChronoPos.Desktop.ViewModels
{
    /// <summary>
    /// ViewModel for displaying a product group in the sales window with quantity tracking
    /// </summary>
    public partial class ProductGroupViewModel : ObservableObject
    {
        private readonly ProductGroupDto _group;
        
        [ObservableProperty]
        private int _quantity;

        [ObservableProperty]
        private decimal _totalPrice;

        public int Id => _group.Id;
        public string Name => _group.Name;
        public string? Description => _group.Description;
        public string Status => _group.Status;

        public ProductGroupDto Group => _group;

        public ProductGroupViewModel(ProductGroupDto group, decimal totalPrice)
        {
            _group = group;
            _totalPrice = totalPrice;
            _quantity = 0;
        }

        public void UpdateTotalPrice(decimal newTotalPrice)
        {
            TotalPrice = newTotalPrice;
        }
    }
}
