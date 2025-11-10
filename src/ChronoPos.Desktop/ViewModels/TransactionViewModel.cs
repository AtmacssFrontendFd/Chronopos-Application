using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Application.Logging;
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Converters;
using InfrastructureServices = ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

public partial class TransactionViewModel : ObservableObject
{
    private readonly ITransactionService _transactionService;
    private readonly IRefundService _refundService;
    private readonly IExchangeService _exchangeService;
    private readonly IPaymentTypeService _paymentTypeService;
    private readonly IReservationService _reservationService;
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActiveCurrencyService _activeCurrencyService;
    private readonly IProductService _productService;
    private readonly InfrastructureServices.IDatabaseLocalizationService _localizationService;
    private readonly ILayoutDirectionService _layoutDirectionService;
    private readonly Action<int>? _navigateToEditTransaction;
    private readonly Action<int>? _navigateToPayBill;
    private readonly Action<int>? _navigateToRefundTransaction;
    private readonly Action<int>? _navigateToExchangeTransaction;
    private readonly Action? _navigateToAddSales;
    private DispatcherTimer? _timerUpdateTimer;
    private bool _isPaymentPopupOpen = false; // Guard flag to prevent duplicate popups

    [ObservableProperty]
    private string currentTab = "Sales";

    [ObservableProperty]
    private string currentTabTitle = "Transaction";

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private object? currentTabContent;

    [ObservableProperty]
    private ObservableCollection<TransactionCardModel> salesTransactions = new();

    [ObservableProperty]
    private ObservableCollection<RefundCardModel> refundTransactions = new();

    [ObservableProperty]
    private ObservableCollection<ExchangeCardModel> exchangeTransactions = new();

    // Filtered collections for search
    [ObservableProperty]
    private ObservableCollection<TransactionCardModel> filteredSalesTransactions = new();

    [ObservableProperty]
    private ObservableCollection<RefundCardModel> filteredRefundTransactions = new();

    [ObservableProperty]
    private ObservableCollection<ExchangeCardModel> filteredExchangeTransactions = new();

    // FlowDirection for RTL/LTR support
    [ObservableProperty]
    private FlowDirection currentFlowDirection = FlowDirection.LeftToRight;

    // Translation Properties
    [ObservableProperty]
    private string transactionTitleLabel = "Transactions";

    [ObservableProperty]
    private string salesTabLabel = "Sales";

    [ObservableProperty]
    private string refundTabLabel = "Refund";

    [ObservableProperty]
    private string exchangeTabLabel = "Exchange";

    [ObservableProperty]
    private string searchPlaceholderLabel = "Search transactions...";

    [ObservableProperty]
    private string createNewTransactionLabel = "Create New Transaction";

    [ObservableProperty]
    private string invoiceLabel = "Invoice";

    [ObservableProperty]
    private string customerLabel = "Customer";

    [ObservableProperty]
    private string tableLabel = "Table";

    [ObservableProperty]
    private string itemsLabel = "Items";

    [ObservableProperty]
    private string totalLabel = "Total";

    [ObservableProperty]
    private string paidLabel = "Paid";

    [ObservableProperty]
    private string remainingLabel = "Remaining";

    [ObservableProperty]
    private string viewDetailsLabel = "View Details";

    [ObservableProperty]
    private string editTransactionLabel = "Edit";

    [ObservableProperty]
    private string payBillLabel = "Pay Bill";

    [ObservableProperty]
    private string printInvoiceLabel = "Print Invoice";

    [ObservableProperty]
    private string processRefundLabel = "Process Refund";

    [ObservableProperty]
    private string processExchangeLabel = "Process Exchange";

    [ObservableProperty]
    private string noSalesTransactionsLabel = "No sales transactions found";

    [ObservableProperty]
    private string noRefundTransactionsLabel = "No refund transactions found";

    [ObservableProperty]
    private string noExchangeTransactionsLabel = "No exchange transactions found";

    [ObservableProperty]
    private string startCreatingSalesLabel = "Click '+' to create a new sale";

    // Settle Popup Translation Properties
    [ObservableProperty]
    private string paymentPopupTitleLabel = "Payment";

    [ObservableProperty]
    private string paymentMethodLabel = "Payment Method:";

    [ObservableProperty]
    private string amountPaidLabel = "Amount Paid:";

    [ObservableProperty]
    private string creditDaysLabel = "Credit Days (for partial payment):";

    [ObservableProperty]
    private string cancelButtonLabel = "Cancel";

    [ObservableProperty]
    private string saveSettleButtonLabel = "Save & Settle";

    [ObservableProperty]
    private string customerPendingAmountLabel = "Customer Pending Amount:";

    [ObservableProperty]
    private string remainingAmountTransactionLabel = "Remaining Amount of Transaction:";

    [ObservableProperty]
    private string alreadyPaidLabelText = "Already Paid:";

    [ObservableProperty]
    private string saleAmountLabel = "Sale Amount:";

    [ObservableProperty]
    private string billTotalLabel = "Bill Total:";

    [ObservableProperty]
    private string customerPendingAddedLabel = "Customer Pending:";

    [ObservableProperty]
    private string storeCreditAvailableLabel = "Store Credit Available:";

    [ObservableProperty]
    private string addedToBillLabel = "(Added to bill)";

    [ObservableProperty]
    private string deductedFromBillLabel = "(Deducted from bill)";

    // Computed properties for empty states
    public bool HasSalesTransactions => FilteredSalesTransactions?.Count > 0;
    public bool HasRefundTransactions => FilteredRefundTransactions?.Count > 0;
    public bool HasExchangeTransactions => FilteredExchangeTransactions?.Count > 0;

    public TransactionViewModel(
        ITransactionService transactionService,
        IRefundService refundService,
        IExchangeService exchangeService,
        IPaymentTypeService paymentTypeService,
        IReservationService reservationService,
        ICustomerService customerService,
        ICurrentUserService currentUserService,
        IActiveCurrencyService activeCurrencyService,
        IProductService productService,
        InfrastructureServices.IDatabaseLocalizationService localizationService,
        ILayoutDirectionService layoutDirectionService,
        Action<int>? navigateToEditTransaction = null,
        Action<int>? navigateToPayBill = null,
        Action<int>? navigateToRefundTransaction = null,
        Action<int>? navigateToExchangeTransaction = null,
        Action? navigateToAddSales = null)
    {
        _transactionService = transactionService;
        _refundService = refundService;
        _exchangeService = exchangeService;
        _paymentTypeService = paymentTypeService;
        _reservationService = reservationService;
        _customerService = customerService;
        _currentUserService = currentUserService;
        _activeCurrencyService = activeCurrencyService ?? throw new ArgumentNullException(nameof(activeCurrencyService));
        _productService = productService;
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _layoutDirectionService = layoutDirectionService ?? throw new ArgumentNullException(nameof(layoutDirectionService));
        _navigateToEditTransaction = navigateToEditTransaction;
        _navigateToPayBill = navigateToPayBill;
        _navigateToRefundTransaction = navigateToRefundTransaction;
        _navigateToExchangeTransaction = navigateToExchangeTransaction;
        _navigateToAddSales = navigateToAddSales;

        // Subscribe to service events
        _localizationService.LanguageChanged += OnLanguageChanged;
        _layoutDirectionService.DirectionChanged += OnLayoutDirectionChanged;

        // Initialize layout direction
        CurrentFlowDirection = _layoutDirectionService.CurrentDirection == LayoutDirection.RightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;

        // Initialize timer for updating transaction durations
        _timerUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1) // Update every minute
        };
        _timerUpdateTimer.Tick += OnTimerUpdate;
        _timerUpdateTimer.Start();

        _ = InitializeAsync();
    }

    private void OnTimerUpdate(object? sender, EventArgs e)
    {
        // Update timer display for all active transactions
        foreach (var transaction in SalesTransactions)
        {
            if (transaction.ShowTimer)
            {
                transaction.TimerDisplay = CalculateElapsedTime(transaction.CreatedAt);
            }
        }
    }

    private async Task InitializeAsync()
    {
        await LoadTranslationsAsync();
        await LoadSalesTransactionsAsync();
        SwitchToSales();
    }

    [RelayCommand]
    private void SwitchToSales()
    {
        CurrentTab = "Sales";
        CurrentTabTitle = "Transaction";
        CurrentTabContent = CreateSalesGrid();
    }

    [RelayCommand]
    private void SwitchToRefund()
    {
        CurrentTab = "Refund";
        CurrentTabTitle = "Refund";
        _ = LoadRefundTransactionsAsync();
        CurrentTabContent = CreateRefundGrid();
    }

    [RelayCommand]
    private void SwitchToExchange()
    {
        CurrentTab = "Exchange";
        CurrentTabTitle = "Exchange";
        _ = LoadExchangeTransactionsAsync();
        CurrentTabContent = CreateExchangeGrid();
    }

    [RelayCommand]
    private void CreateNewTransaction()
    {
        _navigateToAddSales?.Invoke();
    }

    // Search functionality - filters by invoice number and customer name
    partial void OnSearchTextChanged(string value)
    {
        ApplySearchFilter();
    }

    private void ApplySearchFilter()
    {
        var searchLower = SearchText?.ToLower() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(searchLower))
        {
            // No search - show all
            FilteredSalesTransactions = new ObservableCollection<TransactionCardModel>(SalesTransactions);
            FilteredRefundTransactions = new ObservableCollection<RefundCardModel>(RefundTransactions);
            FilteredExchangeTransactions = new ObservableCollection<ExchangeCardModel>(ExchangeTransactions);
        }
        else
        {
            // Filter sales transactions
            FilteredSalesTransactions = new ObservableCollection<TransactionCardModel>(
                SalesTransactions.Where(t =>
                    t.OrderNumber.ToLower().Contains(searchLower) ||
                    (t.CustomerName?.ToLower().Contains(searchLower) ?? false)
                )
            );

            // Filter refund transactions
            FilteredRefundTransactions = new ObservableCollection<RefundCardModel>(
                RefundTransactions.Where(r =>
                    r.RefundNumber.ToLower().Contains(searchLower) ||
                    (r.CustomerName?.ToLower().Contains(searchLower) ?? false) ||
                    (r.OriginalInvoice?.ToLower().Contains(searchLower) ?? false)
                )
            );

            // Filter exchange transactions
            FilteredExchangeTransactions = new ObservableCollection<ExchangeCardModel>(
                ExchangeTransactions.Where(e =>
                    e.ExchangeNumber.ToLower().Contains(searchLower) ||
                    (e.CustomerName?.ToLower().Contains(searchLower) ?? false) ||
                    (e.OriginalInvoice?.ToLower().Contains(searchLower) ?? false)
                )
            );
        }

        // Refresh the current tab view
        RefreshCurrentTab();
    }

    private void RefreshCurrentTab()
    {
        switch (CurrentTab)
        {
            case "Sales":
                CurrentTabContent = CreateSalesGrid();
                break;
            case "Refund":
                CurrentTabContent = CreateRefundGrid();
                break;
            case "Exchange":
                CurrentTabContent = CreateExchangeGrid();
                break;
        }
    }

    [RelayCommand]
    private async Task OpenTransaction(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction != null)
            {
                // Use the navigation callback to open transaction for editing
                if (_navigateToEditTransaction != null)
                {
                    _navigateToEditTransaction(transactionId);
                }
                else
                {
                    MessageBox.Show($"Opening Transaction #{transactionId}\n\nTransaction details:\n- Customer: {transaction.CustomerName}\n- Table: {transaction.TableNumber}\n- Total: {_activeCurrencyService.FormatPrice(transaction.TotalAmount)}\n\nNavigation callback not configured.", 
                        "Open Transaction", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task EditTransaction(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction != null)
            {
                // Use the navigation callback if provided
                if (_navigateToEditTransaction != null)
                {
                    _navigateToEditTransaction(transactionId);
                }
                else
                {
                    MessageBox.Show($"Edit Transaction #{transactionId}\n\nTransaction details:\n- Customer: {transaction.CustomerName}\n- Table: {transaction.TableNumber}\n- Total: {_activeCurrencyService.FormatPrice(transaction.TotalAmount)}\n\nNavigation callback not configured.", 
                        "Edit Transaction", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteTransaction(int transactionId)
    {
        try
        {
            var result = MessageBox.Show($"Are you sure you want to delete transaction #{transactionId}?\n\nThis action cannot be undone.", 
                "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            
            if (result == MessageBoxResult.Yes)
            {
                var success = await _transactionService.DeleteAsync(transactionId);
                if (success)
                {
                    MessageBox.Show("Transaction deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Refresh the list
                    await LoadSalesTransactionsAsync();
                    SwitchToSales();
                }
                else
                {
                    MessageBox.Show("Failed to delete transaction.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error deleting transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task PayBill(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction != null)
            {
                // Use the navigation callback if provided
                if (_navigateToPayBill != null)
                {
                    _navigateToPayBill(transactionId);
                }
                else
                {
                    MessageBox.Show($"Pay Bill for Transaction #{transactionId}\n\nTransaction details:\n- Customer: {transaction.CustomerName}\n- Table: {transaction.TableNumber}\n- Total Amount: {_activeCurrencyService.FormatPrice(transaction.TotalAmount)}\n- Status: {transaction.Status}\n\nNavigation callback not configured.", 
                        "Pay Bill", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task SaveAndPrintFromCard(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction != null)
            {
                // Change status to billed
                await _transactionService.ChangeStatusAsync(transactionId, "billed", transaction.UserId);
                
                // Print the bill
                PrintBill(transaction);
                
                new MessageDialog("Success", $"Transaction #{transactionId} marked as Billed!\n\nBill has been printed.", MessageDialog.MessageType.Success).ShowDialog();
                
                // Refresh the list
                await LoadSalesTransactionsAsync();
                SwitchToSales();
            }
        }
        catch (Exception ex)
        {
            new MessageDialog("Error", $"Error updating transaction: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    [RelayCommand]
    private async Task SettleFromCard(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                MessageBox.Show("Transaction not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show payment popup - same as in AddSalesViewModel
            ShowPaymentPopupForTransaction(transaction);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error settling transaction: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task RefundFromCard(int transactionId)
    {
        try
        {
            // Load transaction
            var transaction = await _transactionService.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                MessageBox.Show("Transaction not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if transaction is settled
            if (transaction.Status?.ToLower() != "settled")
            {
                MessageBox.Show($"Only settled transactions can be refunded.\n\nCurrent status: {transaction.Status}", 
                    "Invalid Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show refund dialog
            var refundDialog = new Views.Dialogs.RefundDialog(
                transaction,
                _refundService,
                _currentUserService,
                _activeCurrencyService,
                _productService,
                _localizationService,
                _layoutDirectionService);

            var result = refundDialog.ShowDialog();

            if (result == true && refundDialog.IsConfirmed)
            {
                // Refresh transaction list to reflect the refund
                await LoadSalesTransactionsAsync();
                SwitchToSales();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error processing refund: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task ExchangeFromCard(int transactionId)
    {
        try
        {
            // Navigate to Exchange screen
            _navigateToExchangeTransaction?.Invoke(transactionId);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error processing exchange: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task OpenRefund(int refundId)
    {
        try
        {
            var refund = await _refundService.GetByIdAsync(refundId);
            if (refund == null)
            {
                MessageBox.Show("Refund not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show refund details popup
            ShowRefundDetailsPopup(refund);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening refund details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task OpenExchange(int exchangeId)
    {
        try
        {
            var exchange = await _exchangeService.GetByIdAsync(exchangeId);
            if (exchange == null)
            {
                MessageBox.Show("Exchange not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show exchange details popup
            ShowExchangeDetailsPopup(exchange);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening exchange details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task PrintRefund(int refundId)
    {
        try
        {
            var refund = await _refundService.GetByIdAsync(refundId);
            if (refund == null)
            {
                MessageBox.Show("Refund not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PrintRefundReceipt(refund);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error printing refund: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task PrintExchange(int exchangeId)
    {
        try
        {
            var exchange = await _exchangeService.GetByIdAsync(exchangeId);
            if (exchange == null)
            {
                MessageBox.Show("Exchange not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            PrintExchangeReceipt(exchange);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error printing exchange: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void ShowPaymentPopupForTransaction(TransactionDto transaction)
    {
        try
        {
            // Guard against duplicate popup display
            if (_isPaymentPopupOpen)
            {
                return; // Popup already open, ignore this call
            }
            _isPaymentPopupOpen = true;

            // Get fresh transaction data to ensure we have the latest values
            var freshTransaction = await _transactionService.GetByIdAsync(transaction.Id);
            if (freshTransaction == null)
            {
                new MessageDialog("Error", "Transaction not found.", MessageDialog.MessageType.Error).ShowDialog();
                _isPaymentPopupOpen = false; // Reset guard flag
                return;
            }
            
            var currentUserId = _currentUserService.CurrentUserId;
            if (!currentUserId.HasValue || currentUserId.Value <= 0)
            {
                new MessageDialog("Error", "Unable to identify current user. Please log in again.", MessageDialog.MessageType.Error).ShowDialog();
                _isPaymentPopupOpen = false;
                return;
            }
            
            var totalAmount = freshTransaction.TotalAmount;
            var alreadyPaid = freshTransaction.AmountPaidCash;
            
            // Get customer balance if customer is linked
            decimal customerBalanceAmount = 0m;
            CustomerDto? customer = null;
            if (freshTransaction.CustomerId.HasValue)
            {
                try
                {
                    customer = await _customerService.GetByIdAsync(freshTransaction.CustomerId.Value);
                    customerBalanceAmount = customer?.CustomerBalanceAmount ?? 0m;
                }
                catch (Exception custEx)
                {
                    AppLogger.LogError($"Failed to load customer {freshTransaction.CustomerId.Value}", custEx);
                }
            }
            
            // Calculate bill total and remaining amount based on transaction status
            decimal billTotal;
            decimal remainingAmount;
            decimal amountToPrefill;
            
            if (freshTransaction.Status == "partial_payment")
            {
                // For partial payment transactions, bill total is the customer's pending amount
                billTotal = customerBalanceAmount;
                remainingAmount = freshTransaction.AmountCreditRemaining; // Show transaction's remaining amount
                amountToPrefill = customerBalanceAmount; // Pre-fill with customer's pending amount
            }
            else
            {
                // For new/other transactions, calculate bill total considering customer balance
                // If customer has pending dues (positive balance), add to bill
                // If customer has credit (negative balance), deduct from bill
                billTotal = totalAmount + customerBalanceAmount;
                remainingAmount = billTotal - alreadyPaid;
                amountToPrefill = remainingAmount;
            }
            
            // Load payment types from database
            var paymentTypes = (await _paymentTypeService.GetActiveAsync()).ToList();
            if (!paymentTypes.Any())
            {
                new MessageDialog("Error", "No payment types available. Please add payment types first.", MessageDialog.MessageType.Error).ShowDialog();
                _isPaymentPopupOpen = false; // Reset guard flag
                return;
            }
            
            var paymentPopup = new Window
            {
                Width = 480,
                Height = 550, // Increased for customer balance info and better spacing
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };
            paymentPopup.SetBinding(Window.TitleProperty, new System.Windows.Data.Binding("PaymentPopupTitleLabel") { Source = this });
            paymentPopup.SetResourceReference(Control.BackgroundProperty, "CardBackground");

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(15) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Total Amount Label - Show customer balance and bill total
            string balanceInfo = "";
            if (customerBalanceAmount > 0)
            {
                balanceInfo = $"\n{CustomerPendingAddedLabel} {_activeCurrencyService.FormatPrice(customerBalanceAmount)} {AddedToBillLabel}";
            }
            else if (customerBalanceAmount < 0)
            {
                balanceInfo = $"\n{StoreCreditAvailableLabel} {_activeCurrencyService.FormatPrice(Math.Abs(customerBalanceAmount))} {DeductedFromBillLabel}";
            }
            
            var totalLabel = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap
            };
            totalLabel.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimary");
            
            // Set text based on transaction status
            if (freshTransaction.Status == "partial_payment")
            {
                // For partial payment transactions, show customer pending as bill total
                var alreadyPaidFromSale = totalAmount - freshTransaction.AmountCreditRemaining;
                totalLabel.Text = $"{CustomerPendingAmountLabel} {_activeCurrencyService.FormatPrice(customerBalanceAmount)}\n" +
                                  $"{RemainingAmountTransactionLabel} {_activeCurrencyService.FormatPrice(remainingAmount)}\n" +
                                  $"{AlreadyPaidLabelText} {_activeCurrencyService.FormatPrice(alreadyPaidFromSale)}";
            }
            else if (alreadyPaid > 0)
            {
                // For transactions with some payment already made
                totalLabel.Text = $"{SaleAmountLabel} {_activeCurrencyService.FormatPrice(totalAmount)}{balanceInfo}\n" +
                                  $"{BillTotalLabel} {_activeCurrencyService.FormatPrice(billTotal)}\n" +
                                  $"{RemainingLabel}: {_activeCurrencyService.FormatPrice(remainingAmount)}\n({AlreadyPaidLabelText} {_activeCurrencyService.FormatPrice(alreadyPaid)})";
            }
            else
            {
                // For new transactions with no payment yet
                totalLabel.Text = $"{SaleAmountLabel} {_activeCurrencyService.FormatPrice(totalAmount)}{balanceInfo}\n" +
                                  $"{BillTotalLabel} {_activeCurrencyService.FormatPrice(billTotal)}";
            }
            
            Grid.SetRow(totalLabel, 0);
            grid.Children.Add(totalLabel);

            // Payment Method Label and ComboBox
            var paymentMethodLabel = new TextBlock
            {
                FontSize = 14
            };
            paymentMethodLabel.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("PaymentMethodLabel") { Source = this });
            paymentMethodLabel.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondary");

            // Payment Method ComboBox - Load from database
            var paymentMethodComboBox = new ComboBox
            {
                Margin = new Thickness(0, 5, 0, 0),
                FontSize = 14,
                Padding = new Thickness(10),
                DisplayMemberPath = "Name",
                SelectedValuePath = "Id"
            };
            
            foreach (var paymentType in paymentTypes)
            {
                paymentMethodComboBox.Items.Add(paymentType);
            }
            paymentMethodComboBox.SelectedIndex = 0;

            var paymentMethodPanel = new StackPanel();
            paymentMethodPanel.Children.Add(paymentMethodLabel);
            paymentMethodPanel.Children.Add(paymentMethodComboBox);
            Grid.SetRow(paymentMethodPanel, 2);
            grid.Children.Add(paymentMethodPanel);

            // Amount Paid Label and TextBox
            var amountLabel = new TextBlock
            {
                FontSize = 14
            };
            amountLabel.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("AmountPaidLabel") { Source = this });
            amountLabel.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondary");

            var amountTextBox = new TextBox
            {
                Text = amountToPrefill.ToString("N2"), // Pre-fill with calculated amount to prefill
                FontSize = 14,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5, 0, 0)
            };

            var amountPanel = new StackPanel();
            amountPanel.Children.Add(amountLabel);
            amountPanel.Children.Add(amountTextBox);
            Grid.SetRow(amountPanel, 4);
            grid.Children.Add(amountPanel);

            // Credit Days Label and TextBox (for partial payment)
            var creditDaysLabel = new TextBlock
            {
                FontSize = 14
            };
            creditDaysLabel.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("CreditDaysLabel") { Source = this });
            creditDaysLabel.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondary");

            var creditDaysTextBox = new TextBox
            {
                Text = freshTransaction.CreditDays.ToString(),
                FontSize = 14,
                Padding = new Thickness(10),
                Margin = new Thickness(0, 5, 0, 0)
            };

            var creditDaysPanel = new StackPanel();
            creditDaysPanel.Children.Add(creditDaysLabel);
            creditDaysPanel.Children.Add(creditDaysTextBox);
            Grid.SetRow(creditDaysPanel, 6);
            grid.Children.Add(creditDaysPanel);

            // Buttons
            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var cancelButton = new System.Windows.Controls.Button
            {
                Width = 100,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            cancelButton.SetBinding(System.Windows.Controls.ContentControl.ContentProperty, new System.Windows.Data.Binding("CancelButtonLabel") { Source = this });
            cancelButton.SetResourceReference(Control.BackgroundProperty, "BorderLight");
            cancelButton.SetResourceReference(Control.ForegroundProperty, "TextPrimary");
            cancelButton.Click += (s, e) => paymentPopup.Close();

            var settleButton = new System.Windows.Controls.Button
            {
                Width = 120,
                Height = 40,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            settleButton.SetBinding(System.Windows.Controls.ContentControl.ContentProperty, new System.Windows.Data.Binding("SaveSettleButtonLabel") { Source = this });
            settleButton.SetResourceReference(Control.BackgroundProperty, "SuccessGreen");
            settleButton.SetResourceReference(Control.ForegroundProperty, "CardBackground");

            settleButton.Click += async (s, e) =>
            {
                TransactionDto? originalTransaction = null;
                bool updateSucceeded = false;
                
                try
                {
                    if (!decimal.TryParse(amountTextBox.Text, out var paidAmount))
                    {
                        new MessageDialog("Validation Error", "Please enter a valid amount.", MessageDialog.MessageType.Warning).ShowDialog();
                        return;
                    }

                    if (!int.TryParse(creditDaysTextBox.Text, out var creditDays) || creditDays < 0)
                    {
                        new MessageDialog("Validation Error", "Please enter a valid number of credit days (0 or more).", MessageDialog.MessageType.Warning).ShowDialog();
                        return;
                    }

                    // Get fresh transaction data to calculate correct remaining amount
                    var currentTransaction = await _transactionService.GetByIdAsync(freshTransaction.Id);
                    if (currentTransaction == null)
                    {
                        new MessageDialog("Error", "Transaction not found.", MessageDialog.MessageType.Error).ShowDialog();
                        return;
                    }
                    
                    // Get fresh customer balance
                    decimal currentCustomerBalance = 0m;
                    if (customer != null)
                    {
                        try
                        {
                            var freshCustomer = await _customerService.GetByIdAsync(customer.Id);
                            currentCustomerBalance = freshCustomer?.CustomerBalanceAmount ?? 0m;
                        }
                        catch (Exception custEx)
                        {
                            AppLogger.LogError($"Failed to refresh customer {customer.Id}", custEx);
                            currentCustomerBalance = customerBalanceAmount; // Use original value
                        }
                    }
                    
                    var currentTotalAmount = currentTransaction.TotalAmount;
                    var currentAlreadyPaid = currentTransaction.AmountPaidCash;
                    
                    // Calculate bill total and remaining amount based on transaction status
                    decimal currentBillTotal;
                    decimal maxAllowedPayment;
                    
                    if (currentTransaction.Status == "partial_payment")
                    {
                        // For partial payment transactions, bill total is customer's pending amount
                        currentBillTotal = currentCustomerBalance;
                        maxAllowedPayment = currentCustomerBalance; // Can pay up to customer's pending amount
                    }
                    else
                    {
                        // For new/other transactions, calculate bill total with customer balance
                        currentBillTotal = currentTotalAmount + currentCustomerBalance;
                        maxAllowedPayment = currentBillTotal - currentAlreadyPaid; // Can pay up to remaining amount
                    }
                    
                    // Validate paid amount
                    if (paidAmount < 0 || paidAmount > maxAllowedPayment)
                    {
                        new MessageDialog("Validation Error", $"Amount paid must be between $0 and ${maxAllowedPayment:N2}.", MessageDialog.MessageType.Warning).ShowDialog();
                        return;
                    }

                    // Save original transaction state for rollback
                    originalTransaction = currentTransaction;
                    
                    // Calculate new totals considering already paid amount and bill total
                    var totalPaidNow = currentAlreadyPaid + paidAmount;
                    var creditRemaining = currentBillTotal - totalPaidNow;
                    string transactionStatus;

                    // Determine transaction status based on total payment against bill total
                    if (totalPaidNow >= currentBillTotal)
                    {
                        transactionStatus = "settled"; // Full payment
                        creditRemaining = 0; // Ensure no negative credit
                    }
                    else if (totalPaidNow > 0)
                    {
                        // Partial payment - check if customer allows credit
                        if (customer != null && !customer.CreditAllowed)
                        {
                            new MessageDialog("Credit Not Allowed", "This customer is not allowed to have credit.\n\nPlease collect full payment or select a customer with credit privileges.", MessageDialog.MessageType.Warning).ShowDialog();
                            return;
                        }
                        transactionStatus = "partial_payment"; // Partial payment
                    }
                    else
                    {
                        // No payment - check if customer allows credit
                        if (customer != null && !customer.CreditAllowed)
                        {
                            new MessageDialog("Credit Not Allowed", "This customer is not allowed to have credit.\n\nPlease collect payment or select a customer with credit privileges.", MessageDialog.MessageType.Warning).ShowDialog();
                            return;
                        }
                        transactionStatus = "pending_payment"; // No payment made
                    }

                    paymentPopup.Close();

                    var selectedPaymentType = (PaymentTypeDto)paymentMethodComboBox.SelectedItem;
                    
                    // Calculate customer balance change
                    // If sale fully settled: CustomerBalanceAmount = 0 (all dues cleared)
                    // If partial/pending: CustomerBalanceAmount = creditRemaining (unpaid amount becomes due)
                    decimal newCustomerBalance = 0m;
                    
                    if (transactionStatus == "settled")
                    {
                        // Full payment - clear customer balance
                        newCustomerBalance = 0m;
                    }
                    else
                    {
                        // Partial or pending payment - update customer balance with unpaid amount
                        // The unpaid portion (creditRemaining) becomes the new balance
                        newCustomerBalance = creditRemaining;
                    }

                    // TRANSACTIONAL OPERATION: Update payment info first
                    var updateDto = new UpdateTransactionDto
                    {
                        CustomerId = currentTransaction.CustomerId,
                        TableId = currentTransaction.TableId,
                        ReservationId = currentTransaction.ReservationId,
                        TotalAmount = currentTotalAmount,
                        TotalVat = currentTransaction.TotalVat,
                        TotalDiscount = currentTransaction.TotalDiscount,
                        AmountPaidCash = totalPaidNow, // Total amount paid so far
                        AmountCreditRemaining = creditRemaining,
                        CreditDays = creditDays,
                        Vat = currentTransaction.Vat,
                        Status = currentTransaction.Status // Keep current status for now
                    };

                    await _transactionService.UpdateAsync(currentTransaction.Id, updateDto, currentTransaction.UserId);
                    updateSucceeded = true; // Mark that update succeeded

                    // TRANSACTIONAL OPERATION: Change status to appropriate state
                    await _transactionService.ChangeStatusAsync(currentTransaction.Id, transactionStatus, currentTransaction.UserId);

                    // Update customer balance if customer is selected
                    if (customer != null)
                    {
                        try
                        {
                            customer.CustomerBalanceAmount = newCustomerBalance;
                            await _customerService.UpdateCustomerAsync(customer);
                            AppLogger.Log($"Settle: Updated customer balance to ${newCustomerBalance:N2} for customer {customer.Id}");
                        }
                        catch (Exception custEx)
                        {
                            AppLogger.LogError($"Settle: Failed to update customer balance", custEx);
                        }
                    }

                    // Update reservation status to completed if transaction is settled and has a reservation
                    if (transactionStatus == "settled" && currentTransaction.ReservationId.HasValue)
                    {
                        try
                        {
                            await _reservationService.CompleteReservationAsync(currentTransaction.ReservationId.Value);
                            AppLogger.Log($"Settle: Reservation #{currentTransaction.ReservationId.Value} marked as completed");
                        }
                        catch (Exception resEx)
                        {
                            AppLogger.LogError($"Settle: Failed to update reservation {currentTransaction.ReservationId.Value} status to completed", resEx);
                        }
                    }

                    var statusMessage = transactionStatus switch
                    {
                        "settled" => "Transaction settled successfully (Full Payment)!",
                        "partial_payment" => $"Transaction saved with Partial Payment!\nCredit Remaining: {_activeCurrencyService.FormatPrice(creditRemaining)}",
                        "pending_payment" => $"Transaction saved as Pending Payment!\nTotal Credit: {_activeCurrencyService.FormatPrice(creditRemaining)}",
                        _ => "Transaction updated!"
                    };

                    new MessageDialog("Payment Complete", $"{statusMessage}\n\nPayment Method: {selectedPaymentType.Name}\nAmount Paid Now: ${paidAmount:N2}\nTotal Paid: ${totalPaidNow:N2}" +
                        (creditDays > 0 ? $"\nCredit Days: {creditDays}" : ""), MessageDialog.MessageType.Success).ShowDialog();

                    // Refresh the list to show updated status
                    await LoadSalesTransactionsAsync();
                    SwitchToSales();
                }
                catch (Exception ex)
                {
                    // ROLLBACK: If any error occurs, attempt to restore original transaction state
                    if (originalTransaction != null)
                    {
                        try
                        {
                            var rollbackDto = new UpdateTransactionDto
                            {
                                CustomerId = originalTransaction.CustomerId,
                                TableId = originalTransaction.TableId,
                                ReservationId = originalTransaction.ReservationId,
                                TotalAmount = originalTransaction.TotalAmount,
                                TotalVat = originalTransaction.TotalVat,
                                TotalDiscount = originalTransaction.TotalDiscount,
                                AmountPaidCash = originalTransaction.AmountPaidCash,
                                AmountCreditRemaining = originalTransaction.AmountCreditRemaining,
                                CreditDays = originalTransaction.CreditDays,
                                Vat = originalTransaction.Vat,
                                Status = originalTransaction.Status
                            };
                            
                            await _transactionService.UpdateAsync(originalTransaction.Id, rollbackDto, originalTransaction.UserId);
                            
                            // Only try to revert status if the update succeeded before failure
                            if (updateSucceeded)
                            {
                                await _transactionService.ChangeStatusAsync(originalTransaction.Id, originalTransaction.Status, originalTransaction.UserId);
                            }
                            
                            new MessageDialog("Error", $"Error settling transaction: {ex.Message}\n\nTransaction has been rolled back to original state.", MessageDialog.MessageType.Error).ShowDialog();
                        }
                        catch
                        {
                            new MessageDialog("Critical Error", $"Error settling transaction: {ex.Message}\n\nFailed to rollback transaction. Please check transaction #{transaction.Id} manually.", MessageDialog.MessageType.Error).ShowDialog();
                        }
                    }
                    else
                    {
                        new MessageDialog("Error", $"Error settling transaction: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
                    }
                    
                    // Refresh list to show current state
                    await LoadSalesTransactionsAsync();
                }
            };

            buttonPanel.Children.Add(cancelButton);
            buttonPanel.Children.Add(settleButton);
            Grid.SetRow(buttonPanel, 8);
            grid.Children.Add(buttonPanel);

            paymentPopup.Content = grid;
            
            // Reset guard flag when popup closes
            paymentPopup.Closed += (s, e) => _isPaymentPopupOpen = false;
            
            paymentPopup.ShowDialog();
        }
        catch (Exception ex)
        {
            _isPaymentPopupOpen = false; // Reset guard flag on error
            new MessageDialog("Error", $"Error showing payment popup: {ex.Message}", MessageDialog.MessageType.Error).ShowDialog();
        }
    }

    private void ShowRefundDetailsPopup(RefundTransactionDto refund)
    {
        try
        {
            // Fetch the original transaction to get modifiers
            TransactionDto? originalTransaction = null;
            try
            {
                originalTransaction = _transactionService.GetByIdAsync(refund.SellingTransactionId).Result;
            }
            catch
            {
                // If we can't get the original transaction, continue without modifiers
            }

            var refundNumber = $"#R{refund.Id:D4}";
            
            var detailsPopup = new Window
            {
                Title = $"Refund Details - {refundNumber}",
                Width = 600,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(20)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(0) };

            // Header
            var headerText = new TextBlock
            {
                Text = $"Refund {refundNumber}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(headerText);

            // Customer info
            var customerSection = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 15)
            };
            var customerStack = new StackPanel();
            customerStack.Children.Add(new TextBlock { Text = "Customer Information", FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 10) });
            customerStack.Children.Add(new TextBlock { Text = $"Name: {refund.CustomerName ?? "Walk-in Customer"}", FontSize = 12, Margin = new Thickness(0, 0, 0, 5) });
            customerStack.Children.Add(new TextBlock { Text = $"Date: {refund.RefundTime:dddd, dd MMM yyyy}", FontSize = 12, Margin = new Thickness(0, 0, 0, 5) });
            customerStack.Children.Add(new TextBlock { Text = $"Time: {refund.RefundTime:h:mm tt}", FontSize = 12, Margin = new Thickness(0, 0, 0, 5) });
            customerStack.Children.Add(new TextBlock { Text = $"Original Transaction: #{refund.SellingTransactionId}", FontSize = 12, Margin = new Thickness(0, 0, 0, 5) });
            customerSection.Child = customerStack;
            stackPanel.Children.Add(customerSection);

            // Products section
            var productsLabel = new TextBlock
            {
                Text = "Refunded Items",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(productsLabel);

            foreach (var product in refund.RefundProducts)
            {
                // Find the original transaction product to get modifiers
                var originalProduct = originalTransaction?.TransactionProducts
                    .FirstOrDefault(tp => tp.Id == product.TransactionProductId);
                
                var productBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FECACA")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var productGrid = new Grid();
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var productInfo = new StackPanel();
                productInfo.Children.Add(new TextBlock { Text = product.ProductName ?? "Unknown", FontSize = 13, FontWeight = FontWeights.SemiBold });
                
                // Add modifiers if available
                if (originalProduct?.Modifiers != null && originalProduct.Modifiers.Any())
                {
                    var modifierText = string.Join(", ", originalProduct.Modifiers.Select(m => m.ModifierName));
                    productInfo.Children.Add(new TextBlock 
                    { 
                        Text = modifierText, 
                        FontSize = 10, 
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")), 
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 2, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    });
                }
                
                productInfo.Children.Add(new TextBlock { Text = $"Quantity: {product.TotalQuantityReturned:0.##}", FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 0) });
                Grid.SetColumn(productInfo, 0);

                var priceText = new TextBlock
                {
                    Text = _activeCurrencyService.FormatPrice(product.TotalAmount),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
                };
                Grid.SetColumn(priceText, 1);

                productGrid.Children.Add(productInfo);
                productGrid.Children.Add(priceText);
                productBorder.Child = productGrid;
                stackPanel.Children.Add(productBorder);
            }

            // Totals section
            var totalsBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 15, 0, 15)
            };
            var totalsStack = new StackPanel();
            
            var totalGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            totalGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            totalGrid.Children.Add(new TextBlock { Text = "Total Refund Amount", FontSize = 16, FontWeight = FontWeights.Bold });
            var totalAmount = new TextBlock { Text = _activeCurrencyService.FormatPrice(refund.TotalAmount), FontSize = 20, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")) };
            Grid.SetColumn(totalAmount, 1);
            totalGrid.Children.Add(totalAmount);
            totalsStack.Children.Add(totalGrid);

            if (refund.TotalVat > 0)
            {
                totalsStack.Children.Add(new TextBlock { Text = $"(Includes VAT: {_activeCurrencyService.FormatPrice(refund.TotalVat)})", FontSize = 11, Foreground = Brushes.Gray });
            }

            totalsBorder.Child = totalsStack;
            stackPanel.Children.Add(totalsBorder);

            // Action buttons
            var buttonPanel = new UniformGrid { Columns = 2, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 10, 0, 0) };
            
            var printButton = new Button
            {
                Content = "Print Receipt",
                Height = 40,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 5, 0),
                Cursor = Cursors.Hand
            };
            printButton.Click += (s, e) =>
            {
                PrintRefundReceipt(refund);
                detailsPopup.Close();
            };

            var closeButton = new Button
            {
                Content = "Close",
                Height = 40,
                Background = Brushes.White,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                BorderThickness = new Thickness(2),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(5, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, e) => detailsPopup.Close();

            buttonPanel.Children.Add(printButton);
            buttonPanel.Children.Add(closeButton);
            stackPanel.Children.Add(buttonPanel);

            scrollViewer.Content = stackPanel;
            detailsPopup.Content = scrollViewer;
            detailsPopup.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error showing refund details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PrintRefundReceipt(RefundTransactionDto refund)
    {
        try
        {
            // Use QuestPDF for professional refund receipt generation and printing
            var refundPrinter = new QuestPdfRefundPrinter(_activeCurrencyService);

            // Get company information from settings or use defaults
            string companyName = "CHRONO POS"; // TODO: Get from settings/database
            string? companyAddress = null; // TODO: Get from settings/database
            string? companyPhone = null; // TODO: Get from settings/database
            string? gstNo = null; // TODO: Get from settings/database

            // Generate PDF and auto-print to thermal printer
            string pdfPath = refundPrinter.GenerateAndPrintRefund(
                refund: refund,
                companyName: companyName,
                companyAddress: companyAddress,
                companyPhone: companyPhone,
                gstNo: gstNo
            );

            AppLogger.Log($"Refund receipt printed successfully. PDF saved at: {pdfPath}");
            MessageBox.Show("Refund receipt printed successfully!", "Print Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error printing refund receipt: {ex.Message}");
            MessageBox.Show($"Error printing refund receipt: {ex.Message}\n\nPlease check your printer connection.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ShowExchangeDetailsPopup(ExchangeTransactionDto exchange)
    {
        try
        {
            // Fetch the original transaction to get modifiers
            TransactionDto? originalTransaction = null;
            try
            {
                originalTransaction = _transactionService.GetByIdAsync(exchange.SellingTransactionId).Result;
            }
            catch
            {
                // If we can't get the original transaction, continue without modifiers
            }

            var exchangeNumber = $"#E{exchange.Id:D4}";
            
            var detailsPopup = new Window
            {
                Title = $"Exchange Details - {exchangeNumber}",
                Width = 600,
                Height = 700,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Padding = new Thickness(20)
            };

            var stackPanel = new StackPanel { Margin = new Thickness(0) };

            // Header
            var headerText = new TextBlock
            {
                Text = $"Exchange {exchangeNumber}",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                Margin = new Thickness(0, 0, 0, 20)
            };
            stackPanel.Children.Add(headerText);

            // Customer info
            var customerSection = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F9FAFB")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 0, 0, 15)
            };
            var customerStack = new StackPanel();
            customerStack.Children.Add(new TextBlock { Text = "Transaction Information", FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 10) });
            customerStack.Children.Add(new TextBlock { Text = $"Customer: {exchange.CustomerName ?? "Walk-in Customer"}", FontSize = 12, Margin = new Thickness(0, 0, 0, 5) });
            customerStack.Children.Add(new TextBlock { Text = $"Date: {exchange.ExchangeTime:dddd, dd MMM yyyy}", FontSize = 12, Margin = new Thickness(0, 0, 0, 5) });
            customerStack.Children.Add(new TextBlock { Text = $"Time: {exchange.ExchangeTime:h:mm tt}", FontSize = 12, Margin = new Thickness(0, 0, 0, 5) });
            customerStack.Children.Add(new TextBlock { Text = $"Original Transaction: #{exchange.SellingTransactionId}", FontSize = 12, Margin = new Thickness(0, 0, 0, 5) });
            customerSection.Child = customerStack;
            stackPanel.Children.Add(customerSection);

            // Items Returned section
            var returnedLabel = new TextBlock
            {
                Text = "Items Returned",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")),
                Margin = new Thickness(0, 0, 0, 10)
            };
            stackPanel.Children.Add(returnedLabel);

            foreach (var product in exchange.ExchangeProducts.Where(p => p.OldProductName != null))
            {
                // Find the original transaction product to get modifiers
                var originalProduct = originalTransaction?.TransactionProducts
                    .FirstOrDefault(tp => tp.Id == product.OriginalTransactionProductId);

                var productBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FECACA")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var productGrid = new Grid();
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var productInfo = new StackPanel();
                productInfo.Children.Add(new TextBlock { Text = product.OldProductName ?? "Unknown", FontSize = 13, FontWeight = FontWeights.SemiBold });
                
                // Add modifiers if available
                if (originalProduct?.Modifiers != null && originalProduct.Modifiers.Any())
                {
                    var modifierText = string.Join(", ", originalProduct.Modifiers.Select(m => m.ModifierName));
                    productInfo.Children.Add(new TextBlock 
                    { 
                        Text = modifierText, 
                        FontSize = 10, 
                        Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9CA3AF")), 
                        FontStyle = FontStyles.Italic,
                        Margin = new Thickness(0, 2, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    });
                }
                
                productInfo.Children.Add(new TextBlock { Text = $"Quantity: {product.ReturnedQuantity:0.##}", FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 0) });
                Grid.SetColumn(productInfo, 0);

                var priceText = new TextBlock
                {
                    Text = _activeCurrencyService.FormatPrice(product.OldProductAmount),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
                };
                Grid.SetColumn(priceText, 1);

                productGrid.Children.Add(productInfo);
                productGrid.Children.Add(priceText);
                productBorder.Child = productGrid;
                stackPanel.Children.Add(productBorder);
            }

            // Items Given section
            var givenLabel = new TextBlock
            {
                Text = "Items Given",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                Margin = new Thickness(0, 15, 0, 10)
            };
            stackPanel.Children.Add(givenLabel);

            foreach (var product in exchange.ExchangeProducts.Where(p => p.NewProductName != null))
            {
                var productBorder = new Border
                {
                    Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0FDF4")),
                    BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BBF7D0")),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(12),
                    Margin = new Thickness(0, 0, 0, 8)
                };

                var productGrid = new Grid();
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                productGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var productInfo = new StackPanel();
                productInfo.Children.Add(new TextBlock { Text = product.NewProductName ?? "Unknown", FontSize = 13, FontWeight = FontWeights.SemiBold });
                productInfo.Children.Add(new TextBlock { Text = $"Quantity: {product.NewQuantity:0.##}", FontSize = 11, Foreground = Brushes.Gray, Margin = new Thickness(0, 2, 0, 0) });
                Grid.SetColumn(productInfo, 0);

                var priceText = new TextBlock
                {
                    Text = _activeCurrencyService.FormatPrice(product.NewProductAmount),
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
                };
                Grid.SetColumn(priceText, 1);

                productGrid.Children.Add(productInfo);
                productGrid.Children.Add(priceText);
                productBorder.Child = productGrid;
                stackPanel.Children.Add(productBorder);
            }

            // Difference section
            var differenceBorder = new Border
            {
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7")),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Margin = new Thickness(0, 15, 0, 15)
            };
            var differenceStack = new StackPanel();
            
            var differenceGrid = new Grid { Margin = new Thickness(0, 0, 0, 5) };
            differenceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            differenceGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            differenceGrid.Children.Add(new TextBlock { Text = "Price Difference", FontSize = 16, FontWeight = FontWeights.Bold });
            var differenceAmount = new TextBlock 
            { 
                Text = $"{_activeCurrencyService.FormatPrice(Math.Abs(exchange.TotalExchangedAmount))} {(exchange.TotalExchangedAmount > 0 ? "(Customer pays)" : exchange.TotalExchangedAmount < 0 ? "(Customer refund)" : "(Even)")}",
                FontSize = 16, 
                FontWeight = FontWeights.Bold, 
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B45309")) 
            };
            Grid.SetColumn(differenceAmount, 1);
            differenceGrid.Children.Add(differenceAmount);
            differenceStack.Children.Add(differenceGrid);

            if (exchange.TotalExchangedVat > 0)
            {
                differenceStack.Children.Add(new TextBlock { Text = $"(Includes VAT: {_activeCurrencyService.FormatPrice(exchange.TotalExchangedVat)})", FontSize = 11, Foreground = Brushes.Gray });
            }

            differenceBorder.Child = differenceStack;
            stackPanel.Children.Add(differenceBorder);

            // Action buttons
            var buttonPanel = new UniformGrid { Columns = 2, HorizontalAlignment = HorizontalAlignment.Stretch, Margin = new Thickness(0, 10, 0, 0) };
            
            var printButton = new Button
            {
                Content = "Print Receipt",
                Height = 40,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 5, 0),
                Cursor = Cursors.Hand
            };
            printButton.Click += (s, e) =>
            {
                PrintExchangeReceipt(exchange);
                detailsPopup.Close();
            };

            var closeButton = new Button
            {
                Content = "Close",
                Height = 40,
                Background = Brushes.White,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")),
                BorderThickness = new Thickness(2),
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(5, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            closeButton.Click += (s, e) => detailsPopup.Close();

            buttonPanel.Children.Add(printButton);
            buttonPanel.Children.Add(closeButton);
            stackPanel.Children.Add(buttonPanel);

            scrollViewer.Content = stackPanel;
            detailsPopup.Content = scrollViewer;
            detailsPopup.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error showing exchange details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PrintExchangeReceipt(ExchangeTransactionDto exchange)
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

            // Convert exchange products to ExchangeItemModel lists
            var returnItems = exchange.ExchangeProducts
                .Where(p => p.OldProductName != null && p.ReturnedQuantity > 0)
                .Select(p => new ExchangeItemModel
                {
                    ProductId = p.OriginalTransactionProductId ?? 0,
                    ProductName = p.OldProductName ?? "Unknown",
                    UnitPrice = p.ReturnedQuantity > 0 ? p.OldProductAmount / p.ReturnedQuantity : 0,
                    ReturnQuantity = (int)Math.Round(p.ReturnedQuantity)
                }).ToList();

            var newItems = exchange.ExchangeProducts
                .Where(p => p.NewProductName != null && p.NewQuantity > 0)
                .Select(p => new ExchangeItemModel
                {
                    ProductId = p.NewProductId ?? 0,
                    ProductName = p.NewProductName ?? "Unknown",
                    UnitPrice = p.NewQuantity > 0 ? p.NewProductAmount / p.NewQuantity : 0,
                    Quantity = (int)Math.Round(p.NewQuantity)
                }).ToList();

            decimal totalReturnAmount = returnItems.Sum(i => i.ReturnQuantity * i.UnitPrice);
            decimal totalNewAmount = newItems.Sum(i => i.Quantity * i.UnitPrice);
            decimal differenceToPay = totalNewAmount - totalReturnAmount;

            // Generate PDF and auto-print to thermal printer
            string pdfPath = exchangePrinter.GenerateAndPrintExchange(
                exchange: exchange,
                returnItems: returnItems,
                newItems: newItems,
                invoiceNumber: $"#{exchange.SellingTransactionId}",
                customerName: exchange.CustomerName ?? "Walk-in",
                totalReturnAmount: totalReturnAmount,
                totalNewAmount: totalNewAmount,
                differenceToPay: differenceToPay,
                companyName: companyName,
                companyAddress: companyAddress,
                companyPhone: companyPhone,
                gstNo: gstNo
            );

            // Success - no MessageBox needed, printing happens silently
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error printing exchange receipt: {ex.Message}\n\nPlease check your printer connection.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadSalesTransactionsAsync()
    {
        try
        {
            var transactions = await _transactionService.GetAllAsync();
            
            SalesTransactions.Clear();
            foreach (var transaction in transactions
                .Where(t => t.Status.ToLower() != "refunded" && t.Status.ToLower() != "exchanged")
                .OrderByDescending(t => t.CreatedAt)
                .Take(20))
            {
                // Get all items for counting
                var allItems = new ObservableCollection<TransactionItemModel>();
                foreach (var product in transaction.TransactionProducts)
                {
                    // Build product name with modifiers (like in the bill)
                    string productNameWithModifiers = product.ProductName ?? "Unknown";
                    
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
                    
                    // Calculate total price including modifiers
                    decimal totalModifierPrice = product.Modifiers?.Sum(m => m.ExtraPrice) ?? 0m;
                    decimal totalPrice = product.SellingPrice + totalModifierPrice;
                    
                    allItems.Add(new TransactionItemModel
                    {
                        Quantity = product.Quantity.ToString("0.##"),
                        ItemName = productNameWithModifiers,
                        Price = _activeCurrencyService.FormatPrice(totalPrice)
                    });
                }

                // Get up to 2 products for display
                var displayItems = new ObservableCollection<TransactionItemModel>(allItems.Take(2));

                var cardModel = new TransactionCardModel
                {
                    OrderNumber = $"#{transaction.Id:D4}",
                    CustomerName = transaction.CustomerName ?? "Walk-in Customer",
                    Date = transaction.CreatedAt.ToString("dddd, dd, yyyy"),
                    Time = transaction.CreatedAt.ToString("h:mm tt"),
                    Items = allItems,
                    DisplayItems = displayItems,
                    TotalItemCount = allItems.Count,
                    Subtotal = transaction.TotalAmount,
                    Status = transaction.Status,
                    StatusColor = GetStatusColor(transaction.Status),
                    TransactionId = transaction.Id,
                    TableName = transaction.TableNumber ?? "N/A",
                    AmountCreditRemaining = transaction.AmountCreditRemaining,
                    ReservationInfo = transaction.ReservationId.HasValue ? $"Reservation #{transaction.ReservationId}" : string.Empty,
                    CreatedAt = transaction.CreatedAt
                };
                
                cardModel.SetCurrencyService(_activeCurrencyService);

                // Calculate timer display for active transactions
                var statusLower = transaction.Status.ToLower();
                if (statusLower == "draft" || statusLower == "billed")
                {
                    cardModel.TimerDisplay = CalculateElapsedTime(transaction.CreatedAt);
                }

                SalesTransactions.Add(cardModel);
            }
            
            // Apply search filter after loading
            ApplySearchFilter();
            
            // Notify empty state property changed
            OnPropertyChanged(nameof(HasSalesTransactions));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading sales: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadRefundTransactionsAsync()
    {
        try
        {
            var refunds = await _refundService.GetAllAsync();
            
            RefundTransactions.Clear();
            foreach (var refund in refunds.OrderByDescending(r => r.CreatedAt).Take(20))
            {
                // Get the original transaction to fetch modifiers
                TransactionDto? originalTransaction = null;
                try
                {
                    originalTransaction = await _transactionService.GetByIdAsync(refund.SellingTransactionId);
                }
                catch
                {
                    // If we can't get the original transaction, continue without modifiers
                }

                // Get all products
                var allItems = new ObservableCollection<TransactionItemModel>();
                foreach (var product in refund.RefundProducts)
                {
                    // Find the original transaction product to get modifiers
                    var originalProduct = originalTransaction?.TransactionProducts
                        .FirstOrDefault(tp => tp.Id == product.TransactionProductId);
                    
                    // Build modifier string
                    string modifierString = string.Empty;
                    if (originalProduct?.Modifiers != null && originalProduct.Modifiers.Any())
                    {
                        modifierString = string.Join(", ", originalProduct.Modifiers.Select(m => m.ModifierName));
                    }

                    allItems.Add(new TransactionItemModel
                    {
                        Quantity = product.TotalQuantityReturned.ToString("0.##"),
                        ItemName = product.ProductName ?? "Unknown",
                        Price = _activeCurrencyService.FormatPrice(product.TotalAmount),
                        Modifiers = modifierString
                    });
                }

                // Get up to 2 products for display
                var displayItems = new ObservableCollection<TransactionItemModel>(allItems.Take(2));
                
                var totalItemCount = refund.RefundProducts.Sum(p => (int)p.TotalQuantityReturned);

                RefundTransactions.Add(new RefundCardModel
                {
                    RefundId = refund.Id,
                    RefundNumber = $"#R{refund.Id:D4}",
                    CustomerName = refund.CustomerName ?? "Walk-in Customer",
                    Date = refund.RefundTime.ToString("dddd, dd MMM yyyy"),
                    Time = refund.RefundTime.ToString("h:mm tt"),
                    OriginalInvoice = $"INV-{refund.SellingTransactionId}",
                    Items = allItems,
                    DisplayItems = displayItems,
                    TotalItemCount = totalItemCount,
                    ShowViewAllButton = allItems.Count > 2,
                    RefundAmount = refund.TotalAmount,
                    Status = refund.Status,
                    StatusColor = GetStatusColor(refund.Status)
                });
            }
            
            // Apply search filter after loading
            ApplySearchFilter();
            
            // Notify empty state property changed
            OnPropertyChanged(nameof(HasRefundTransactions));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading refunds: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadExchangeTransactionsAsync()
    {
        try
        {
            var exchanges = await _exchangeService.GetAllAsync();
            
            ExchangeTransactions.Clear();
            foreach (var exchange in exchanges.OrderByDescending(e => e.CreatedAt).Take(20))
            {
                // Get the original transaction to fetch modifiers
                TransactionDto? originalTransaction = null;
                try
                {
                    originalTransaction = await _transactionService.GetByIdAsync(exchange.SellingTransactionId);
                }
                catch
                {
                    // If we can't get the original transaction, continue without modifiers
                }

                // Get returned items
                var itemsReturned = new ObservableCollection<ExchangeCardItemModel>();
                var itemsGiven = new ObservableCollection<ExchangeCardItemModel>();
                
                // Process returned items - get modifiers from original transaction product
                foreach (var exchangeProduct in exchange.ExchangeProducts.Where(p => p.OldProductName != null))
                {
                    // Find the original transaction product to get modifiers
                    var originalProduct = originalTransaction?.TransactionProducts
                        .FirstOrDefault(tp => tp.Id == exchangeProduct.OriginalTransactionProductId);
                    
                    // Build modifier string for returned item
                    string modifierString = string.Empty;
                    if (originalProduct?.Modifiers != null && originalProduct.Modifiers.Any())
                    {
                        modifierString = string.Join(", ", originalProduct.Modifiers.Select(m => m.ModifierName));
                    }

                    itemsReturned.Add(new ExchangeCardItemModel
                    {
                        Quantity = $"{exchangeProduct.ReturnedQuantity}x",
                        ItemName = exchangeProduct.OldProductName ?? "",
                        Price = _activeCurrencyService.FormatPrice(exchangeProduct.OldProductAmount),
                        Modifiers = modifierString
                    });
                }
                
                // Process given items - these are new products, so check if they have modifiers
                // Note: For new products in exchange, we need to check if the exchange transaction stores modifier info
                // For now, we'll just display the product name without modifiers for new items
                foreach (var exchangeProduct in exchange.ExchangeProducts.Where(p => p.NewProductName != null))
                {
                    itemsGiven.Add(new ExchangeCardItemModel
                    {
                        Quantity = $"{exchangeProduct.NewQuantity}x",
                        ItemName = exchangeProduct.NewProductName ?? "",
                        Price = _activeCurrencyService.FormatPrice(exchangeProduct.NewProductAmount),
                        Modifiers = string.Empty // New products don't have modifiers in exchange DTO
                    });
                }

                var exchangeCard = new ExchangeCardModel
                {
                    ExchangeId = exchange.Id,
                    ExchangeNumber = $"#E{exchange.Id:D4}",
                    CustomerName = exchange.CustomerName ?? "Walk-in Customer",
                    Date = exchange.ExchangeTime.ToString("dddd, dd MMM yyyy"),
                    Time = exchange.ExchangeTime.ToString("h:mm tt"),
                    OriginalInvoice = exchange.InvoiceNumber ?? "",
                    ItemsReturned = itemsReturned,
                    ItemsGiven = itemsGiven,
                    DisplayItemsReturned = new ObservableCollection<ExchangeCardItemModel>(itemsReturned.Take(2)),
                    DisplayItemsGiven = new ObservableCollection<ExchangeCardItemModel>(itemsGiven.Take(2)),
                    TotalReturnedCount = itemsReturned.Count,
                    TotalGivenCount = itemsGiven.Count,
                    Difference = exchange.TotalExchangedAmount,
                    Status = "Exchanged",
                    StatusColor = "#3B82F6"
                };
                
                ExchangeTransactions.Add(exchangeCard);
            }
            
            // Apply search filter after loading
            ApplySearchFilter();
            
            // Notify empty state property changed
            OnPropertyChanged(nameof(HasExchangeTransactions));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading exchanges: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Calculate elapsed time from a start date to now in a human-readable format
    /// </summary>
    private string CalculateElapsedTime(DateTime startTime)
    {
        var elapsed = DateTime.Now - startTime;
        
        if (elapsed.TotalMinutes < 1)
            return "0min";
        else if (elapsed.TotalMinutes < 60)
            return $"{(int)elapsed.TotalMinutes}min";
        else if (elapsed.TotalHours < 24)
        {
            var hours = (int)elapsed.TotalHours;
            var minutes = (int)(elapsed.TotalMinutes % 60);
            return minutes > 0 ? $"{hours}hr {minutes}min" : $"{hours}hr";
        }
        else
        {
            var days = (int)elapsed.TotalDays;
            var hours = (int)(elapsed.TotalHours % 24);
            return hours > 0 ? $"{days}d {hours}hr" : $"{days}d";
        }
    }

    private string GetStatusColor(string status)
    {
        return status.ToLower() switch
        {
            "completed" or "settled" => "#10B981",      // Green
            "pending" or "draft" => "#F59E0B",          // Orange/Yellow
            "billed" => "#3B82F6",                      // Blue
            "cancelled" => "#EF4444",                   // Red
            "hold" => "#60A5FA",                        // Blue (replaced purple with lighter blue)
            "pending_payment" => "#F97316",             // Dark Orange
            "partial_payment" => "#06B6D4",             // Cyan
            "refunded" => "#DC2626",                    // Dark Red
            "exchanged" => "#3B82F6",                   // Blue (replaced violet)
            _ => "#6B7280"                              // Gray
        };
    }

    private UIElement CreateSalesGrid()
    {
        // Create main grid container
        var mainGrid = new Grid();
        
        // Create ItemsControl for data
        var itemsControl = new ItemsControl
        {
            ItemsSource = FilteredSalesTransactions
        };

        // Wrap panel for card layout
        var wrapPanelFactory = new FrameworkElementFactory(typeof(WrapPanel));
        wrapPanelFactory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
        
        itemsControl.ItemsPanel = new ItemsPanelTemplate(wrapPanelFactory);
        itemsControl.ItemTemplate = CreateSalesCardTemplate();
        
        // Bind visibility - show when HasSalesTransactions is true
        itemsControl.SetBinding(UIElement.VisibilityProperty, new System.Windows.Data.Binding("HasSalesTransactions")
        {
            Source = this,
            Converter = new System.Windows.Controls.BooleanToVisibilityConverter()
        });
        
        // Create empty state
        var emptyState = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var icon = new TextBlock
        {
            Text = "💳",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16)
        };
        
        var primaryMessage = new TextBlock
        {
            FontSize = 16,
            FontWeight = FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        primaryMessage.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("NoSalesTransactionsLabel")
        {
            Source = this
        });
        primaryMessage.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimary");
        
        var secondaryMessage = new TextBlock
        {
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        secondaryMessage.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("StartCreatingSalesLabel")
        {
            Source = this
        });
        secondaryMessage.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondary");
        
        emptyState.Children.Add(icon);
        emptyState.Children.Add(primaryMessage);
        emptyState.Children.Add(secondaryMessage);
        
        // Bind visibility - show when HasSalesTransactions is false
        emptyState.SetBinding(UIElement.VisibilityProperty, new System.Windows.Data.Binding("HasSalesTransactions")
        {
            Source = this,
            Converter = new InverseBoolToVisibilityConverter()
        });
        
        // Add both to grid
        mainGrid.Children.Add(emptyState);
        mainGrid.Children.Add(itemsControl);

        return mainGrid;
    }

    private DataTemplate CreateSalesCardTemplate()
    {
        var xaml = @"
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <Border Background='{DynamicResource CardBackground}' BorderBrush='{DynamicResource BorderLight}' BorderThickness='1' CornerRadius='12' Padding='16' Margin='0,0,15,15' Width='340' Height='Auto' Cursor='Hand'>
                    <Border.InputBindings>
                        <MouseBinding MouseAction='LeftClick' Command='{Binding DataContext.OpenTransactionCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}' CommandParameter='{Binding TransactionId}'/>
                    </Border.InputBindings>
                    <Border.Style>
                        <Style TargetType='Border'>
                            <Setter Property='Effect'>
                                <Setter.Value>
                                    <DropShadowEffect Color='#00000010' BlurRadius='10' ShadowDepth='2' Opacity='0.3'/>
                                </Setter.Value>
                            </Setter>
                            <Style.Triggers>
                                <Trigger Property='IsMouseOver' Value='True'>
                                    <Setter Property='Effect'>
                                        <Setter.Value>
                                            <DropShadowEffect Color='#F59E0B' BlurRadius='15' ShadowDepth='3' Opacity='0.4'/>
                                        </Setter.Value>
                                    </Setter>
                                    <Setter Property='BorderBrush' Value='#F59E0B'/>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </Border.Style>
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='8'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='12'/>
                            <RowDefinition Height='Auto'/>
                        </Grid.RowDefinitions>
                        
                        <!-- Header: Order Number + Status Badge -->
                        <Grid Grid.Row='0'>
                            <Border Background='{DynamicResource ChronoPosOrange}' CornerRadius='8' Padding='10,5' HorizontalAlignment='Left'>
                                <TextBlock Text='{Binding OrderNumber}' FontSize='13' FontWeight='Bold' Foreground='White'/>
                            </Border>
                            <Border Background='{Binding StatusColor}' CornerRadius='6' Padding='8,4' HorizontalAlignment='Right'>
                                <TextBlock Text='{Binding Status}' FontSize='10' FontWeight='SemiBold' Foreground='White'/>
                            </Border>
                        </Grid>
                        
                        <!-- Customer Info -->
                        <StackPanel Grid.Row='1' Margin='0,12,0,0'>
                            <TextBlock Text='{Binding CustomerName}' FontSize='15' FontWeight='SemiBold' Foreground='{DynamicResource TextPrimary}'/>
                            <TextBlock FontSize='11' Foreground='{DynamicResource TextSecondary}' Margin='0,4,0,0'>
                                <Run Text='{Binding Date}'/> - <Run Text='{Binding Time}'/>
                            </TextBlock>
                            <Grid Margin='0,4,0,0'>
                                <TextBlock FontSize='12' Foreground='{DynamicResource Info}' FontWeight='Medium' HorizontalAlignment='Left'>
                                    <Run Text='Table: '/><Run Text='{Binding TableName}'/>
                                </TextBlock>
                                <!-- Reservation Info -->
                                <TextBlock FontSize='11' Foreground='#8B5CF6' FontWeight='Medium' HorizontalAlignment='Right'>
                                    <TextBlock.Style>
                                        <Style TargetType='TextBlock'>
                                            <Setter Property='Visibility' Value='Collapsed'/>
                                            <Setter Property='Text' Value='{Binding ReservationInfo}'/>
                                            <Style.Triggers>
                                                <DataTrigger Binding='{Binding ShowReservation}' Value='True'>
                                                    <Setter Property='Visibility' Value='Visible'/>
                                                </DataTrigger>
                                            </Style.Triggers>
                                        </Style>
                                    </TextBlock.Style>
                                </TextBlock>
                            </Grid>
                            <!-- Credit Remaining for Partial/Pending Payment -->
                            <Border Background='#FEE2E2' CornerRadius='6' Padding='8,4' HorizontalAlignment='Left' Margin='0,6,0,0'>
                                <Border.Style>
                                    <Style TargetType='Border'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding ShowCreditRemaining}' Value='True'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Border.Style>
                                <TextBlock Text='{Binding CreditRemainingDisplay}' FontSize='11' FontWeight='SemiBold' Foreground='#991B1B'/>
                            </Border>
                            <!-- Timer Display for Active Transactions -->
                            <Border Background='#FEF3C7' CornerRadius='6' Padding='8,4' HorizontalAlignment='Left' Margin='0,6,0,0'>
                                <Border.Style>
                                    <Style TargetType='Border'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding ShowTimer}' Value='True'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Border.Style>
                                <StackPanel Orientation='Horizontal'>
                                    <TextBlock Text='⏱️ ' FontSize='11' VerticalAlignment='Center'/>
                                    <TextBlock Text='{Binding TimerDisplay}' FontSize='11' FontWeight='SemiBold' Foreground='#92400E' VerticalAlignment='Center'/>
                                </StackPanel>
                            </Border>
                        </StackPanel>
                        
                        <!-- Section Label with Quantity and View All Button -->
                        <Grid Grid.Row='3' Margin='0,8,0,4'>
                            <StackPanel Orientation='Horizontal'>
                                <TextBlock Text='Items: ' FontSize='11' FontWeight='SemiBold' Foreground='#6B7280'/>
                                <TextBlock Text='{Binding TotalItemCount}' FontSize='11' FontWeight='Bold' Foreground='#F59E0B'/>
                            </StackPanel>
                            <Button Content='View All' FontSize='10' FontWeight='SemiBold' Foreground='#3B82F6' Background='Transparent' 
                                    BorderThickness='0' HorizontalAlignment='Right' Cursor='Hand' Padding='4,0'
                                    Command='{Binding DataContext.OpenTransactionCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding TransactionId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' Padding='{TemplateBinding Padding}'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding ShowViewAllButton}' Value='True'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <Trigger Property='IsMouseOver' Value='True'>
                                                <Setter Property='Foreground' Value='#1D4ED8'/>
                                                <Setter Property='TextBlock.TextDecorations' Value='Underline'/>
                                            </Trigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                        </Grid>
                        
                        <!-- Product Items (up to 2) -->
                        <ItemsControl Grid.Row='4' ItemsSource='{Binding DisplayItems}' Margin='0,0,0,8'>
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background='{DynamicResource PageBackground}' BorderBrush='{DynamicResource BorderLight}' BorderThickness='1' CornerRadius='6' Padding='8,6' Margin='0,0,0,4'>
                                        <Grid>
                                            <Grid.ColumnDefinitions>
                                                <ColumnDefinition Width='Auto'/>
                                                <ColumnDefinition Width='*'/>
                                                <ColumnDefinition Width='Auto'/>
                                            </Grid.ColumnDefinitions>
                                            <TextBlock Grid.Column='0' FontSize='10' FontWeight='Bold' Foreground='{DynamicResource ChronoPosOrange}' Margin='0,0,6,0'>
                                                <Run Text='{Binding Quantity}'/><Run Text='x'/>
                                            </TextBlock>
                                            <TextBlock Grid.Column='1' Text='{Binding ItemName}' FontSize='10' Foreground='{DynamicResource TextPrimary}' TextTrimming='CharacterEllipsis'/>
                                            <TextBlock Grid.Column='2' Text='{Binding Price}' FontSize='10' FontWeight='SemiBold' Foreground='{DynamicResource TextPrimary}'/>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                        
                        <!-- Total -->
                        <Grid Grid.Row='5' Margin='0,4,0,0'>
                            <TextBlock Text='Total' FontSize='13' FontWeight='SemiBold' Foreground='{DynamicResource TextPrimary}'/>
                            <TextBlock Text='{Binding Subtotal, Converter={StaticResource CurrencyPriceConverter}}' FontSize='15' FontWeight='Bold' Foreground='{DynamicResource TextPrimary}' HorizontalAlignment='Right'/>
                        </Grid>
                        
                        <!-- Action Buttons - Status Based -->
                        <UniformGrid Grid.Row='7' Columns='2' HorizontalAlignment='Stretch'>
                            <!-- Save & Print Button - for draft/billed/hold/pending/partial -->
                            <Button Content='Save &amp; Print' Height='35' Margin='0,0,4,0' Background='{DynamicResource Info}' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
                                    Command='{Binding DataContext.SaveAndPrintFromCardCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding TransactionId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' CornerRadius='6' Padding='8,0'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='draft'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='billed'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='hold'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='pending_payment'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='partial_payment'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                            
                            <!-- Settle Button - for draft/billed/hold/pending/partial -->
                            <Button Content='Settle' Height='35' Margin='4,0,0,0' Background='{DynamicResource SuccessGreen}' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
                                    Command='{Binding DataContext.SettleFromCardCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding TransactionId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' CornerRadius='6' Padding='8,0'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='draft'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='billed'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='hold'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='pending_payment'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='partial_payment'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                            
                            <!-- Refund Button - for settled -->
                            <Button Content='Refund' Height='35' Margin='0,0,4,0' Background='{DynamicResource ErrorRed}' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
                                    Command='{Binding DataContext.RefundFromCardCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding TransactionId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' CornerRadius='6' Padding='8,0'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='settled'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                            
                            <!-- Exchange Button - for settled -->
                            <Button Content='Exchange' Height='35' Margin='4,0,0,0' Background='{DynamicResource Info}' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
                                    Command='{Binding DataContext.ExchangeFromCardCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding TransactionId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' CornerRadius='6' Padding='8,0'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding StatusLower}' Value='settled'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                        </UniformGrid>
                    </Grid>
                </Border>
            </DataTemplate>";
        
        return (DataTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
    }

    private UIElement CreateRefundGrid()
    {
        // Create main grid container
        var mainGrid = new Grid();
        
        // Create ItemsControl for data
        var itemsControl = new ItemsControl
        {
            ItemsSource = FilteredRefundTransactions
        };

        // Wrap panel for card layout
        var wrapPanelFactory = new FrameworkElementFactory(typeof(WrapPanel));
        wrapPanelFactory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
        
        itemsControl.ItemsPanel = new ItemsPanelTemplate(wrapPanelFactory);
        itemsControl.ItemTemplate = CreateRefundCardTemplate();
        
        // Bind visibility - show when HasRefundTransactions is true
        itemsControl.SetBinding(UIElement.VisibilityProperty, new System.Windows.Data.Binding("HasRefundTransactions")
        {
            Source = this,
            Converter = new System.Windows.Controls.BooleanToVisibilityConverter()
        });
        
        // Create empty state
        var emptyState = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var icon = new TextBlock
        {
            Text = "🔄",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16)
        };
        
        var primaryMessage = new TextBlock
        {
            FontSize = 16,
            FontWeight = FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        primaryMessage.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("NoRefundTransactionsLabel")
        {
            Source = this
        });
        primaryMessage.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimary");
        
        var secondaryMessage = new TextBlock
        {
            Text = "Refunds will appear here when transactions are refunded",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        secondaryMessage.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondary");
        
        emptyState.Children.Add(icon);
        emptyState.Children.Add(primaryMessage);
        emptyState.Children.Add(secondaryMessage);
        
        // Bind visibility - show when HasRefundTransactions is false
        emptyState.SetBinding(UIElement.VisibilityProperty, new System.Windows.Data.Binding("HasRefundTransactions")
        {
            Source = this,
            Converter = new InverseBoolToVisibilityConverter()
        });
        
        // Add both to grid
        mainGrid.Children.Add(emptyState);
        mainGrid.Children.Add(itemsControl);

        return mainGrid;
    }

    private DataTemplate CreateRefundCardTemplate()
    {
        var xaml = @"
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <Border Background='{DynamicResource CardBackground}' BorderBrush='{DynamicResource BorderLight}' BorderThickness='1' CornerRadius='12' Padding='16' Margin='0,0,15,15' Width='340' Height='Auto' Cursor='Hand'>
                    <Border.InputBindings>
                        <MouseBinding MouseAction='LeftClick' Command='{Binding DataContext.OpenRefundCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}' CommandParameter='{Binding RefundId}'/>
                    </Border.InputBindings>
                    <Border.Style>
                        <Style TargetType='Border'>
                            <Setter Property='Effect'>
                                <Setter.Value>
                                    <DropShadowEffect Color='#00000010' BlurRadius='10' ShadowDepth='2' Opacity='0.3'/>
                                </Setter.Value>
                            </Setter>
                            <Style.Triggers>
                                <Trigger Property='IsMouseOver' Value='True'>
                                    <Setter Property='Effect'>
                                        <Setter.Value>
                                            <DropShadowEffect Color='#EF4444' BlurRadius='15' ShadowDepth='3' Opacity='0.4'/>
                                        </Setter.Value>
                                    </Setter>
                                    <Setter Property='BorderBrush' Value='#EF4444'/>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </Border.Style>
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='8'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='12'/>
                            <RowDefinition Height='Auto'/>
                        </Grid.RowDefinitions>
                        
                        <!-- Header: Refund Number + Status Badge -->
                        <Grid Grid.Row='0'>
                            <Border Background='{DynamicResource ErrorRed}' CornerRadius='8' Padding='10,5' HorizontalAlignment='Left'>
                                <TextBlock Text='{Binding RefundNumber}' FontSize='13' FontWeight='Bold' Foreground='White'/>
                            </Border>
                            <Border Background='{Binding StatusColor}' CornerRadius='6' Padding='8,4' HorizontalAlignment='Right'>
                                <TextBlock Text='{Binding Status}' FontSize='10' FontWeight='SemiBold' Foreground='White'/>
                            </Border>
                        </Grid>
                        
                        <!-- Customer Info -->
                        <StackPanel Grid.Row='1' Margin='0,12,0,0'>
                            <TextBlock Text='{Binding CustomerName}' FontSize='15' FontWeight='SemiBold' Foreground='{DynamicResource TextPrimary}'/>
                            <TextBlock FontSize='11' Foreground='{DynamicResource TextSecondary}' Margin='0,4,0,0'>
                                <Run Text='{Binding Date}'/> - <Run Text='{Binding Time}'/>
                            </TextBlock>
                            <TextBlock FontSize='12' Foreground='{DynamicResource ErrorRed}' FontWeight='Medium' Margin='0,4,0,0'>
                                <Run Text='Original Invoice: '/><Run Text='{Binding OriginalInvoice}'/>
                            </TextBlock>
                        </StackPanel>
                        
                        <!-- Section Label with Quantity and View All Button -->
                        <Grid Grid.Row='3' Margin='0,8,0,4'>
                            <StackPanel Orientation='Horizontal'>
                                <TextBlock Text='Items: ' FontSize='11' FontWeight='SemiBold' Foreground='#6B7280'/>
                                <TextBlock Text='{Binding TotalItemCount}' FontSize='11' FontWeight='Bold' Foreground='#EF4444'/>
                            </StackPanel>
                            <Button Content='View All' FontSize='10' FontWeight='SemiBold' Foreground='#EF4444' Background='Transparent' 
                                    BorderThickness='0' HorizontalAlignment='Right' Cursor='Hand' Padding='4,0'
                                    Command='{Binding DataContext.OpenRefundCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding RefundId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' Padding='{TemplateBinding Padding}'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding ShowViewAllButton}' Value='True'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <Trigger Property='IsMouseOver' Value='True'>
                                                <Setter Property='Foreground' Value='#DC2626'/>
                                                <Setter Property='TextBlock.TextDecorations' Value='Underline'/>
                                            </Trigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                        </Grid>
                        
                        <!-- Refunded Items (up to 2) -->
                        <ItemsControl Grid.Row='4' ItemsSource='{Binding DisplayItems}' Margin='0,0,0,8'>
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background='#FEF2F2' BorderBrush='#FECACA' BorderThickness='1' CornerRadius='6' Padding='8,6' Margin='0,0,0,4'>
                                        <Grid>
                                            <Grid.RowDefinitions>
                                                <RowDefinition Height='Auto'/>
                                                <RowDefinition Height='Auto'/>
                                            </Grid.RowDefinitions>
                                            
                                            <!-- Product line -->
                                            <Grid Grid.Row='0'>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width='Auto'/>
                                                    <ColumnDefinition Width='*'/>
                                                    <ColumnDefinition Width='Auto'/>
                                                </Grid.ColumnDefinitions>
                                                <TextBlock Grid.Column='0' FontSize='10' FontWeight='Bold' Foreground='#EF4444' Margin='0,0,6,0'>
                                                    <Run Text='{Binding Quantity}'/><Run Text='x'/>
                                                </TextBlock>
                                                <TextBlock Grid.Column='1' Text='{Binding ItemName}' FontSize='10' Foreground='#374151' TextTrimming='CharacterEllipsis'/>
                                                <TextBlock Grid.Column='2' Text='{Binding Price}' FontSize='10' FontWeight='SemiBold' Foreground='#1F2937'/>
                                            </Grid>
                                            
                                            <!-- Modifiers line -->
                                            <TextBlock Grid.Row='1' Text='{Binding Modifiers}' FontSize='9' Foreground='#9CA3AF' FontStyle='Italic' 
                                                       Margin='20,2,0,0' TextTrimming='CharacterEllipsis'>
                                                <TextBlock.Style>
                                                    <Style TargetType='TextBlock'>
                                                        <Setter Property='Visibility' Value='Collapsed'/>
                                                        <Style.Triggers>
                                                            <DataTrigger Binding='{Binding HasModifiers}' Value='True'>
                                                                <Setter Property='Visibility' Value='Visible'/>
                                                            </DataTrigger>
                                                        </Style.Triggers>
                                                    </Style>
                                                </TextBlock.Style>
                                            </TextBlock>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                        
                        <!-- Refund Amount -->
                        <Grid Grid.Row='5' Margin='0,4,0,0'>
                            <TextBlock Text='Refund Amount' FontSize='13' FontWeight='SemiBold' Foreground='#1F2937'/>
                            <TextBlock Text='{Binding RefundAmount, Converter={StaticResource CurrencyPriceConverter}}' FontSize='15' FontWeight='Bold' Foreground='#EF4444' HorizontalAlignment='Right'/>
                        </Grid>
                        
                        <!-- Action Buttons -->
                        <UniformGrid Grid.Row='7' Columns='2' HorizontalAlignment='Stretch'>
                            <Button Content='View Details' Height='35' Margin='0,0,4,0' Background='#EF4444' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
                                    Command='{Binding DataContext.OpenRefundCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding RefundId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' CornerRadius='6' Padding='8,0'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <Trigger Property='IsMouseOver' Value='True'>
                                                <Setter Property='Background' Value='#DC2626'/>
                                            </Trigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                            <Button Content='Print' Height='35' Margin='4,0,0,0' Background='White' Foreground='#EF4444' BorderBrush='#EF4444' BorderThickness='2' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
                                    Command='{Binding DataContext.PrintRefundCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding RefundId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' CornerRadius='6' Padding='8,0'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <Trigger Property='IsMouseOver' Value='True'>
                                                <Setter Property='Background' Value='#FEF2F2'/>
                                            </Trigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                        </UniformGrid>
                    </Grid>
                </Border>
            </DataTemplate>";
        
        return (DataTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
    }

    private UIElement CreateExchangeGrid()
    {
        // Create main grid container
        var mainGrid = new Grid();
        
        // Create ItemsControl for data
        var itemsControl = new ItemsControl
        {
            ItemsSource = FilteredExchangeTransactions
        };

        // Wrap panel for card layout
        var wrapPanelFactory = new FrameworkElementFactory(typeof(WrapPanel));
        wrapPanelFactory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
        
        itemsControl.ItemsPanel = new ItemsPanelTemplate(wrapPanelFactory);
        itemsControl.ItemTemplate = CreateExchangeCardTemplate();
        
        // Bind visibility - show when HasExchangeTransactions is true
        itemsControl.SetBinding(UIElement.VisibilityProperty, new System.Windows.Data.Binding("HasExchangeTransactions")
        {
            Source = this,
            Converter = new System.Windows.Controls.BooleanToVisibilityConverter()
        });
        
        // Create empty state
        var emptyState = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        
        var icon = new TextBlock
        {
            Text = "🔁",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 16)
        };
        
        var primaryMessage = new TextBlock
        {
            FontSize = 16,
            FontWeight = FontWeights.Medium,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 8)
        };
        primaryMessage.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("NoExchangeTransactionsLabel")
        {
            Source = this
        });
        primaryMessage.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimary");
        
        var secondaryMessage = new TextBlock
        {
            Text = "Product exchanges will appear here when processed",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        secondaryMessage.SetResourceReference(TextBlock.ForegroundProperty, "TextSecondary");
        
        emptyState.Children.Add(icon);
        emptyState.Children.Add(primaryMessage);
        emptyState.Children.Add(secondaryMessage);
        
        // Bind visibility - show when HasExchangeTransactions is false
        emptyState.SetBinding(UIElement.VisibilityProperty, new System.Windows.Data.Binding("HasExchangeTransactions")
        {
            Source = this,
            Converter = new InverseBoolToVisibilityConverter()
        });
        
        // Add both to grid
        mainGrid.Children.Add(emptyState);
        mainGrid.Children.Add(itemsControl);

        return mainGrid;
    }

    private DataTemplate CreateExchangeCardTemplate()
    {
        var xaml = @"
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <Border Background='{DynamicResource CardBackground}' BorderBrush='{DynamicResource BorderLight}' BorderThickness='1' CornerRadius='12' Padding='16' Margin='0,0,15,15' Width='340' Height='Auto' Cursor='Hand'>
                    <Border.InputBindings>
                        <MouseBinding MouseAction='LeftClick' Command='{Binding DataContext.OpenExchangeCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}' CommandParameter='{Binding ExchangeId}'/>
                    </Border.InputBindings>
                    <Border.Style>
                        <Style TargetType='Border'>
                            <Setter Property='Effect'>
                                <Setter.Value>
                                    <DropShadowEffect Color='#00000010' BlurRadius='10' ShadowDepth='2' Opacity='0.3'/>
                                </Setter.Value>
                            </Setter>
                            <Style.Triggers>
                                <Trigger Property='IsMouseOver' Value='True'>
                                    <Setter Property='Effect'>
                                        <Setter.Value>
                                            <DropShadowEffect Color='#3B82F6' BlurRadius='15' ShadowDepth='3' Opacity='0.4'/>
                                        </Setter.Value>
                                    </Setter>
                                    <Setter Property='BorderBrush' Value='#3B82F6'/>
                                </Trigger>
                            </Style.Triggers>
                        </Style>
                    </Border.Style>
                    <Grid>
                        <Grid.RowDefinitions>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='8'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='8'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='Auto'/>
                            <RowDefinition Height='12'/>
                            <RowDefinition Height='Auto'/>
                        </Grid.RowDefinitions>
                        
                        <!-- Header: Exchange Number + Status Badge -->
                        <Grid Grid.Row='0'>
                            <Border Background='{DynamicResource Info}' CornerRadius='8' Padding='10,5' HorizontalAlignment='Left'>
                                <TextBlock Text='{Binding ExchangeNumber}' FontSize='13' FontWeight='Bold' Foreground='White'/>
                            </Border>
                            <Border Background='{Binding StatusColor}' CornerRadius='6' Padding='8,4' HorizontalAlignment='Right'>
                                <TextBlock Text='{Binding Status}' FontSize='10' FontWeight='SemiBold' Foreground='White'/>
                            </Border>
                        </Grid>
                        
                        <!-- Customer Info -->
                        <StackPanel Grid.Row='1' Margin='0,12,0,0'>
                            <TextBlock Text='{Binding CustomerName}' FontSize='15' FontWeight='SemiBold' Foreground='{DynamicResource TextPrimary}'/>
                            <TextBlock FontSize='11' Foreground='{DynamicResource TextSecondary}' Margin='0,4,0,0'>
                                <Run Text='{Binding Date}'/> - <Run Text='{Binding Time}'/>
                            </TextBlock>
                            <TextBlock FontSize='12' Foreground='{DynamicResource Info}' FontWeight='Medium' Margin='0,4,0,0'>
                                <Run Text='Original Invoice: '/><Run Text='{Binding OriginalInvoice}'/>
                            </TextBlock>
                        </StackPanel>
                        
                        <!-- Items Returned Section with Count and View All -->
                        <Grid Grid.Row='3' Margin='0,8,0,4'>
                            <StackPanel Orientation='Horizontal'>
                                <TextBlock Text='Items Returned: ' FontSize='11' FontWeight='SemiBold' Foreground='#EF4444'/>
                                <TextBlock Text='{Binding TotalReturnedCount}' FontSize='11' FontWeight='Bold' Foreground='#EF4444'/>
                            </StackPanel>
                            <Button Content='View All' FontSize='10' FontWeight='SemiBold' Foreground='#3B82F6' Background='Transparent' 
                                    BorderThickness='0' HorizontalAlignment='Right' Cursor='Hand' Padding='4,0'
                                    Command='{Binding DataContext.OpenExchangeCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding ExchangeId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Visibility' Value='Collapsed'/>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' Padding='{TemplateBinding Padding}'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <DataTrigger Binding='{Binding ShowViewAllButton}' Value='True'>
                                                <Setter Property='Visibility' Value='Visible'/>
                                            </DataTrigger>
                                            <Trigger Property='IsMouseOver' Value='True'>
                                                <Setter Property='Foreground' Value='#2563EB'/>
                                                <Setter Property='TextBlock.TextDecorations' Value='Underline'/>
                                            </Trigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                        </Grid>
                        
                        <!-- Items Returned (up to 2) -->
                        <ItemsControl Grid.Row='4' ItemsSource='{Binding DisplayItemsReturned}' Margin='0,0,0,0'>
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background='#FEF2F2' BorderBrush='#FECACA' BorderThickness='1' CornerRadius='6' Padding='8,6' Margin='0,0,0,4'>
                                        <Grid>
                                            <Grid.RowDefinitions>
                                                <RowDefinition Height='Auto'/>
                                                <RowDefinition Height='Auto'/>
                                            </Grid.RowDefinitions>
                                            
                                            <!-- Product line -->
                                            <Grid Grid.Row='0'>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width='Auto'/>
                                                    <ColumnDefinition Width='*'/>
                                                    <ColumnDefinition Width='Auto'/>
                                                </Grid.ColumnDefinitions>
                                                <TextBlock Grid.Column='0' FontSize='10' FontWeight='Bold' Foreground='#EF4444' Margin='0,0,6,0'>
                                                    <Run Text='{Binding Quantity}'/>
                                                </TextBlock>
                                                <TextBlock Grid.Column='1' Text='{Binding ItemName}' FontSize='10' Foreground='#374151' TextTrimming='CharacterEllipsis'/>
                                                <TextBlock Grid.Column='2' Text='{Binding Price}' FontSize='10' FontWeight='SemiBold' Foreground='#1F2937'/>
                                            </Grid>
                                            
                                            <!-- Modifiers line -->
                                            <TextBlock Grid.Row='1' Text='{Binding Modifiers}' FontSize='9' Foreground='#9CA3AF' FontStyle='Italic' 
                                                       Margin='20,2,0,0' TextTrimming='CharacterEllipsis'>
                                                <TextBlock.Style>
                                                    <Style TargetType='TextBlock'>
                                                        <Setter Property='Visibility' Value='Collapsed'/>
                                                        <Style.Triggers>
                                                            <DataTrigger Binding='{Binding HasModifiers}' Value='True'>
                                                                <Setter Property='Visibility' Value='Visible'/>
                                                            </DataTrigger>
                                                        </Style.Triggers>
                                                    </Style>
                                                </TextBlock.Style>
                                            </TextBlock>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                        
                        <!-- Items Given Section with Count -->
                        <Grid Grid.Row='6' Margin='0,4,0,4'>
                            <StackPanel Orientation='Horizontal'>
                                <TextBlock Text='Items Given: ' FontSize='11' FontWeight='SemiBold' Foreground='#10B981'/>
                                <TextBlock Text='{Binding TotalGivenCount}' FontSize='11' FontWeight='Bold' Foreground='#10B981'/>
                            </StackPanel>
                        </Grid>
                        
                        <!-- Items Given (up to 2) -->
                        <ItemsControl Grid.Row='7' ItemsSource='{Binding DisplayItemsGiven}' Margin='0,0,0,8'>
                            <ItemsControl.ItemTemplate>
                                <DataTemplate>
                                    <Border Background='#F0FDF4' BorderBrush='#BBF7D0' BorderThickness='1' CornerRadius='6' Padding='8,6' Margin='0,0,0,4'>
                                        <Grid>
                                            <Grid.RowDefinitions>
                                                <RowDefinition Height='Auto'/>
                                                <RowDefinition Height='Auto'/>
                                            </Grid.RowDefinitions>
                                            
                                            <!-- Product line -->
                                            <Grid Grid.Row='0'>
                                                <Grid.ColumnDefinitions>
                                                    <ColumnDefinition Width='Auto'/>
                                                    <ColumnDefinition Width='*'/>
                                                    <ColumnDefinition Width='Auto'/>
                                                </Grid.ColumnDefinitions>
                                                <TextBlock Grid.Column='0' FontSize='10' FontWeight='Bold' Foreground='#10B981' Margin='0,0,6,0'>
                                                    <Run Text='{Binding Quantity}'/>
                                                </TextBlock>
                                                <TextBlock Grid.Column='1' Text='{Binding ItemName}' FontSize='10' Foreground='#374151' TextTrimming='CharacterEllipsis'/>
                                                <TextBlock Grid.Column='2' Text='{Binding Price}' FontSize='10' FontWeight='SemiBold' Foreground='#1F2937'/>
                                            </Grid>
                                            
                                            <!-- Modifiers line -->
                                            <TextBlock Grid.Row='1' Text='{Binding Modifiers}' FontSize='9' Foreground='#9CA3AF' FontStyle='Italic' 
                                                       Margin='20,2,0,0' TextTrimming='CharacterEllipsis'>
                                                <TextBlock.Style>
                                                    <Style TargetType='TextBlock'>
                                                        <Setter Property='Visibility' Value='Collapsed'/>
                                                        <Style.Triggers>
                                                            <DataTrigger Binding='{Binding HasModifiers}' Value='True'>
                                                                <Setter Property='Visibility' Value='Visible'/>
                                                            </DataTrigger>
                                                        </Style.Triggers>
                                                    </Style>
                                                </TextBlock.Style>
                                            </TextBlock>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                        
                        <!-- Difference -->
                        <Grid Grid.Row='8' Margin='0,4,0,0'>
                            <TextBlock Text='Price Difference' FontSize='13' FontWeight='SemiBold' Foreground='#1F2937'/>
                            <TextBlock Text='{Binding Difference, Converter={StaticResource CurrencyPriceConverter}}' FontSize='15' FontWeight='Bold' Foreground='#3B82F6' HorizontalAlignment='Right'/>
                        </Grid>
                        
                        <!-- Action Buttons -->
                        <UniformGrid Grid.Row='10' Columns='2' HorizontalAlignment='Stretch'>
                            <Button Content='View Details' Height='35' Margin='0,0,4,0' Background='#3B82F6' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
                                    Command='{Binding DataContext.OpenExchangeCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding ExchangeId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' CornerRadius='6'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <Trigger Property='IsMouseOver' Value='True'>
                                                <Setter Property='Background' Value='#2563EB'/>
                                            </Trigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                            <Button Content='Print' Height='35' Margin='4,0,0,0' Background='White' BorderBrush='#3B82F6' BorderThickness='2' Foreground='#3B82F6' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
                                    Command='{Binding DataContext.PrintExchangeCommand, RelativeSource={RelativeSource AncestorType=ItemsControl}}'
                                    CommandParameter='{Binding ExchangeId}'>
                                <Button.Style>
                                    <Style TargetType='Button'>
                                        <Setter Property='Template'>
                                            <Setter.Value>
                                                <ControlTemplate TargetType='Button'>
                                                    <Border Background='{TemplateBinding Background}' 
                                                            BorderBrush='{TemplateBinding BorderBrush}'
                                                            BorderThickness='{TemplateBinding BorderThickness}'
                                                            CornerRadius='6'>
                                                        <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
                                                    </Border>
                                                </ControlTemplate>
                                            </Setter.Value>
                                        </Setter>
                                        <Style.Triggers>
                                            <Trigger Property='IsMouseOver' Value='True'>
                                                <Setter Property='Background' Value='#F3E8FF'/>
                                            </Trigger>
                                        </Style.Triggers>
                                    </Style>
                                </Button.Style>
                            </Button>
                        </UniformGrid>
                    </Grid>
                </Border>
            </DataTemplate>";
        
        return (DataTemplate)System.Windows.Markup.XamlReader.Parse(xaml);
    }
    
    /// <summary>
    /// Print professional bill using QuestPDF
    /// </summary>
    private void PrintBill(TransactionDto transaction)
    {
        try
        {
            // Use QuestPDF for professional receipt generation and printing
            var pdfPrinter = new QuestPdfReceiptPrinter(_activeCurrencyService);

            // Get company information from settings or use defaults
            string companyName = "CHRONO POS"; // TODO: Get from settings/database
            string? companyAddress = null; // TODO: Get from settings/database
            string? companyPhone = null; // TODO: Get from settings/database
            string? gstNo = null; // TODO: Get from settings/database

            // Convert TransactionProducts to CartItemModel for the printer
            var cartItems = transaction.TransactionProducts.Select(p => new CartItemModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName ?? "Unknown",
                ProductUnitId = p.ProductUnitId,
                Quantity = (int)Math.Round(p.Quantity), // Convert decimal to int
                UnitPrice = p.SellingPrice,
                TotalPrice = p.Quantity * p.SellingPrice
            }).ToList();

            // Get customer info if available
            CustomerDto? customer = null;
            if (transaction.CustomerId.HasValue)
            {
                try
                {
                    customer = _customerService.GetByIdAsync(transaction.CustomerId.Value).Result;
                }
                catch { /* Ignore if customer not found */ }
            }

            // Create table DTO from transaction data if available
            RestaurantTableDto? table = null;
            if (transaction.TableId.HasValue && !string.IsNullOrEmpty(transaction.TableNumber))
            {
                table = new RestaurantTableDto
                {
                    Id = transaction.TableId.Value,
                    TableNumber = transaction.TableNumber
                };
            }

            // Calculate amounts
            decimal subtotal = transaction.TransactionProducts.Sum(p => p.Quantity * p.SellingPrice);
            decimal discount = transaction.TotalDiscount;
            decimal taxPercent = transaction.Vat;
            decimal taxAmount = transaction.TotalVat;
            decimal serviceCharge = 0m; // TODO: Get service charge from transaction if stored
            decimal total = transaction.TotalAmount;

            // Generate PDF and auto-print to thermal printer
            string pdfPath = pdfPrinter.GenerateAndPrintReceipt(
                transaction: transaction,
                items: cartItems,
                customer: customer,
                table: table,
                subtotal: subtotal,
                discount: discount,
                taxPercent: taxPercent,
                taxAmount: taxAmount,
                serviceCharge: serviceCharge,
                total: total,
                companyName: companyName,
                companyAddress: companyAddress,
                companyPhone: companyPhone,
                gstNo: gstNo
            );

            // Success - no MessageBox needed, printing happens silently
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error printing bill: {ex.Message}\n\nThe transaction was saved successfully.", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #region Translation and Language Support

    private async Task LoadTranslationsAsync()
    {
        TransactionTitleLabel = await _localizationService.GetTranslationAsync("transaction_title");
        SalesTabLabel = await _localizationService.GetTranslationAsync("sales_tab");
        RefundTabLabel = await _localizationService.GetTranslationAsync("refund_tab");
        ExchangeTabLabel = await _localizationService.GetTranslationAsync("exchange_tab");
        SearchPlaceholderLabel = await _localizationService.GetTranslationAsync("search_placeholder");
        CreateNewTransactionLabel = await _localizationService.GetTranslationAsync("create_new_transaction");
        
        InvoiceLabel = await _localizationService.GetTranslationAsync("invoice_label");
        CustomerLabel = await _localizationService.GetTranslationAsync("customer_label");
        TableLabel = await _localizationService.GetTranslationAsync("table_label");
        ItemsLabel = await _localizationService.GetTranslationAsync("items_label");
        TotalLabel = await _localizationService.GetTranslationAsync("total_label");
        PaidLabel = await _localizationService.GetTranslationAsync("paid_label");
        RemainingLabel = await _localizationService.GetTranslationAsync("remaining_label");
        
        ViewDetailsLabel = await _localizationService.GetTranslationAsync("view_details");
        EditTransactionLabel = await _localizationService.GetTranslationAsync("edit_transaction");
        PayBillLabel = await _localizationService.GetTranslationAsync("pay_bill");
        PrintInvoiceLabel = await _localizationService.GetTranslationAsync("print_invoice");
        ProcessRefundLabel = await _localizationService.GetTranslationAsync("process_refund");
        ProcessExchangeLabel = await _localizationService.GetTranslationAsync("process_exchange");
        
        NoSalesTransactionsLabel = await _localizationService.GetTranslationAsync("no_sales_transactions");
        NoRefundTransactionsLabel = await _localizationService.GetTranslationAsync("no_refund_transactions");
        NoExchangeTransactionsLabel = await _localizationService.GetTranslationAsync("no_exchange_transactions");
        StartCreatingSalesLabel = await _localizationService.GetTranslationAsync("start_creating_sales");
        
        // Settle Popup Translations
        PaymentPopupTitleLabel = await _localizationService.GetTranslationAsync("payment_popup_title");
        PaymentMethodLabel = await _localizationService.GetTranslationAsync("payment_method_label");
        AmountPaidLabel = await _localizationService.GetTranslationAsync("amount_paid_label");
        CreditDaysLabel = await _localizationService.GetTranslationAsync("credit_days_label");
        CancelButtonLabel = await _localizationService.GetTranslationAsync("cancel_button");
        SaveSettleButtonLabel = await _localizationService.GetTranslationAsync("save_settle_button");
        CustomerPendingAmountLabel = await _localizationService.GetTranslationAsync("customer_pending_amount");
        RemainingAmountTransactionLabel = await _localizationService.GetTranslationAsync("remaining_amount_transaction");
        AlreadyPaidLabelText = await _localizationService.GetTranslationAsync("already_paid_label");
        SaleAmountLabel = await _localizationService.GetTranslationAsync("sale_amount_label");
        BillTotalLabel = await _localizationService.GetTranslationAsync("bill_total_label");
        CustomerPendingAddedLabel = await _localizationService.GetTranslationAsync("customer_pending_added");
        StoreCreditAvailableLabel = await _localizationService.GetTranslationAsync("store_credit_available");
        AddedToBillLabel = await _localizationService.GetTranslationAsync("added_to_bill");
        DeductedFromBillLabel = await _localizationService.GetTranslationAsync("deducted_from_bill");
    }

    private async void OnLanguageChanged(object? sender, string languageCode)
    {
        await LoadTranslationsAsync();
        // Refresh the current tab to apply translations
        switch (CurrentTab)
        {
            case "Sales":
                SwitchToSales();
                break;
            case "Refund":
                SwitchToRefund();
                break;
            case "Exchange":
                SwitchToExchange();
                break;
        }
    }

    private void OnLayoutDirectionChanged(LayoutDirection direction)
    {
        var newDirection = direction == LayoutDirection.RightToLeft
            ? FlowDirection.RightToLeft
            : FlowDirection.LeftToRight;
        
        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
        {
            CurrentFlowDirection = newDirection;
            Console.WriteLine($"[TransactionViewModel] FlowDirection changed to: {CurrentFlowDirection}");
        });
    }

    public void Cleanup()
    {
        _localizationService.LanguageChanged -= OnLanguageChanged;
        _layoutDirectionService.DirectionChanged -= OnLayoutDirectionChanged;
        _timerUpdateTimer?.Stop();
    }

    #endregion
}

// Models
public partial class TransactionCardModel : ObservableObject
{
    private IActiveCurrencyService? _currencyService;
    
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public ObservableCollection<TransactionItemModel> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string StatusLower => Status.ToLower();
    public int TransactionId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string ReservationInfo { get; set; } = string.Empty; // For showing reservation details
    public int TotalItemCount { get; set; }
    public bool ShowViewAllButton => TotalItemCount > 2;
    public ObservableCollection<TransactionItemModel> DisplayItems { get; set; } = new();
    
    // Payment info for partial/pending payments
    public decimal AmountCreditRemaining { get; set; }
    public string CreditRemainingDisplay => AmountCreditRemaining > 0 && _currencyService != null 
        ? $"Remaining: {_currencyService.FormatPrice(AmountCreditRemaining)}" 
        : string.Empty;
    public bool ShowCreditRemaining => StatusLower == "partial_payment" || StatusLower == "pending_payment";
    
    public void SetCurrencyService(IActiveCurrencyService service)
    {
        _currencyService = service;
    }
    
    // Timer properties for active transactions
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    [ObservableProperty]
    private string timerDisplay = string.Empty;
    
    public bool ShowTimer => StatusLower == "draft" || StatusLower == "billed";
    
    // Reservation display
    public bool ShowReservation => !string.IsNullOrEmpty(ReservationInfo);
}

public class TransactionItemModel
{
    public string Quantity { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Modifiers { get; set; } = string.Empty;
    public bool HasModifiers => !string.IsNullOrEmpty(Modifiers);
}

public class RefundCardModel
{
    public int RefundId { get; set; }
    public string RefundNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string OriginalInvoice { get; set; } = string.Empty;
    public ObservableCollection<TransactionItemModel> Items { get; set; } = new();
    public ObservableCollection<TransactionItemModel> DisplayItems { get; set; } = new();
    public int TotalItemCount { get; set; }
    public bool ShowViewAllButton { get; set; }
    public decimal RefundAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
}

public class ExchangeCardModel
{
    public int ExchangeId { get; set; }
    public string ExchangeNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string OriginalInvoice { get; set; } = string.Empty;
    public ObservableCollection<ExchangeCardItemModel> ItemsReturned { get; set; } = new();
    public ObservableCollection<ExchangeCardItemModel> ItemsGiven { get; set; } = new();
    public ObservableCollection<ExchangeCardItemModel> DisplayItemsReturned { get; set; } = new();
    public ObservableCollection<ExchangeCardItemModel> DisplayItemsGiven { get; set; } = new();
    public int TotalReturnedCount { get; set; }
    public int TotalGivenCount { get; set; }
    public bool ShowViewAllButton => TotalReturnedCount > 2 || TotalGivenCount > 2;
    public decimal Difference { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
}

public class ExchangeCardItemModel
{
    public string Quantity { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string Modifiers { get; set; } = string.Empty;
    public bool HasModifiers => !string.IsNullOrEmpty(Modifiers);
}
