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

    public TransactionViewModel(
        ITransactionService transactionService,
        IRefundService refundService,
        IExchangeService exchangeService,
        IPaymentTypeService paymentTypeService,
        IReservationService reservationService,
        ICustomerService customerService,
        ICurrentUserService currentUserService,
        IActiveCurrencyService activeCurrencyService,
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
        _navigateToEditTransaction = navigateToEditTransaction;
        _navigateToPayBill = navigateToPayBill;
        _navigateToRefundTransaction = navigateToRefundTransaction;
        _navigateToExchangeTransaction = navigateToExchangeTransaction;
        _navigateToAddSales = navigateToAddSales;

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
                _currentUserService);

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
                Title = "Payment",
                Width = 480,
                Height = 550, // Increased for customer balance info and better spacing
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize,
                Background = Brushes.White
            };

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
                balanceInfo = $"\nCustomer Pending: ${customerBalanceAmount:N2} (Added to bill)";
            }
            else if (customerBalanceAmount < 0)
            {
                balanceInfo = $"\nStore Credit Available: ${Math.Abs(customerBalanceAmount):N2} (Deducted from bill)";
            }
            
            var totalLabel = new TextBlock
            {
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937")),
                TextWrapping = TextWrapping.Wrap
            };
            
            // Set text based on transaction status
            if (freshTransaction.Status == "partial_payment")
            {
                // For partial payment transactions, show customer pending as bill total
                var alreadyPaidFromSale = totalAmount - freshTransaction.AmountCreditRemaining;
                totalLabel.Text = $"Customer Pending Amount: ${customerBalanceAmount:N2}\n" +
                                  $"Remaining Amount of Transaction: ${remainingAmount:N2}\n" +
                                  $"Already Paid: ${alreadyPaidFromSale:N2}";
            }
            else if (alreadyPaid > 0)
            {
                // For transactions with some payment already made
                totalLabel.Text = $"Sale Amount: ${totalAmount:N2}{balanceInfo}\n" +
                                  $"Bill Total: ${billTotal:N2}\n" +
                                  $"Remaining: ${remainingAmount:N2}\n(Already Paid: ${alreadyPaid:N2})";
            }
            else
            {
                // For new transactions with no payment yet
                totalLabel.Text = $"Sale Amount: ${totalAmount:N2}{balanceInfo}\n" +
                                  $"Bill Total: ${billTotal:N2}";
            }
            
            Grid.SetRow(totalLabel, 0);
            grid.Children.Add(totalLabel);

            // Payment Method Label and ComboBox
            var paymentMethodLabel = new TextBlock
            {
                Text = "Payment Method:",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
            };

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
                Text = "Amount Paid:",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
            };

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
                Text = "Credit Days (for partial payment):",
                FontSize = 14,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"))
            };

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
                Content = "Cancel",
                Width = 100,
                Height = 40,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F2937")),
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            cancelButton.Click += (s, e) => paymentPopup.Close();

            var settleButton = new System.Windows.Controls.Button
            {
                Content = "Save & Settle",
                Width = 120,
                Height = 40,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };

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
                    Text = $"{product.TotalAmount:C2}",
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
                totalsStack.Children.Add(new TextBlock { Text = $"(Includes VAT: {refund.TotalVat:C2})", FontSize = 11, Foreground = Brushes.Gray });
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
            var refundNumber = $"R{refund.Id:D4}";
            
            var document = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Courier New"),
                FontSize = 11
            };

            // Header
            var headerPara = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            headerPara.Inlines.Add(new Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new Run("REFUND RECEIPT\n") { FontSize = 16, FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(headerPara);

            // Refund info
            var infoPara = new Paragraph { Margin = new Thickness(0, 10, 0, 10) };
            infoPara.Inlines.Add(new Run($"Refund #: {refundNumber}\n"));
            infoPara.Inlines.Add(new Run($"Original Transaction: #{refund.SellingTransactionId}\n"));
            infoPara.Inlines.Add(new Run($"Date: {refund.RefundTime:dd/MM/yyyy}\n"));
            infoPara.Inlines.Add(new Run($"Time: {refund.RefundTime:HH:mm:ss}\n"));
            infoPara.Inlines.Add(new Run($"Customer: {refund.CustomerName ?? "Walk-in"}\n"));
            document.Blocks.Add(infoPara);

            // Separator
            var separatorPara1 = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara1.Inlines.Add(new Run("───────────────────────────────────") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara1);

            // Items header
            var itemsHeaderPara = new Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            itemsHeaderPara.Inlines.Add(new Run("REFUNDED ITEMS\n") { FontWeight = FontWeights.Bold });
            itemsHeaderPara.Inlines.Add(new Run("───────────────────────────────────\n"));
            document.Blocks.Add(itemsHeaderPara);

            // Items
            decimal subtotal = 0;
            foreach (var product in refund.RefundProducts)
            {
                var itemPara = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                
                // Product name
                itemPara.Inlines.Add(new Run($"{product.ProductName ?? "Unknown"}\n"));
                
                // Quantity and price
                var qtyPriceText = $"  {product.TotalQuantityReturned:0.##} x {_activeCurrencyService.FormatPrice(product.TotalAmount / product.TotalQuantityReturned)}";
                var totalText = _activeCurrencyService.FormatPrice(product.TotalAmount);
                var spacing = new string(' ', Math.Max(0, 35 - qtyPriceText.Length - totalText.Length));
                itemPara.Inlines.Add(new Run($"{qtyPriceText}{spacing}{totalText}\n"));
                
                document.Blocks.Add(itemPara);
                subtotal += product.TotalAmount;
            }

            // Separator
            var separatorPara2 = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara2.Inlines.Add(new Run("───────────────────────────────────") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara2);

            // Totals
            var totalsPara = new Paragraph { Margin = new Thickness(0, 5, 0, 10) };
            
            // Subtotal
            var subtotalLine = $"Subtotal:";
            var subtotalAmount = _activeCurrencyService.FormatPrice(subtotal);
            var subtotalSpacing = new string(' ', Math.Max(0, 35 - subtotalLine.Length - subtotalAmount.Length));
            totalsPara.Inlines.Add(new Run($"{subtotalLine}{subtotalSpacing}{subtotalAmount}\n"));

            // VAT
            if (refund.TotalVat > 0)
            {
                var vatLine = $"VAT:";
                var vatAmount = _activeCurrencyService.FormatPrice(refund.TotalVat);
                var vatSpacing = new string(' ', Math.Max(0, 35 - vatLine.Length - vatAmount.Length));
                totalsPara.Inlines.Add(new Run($"{vatLine}{vatSpacing}{vatAmount}\n"));
            }

            // Total
            totalsPara.Inlines.Add(new Run("───────────────────────────────────\n") { FontWeight = FontWeights.Bold });
            var totalLine = $"TOTAL REFUND:";
            var totalAmount = _activeCurrencyService.FormatPrice(refund.TotalAmount);
            var totalSpacing = new string(' ', Math.Max(0, 35 - totalLine.Length - totalAmount.Length));
            totalsPara.Inlines.Add(new Run($"{totalLine}{totalSpacing}{totalAmount}\n") { FontWeight = FontWeights.Bold, FontSize = 13 });
            
            document.Blocks.Add(totalsPara);

            // Footer
            var footerPara = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            footerPara.Inlines.Add(new Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            footerPara.Inlines.Add(new Run("Amount refunded as per above\n"));
            footerPara.Inlines.Add(new Run("Thank you!\n"));
            footerPara.Inlines.Add(new Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(footerPara);

            // Print
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
                printDialog.PrintDocument(paginator, $"Refund Receipt - {refundNumber}");
                MessageBox.Show("Refund receipt printed successfully!", "Print Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
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
                    Text = $"{product.OldProductAmount:C2}",
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
                    Text = $"{product.NewProductAmount:C2}",
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
                Text = $"{Math.Abs(exchange.TotalExchangedAmount):C2} {(exchange.TotalExchangedAmount > 0 ? "(Customer pays)" : exchange.TotalExchangedAmount < 0 ? "(Customer refund)" : "(Even)")}",
                FontSize = 16, 
                FontWeight = FontWeights.Bold, 
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B45309")) 
            };
            Grid.SetColumn(differenceAmount, 1);
            differenceGrid.Children.Add(differenceAmount);
            differenceStack.Children.Add(differenceGrid);

            if (exchange.TotalExchangedVat > 0)
            {
                differenceStack.Children.Add(new TextBlock { Text = $"(Includes VAT: {exchange.TotalExchangedVat:C2})", FontSize = 11, Foreground = Brushes.Gray });
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
            var exchangeNumber = $"E{exchange.Id:D4}";
            
            var document = new FlowDocument
            {
                PagePadding = new Thickness(50),
                FontFamily = new FontFamily("Courier New"),
                FontSize = 11
            };

            // Header
            var headerPara = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 10)
            };
            headerPara.Inlines.Add(new Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new Run("EXCHANGE RECEIPT\n") { FontSize = 16, FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(headerPara);

            // Exchange info
            var infoPara = new Paragraph { Margin = new Thickness(0, 10, 0, 10) };
            infoPara.Inlines.Add(new Run($"Exchange #: {exchangeNumber}\n"));
            infoPara.Inlines.Add(new Run($"Original Transaction: #{exchange.SellingTransactionId}\n"));
            infoPara.Inlines.Add(new Run($"Date: {exchange.ExchangeTime:dd/MM/yyyy}\n"));
            infoPara.Inlines.Add(new Run($"Time: {exchange.ExchangeTime:HH:mm:ss}\n"));
            infoPara.Inlines.Add(new Run($"Customer: {exchange.CustomerName ?? "Walk-in"}\n"));
            document.Blocks.Add(infoPara);

            // Separator
            var separatorPara1 = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara1.Inlines.Add(new Run("───────────────────────────────────"));
            document.Blocks.Add(separatorPara1);

            // Items Returned
            var returnedHeader = new Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            returnedHeader.Inlines.Add(new Run("ITEMS RETURNED\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(returnedHeader);

            foreach (var product in exchange.ExchangeProducts.Where(p => p.OldProductName != null))
            {
                var productPara = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                productPara.Inlines.Add(new Run($"{product.OldProductName}\n"));
                productPara.Inlines.Add(new Run($"  Qty: {product.ReturnedQuantity:0.##}"));
                productPara.Inlines.Add(new Run(_activeCurrencyService.FormatPrice(product.OldProductAmount).PadLeft(35 - $"  Qty: {product.ReturnedQuantity:0.##}".Length)) { FontWeight = FontWeights.Bold });
                document.Blocks.Add(productPara);
            }

            // Separator
            var separatorPara2 = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara2.Inlines.Add(new Run("───────────────────────────────────"));
            document.Blocks.Add(separatorPara2);

            // Items Given
            var givenHeader = new Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            givenHeader.Inlines.Add(new Run("ITEMS GIVEN\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(givenHeader);

            foreach (var product in exchange.ExchangeProducts.Where(p => p.NewProductName != null))
            {
                var productPara = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                productPara.Inlines.Add(new Run($"{product.NewProductName}\n"));
                productPara.Inlines.Add(new Run($"  Qty: {product.NewQuantity:0.##}"));
                productPara.Inlines.Add(new Run(_activeCurrencyService.FormatPrice(product.NewProductAmount).PadLeft(35 - $"  Qty: {product.NewQuantity:0.##}".Length)) { FontWeight = FontWeights.Bold });
                document.Blocks.Add(productPara);
            }

            // Separator
            var separatorPara3 = new Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara3.Inlines.Add(new Run("═══════════════════════════════════") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara3);

            // Total difference
            var totalPara = new Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            totalPara.Inlines.Add(new Run("PRICE DIFFERENCE:"));
            totalPara.Inlines.Add(new Run(_activeCurrencyService.FormatPrice(Math.Abs(exchange.TotalExchangedAmount)).PadLeft(18)) { FontWeight = FontWeights.Bold, FontSize = 13 });
            totalPara.Inlines.Add(new Run($"\n({(exchange.TotalExchangedAmount > 0 ? "Customer pays" : exchange.TotalExchangedAmount < 0 ? "Customer refund" : "Even exchange")})") { FontSize = 9 });
            document.Blocks.Add(totalPara);

            // Footer
            var footerPara = new Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 15, 0, 0)
            };
            footerPara.Inlines.Add(new Run("───────────────────────────────────\n"));
            footerPara.Inlines.Add(new Run("Thank you for your business!\n"));
            footerPara.Inlines.Add(new Run("───────────────────────────────────"));
            document.Blocks.Add(footerPara);

            // Print the document
            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
                printDialog.PrintDocument(paginator, "Exchange Receipt");
            }
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
                        Price = totalPrice.ToString("C2")
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
                        Price = product.TotalAmount.ToString("C2"),
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
                // Group returned items by product
                var returnedGroups = exchange.ExchangeProducts
                    .Where(p => p.OldProductName != null)
                    .GroupBy(p => new { p.OldProductName, p.OldProductAmount })
                    .Select(g => new ExchangeCardItemModel
                    {
                        Quantity = $"{g.Sum(x => x.ReturnedQuantity)}x",
                        ItemName = g.Key.OldProductName ?? "",
                        Price = _activeCurrencyService.FormatPrice(g.Key.OldProductAmount)
                    });
                
                foreach (var item in returnedGroups)
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
                        Price = exchangeProduct.OldProductAmount.ToString("C2"),
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
                        Price = exchangeProduct.NewProductAmount.ToString("C2"),
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
        var itemsControl = new ItemsControl
        {
            ItemsSource = SalesTransactions
        };

        // Wrap panel for card layout
        var wrapPanelFactory = new FrameworkElementFactory(typeof(WrapPanel));
        wrapPanelFactory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
        
        itemsControl.ItemsPanel = new ItemsPanelTemplate(wrapPanelFactory);
        itemsControl.ItemTemplate = CreateSalesCardTemplate();

        return itemsControl;
    }

    private DataTemplate CreateSalesCardTemplate()
    {
        var xaml = @"
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <Border Background='White' BorderBrush='#E5E7EB' BorderThickness='1' CornerRadius='12' Padding='16' Margin='0,0,15,15' Width='340' Height='Auto' Cursor='Hand'>
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
                            <Border Background='#F59E0B' CornerRadius='8' Padding='10,5' HorizontalAlignment='Left'>
                                <TextBlock Text='{Binding OrderNumber}' FontSize='13' FontWeight='Bold' Foreground='White'/>
                            </Border>
                            <Border Background='{Binding StatusColor}' CornerRadius='6' Padding='8,4' HorizontalAlignment='Right'>
                                <TextBlock Text='{Binding Status}' FontSize='10' FontWeight='SemiBold' Foreground='White'/>
                            </Border>
                        </Grid>
                        
                        <!-- Customer Info -->
                        <StackPanel Grid.Row='1' Margin='0,12,0,0'>
                            <TextBlock Text='{Binding CustomerName}' FontSize='15' FontWeight='SemiBold' Foreground='#1F2937'/>
                            <TextBlock FontSize='11' Foreground='#6B7280' Margin='0,4,0,0'>
                                <Run Text='{Binding Date}'/> - <Run Text='{Binding Time}'/>
                            </TextBlock>
                            <Grid Margin='0,4,0,0'>
                                <TextBlock FontSize='12' Foreground='#3B82F6' FontWeight='Medium' HorizontalAlignment='Left'>
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
                                    <Border Background='#F9FAFB' BorderBrush='#E5E7EB' BorderThickness='1' CornerRadius='6' Padding='8,6' Margin='0,0,0,4'>
                                        <Grid>
                                            <Grid.ColumnDefinitions>
                                                <ColumnDefinition Width='Auto'/>
                                                <ColumnDefinition Width='*'/>
                                                <ColumnDefinition Width='Auto'/>
                                            </Grid.ColumnDefinitions>
                                            <TextBlock Grid.Column='0' FontSize='10' FontWeight='Bold' Foreground='#F59E0B' Margin='0,0,6,0'>
                                                <Run Text='{Binding Quantity}'/><Run Text='x'/>
                                            </TextBlock>
                                            <TextBlock Grid.Column='1' Text='{Binding ItemName}' FontSize='10' Foreground='#374151' TextTrimming='CharacterEllipsis'/>
                                            <TextBlock Grid.Column='2' Text='{Binding Price}' FontSize='10' FontWeight='SemiBold' Foreground='#1F2937'/>
                                        </Grid>
                                    </Border>
                                </DataTemplate>
                            </ItemsControl.ItemTemplate>
                        </ItemsControl>
                        
                        <!-- Total -->
                        <Grid Grid.Row='5' Margin='0,4,0,0'>
                            <TextBlock Text='Subtotal' FontSize='13' FontWeight='SemiBold' Foreground='#1F2937'/>
                            <TextBlock Text='{Binding Subtotal, Converter={StaticResource CurrencyPriceConverter}}' FontSize='15' FontWeight='Bold' Foreground='#1F2937' HorizontalAlignment='Right'/>
                        </Grid>
                        
                        <!-- Action Buttons - Status Based -->
                        <UniformGrid Grid.Row='7' Columns='2' HorizontalAlignment='Stretch'>
                            <!-- Save & Print Button - for draft/billed/hold/pending/partial -->
                            <Button Content='Save &amp; Print' Height='35' Margin='0,0,4,0' Background='#3B82F6' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
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
                            <Button Content='Settle' Height='35' Margin='4,0,0,0' Background='#10B981' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
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
                            <Button Content='Refund' Height='35' Margin='0,0,4,0' Background='#EF4444' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
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
                            <Button Content='Exchange' Height='35' Margin='4,0,0,0' Background='#60A5FA' Foreground='White' BorderThickness='0' FontWeight='SemiBold' FontSize='11' Cursor='Hand'
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
        var itemsControl = new ItemsControl
        {
            ItemsSource = RefundTransactions
        };

        // Wrap panel for card layout
        var wrapPanelFactory = new FrameworkElementFactory(typeof(WrapPanel));
        wrapPanelFactory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
        
        itemsControl.ItemsPanel = new ItemsPanelTemplate(wrapPanelFactory);
        itemsControl.ItemTemplate = CreateRefundCardTemplate();

        return itemsControl;
    }

    private DataTemplate CreateRefundCardTemplate()
    {
        var xaml = @"
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <Border Background='White' BorderBrush='#E5E7EB' BorderThickness='1' CornerRadius='12' Padding='16' Margin='0,0,15,15' Width='340' Height='Auto' Cursor='Hand'>
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
                            <Border Background='#EF4444' CornerRadius='8' Padding='10,5' HorizontalAlignment='Left'>
                                <TextBlock Text='{Binding RefundNumber}' FontSize='13' FontWeight='Bold' Foreground='White'/>
                            </Border>
                            <Border Background='{Binding StatusColor}' CornerRadius='6' Padding='8,4' HorizontalAlignment='Right'>
                                <TextBlock Text='{Binding Status}' FontSize='10' FontWeight='SemiBold' Foreground='White'/>
                            </Border>
                        </Grid>
                        
                        <!-- Customer Info -->
                        <StackPanel Grid.Row='1' Margin='0,12,0,0'>
                            <TextBlock Text='{Binding CustomerName}' FontSize='15' FontWeight='SemiBold' Foreground='#1F2937'/>
                            <TextBlock FontSize='11' Foreground='#6B7280' Margin='0,4,0,0'>
                                <Run Text='{Binding Date}'/> - <Run Text='{Binding Time}'/>
                            </TextBlock>
                            <TextBlock FontSize='12' Foreground='#EF4444' FontWeight='Medium' Margin='0,4,0,0'>
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
        var itemsControl = new ItemsControl
        {
            ItemsSource = ExchangeTransactions
        };

        // Wrap panel for card layout
        var wrapPanelFactory = new FrameworkElementFactory(typeof(WrapPanel));
        wrapPanelFactory.SetValue(WrapPanel.OrientationProperty, Orientation.Horizontal);
        
        itemsControl.ItemsPanel = new ItemsPanelTemplate(wrapPanelFactory);
        itemsControl.ItemTemplate = CreateExchangeCardTemplate();

        return itemsControl;
    }

    private DataTemplate CreateExchangeCardTemplate()
    {
        var xaml = @"
            <DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <Border Background='White' BorderBrush='#E5E7EB' BorderThickness='1' CornerRadius='12' Padding='16' Margin='0,0,15,15' Width='340' Height='Auto' Cursor='Hand'>
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
                            <Border Background='#3B82F6' CornerRadius='8' Padding='10,5' HorizontalAlignment='Left'>
                                <TextBlock Text='{Binding ExchangeNumber}' FontSize='13' FontWeight='Bold' Foreground='White'/>
                            </Border>
                            <Border Background='{Binding StatusColor}' CornerRadius='6' Padding='8,4' HorizontalAlignment='Right'>
                                <TextBlock Text='{Binding Status}' FontSize='10' FontWeight='SemiBold' Foreground='White'/>
                            </Border>
                        </Grid>
                        
                        <!-- Customer Info -->
                        <StackPanel Grid.Row='1' Margin='0,12,0,0'>
                            <TextBlock Text='{Binding CustomerName}' FontSize='15' FontWeight='SemiBold' Foreground='#1F2937'/>
                            <TextBlock FontSize='11' Foreground='#6B7280' Margin='0,4,0,0'>
                                <Run Text='{Binding Date}'/> - <Run Text='{Binding Time}'/>
                            </TextBlock>
                            <TextBlock FontSize='12' Foreground='#3B82F6' FontWeight='Medium' Margin='0,4,0,0'>
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
    /// Print professional bill
    /// </summary>
    private void PrintBill(TransactionDto transaction)
    {
        try
        {
            var printDialog = new System.Windows.Controls.PrintDialog();
            
            // Create print document
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
            headerPara.Inlines.Add(new System.Windows.Documents.Run("SALES RECEIPT\n") { FontSize = 16, FontWeight = FontWeights.Bold });
            headerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(headerPara);

            // Bill Info
            var infoPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 10, 0, 10) };
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Invoice #: {transaction.InvoiceNumber ?? transaction.Id.ToString()}\n"));
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Date: {transaction.SellingTime:dd-MM-yyyy}\n"));
            infoPara.Inlines.Add(new System.Windows.Documents.Run($"Time: {transaction.SellingTime:HH:mm:ss}\n"));
            
            if (!string.IsNullOrEmpty(transaction.CustomerName))
            {
                infoPara.Inlines.Add(new System.Windows.Documents.Run($"Customer: {transaction.CustomerName}\n"));
            }
            else
            {
                infoPara.Inlines.Add(new System.Windows.Documents.Run("Customer: Walk-in\n"));
            }
            
            if (!string.IsNullOrEmpty(transaction.TableNumber))
            {
                infoPara.Inlines.Add(new System.Windows.Documents.Run($"Table: {transaction.TableNumber}\n"));
            }
            
            document.Blocks.Add(infoPara);

            // Separator
            var separatorPara1 = new System.Windows.Documents.Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara1.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara1);

            // Items Header
            var itemsHeaderPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 5) };
            itemsHeaderPara.Inlines.Add(new System.Windows.Documents.Run("ITEMS\n") { FontWeight = FontWeights.Bold });
            itemsHeaderPara.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────\n"));
            document.Blocks.Add(itemsHeaderPara);

            // Items
            foreach (var item in transaction.TransactionProducts)
            {
                var itemPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 2, 0, 2) };
                
                // Product name
                itemPara.Inlines.Add(new System.Windows.Documents.Run($"{item.ProductName}\n"));
                
                // Modifiers if any
                if (item.Modifiers != null && item.Modifiers.Any())
                {
                    var modifierText = "  + " + string.Join(", ", item.Modifiers.Select(m => m.ModifierName));
                    itemPara.Inlines.Add(new System.Windows.Documents.Run($"{modifierText}\n") { FontStyle = FontStyles.Italic, Foreground = Brushes.Gray });
                }
                
                // Quantity and price
                var qtyPriceText = $"  {item.Quantity:0.##} x ${item.SellingPrice:N2}";
                var totalText = $"${(item.Quantity * item.SellingPrice):N2}";
                var spacing = new string(' ', Math.Max(0, 35 - qtyPriceText.Length - totalText.Length));
                itemPara.Inlines.Add(new System.Windows.Documents.Run($"{qtyPriceText}{spacing}{totalText}\n"));
                
                document.Blocks.Add(itemPara);
            }

            // Separator
            var separatorPara2 = new System.Windows.Documents.Paragraph
            {
                Margin = new Thickness(0, 5, 0, 5),
                TextAlignment = TextAlignment.Center
            };
            separatorPara2.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(separatorPara2);

            // Totals
            var totalsPara = new System.Windows.Documents.Paragraph { Margin = new Thickness(0, 5, 0, 10) };
            
            // Subtotal
            var subtotalLine = $"Subtotal:";
            var subtotalAmount = $"${transaction.TotalAmount:N2}";
            var subtotalSpacing = new string(' ', Math.Max(0, 35 - subtotalLine.Length - subtotalAmount.Length));
            totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{subtotalLine}{subtotalSpacing}{subtotalAmount}\n"));
            
            if (transaction.TotalDiscount > 0)
            {
                var discountLine = $"Discount:";
                var discountAmount = $"-${transaction.TotalDiscount:N2}";
                var discountSpacing = new string(' ', Math.Max(0, 35 - discountLine.Length - discountAmount.Length));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{discountLine}{discountSpacing}{discountAmount}\n"));
            }
            
            if (transaction.Vat > 0)
            {
                var taxLine = $"Tax ({transaction.Vat}%):";
                var taxAmount = $"${transaction.TotalVat:N2}";
                var taxSpacing = new string(' ', Math.Max(0, 35 - taxLine.Length - taxAmount.Length));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{taxLine}{taxSpacing}{taxAmount}\n"));
            }
            
            // Get customer balance if customer exists
            decimal customerBalanceAmount = 0m;
            if (transaction.CustomerId.HasValue)
            {
                try
                {
                    var customer = _customerService.GetByIdAsync(transaction.CustomerId.Value).Result;
                    customerBalanceAmount = customer?.CustomerBalanceAmount ?? 0m;
                }
                catch
                {
                    // Ignore customer balance fetch errors
                }
            }
            
            // Show customer balance if applicable
            if (customerBalanceAmount != 0)
            {
                string balanceLabel = customerBalanceAmount > 0 ? "Previous Pending:" : "Store Credit:";
                var balanceLine = $"{balanceLabel}";
                var balanceAmount = customerBalanceAmount > 0 ? $"${customerBalanceAmount:N2}" : $"-${Math.Abs(customerBalanceAmount):N2}";
                var balanceSpacing = new string(' ', Math.Max(0, 35 - balanceLine.Length - balanceAmount.Length));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{balanceLine}{balanceSpacing}{balanceAmount}\n"));
            }
            
            // Total line separator
            totalsPara.Inlines.Add(new System.Windows.Documents.Run("───────────────────────────────────\n") { FontWeight = FontWeights.Bold });
            
            // Calculate bill total (sale + customer balance)
            decimal billTotal = transaction.TotalAmount + customerBalanceAmount;
            
            // Grand Total
            var totalLine = $"TOTAL:";
            var totalAmount = $"${billTotal:N2}";
            var totalSpacing = new string(' ', Math.Max(0, 35 - totalLine.Length - totalAmount.Length));
            totalsPara.Inlines.Add(new System.Windows.Documents.Run($"{totalLine}{totalSpacing}{totalAmount}\n") { FontWeight = FontWeights.Bold, FontSize = 13 });
            
            // Show payment status for billed transactions
            if (transaction.Status == "billed" && billTotal > 0)
            {
                totalsPara.Inlines.Add(new System.Windows.Documents.Run("\n"));
                totalsPara.Inlines.Add(new System.Windows.Documents.Run("STATUS: PENDING PAYMENT\n") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                totalsPara.Inlines.Add(new System.Windows.Documents.Run($"Amount Due: ${billTotal:N2}\n") { FontWeight = FontWeights.Bold });
            }
            
            document.Blocks.Add(totalsPara);

            // Footer
            var footerPara = new System.Windows.Documents.Paragraph
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };
            footerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            footerPara.Inlines.Add(new System.Windows.Documents.Run("Thank you for your business!\n"));
            footerPara.Inlines.Add(new System.Windows.Documents.Run("Please come again\n"));
            footerPara.Inlines.Add(new System.Windows.Documents.Run("═══════════════════════════════════\n") { FontWeight = FontWeights.Bold });
            document.Blocks.Add(footerPara);

            // Print
            var paginator = ((System.Windows.Documents.IDocumentPaginatorSource)document).DocumentPaginator;
            printDialog.PrintDocument(paginator, "Sales Receipt");
        }
        catch (Exception ex)
        {
            AppLogger.LogError($"Error printing bill: {ex.Message}");
            new MessageDialog("Print Error", $"Error printing bill: {ex.Message}\n\nThe transaction was saved successfully.", MessageDialog.MessageType.Warning).ShowDialog();
        }
    }
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
