using ChronoPos.Application.DTOs;

namespace ChronoPos.Application.Interfaces;

/// <summary>
/// Service interface for ShopLocation operations
/// </summary>
public interface IShopLocationService
{
    /// <summary>
    /// Gets all shop locations
    /// </summary>
    /// <returns>Collection of shop location DTOs</returns>
    Task<IEnumerable<ShopLocationDto>> GetAllShopLocationsAsync();

    /// <summary>
    /// Gets active shop locations
    /// </summary>
    /// <returns>Collection of active shop location DTOs</returns>
    Task<IEnumerable<ShopLocationDto>> GetShopLocationsAsync();

    /// <summary>
    /// Gets shop location by ID
    /// </summary>
    /// <param name="id">Shop location ID</param>
    /// <returns>Shop location DTO if found</returns>
    Task<ShopLocationDto?> GetShopLocationByIdAsync(int id);

    /// <summary>
    /// Gets shop locations by type
    /// </summary>
    /// <param name="locationType">Location type</param>
    /// <returns>Collection of shop location DTOs</returns>
    Task<IEnumerable<ShopLocationDto>> GetShopLocationsByTypeAsync(string locationType);

    /// <summary>
    /// Gets shop locations by city
    /// </summary>
    /// <param name="city">City name</param>
    /// <returns>Collection of shop location DTOs</returns>
    Task<IEnumerable<ShopLocationDto>> GetShopLocationsByCityAsync(string city);
}