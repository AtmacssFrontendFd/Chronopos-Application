using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Controls;

namespace ChronoPos.Desktop.ViewModels;

public partial class ExchangeSalesViewModel : ObservableObject
{
    private readonly ITransactionService _transactionService;
    private readonly IProductService _productService;
    private readonly IExchangeService _exchangeService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActiveCurrencyService _activeCurrencyService;
    private readonly Action? _onExchangeComplete;
    private readonly Action? _onBack;

    [ObservableProperty]
    private int sourceTransactionId;

    [ObservableProperty]
    private string customerName = string.Empty;

    [ObservableProperty]
    private string customerPhone = string.Empty;

    [ObservableProperty]
    private string invoiceNumber = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ExchangeItemModel> returnItems = new();

    [ObservableProperty]
    private ObservableCollection<ProductDto> availableProducts = new();

    [ObservableProperty]
    private ObservableCollection<ExchangeItemModel> newItems = new();

    [ObservableProperty]
    private decimal totalReturnAmount = 0m;

    [ObservableProperty]
    private decimal totalNewAmount = 0m;

    [ObservableProperty]
    private decimal differenceToPay = 0m;

    [ObservableProperty]
    private string differenceText = string.Empty;

    [ObservableProperty]
    private ProductDto? selectedProduct;

    [ObservableProperty]
    private string searchText = string.Empty;

    // Formatted currency properties for display
    public string TotalReturnAmountFormatted => _activeCurrencyService.FormatPrice(TotalReturnAmount);
    public string TotalNewAmountFormatted => _activeCurrencyService.FormatPrice(TotalNewAmount);
    public IActiveCurrencyService ActiveCurrencyService => _activeCurrencyService;

    public ExchangeSalesViewModel(
        ITransactionService transactionService,
        IProductService productService,
        IExchangeService exchangeService,
        ICustomerService customerService,
        ICurrentUserService currentUserService,
        IActiveCurrencyService activeCurrencyService,
        Action? onExchangeComplete = null,
        Action? onBack = null)
    {
        _transactionService = transactionService;
        _productService = productService;
        _exchangeService = exchangeService;
        _customerService = customerService;
        _currentUserService = currentUserService;
        _activeCurrencyService = activeCurrencyService ?? throw new ArgumentNullException(nameof(activeCurrencyService));
        _onExchangeComplete = onExchangeComplete;
        _onBack = onBack;
    }

    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }

    public async Task LoadTransaction(int transactionId)
    {
        try
        {
            SourceTransactionId = transactionId;
            
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                new MessageDialog("Error", "Transaction not found.", MessageDialog.MessageType.Error).ShowDialog();
                return;
            }

            InvoiceNumber = transaction.InvoiceNumber ?? $"INV-{transactionId}";

            // Load customer
            if (transaction.CustomerId.HasValue)
            {
                var customer = await _customerService.GetByIdAsync(transaction.CustomerId.Value);
                if (customer != null)
                {
                    CustomerName = customer.DisplayName;
                    CustomerPhone = customer.PrimaryMobile ?? string.Empty;
                }
            }

            // Load return items from transaction
            ReturnItems.Clear();
            foreach (var transactionProduct in transaction.TransactionProducts)
            {
                // Get product details to check CanReturn property
                var product = await _productService.GetProductByIdAsync(transactionProduct.ProductId);
                
                // Build modifier string
                string modifierString = string.Empty;
                if (transactionProduct.Modifiers != null && transactionProduct.Modifiers.Any())
                {
                    modifierString = string.Join(", ", transactionProduct.Modifiers.Select(m => m.ModifierName));
                }

                var returnItem = new ExchangeItemModel
                {
                    TransactionProductId = transactionProduct.Id,
                    ProductId = transactionProduct.ProductId,
                    ProductName = transactionProduct.ProductName,
                    Modifiers = modifierString,
                    OriginalQuantity = (int)transactionProduct.Quantity,
                    ReturnQuantity = 0,
                    UnitPrice = transactionProduct.SellingPrice,
                    TotalPrice = 0,
                    IsSelected = false,
                    CanReturn = product?.CanReturn ?? true // Store CanReturn flag
                };
                
                // Subscribe to property changes for each return item
                returnItem.PropertyChanged += OnReturnItemPropertyChanged;
                ReturnItems.Add(returnItem);
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error loading transaction: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            AvailableProducts = new ObservableCollection<ProductDto>(products);
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error loading products: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private void AddProductToExchange()
    {
        if (SelectedProduct == null)
        {
            new MessageDialog("Validation", "Please select a product.", MessageDialog.MessageType.Warning).ShowDialog();
            return;
        }

        // Check if product already added
        var existingItem = NewItems.FirstOrDefault(x => x.ProductId == SelectedProduct.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
            existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
        }
        else
        {
            var newItem = new ExchangeItemModel
            {
                ProductId = SelectedProduct.Id,
                ProductName = SelectedProduct.Name,
                Quantity = 1,
                UnitPrice = SelectedProduct.Price,
                TotalPrice = SelectedProduct.Price,
                IsSelected = false
            };
            
            // Subscribe to property changes for the new item
            newItem.PropertyChanged += OnNewItemPropertyChanged;
            NewItems.Add(newItem);
        }

        RecalculateTotals();
    }

    [RelayCommand]
    private void RemoveNewItem(ExchangeItemModel item)
    {
        NewItems.Remove(item);
        RecalculateTotals();
    }

    partial void OnReturnItemsChanged(ObservableCollection<ExchangeItemModel> value)
    {
        if (value != null)
        {
            // Unsubscribe from old items
            if (ReturnItems != null)
            {
                foreach (var item in ReturnItems)
                {
                    item.PropertyChanged -= OnReturnItemPropertyChanged;
                }
            }

            // Subscribe to new items
            foreach (var item in value)
            {
                item.PropertyChanged += OnReturnItemPropertyChanged;
            }
        }
    }

    private void OnReturnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is ExchangeItemModel item)
        {
            // Check if quantity increased and product cannot be returned
            if (e.PropertyName == nameof(ExchangeItemModel.ReturnQuantity) && item.ReturnQuantity > 0 && !item.CanReturn)
            {
                // Show warning dialog
                var confirmDialog = new ConfirmationDialog(
                    "Product Return Not Allowed",
                    $"The product '{item.ProductName}' is marked as non-returnable.\n\n" +
                    $"Do you still want to proceed with the exchange for this item?",
                    ConfirmationDialog.DialogType.Warning);
                
                var result = confirmDialog.ShowDialog();
                
                if (result != true)
                {
                    // User declined, reset quantity to 0
                    item.ReturnQuantity = 0;
                    item.IsSelected = false;
                    return;
                }
            }
        }
        
        if (e.PropertyName == nameof(ExchangeItemModel.ReturnQuantity) || 
            e.PropertyName == nameof(ExchangeItemModel.IsSelected) ||
            e.PropertyName == nameof(ExchangeItemModel.TotalPrice))
        {
            RecalculateTotals();
        }
    }

    partial void OnNewItemsChanged(ObservableCollection<ExchangeItemModel> value)
    {
        if (value != null)
        {
            // Unsubscribe from old items
            if (NewItems != null)
            {
                foreach (var item in NewItems)
                {
                    item.PropertyChanged -= OnNewItemPropertyChanged;
                }
            }

            // Subscribe to new items
            foreach (var item in value)
            {
                item.PropertyChanged += OnNewItemPropertyChanged;
            }
        }
    }

    private void OnNewItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ExchangeItemModel.Quantity) ||
            e.PropertyName == nameof(ExchangeItemModel.TotalPrice))
        {
            RecalculateTotals();
        }
    }

    private void RecalculateTotals()
    {
        TotalReturnAmount = ReturnItems
            .Where(x => x.IsSelected && x.ReturnQuantity > 0)
            .Sum(x => x.ReturnQuantity * x.UnitPrice);

        TotalNewAmount = NewItems.Sum(x => x.Quantity * x.UnitPrice);

        DifferenceToPay = TotalNewAmount - TotalReturnAmount;
        
        // Notify UI of formatted property changes
        OnPropertyChanged(nameof(TotalReturnAmountFormatted));
        OnPropertyChanged(nameof(TotalNewAmountFormatted));

        if (DifferenceToPay > 0)
        {
            DifferenceText = $"Customer Pays: {_activeCurrencyService.FormatPrice(DifferenceToPay)}";
        }
        else if (DifferenceToPay < 0)
        {
            DifferenceText = $"Refund to Customer: {_activeCurrencyService.FormatPrice(Math.Abs(DifferenceToPay))}";
        }
        else
        {
            DifferenceText = "Even Exchange";
        }
    }

    [RelayCommand]
    private async Task SaveExchange()
    {
        try
        {
            // Validate return items
            var returnItemsToExchange = ReturnItems.Where(x => x.IsSelected && x.ReturnQuantity > 0).ToList();
            if (!returnItemsToExchange.Any())
            {
                new MessageDialog("Validation", "Please select items to return.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            // Validate new items
            if (!NewItems.Any())
            {
                new MessageDialog("Validation", "Please add new items to exchange.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            // Validate return quantities
            foreach (var item in returnItemsToExchange)
            {
                if (item.ReturnQuantity > item.OriginalQuantity)
                {
                    new MessageDialog("Validation", $"Return quantity for '{item.ProductName}' cannot exceed original quantity ({item.OriginalQuantity}).", MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }
            }

            // Confirm exchange
            var confirmResult = new ConfirmationDialog(
                "Confirm Exchange",
                $"Confirm Exchange?\n\n" +
                $"Returning: {_activeCurrencyService.FormatPrice(TotalReturnAmount)}\n" +
                $"New Items: {_activeCurrencyService.FormatPrice(TotalNewAmount)}\n" +
                $"{DifferenceText}\n\n" +
                $"Do you want to proceed?",
                ConfirmationDialog.DialogType.Warning).ShowDialog();

            if (confirmResult != true)
                return;

            // Create exchange DTO
            var exchangeDto = new CreateExchangeTransactionDto
            {
                SellingTransactionId = SourceTransactionId,
                TotalExchangedAmount = Math.Abs(DifferenceToPay),
                TotalExchangedVat = 0m, // Calculate VAT if needed
                ProductExchangedQuantity = returnItemsToExchange.Sum(x => x.ReturnQuantity),
                ExchangeTime = DateTime.Now,
                Products = new List<CreateExchangeTransactionProductDto>()
            };

            // Map exchange products - one entry per return item with its corresponding new item
            foreach (var returnItem in returnItemsToExchange)
            {
                // For each returned item, create exchange entries with new products proportionally
                foreach (var newItem in NewItems)
                {
                    exchangeDto.Products.Add(new CreateExchangeTransactionProductDto
                    {
                        OriginalTransactionProductId = returnItem.TransactionProductId,
                        NewProductId = newItem.ProductId,
                        ReturnedQuantity = returnItem.ReturnQuantity,
                        NewQuantity = newItem.Quantity,
                        OldProductAmount = returnItem.ReturnQuantity * returnItem.UnitPrice,
                        NewProductAmount = newItem.Quantity * newItem.UnitPrice,
                        PriceDifference = (newItem.Quantity * newItem.UnitPrice) - (returnItem.ReturnQuantity * returnItem.UnitPrice),
                        VatDifference = 0m
                    });
                }
            }

            // Save exchange
            var savedExchange = await _exchangeService.CreateAsync(exchangeDto);

            new MessageDialog(
                "Success",
                $"Exchange completed successfully!\n\n" +
                $"Exchange ID: {savedExchange.Id}\n" +
                $"{DifferenceText}",
                MessageDialog.MessageType.Success).ShowDialog();

            // Notify completion
            _onExchangeComplete?.Invoke();
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error saving exchange: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private async Task SaveAndPrintExchange()
    {
        try
        {
            // Validate return items
            var returnItemsToExchange = ReturnItems.Where(x => x.IsSelected && x.ReturnQuantity > 0).ToList();
            if (!returnItemsToExchange.Any())
            {
                new MessageDialog("Validation", "Please select items to return.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            // Validate new items
            if (!NewItems.Any())
            {
                new MessageDialog("Validation", "Please add new items to exchange.", MessageDialog.MessageType.Warning).ShowDialog();
                return;
            }

            // Validate return quantities
            foreach (var item in returnItemsToExchange)
            {
                if (item.ReturnQuantity > item.OriginalQuantity)
                {
                    new MessageDialog("Validation", $"Return quantity for '{item.ProductName}' cannot exceed original quantity ({item.OriginalQuantity}).", MessageDialog.MessageType.Warning).ShowDialog();
                    return;
                }
            }

            // Confirm exchange
            var confirmResult = new ConfirmationDialog(
                "Confirm Exchange",
                $"Confirm Exchange?\n\n" +
                $"Returning: {_activeCurrencyService.FormatPrice(TotalReturnAmount)}\n" +
                $"New Items: {_activeCurrencyService.FormatPrice(TotalNewAmount)}\n" +
                $"{DifferenceText}\n\n" +
                $"Do you want to proceed and print?",
                ConfirmationDialog.DialogType.Warning).ShowDialog();

            if (confirmResult != true)
                return;

            // Create exchange DTO
            var exchangeDto = new CreateExchangeTransactionDto
            {
                SellingTransactionId = SourceTransactionId,
                TotalExchangedAmount = Math.Abs(DifferenceToPay),
                TotalExchangedVat = 0m,
                ProductExchangedQuantity = returnItemsToExchange.Sum(x => x.ReturnQuantity),
                ExchangeTime = DateTime.Now,
                Products = new List<CreateExchangeTransactionProductDto>()
            };

            // Map exchange products
            foreach (var returnItem in returnItemsToExchange)
            {
                foreach (var newItem in NewItems)
                {
                    exchangeDto.Products.Add(new CreateExchangeTransactionProductDto
                    {
                        OriginalTransactionProductId = returnItem.TransactionProductId,
                        NewProductId = newItem.ProductId,
                        ReturnedQuantity = returnItem.ReturnQuantity,
                        NewQuantity = newItem.Quantity,
                        OldProductAmount = returnItem.ReturnQuantity * returnItem.UnitPrice,
                        NewProductAmount = newItem.Quantity * newItem.UnitPrice,
                        PriceDifference = (newItem.Quantity * newItem.UnitPrice) - (returnItem.ReturnQuantity * returnItem.UnitPrice),
                        VatDifference = 0m
                    });
                }
            }

            // Save exchange
            var savedExchange = await _exchangeService.CreateAsync(exchangeDto);

            // Print receipt
            PrintExchangeReceipt(savedExchange, returnItemsToExchange, NewItems.ToList());

            new MessageDialog(
                "Success",
                $"Exchange completed and printed successfully!\n\n" +
                $"Exchange ID: {savedExchange.Id}\n" +
                $"{DifferenceText}",
                MessageDialog.MessageType.Success).ShowDialog();

            // Notify completion
            _onExchangeComplete?.Invoke();
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error saving exchange: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private void PrintExchangeReceipt(ExchangeTransactionDto exchange, List<ExchangeItemModel> returnItems, List<ExchangeItemModel> newItems)
    {
        try
        {
            // Use QuestPDF for professional exchange receipt generation and printing
            var exchangePrinter = new QuestPdfExchangePrinter(_activeCurrencyService);

            // Get company information from settings or use defaults
            string companyName = "CHRONO POS"; // TODO: Get from settings/database
            string? companyAddress = null; // TODO: Get from settings/database
            string? companyPhone = null; // TODO: Get from settings/database
            string? gstNo = null; // TODO: Get from settings/database

            // Generate PDF and auto-print to thermal printer
            string pdfPath = exchangePrinter.GenerateAndPrintExchange(
                exchange: exchange,
                returnItems: returnItems,
                newItems: newItems,
                invoiceNumber: InvoiceNumber,
                customerName: CustomerName,
                totalReturnAmount: TotalReturnAmount,
                totalNewAmount: TotalNewAmount,
                differenceToPay: DifferenceToPay,
                companyName: companyName,
                companyAddress: companyAddress,
                companyPhone: companyPhone,
                gstNo: gstNo
            );

            // Success - no error dialog needed, printing happens silently
        }
        catch (Exception ex)
        {
            new MessageDialog("Print Error", $"Error printing receipt: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private void Back()
    {
        _onBack?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        _onExchangeComplete?.Invoke();
    }
}

public partial class ExchangeItemModel : ObservableObject
{
    [ObservableProperty]
    private int transactionProductId;

    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private string modifiers = string.Empty;

    public bool HasModifiers => !string.IsNullOrEmpty(Modifiers);

    [ObservableProperty]
    private int originalQuantity;

    [ObservableProperty]
    private int returnQuantity;

    [ObservableProperty]
    private int quantity;

    [ObservableProperty]
    private decimal unitPrice;

    [ObservableProperty]
    private decimal totalPrice;

    [ObservableProperty]
    private bool isSelected;
    
    [ObservableProperty]
    private bool canReturn = true; // Product's CanReturn setting

    partial void OnReturnQuantityChanged(int value)
    {
        TotalPrice = value * UnitPrice;
    }

    partial void OnQuantityChanged(int value)
    {
        TotalPrice = value * UnitPrice;
    }
}
