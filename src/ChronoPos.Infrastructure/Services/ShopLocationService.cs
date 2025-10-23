using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ChronoPos.Infrastructure.Services;

/// <summary>
/// Service implementation for ShopLocation operations
/// </summary>
public class ShopLocationService : IShopLocationService
{
    private readonly ChronoPosDbContext _context;

    public ShopLocationService(ChronoPosDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets all shop locations
    /// </summary>
    /// <returns>Collection of shop location DTOs</returns>
    public async Task<IEnumerable<ShopLocationDto>> GetAllShopLocationsAsync()
    {
        return await _context.ShopLocations
            .Select(sl => new ShopLocationDto
            {
                Id = sl.Id,
                LocationName = sl.LocationName,
                LocationType = sl.LocationType,
                City = sl.City,
                Status = sl.Status
            })
            .OrderBy(sl => sl.LocationName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets active shop locations
    /// </summary>
    /// <returns>Collection of active shop location DTOs</returns>
    public async Task<IEnumerable<ShopLocationDto>> GetShopLocationsAsync()
    {
        return await _context.ShopLocations
            .Where(sl => sl.Status == "Active" && sl.DeletedAt == null)
            .Select(sl => new ShopLocationDto
            {
                Id = sl.Id,
                LocationName = sl.LocationName,
                LocationType = sl.LocationType,
                City = sl.City,
                Status = sl.Status
            })
            .OrderBy(sl => sl.LocationName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets shop location by ID
    /// </summary>
    /// <param name="id">Shop location ID</param>
    /// <returns>Shop location DTO if found</returns>
    public async Task<ShopLocationDto?> GetShopLocationByIdAsync(int id)
    {
        var location = await _context.ShopLocations
            .Where(sl => sl.Id == id)
            .Select(sl => new ShopLocationDto
            {
                Id = sl.Id,
                LocationName = sl.LocationName,
                LocationType = sl.LocationType,
                City = sl.City,
                Status = sl.Status
            })
            .FirstOrDefaultAsync();

        return location;
    }

    /// <summary>
    /// Gets shop locations by type
    /// </summary>
    /// <param name="locationType">Location type</param>
    /// <returns>Collection of shop location DTOs</returns>
    public async Task<IEnumerable<ShopLocationDto>> GetShopLocationsByTypeAsync(string locationType)
    {
        return await _context.ShopLocations
            .Where(sl => sl.LocationType == locationType && sl.Status == "Active" && sl.DeletedAt == null)
            .Select(sl => new ShopLocationDto
            {
                Id = sl.Id,
                LocationName = sl.LocationName,
                LocationType = sl.LocationType,
                City = sl.City,
                Status = sl.Status
            })
            .OrderBy(sl => sl.LocationName)
            .ToListAsync();
    }

    /// <summary>
    /// Gets shop locations by city
    /// </summary>
    /// <param name="city">City name</param>
    /// <returns>Collection of shop location DTOs</returns>
    public async Task<IEnumerable<ShopLocationDto>> GetShopLocationsByCityAsync(string city)
    {
        return await _context.ShopLocations
            .Where(sl => sl.City == city && sl.Status == "Active" && sl.DeletedAt == null)
            .Select(sl => new ShopLocationDto
            {
                Id = sl.Id,
                LocationName = sl.LocationName,
                LocationType = sl.LocationType,
                City = sl.City,
                Status = sl.Status
            })
            .OrderBy(sl => sl.LocationName)
            .ToListAsync();
    }
}