using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class CustomerTransactionsViewModel : ObservableObject
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly int _customerId;
        private readonly Action _goBackAction;

        [ObservableProperty]
        private string _customerName = string.Empty;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private Transaction? _selectedTransaction;

        [ObservableProperty]
        private int _totalTransactionCount;

        [ObservableProperty]
        private decimal _totalSalesAmount;

        [ObservableProperty]
        private decimal _totalCreditAmount;

        [ObservableProperty]
        private bool _isBusy;

        private ObservableCollection<Transaction> _transactions = new();
        public ObservableCollection<Transaction> Transactions
        {
            get => _transactions;
            set => SetProperty(ref _transactions, value);
        }

        private readonly ICollectionView _filteredTransactionsView;
        public ICollectionView FilteredTransactions => _filteredTransactionsView;

        public CustomerTransactionsViewModel(
            ITransactionRepository transactionRepository,
            int customerId,
            string customerName,
            Action goBackAction)
        {
            _transactionRepository = transactionRepository;
            _customerId = customerId;
            _customerName = customerName;
            _goBackAction = goBackAction;

            // Setup filtered view
            _filteredTransactionsView = CollectionViewSource.GetDefaultView(Transactions);
            _filteredTransactionsView.Filter = FilterTransactions;

            _ = LoadTransactionsAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            _filteredTransactionsView.Refresh();
        }

        private bool FilterTransactions(object obj)
        {
            if (obj is not Transaction transaction)
                return false;

            if (string.IsNullOrWhiteSpace(SearchText))
                return true;

            var searchLower = SearchText.ToLower();

            return (transaction.InvoiceNumber?.ToLower().Contains(searchLower) ?? false) ||
                   transaction.TotalAmount.ToString().Contains(searchLower) ||
                   transaction.Status.ToLower().Contains(searchLower) ||
                   transaction.SellingTime.ToString("dd/MM/yyyy").Contains(searchLower);
        }

        [RelayCommand]
        private async Task LoadTransactionsAsync()
        {
            try
            {
                IsBusy = true;

                var transactions = await _transactionRepository.GetByCustomerIdAsync(_customerId);
                
                // Sort by selling time descending (newest first)
                var sortedTransactions = transactions
                    .OrderByDescending(t => t.SellingTime)
                    .ToList();

                Transactions.Clear();
                foreach (var transaction in sortedTransactions)
                {
                    Transactions.Add(transaction);
                }

                // Calculate summary stats
                TotalTransactionCount = Transactions.Count;
                TotalSalesAmount = Transactions.Sum(t => t.TotalAmount);
                TotalCreditAmount = Transactions.Sum(t => t.AmountCreditRemaining);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine($"Error loading transactions: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadTransactionsAsync();
        }

        [RelayCommand]
        private void GoBack()
        {
            _goBackAction?.Invoke();
        }

        [RelayCommand]
        private void ClearSearch()
        {
            SearchText = string.Empty;
        }
    }
}
