using System.ComponentModel;

namespace ChronoPos.Application.DTOs;

/// <summary>
/// Represents a global search result
/// </summary>
public class GlobalSearchResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string SearchType { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string? ImagePath { get; set; }
    public double? Price { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Search filter for global search
/// </summary>
public class GlobalSearchFilterDto
{
    public string Query { get; set; } = string.Empty;
    public string[]? Modules { get; set; }
    public string[]? Categories { get; set; }
    public int MaxResults { get; set; } = 20;
    public bool IncludeProducts { get; set; } = true;
    public bool IncludeCustomers { get; set; } = true;
    public bool IncludeSales { get; set; } = true;
    public bool IncludeStock { get; set; } = true;
    public bool IncludeBrands { get; set; } = true;
    public bool IncludeCategories { get; set; } = true;
}

/// <summary>
/// Grouped search results
/// </summary>
public class GlobalSearchResponseDto
{
    public string Query { get; set; } = string.Empty;
    public int TotalResults { get; set; }
    public List<GlobalSearchResultDto> Results { get; set; } = new();
    public Dictionary<string, int> ResultsByModule { get; set; } = new();
    public bool HasMoreResults { get; set; }
}