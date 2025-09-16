using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Implementation of global search service
/// </summary>
public class GlobalSearchService : IGlobalSearchService
{
    private readonly IChronoPosDbContext _context;

    public GlobalSearchService(IChronoPosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<GlobalSearchResponseDto> SearchAsync(GlobalSearchFilterDto filter)
    {
        var response = new GlobalSearchResponseDto
        {
            Query = filter.Query,
            Results = new List<GlobalSearchResultDto>(),
            ResultsByModule = new Dictionary<string, int>()
        };

        if (string.IsNullOrWhiteSpace(filter.Query))
            return response;

        var tasks = new List<Task<List<GlobalSearchResultDto>>>();

        // Search in different modules based on filter
        if (filter.IncludeProducts)
            tasks.Add(SearchProductsAsync(filter.Query, filter.MaxResults / 6));

        if (filter.IncludeCustomers)
            tasks.Add(SearchCustomersAsync(filter.Query, filter.MaxResults / 6));

        if (filter.IncludeSales)
            tasks.Add(SearchSalesAsync(filter.Query, filter.MaxResults / 6));

        if (filter.IncludeStock)
            tasks.Add(SearchStockAsync(filter.Query, filter.MaxResults / 6));

        if (filter.IncludeBrands)
            tasks.Add(SearchBrandsAsync(filter.Query, filter.MaxResults / 6));

        if (filter.IncludeCategories)
            tasks.Add(SearchCategoriesAsync(filter.Query, filter.MaxResults / 6));

        var results = await Task.WhenAll(tasks);

        // Combine and sort results
        var allResults = results.SelectMany(r => r).ToList();
        
        // Sort by relevance (simple scoring based on exact matches, then partial matches)
        var sortedResults = allResults
            .OrderByDescending(r => CalculateRelevanceScore(r, filter.Query))
            .Take(filter.MaxResults)
            .ToList();

        response.Results = sortedResults;
        response.TotalResults = sortedResults.Count;
        response.HasMoreResults = allResults.Count > filter.MaxResults;

        // Group by module for summary
        response.ResultsByModule = sortedResults
            .GroupBy(r => r.Module)
            .ToDictionary(g => g.Key, g => g.Count());

        return response;
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string query, int maxSuggestions = 10)
    {
        var suggestions = new HashSet<string>();

        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return suggestions.ToList();

        try
        {
            // Product name suggestions
            var productSuggestions = await _context.Products
                .Where(p => p.Name.Contains(query))
                .Select(p => p.Name)
                .Take(maxSuggestions / 3)
                .ToListAsync();
            
            foreach (var suggestion in productSuggestions)
                suggestions.Add(suggestion);

            // Brand name suggestions
            var brandSuggestions = await _context.Brands
                .Where(b => b.Name.Contains(query))
                .Select(b => b.Name)
                .Take(maxSuggestions / 3)
                .ToListAsync();
            
            foreach (var suggestion in brandSuggestions)
                suggestions.Add(suggestion);

            // Category name suggestions
            var categorySuggestions = await _context.Categories
                .Where(c => c.Name.Contains(query))
                .Select(c => c.Name)
                .Take(maxSuggestions / 3)
                .ToListAsync();
            
            foreach (var suggestion in categorySuggestions)
                suggestions.Add(suggestion);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error getting search suggestions: {ex.Message}");
        }

        return suggestions.Take(maxSuggestions).ToList();
    }

    public async Task<List<GlobalSearchResultDto>> GetQuickSearchAsync(string query, int maxResults = 5)
    {
        var filter = new GlobalSearchFilterDto
        {
            Query = query,
            MaxResults = maxResults,
            IncludeProducts = true,
            IncludeCustomers = true,
            IncludeSales = false, // Exclude sales for quick search
            IncludeStock = false, // Exclude stock for quick search
            IncludeBrands = true,
            IncludeCategories = true
        };

        var response = await SearchAsync(filter);
        return response.Results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchProductsAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var products = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Where(p => p.Name.Contains(query) || 
                           (p.Description != null && p.Description.Contains(query)) ||
                           (p.Code != null && p.Code.Contains(query)) ||
                           (p.Brand != null && p.Brand.Name.Contains(query)))
                .Take(maxResults)
                .ToListAsync();

            foreach (var product in products)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = product.Id,
                    Title = product.Name,
                    Description = $"Code: {product.Code} | {product.Brand?.Name ?? "No Brand"}",
                    Category = product.Category?.Name ?? "Uncategorized",
                    Module = "Products",
                    SearchType = "Product",
                    Data = product,
                    ImagePath = product.ProductImages?.FirstOrDefault()?.ImageUrl,
                    Price = (double?)product.Price,
                    Status = product.IsActive ? "Active" : "Inactive"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching products: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchCustomersAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var customers = await _context.Customers
                .Where(c => c.FirstName.Contains(query) || 
                           c.LastName.Contains(query) ||
                           c.Email.Contains(query) ||
                           c.PhoneNumber.Contains(query))
                .Take(maxResults)
                .ToListAsync();

            foreach (var customer in customers)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = customer.Id,
                    Title = $"{customer.FirstName} {customer.LastName}",
                    Description = $"Email: {customer.Email} | Phone: {customer.PhoneNumber}",
                    Category = "Customer",
                    Module = "Customers",
                    SearchType = "Customer",
                    Data = customer,
                    Status = "Active"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching customers: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchSalesAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var sales = await _context.Sales
                .Include(s => s.Customer)
                .Where(s => s.TransactionNumber.Contains(query) ||
                           (s.Customer != null && 
                            (s.Customer.FirstName.Contains(query) || s.Customer.LastName.Contains(query))))
                .Take(maxResults)
                .ToListAsync();

            foreach (var sale in sales)
            {
                var customerName = sale.Customer != null 
                    ? $"{sale.Customer.FirstName} {sale.Customer.LastName}" 
                    : "Walk-in";
                    
                results.Add(new GlobalSearchResultDto
                {
                    Id = sale.Id,
                    Title = $"Transaction #{sale.TransactionNumber}",
                    Description = $"Customer: {customerName} | Date: {sale.SaleDate:MMM dd, yyyy}",
                    Category = "Transaction",
                    Module = "Sales",
                    SearchType = "Sale",
                    Data = sale,
                    Price = (double)sale.TotalAmount,
                    Status = sale.Status.ToString()
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching sales: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchStockAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var stockAdjustments = await _context.StockAdjustments
                .Include(sa => sa.Reason)
                .Where(sa => sa.AdjustmentNo.Contains(query) ||
                            (sa.Reason != null && sa.Reason.Name.Contains(query)) ||
                            (sa.Remarks != null && sa.Remarks.Contains(query)))
                .Take(maxResults)
                .ToListAsync();

            foreach (var adjustment in stockAdjustments)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = adjustment.AdjustmentId,
                    Title = $"Stock Adjustment - {adjustment.AdjustmentNo}",
                    Description = $"Reason: {adjustment.Reason?.Name ?? "Unknown"} | Status: {adjustment.Status}",
                    Category = "Stock",
                    Module = "Stock",
                    SearchType = "StockAdjustment",
                    Data = adjustment,
                    Status = $"{adjustment.AdjustmentDate:MMM dd, yyyy}"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching stock: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchBrandsAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var brands = await _context.Brands
                .Where(b => b.Name.Contains(query) || 
                           (b.Description != null && b.Description.Contains(query)))
                .Take(maxResults)
                .ToListAsync();

            foreach (var brand in brands)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = brand.Id,
                    Title = brand.Name,
                    Description = brand.Description ?? "Brand",
                    Category = "Brand",
                    Module = "Brands",
                    SearchType = "Brand",
                    Data = brand,
                    Status = brand.IsActive ? "Active" : "Inactive"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching brands: {ex.Message}");
        }

        return results;
    }

    private async Task<List<GlobalSearchResultDto>> SearchCategoriesAsync(string query, int maxResults)
    {
        var results = new List<GlobalSearchResultDto>();

        try
        {
            var categories = await _context.Categories
                .Where(c => c.Name.Contains(query) || 
                           c.Description.Contains(query))
                .Take(maxResults)
                .ToListAsync();

            foreach (var category in categories)
            {
                results.Add(new GlobalSearchResultDto
                {
                    Id = category.Id,
                    Title = category.Name,
                    Description = category.Description ?? "Category",
                    Category = "Category",
                    Module = "Categories",
                    SearchType = "Category",
                    Data = category,
                    Status = category.IsActive ? "Active" : "Inactive"
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching categories: {ex.Message}");
        }

        return results;
    }

    private static double CalculateRelevanceScore(GlobalSearchResultDto result, string query)
    {
        double score = 0;
        var lowerQuery = query.ToLowerInvariant();
        var lowerTitle = result.Title.ToLowerInvariant();
        var lowerDescription = result.Description.ToLowerInvariant();

        // Exact title match gets highest score
        if (lowerTitle == lowerQuery)
            score += 100;
        // Title starts with query
        else if (lowerTitle.StartsWith(lowerQuery))
            score += 80;
        // Title contains query
        else if (lowerTitle.Contains(lowerQuery))
            score += 60;

        // Description matches
        if (lowerDescription.Contains(lowerQuery))
            score += 20;

        // Boost popular modules
        switch (result.Module.ToLowerInvariant())
        {
            case "products":
                score += 10;
                break;
            case "customers":
                score += 8;
                break;
            case "sales":
                score += 6;
                break;
        }

        return score;
    }
}