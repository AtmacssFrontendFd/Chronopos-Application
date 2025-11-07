using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;

namespace ChronoPos.Desktop.Views.Dialogs
{
    public partial class RefundDialog : Window
    {
        private readonly TransactionDto _transaction;
        private readonly decimal _taxPercentage;
        private readonly IRefundService _refundService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IActiveCurrencyService _activeCurrencyService;
        private readonly ObservableCollection<RefundItemModel> _refundItems;

        public bool IsConfirmed { get; private set; }
        public decimal RefundAmount { get; private set; }
        public IActiveCurrencyService ActiveCurrencyService => _activeCurrencyService;

        public RefundDialog(
            TransactionDto transaction,
            IRefundService refundService,
            ICurrentUserService currentUserService,
            IActiveCurrencyService activeCurrencyService)
        {
            InitializeComponent();

            _transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _activeCurrencyService = activeCurrencyService ?? throw new ArgumentNullException(nameof(activeCurrencyService));
            _taxPercentage = transaction.Vat;

            // Set DataContext for bindings
            DataContext = this;

            // Initialize refund items collection
            _refundItems = new ObservableCollection<RefundItemModel>();

            // Load transaction data
            LoadTransactionData();
        }

        private void LoadTransactionData()
        {
            // Set transaction header info
            TransactionInfoText.Text = $"Transaction #{_transaction.Id:D4} | Customer: {_transaction.CustomerName ?? "Walk-in"}";
            TransactionDateText.Text = $"{_transaction.SellingTime:dd MMM yyyy, h:mm tt}";
            TransactionTotalText.Text = _activeCurrencyService.FormatPrice(_transaction.TotalAmount);
            TransactionStatusText.Text = _transaction.Status?.ToUpper() ?? "SETTLED";

            // Load transaction products into refund items
            foreach (var product in _transaction.TransactionProducts)
            {
                // Build product name with modifiers
                string productNameWithModifiers = product.ProductName ?? "Unknown Product";
                
                if (product.Modifiers != null && product.Modifiers.Any())
                {
                    var modifierNames = product.Modifiers
                        .Select(m => m.ModifierName)
                        .Where(name => !string.IsNullOrEmpty(name));
                    
                    if (modifierNames.Any())
                    {
                        productNameWithModifiers += " +" + string.Join(", ", modifierNames);
                    }
                }

                var refundItem = new RefundItemModel
                {
                    TransactionProductId = product.Id,
                    ProductId = product.ProductId,
                    ProductName = productNameWithModifiers,
                    UnitPrice = product.SellingPrice,
                    MaxRefundQuantity = (int)product.Quantity,
                    RefundQuantity = 0,
                    IsSelected = false
                };

                refundItem.PropertyChanged += RefundItem_PropertyChanged;
                _refundItems.Add(refundItem);
            }

            // Bind items to UI
            RefundItemsControl.ItemsSource = _refundItems;

            // Update summary
            UpdateRefundSummary();
        }

        private void RefundItem_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RefundItemModel.IsSelected) || 
                e.PropertyName == nameof(RefundItemModel.RefundQuantity))
            {
                UpdateRefundSummary();
            }
        }

        private void UpdateRefundSummary()
        {
            var selectedItems = _refundItems.Where(i => i.IsSelected && i.RefundQuantity > 0).ToList();

            decimal subtotal = 0;
            foreach (var item in selectedItems)
            {
                subtotal += item.UnitPrice * item.RefundQuantity;
            }

            decimal vat = subtotal * (_taxPercentage / 100);
            decimal total = subtotal + vat;

            RefundSubtotalText.Text = _activeCurrencyService.FormatPrice(subtotal);
            RefundVatText.Text = _activeCurrencyService.FormatPrice(vat);
            RefundTotalText.Text = _activeCurrencyService.FormatPrice(total);

            RefundAmount = total;

            // Enable/disable confirm button
            ConfirmRefundButton.IsEnabled = selectedItems.Any();
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is RefundItemModel item)
            {
                if (item.RefundQuantity < item.MaxRefundQuantity)
                {
                    item.RefundQuantity++;
                    if (item.RefundQuantity > 0 && !item.IsSelected)
                    {
                        item.IsSelected = true;
                    }
                }
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is RefundItemModel item)
            {
                if (item.RefundQuantity > 0)
                {
                    item.RefundQuantity--;
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }

        private async void ConfirmRefundButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate refund items
                var refundItems = _refundItems.Where(item => item.IsSelected && item.RefundQuantity > 0).ToList();
                
                if (!refundItems.Any())
                {
                    new MessageDialog("Validation", "Please select items and specify quantities to refund.", MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }

                // Calculate refund amount
                decimal refundAmount = refundItems.Sum(item => item.UnitPrice * item.RefundQuantity);
                decimal refundVat = refundAmount * (_taxPercentage / 100);
                decimal totalRefund = refundAmount + refundVat;

                // Confirm refund
                var confirmDialog = new ConfirmationDialog(
                    "Confirm Refund",
                    $"Are you sure you want to process this refund?\n\n" +
                    $"Items to refund: {refundItems.Count}\n" +
                    $"Refund amount: {_activeCurrencyService.FormatPrice(totalRefund)}\n\n" +
                    $"This action cannot be undone.",
                    ConfirmationDialog.DialogType.Warning);

                var confirmed = confirmDialog.ShowDialog();

                if (confirmed != true)
                    return;

                var currentUserId = _currentUserService.CurrentUserId;
                if (!currentUserId.HasValue || currentUserId.Value <= 0)
                {
                    new MessageDialog("Error", "Unable to identify current user. Please log in again.", MessageDialog.MessageType.Error).ShowDialog();
                    return;
                }

                var shiftId = 1; // TODO: Get current shift ID

                // Create refund DTO
                var refundDto = new CreateRefundTransactionDto
                {
                    SellingTransactionId = _transaction.Id,
                    CustomerId = _transaction.CustomerId,
                    ShiftId = shiftId,
                    UserId = currentUserId.Value,
                    TotalAmount = totalRefund,
                    TotalVat = refundVat,
                    IsCash = true,
                    RefundTime = DateTime.Now,
                    Products = refundItems.Select(item => new CreateRefundTransactionProductDto
                    {
                        TransactionProductId = item.TransactionProductId,
                        TotalQuantityReturned = item.RefundQuantity,
                        TotalVat = (item.UnitPrice * item.RefundQuantity) * (_taxPercentage / 100),
                        TotalAmount = item.UnitPrice * item.RefundQuantity
                    }).ToList()
                };

                // Save refund using RefundService
                var savedRefund = await _refundService.CreateAsync(refundDto);

                IsConfirmed = true;
                RefundAmount = totalRefund;

                new MessageDialog(
                    "Success",
                    $"Refund processed successfully!\n\n" +
                    $"Refund ID: #{savedRefund.Id}\n" +
                    $"Amount: {_activeCurrencyService.FormatPrice(totalRefund)}",
                    MessageDialog.MessageType.Success).ShowDialog();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                new MessageDialog("Error", $"Error processing refund: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
            }
        }
    }

    /// <summary>
    /// Model for refund item in the dialog
    /// </summary>
    public class RefundItemModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private bool _isSelected;
        private int _refundQuantity;

        public int TransactionProductId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int MaxRefundQuantity { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public int RefundQuantity
        {
            get => _refundQuantity;
            set
            {
                if (value < 0) value = 0;
                if (value > MaxRefundQuantity) value = MaxRefundQuantity;
                SetProperty(ref _refundQuantity, value);
            }
        }
    }
}
