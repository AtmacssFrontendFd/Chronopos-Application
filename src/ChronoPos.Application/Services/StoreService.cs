using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Store operations
/// </summary>
public class StoreService : IStoreService
{
    private readonly IStoreRepository _storeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StoreService(IStoreRepository storeRepository, IUnitOfWork unitOfWork)
    {
        _storeRepository = storeRepository ?? throw new ArgumentNullException(nameof(storeRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all stores
    /// </summary>
    /// <returns>Collection of store DTOs</returns>
    public async Task<IEnumerable<StoreDto>> GetAllAsync()
    {
        var stores = await _storeRepository.GetAllAsync();
        return stores.Select(MapToDto);
    }

    /// <summary>
    /// Gets all active stores
    /// </summary>
    /// <returns>Collection of active store DTOs</returns>
    public async Task<IEnumerable<StoreDto>> GetActiveStoresAsync()
    {
        var stores = await _storeRepository.GetActiveStoresAsync();
        return stores.Select(MapToDto);
    }

    /// <summary>
    /// Gets store by ID
    /// </summary>
    /// <param name="id">Store ID</param>
    /// <returns>Store DTO if found</returns>
    public async Task<StoreDto?> GetByIdAsync(int id)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        return store != null ? MapToDto(store) : null;
    }

    /// <summary>
    /// Gets store by name
    /// </summary>
    /// <param name="name">Store name</param>
    /// <returns>Store DTO if found</returns>
    public async Task<StoreDto?> GetByNameAsync(string name)
    {
        var store = await _storeRepository.GetByNameAsync(name);
        return store != null ? MapToDto(store) : null;
    }

    /// <summary>
    /// Gets the default store
    /// </summary>
    /// <returns>Default store DTO if found</returns>
    public async Task<StoreDto?> GetDefaultStoreAsync()
    {
        var store = await _storeRepository.GetDefaultStoreAsync();
        return store != null ? MapToDto(store) : null;
    }

    /// <summary>
    /// Creates a new store
    /// </summary>
    /// <param name="createStoreDto">Store data</param>
    /// <returns>Created store DTO</returns>
    public async Task<StoreDto> CreateAsync(CreateStoreDto createStoreDto)
    {
        // Check if name already exists
        if (await _storeRepository.NameExistsAsync(createStoreDto.Name))
        {
            throw new InvalidOperationException($"Store with name '{createStoreDto.Name}' already exists");
        }

        // If this is set as default, ensure no other store is default
        if (createStoreDto.IsDefault)
        {
            var existingDefault = await _storeRepository.GetDefaultStoreAsync();
            if (existingDefault != null)
            {
                existingDefault.IsDefault = false;
                existingDefault.UpdatedAt = DateTime.UtcNow;
                await _storeRepository.UpdateAsync(existingDefault);
            }
        }

        var store = new Store
        {
            Name = createStoreDto.Name.Trim(),
            Address = createStoreDto.Address?.Trim(),
            PhoneNumber = createStoreDto.PhoneNumber?.Trim(),
            Email = createStoreDto.Email?.Trim(),
            ManagerName = createStoreDto.ManagerName?.Trim(),
            IsActive = createStoreDto.IsActive,
            IsDefault = createStoreDto.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _storeRepository.AddAsync(store);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(store);
    }

    /// <summary>
    /// Updates an existing store
    /// </summary>
    /// <param name="id">Store ID</param>
    /// <param name="updateStoreDto">Updated store data</param>
    /// <returns>Updated store DTO</returns>
    public async Task<StoreDto> UpdateAsync(int id, UpdateStoreDto updateStoreDto)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        if (store == null)
        {
            throw new ArgumentException($"Store with ID {id} not found");
        }

        // Check if name already exists (excluding current store)
        if (await _storeRepository.NameExistsAsync(updateStoreDto.Name, id))
        {
            throw new InvalidOperationException($"Store with name '{updateStoreDto.Name}' already exists");
        }

        // If this is set as default, ensure no other store is default
        if (updateStoreDto.IsDefault && !store.IsDefault)
        {
            var existingDefault = await _storeRepository.GetDefaultStoreAsync();
            if (existingDefault != null && existingDefault.Id != id)
            {
                existingDefault.IsDefault = false;
                existingDefault.UpdatedAt = DateTime.UtcNow;
                await _storeRepository.UpdateAsync(existingDefault);
            }
        }

        store.Name = updateStoreDto.Name.Trim();
        store.Address = updateStoreDto.Address?.Trim();
        store.PhoneNumber = updateStoreDto.PhoneNumber?.Trim();
        store.Email = updateStoreDto.Email?.Trim();
        store.ManagerName = updateStoreDto.ManagerName?.Trim();
        store.IsActive = updateStoreDto.IsActive;
        store.IsDefault = updateStoreDto.IsDefault;
        store.UpdatedAt = DateTime.UtcNow;

        await _storeRepository.UpdateAsync(store);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(store);
    }

    /// <summary>
    /// Deletes a store
    /// </summary>
    /// <param name="id">Store ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var store = await _storeRepository.GetByIdAsync(id);
        if (store == null)
        {
            return false;
        }

        // Check if store has associated stock levels or transactions
        if (store.StockLevels?.Any() == true)
        {
            throw new InvalidOperationException("Cannot delete store that has associated stock levels");
        }

        if (store.StockTransactions?.Any() == true)
        {
            throw new InvalidOperationException("Cannot delete store that has associated stock transactions");
        }

        // Don't allow deleting the default store
        if (store.IsDefault)
        {
            throw new InvalidOperationException("Cannot delete the default store");
        }

        await _storeRepository.DeleteAsync(store.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if store name exists
    /// </summary>
    /// <param name="name">Store name</param>
    /// <param name="excludeId">Store ID to exclude from check</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _storeRepository.NameExistsAsync(name, excludeId);
    }

    /// <summary>
    /// Sets a store as default
    /// </summary>
    /// <param name="id">Store ID to set as default</param>
    /// <returns>Updated store DTO</returns>
    public async Task<StoreDto> SetAsDefaultAsync(int id)
    {
        var store = await _storeRepository.SetAsDefaultAsync(id);
        return MapToDto(store);
    }

    /// <summary>
    /// Gets stores with their stock levels count
    /// </summary>
    /// <returns>Collection of stores with stock levels</returns>
    public async Task<IEnumerable<StoreDto>> GetStoresWithStockLevelsAsync()
    {
        var stores = await _storeRepository.GetStoresWithStockLevelsAsync();
        return stores.Select(s => 
        {
            var dto = MapToDto(s);
            return dto;
        });
    }

    /// <summary>
    /// Maps Store entity to StoreDto
    /// </summary>
    /// <param name="store">Store entity</param>
    /// <returns>Store DTO</returns>
    private static StoreDto MapToDto(Store store)
    {
        return new StoreDto
        {
            Id = store.Id,
            Name = store.Name,
            Address = store.Address,
            PhoneNumber = store.PhoneNumber,
            Email = store.Email,
            ManagerName = store.ManagerName,
            IsActive = store.IsActive,
            IsDefault = store.IsDefault,
            CreatedAt = store.CreatedAt,
            UpdatedAt = store.UpdatedAt
        };
    }
}