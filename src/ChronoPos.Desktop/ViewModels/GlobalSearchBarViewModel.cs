using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows.Input;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.DTOs;
using ChronoPos.Infrastructure.Services;

namespace ChronoPos.Desktop.ViewModels;

/// <summary>
/// ViewModel for the global search bar functionality
/// </summary>
public partial class GlobalSearchBarViewModel : ObservableObject
{
    private readonly IGlobalSearchService _globalSearchService;
    private readonly IDatabaseLocalizationService _databaseLocalizationService;
    private System.Timers.Timer? _searchDelayTimer;

    [ObservableProperty]
    private string searchQuery = string.Empty;

    [ObservableProperty]
    private string placeholderText = "Search products, customers, sales...";

    [ObservableProperty]
    private bool showSearchResults = false;

    [ObservableProperty]
    private bool hasSearchText = false;

    [ObservableProperty]
    private bool isSearching = false;

    [ObservableProperty]
    private string searchResultsHeader = "Search Results";

    [ObservableProperty]
    private string moreResultsText = "Press Enter to see all results";

    [ObservableProperty]
    private bool hasMoreResults = false;

    [ObservableProperty]
    private GlobalSearchResultDto? selectedSearchResult;

    [ObservableProperty]
    private ObservableCollection<GlobalSearchResultDto> searchResults = new();

    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand ShowAllResultsCommand { get; }

    // Events for navigation
    public event Action<GlobalSearchResultDto>? NavigateToResult;
    public event Action<string>? ShowAllResults;

    public GlobalSearchBarViewModel(
        IGlobalSearchService globalSearchService,
        IDatabaseLocalizationService databaseLocalizationService)
    {
        _globalSearchService = globalSearchService ?? throw new ArgumentNullException(nameof(globalSearchService));
        _databaseLocalizationService = databaseLocalizationService ?? throw new ArgumentNullException(nameof(databaseLocalizationService));

        SearchCommand = new AsyncRelayCommand(ExecuteSearchAsync);
        ClearSearchCommand = new RelayCommand(ClearSearch);
        ShowAllResultsCommand = new RelayCommand(ShowAllSearchResults);

        // Initialize search delay timer
        _searchDelayTimer = new System.Timers.Timer(300); // 300ms delay
        _searchDelayTimer.Elapsed += OnSearchDelayElapsed;
        _searchDelayTimer.AutoReset = false;

        // Initialize translations
        _ = InitializeTranslationsAsync();

        // Subscribe to language changes
        _databaseLocalizationService.LanguageChanged += OnLanguageChanged;
    }

    partial void OnSearchQueryChanged(string value)
    {
        HasSearchText = !string.IsNullOrWhiteSpace(value) && value != PlaceholderText;
        
        // Reset and restart the search delay timer
        _searchDelayTimer?.Stop();
        
        if (HasSearchText)
        {
            _searchDelayTimer?.Start();
        }
        else
        {
            ClearSearchResults();
        }
    }

    private async void OnSearchDelayElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        if (HasSearchText)
        {
            await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () => 
            {
                await PerformQuickSearchAsync();
            });
        }
    }

    private async Task PerformQuickSearchAsync()
    {
        try
        {
            IsSearching = true;
            var results = await _globalSearchService.GetQuickSearchAsync(SearchQuery, 5);
            
            SearchResults.Clear();
            foreach (var result in results)
            {
                SearchResults.Add(result);
            }

            HasMoreResults = results.Count >= 5;
            ShowSearchResults = SearchResults.Count > 0;
            
            // Update header with result count
            SearchResultsHeader = SearchResults.Count > 0 
                ? $"Found {SearchResults.Count} result{(SearchResults.Count != 1 ? "s" : "")}"
                : "No results found";
        }
        catch (Exception ex)
        {
            // Log error but don't show to user for quick search
            System.Diagnostics.Debug.WriteLine($"Quick search error: {ex.Message}");
            ClearSearchResults();
        }
        finally
        {
            IsSearching = false;
        }
    }

    private Task ExecuteSearchAsync()
    {
        if (!HasSearchText)
            return Task.CompletedTask;

        try
        {
            IsSearching = true;
            ShowSearchResults = false;

            // Navigate to full search results
            ShowAllResults?.Invoke(SearchQuery);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Search execution error: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
        
        return Task.CompletedTask;
    }

    public void ClearSearch()
    {
        SearchQuery = string.Empty;
        ClearSearchResults();
    }

    private void ClearSearchResults()
    {
        SearchResults.Clear();
        ShowSearchResults = false;
        HasMoreResults = false;
        SelectedSearchResult = null;
    }

    private void ShowAllSearchResults()
    {
        if (HasSearchText)
        {
            ShowAllResults?.Invoke(SearchQuery);
            ShowSearchResults = false;
        }
    }

    public void SelectNextResult()
    {
        if (SearchResults.Count == 0)
            return;

        var currentIndex = SelectedSearchResult != null ? SearchResults.IndexOf(SelectedSearchResult) : -1;
        var nextIndex = (currentIndex + 1) % SearchResults.Count;
        SelectedSearchResult = SearchResults[nextIndex];
    }

    public void SelectPreviousResult()
    {
        if (SearchResults.Count == 0)
            return;

        var currentIndex = SelectedSearchResult != null ? SearchResults.IndexOf(SelectedSearchResult) : 0;
        var previousIndex = currentIndex <= 0 ? SearchResults.Count - 1 : currentIndex - 1;
        SelectedSearchResult = SearchResults[previousIndex];
    }

    public void OpenSelectedResult()
    {
        if (SelectedSearchResult != null)
        {
            OpenSearchResult(SelectedSearchResult);
        }
        else if (SearchResults.Count > 0)
        {
            OpenSearchResult(SearchResults[0]);
        }
    }

    public void OpenSearchResult(object? result)
    {
        if (result is GlobalSearchResultDto searchResult)
        {
            NavigateToResult?.Invoke(searchResult);
            ShowSearchResults = false;
        }
    }

    private async Task InitializeTranslationsAsync()
    {
        try
        {
            PlaceholderText = await _databaseLocalizationService.GetTranslationAsync("UI_SEARCH_PLACEHOLDER", "Search products, customers, sales...");
            SearchResultsHeader = await _databaseLocalizationService.GetTranslationAsync("UI_SEARCH_RESULTS_HEADER", "Search Results");
            MoreResultsText = await _databaseLocalizationService.GetTranslationAsync("UI_SEARCH_MORE_RESULTS", "Press Enter to see all results");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Translation initialization error: {ex.Message}");
        }
    }

    private async void OnLanguageChanged(object? sender, string e)
    {
        await InitializeTranslationsAsync();
    }

    protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        
        // Handle property change notifications
        if (e.PropertyName == nameof(SearchQuery))
        {
            OnSearchQueryChanged(SearchQuery);
        }
    }

    public void Dispose()
    {
        _searchDelayTimer?.Stop();
        _searchDelayTimer?.Dispose();
        _databaseLocalizationService.LanguageChanged -= OnLanguageChanged;
    }
}