using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
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

    public ExchangeSalesViewModel(
        ITransactionService transactionService,
        IProductService productService,
        IExchangeService exchangeService,
        ICustomerService customerService,
        ICurrentUserService currentUserService,
        Action? onExchangeComplete = null,
        Action? onBack = null)
    {
        _transactionService = transactionService;
        _productService = productService;
        _exchangeService = exchangeService;
        _customerService = customerService;
        _currentUserService = currentUserService;
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
            foreach (var product in transaction.TransactionProducts)
            {
                var returnItem = new ExchangeItemModel
                {
                    TransactionProductId = product.Id,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    OriginalQuantity = (int)product.Quantity,
                    ReturnQuantity = 0,
                    UnitPrice = product.SellingPrice,
                    TotalPrice = 0,
                    IsSelected = false
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

        if (DifferenceToPay > 0)
        {
            DifferenceText = $"Customer Pays: ${DifferenceToPay:N2}";
        }
        else if (DifferenceToPay < 0)
        {
            DifferenceText = $"Refund to Customer: ${Math.Abs(DifferenceToPay):N2}";
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
                $"Returning: ${TotalReturnAmount:N2}\n" +
                $"New Items: ${TotalNewAmount:N2}\n" +
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
                $"Returning: ${TotalReturnAmount:N2}\n" +
                $"New Items: ${TotalNewAmount:N2}\n" +
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
            var exchangeNumber = $"E{exchange.Id:D4}";
            
            var document = new System.Windows.Documents.FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Courier New"),
                FontSize = 11
            };

            // Header
            var headerPara = new System.Windows.Documents.Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            headerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new System.Windows.Documents.Run("EXCHANGE RECEIPT\n") { FontSize = 16, FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(headerPara);

            // Exchange info
            var infoPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 10, 0, 10) };
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Exchange #: {exchangeNumber}\n"));
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Original Transaction: {InvoiceNumber}\n"));
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Date: {exchange.ExchangeTime:dd/MM/yyyy}\n"));
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Time: {exchange.ExchangeTime:HH:mm:ss}\n"));
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Customer: {CustomerName}\n"));
            document.Blocks.Add(infoPara);

            // Separator
            var separatorPara1 = new System.Windows.Documents.Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara1.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara1);

            // RETURNED ITEMS
            var returnHeaderPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            returnHeaderPara.Inlines.Add(new System.Windows.Documents.Run("RETURNED ITEMS\n") { FontWeight = FontWeights.Bold });
            returnHeaderPara.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────\n"));
            document.Blocks.Add(returnHeaderPara);

            foreach (var item in returnItems)
            {
                var itemPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                itemPara.Inlines.Add(new System.Windows.Documents.Run($"{item.ProductName}\n"));
                
                var qtyPriceText = $"  {item.ReturnQuantity} x {item.UnitPrice:C2}";
                var totalText = $"{(item.ReturnQuantity * item.UnitPrice):C2}";
                var spacing = new string(' ', Math.Max(0, 35 - qtyPriceText.Length - totalText.Length));
                itemPara.Inlines.Add(new System.Windows.Documents.Run($"{qtyPriceText}{spacing}{totalText}\n"));
                
                document.Blocks.Add(itemPara);
            }

            var returnTotalPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            var returnTotalLine = "Total Returned:";
            var returnTotalAmount = $"{TotalReturnAmount:C2}";
            var returnTotalSpacing = new string(' ', Math.Max(0, 35 - returnTotalLine.Length - returnTotalAmount.Length));
            returnTotalPara.Inlines.Add(new System.Windows.Documents.Run($"{returnTotalLine}{returnTotalSpacing}{returnTotalAmount}\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(returnTotalPara);

            // Separator
            var separatorPara2 = new System.Windows.Documents.Paragraph
            {
                Margin = new Thickness(0, 10, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara2.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara2);

            // NEW ITEMS
            var newHeaderPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            newHeaderPara.Inlines.Add(new System.Windows.Documents.Run("NEW ITEMS\n") { FontWeight = FontWeights.Bold });
            newHeaderPara.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────\n"));
            document.Blocks.Add(newHeaderPara);

            foreach (var item in newItems)
            {
                var itemPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                itemPara.Inlines.Add(new System.Windows.Documents.Run($"{item.ProductName}\n"));
                
                var qtyPriceText = $"  {item.Quantity} x {item.UnitPrice:C2}";
                var totalText = $"{(item.Quantity * item.UnitPrice):C2}";
                var spacing = new string(' ', Math.Max(0, 35 - qtyPriceText.Length - totalText.Length));
                itemPara.Inlines.Add(new System.Windows.Documents.Run($"{qtyPriceText}{spacing}{totalText}\n"));
                
                document.Blocks.Add(itemPara);
            }

            var newTotalPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            var newTotalLine = "Total New Items:";
            var newTotalAmount = $"{TotalNewAmount:C2}";
            var newTotalSpacing = new string(' ', Math.Max(0, 35 - newTotalLine.Length - newTotalAmount.Length));
            newTotalPara.Inlines.Add(new System.Windows.Documents.Run($"{newTotalLine}{newTotalSpacing}{newTotalAmount}\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(newTotalPara);

            // Final separator
            var separatorPara3 = new System.Windows.Documents.Paragraph
            {
                Margin = new Thickness(0, 10, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara3.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara3);

            // Difference
            var differencePara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 10), TextAlignment = TextAlignment.Center };
            differencePara.Inlines.Add(new System.Windows.Documents.Run($"{DifferenceText}\n") { FontWeight = FontWeights.Bold, FontSize = 13 });
            document.Blocks.Add(differencePara);

            // Footer
            var footerPara = new System.Windows.Documents.Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0),
                FontSize = 10
            };
            footerPara.Inlines.Add(new System.Windows.Documents.Run("Thank you for your business!\n"));
            footerPara.Inlines.Add(new System.Windows.Documents.Run($"Printed: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"));
            document.Blocks.Add(footerPara);

            // Print
            var printDialog = new System.Windows.Controls.PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var paginator = ((System.Windows.Documents.IDocumentPaginatorSource)document).DocumentPaginator;
                printDialog.PrintDocument(paginator, $"Exchange Receipt - {exchangeNumber}");
            }
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

    partial void OnReturnQuantityChanged(int value)
    {
        TotalPrice = value * UnitPrice;
    }

    partial void OnQuantityChanged(int value)
    {
        TotalPrice = value * UnitPrice;
    }
}
