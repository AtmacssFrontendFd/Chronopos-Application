using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service for global search functionality across all modules
/// </summary>
public interface IGlobalSearchService
{
    /// <summary>
    /// Perform a global search across all modules
    /// </summary>
    /// <param name="filter">Search filter criteria</param>
    /// <returns>Global search response with results</returns>
    Task<GlobalSearchResponseDto> SearchAsync(GlobalSearchFilterDto filter);

    /// <summary>
    /// Get search suggestions for autocomplete
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="maxSuggestions">Maximum number of suggestions</param>
    /// <returns>List of search suggestions</returns>
    Task<List<string>> GetSearchSuggestionsAsync(string query, int maxSuggestions = 10);

    /// <summary>
    /// Get quick search results for instant search
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <returns>Quick search results</returns>
    Task<List<GlobalSearchResultDto>> GetQuickSearchAsync(string query, int maxResults = 5);
}