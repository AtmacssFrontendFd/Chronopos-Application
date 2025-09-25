using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for Brand operations
/// </summary>
public class BrandService : IBrandService
{
    private readonly IBrandRepository _brandRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BrandService(IBrandRepository brandRepository, IUnitOfWork unitOfWork)
    {
        _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    /// <summary>
    /// Gets all brands
    /// </summary>
    /// <returns>Collection of brand DTOs</returns>
    public async Task<IEnumerable<BrandDto>> GetAllAsync()
    {
        var brands = await _brandRepository.GetAllAsync();
        return brands.Select(MapToDto);
    }

    /// <summary>
    /// Gets all active brands
    /// </summary>
    /// <returns>Collection of active brand DTOs</returns>
    public async Task<IEnumerable<BrandDto>> GetActiveBrandsAsync()
    {
        var brands = await _brandRepository.GetActiveBrandsAsync();
        return brands.Select(MapToDto);
    }

    /// <summary>
    /// Gets brand by ID
    /// </summary>
    /// <param name="id">Brand ID</param>
    /// <returns>Brand DTO if found</returns>
    public async Task<BrandDto?> GetByIdAsync(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        return brand != null ? MapToDto(brand) : null;
    }

    /// <summary>
    /// Gets brand by name
    /// </summary>
    /// <param name="name">Brand name</param>
    /// <returns>Brand DTO if found</returns>
    public async Task<BrandDto?> GetByNameAsync(string name)
    {
        var brand = await _brandRepository.GetByNameAsync(name);
        return brand != null ? MapToDto(brand) : null;
    }

    /// <summary>
    /// Creates a new brand
    /// </summary>
    /// <param name="createBrandDto">Brand data</param>
    /// <returns>Created brand DTO</returns>
    public async Task<BrandDto> CreateAsync(CreateBrandDto createBrandDto)
    {
        // Check if name already exists
        if (await _brandRepository.NameExistsAsync(createBrandDto.Name))
        {
            throw new InvalidOperationException($"Brand with name '{createBrandDto.Name}' already exists");
        }

        var brand = new Brand
        {
            Name = createBrandDto.Name.Trim(),
            NameArabic = createBrandDto.NameArabic?.Trim(),
            Description = createBrandDto.Description?.Trim(),
            LogoUrl = createBrandDto.LogoUrl?.Trim(),
            IsActive = createBrandDto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _brandRepository.AddAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(brand);
    }

    /// <summary>
    /// Updates an existing brand
    /// </summary>
    /// <param name="id">Brand ID</param>
    /// <param name="updateBrandDto">Updated brand data</param>
    /// <returns>Updated brand DTO</returns>
    public async Task<BrandDto> UpdateAsync(int id, UpdateBrandDto updateBrandDto)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            throw new ArgumentException($"Brand with ID {id} not found");
        }

        // Check if name already exists (excluding current brand)
        if (await _brandRepository.NameExistsAsync(updateBrandDto.Name, id))
        {
            throw new InvalidOperationException($"Brand with name '{updateBrandDto.Name}' already exists");
        }

        brand.Name = updateBrandDto.Name.Trim();
        brand.NameArabic = updateBrandDto.NameArabic?.Trim();
        brand.Description = updateBrandDto.Description?.Trim();
        brand.LogoUrl = updateBrandDto.LogoUrl?.Trim();
        brand.IsActive = updateBrandDto.IsActive;
        brand.UpdatedAt = DateTime.UtcNow;

        await _brandRepository.UpdateAsync(brand);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(brand);
    }

    /// <summary>
    /// Deletes a brand
    /// </summary>
    /// <param name="id">Brand ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            return false;
        }

        // Check if brand has associated products
        if (brand.Products?.Any() == true)
        {
            throw new InvalidOperationException("Cannot delete brand that has associated products");
        }

        await _brandRepository.DeleteAsync(brand.Id);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Checks if brand name exists
    /// </summary>
    /// <param name="name">Brand name</param>
    /// <param name="excludeId">Brand ID to exclude from check</param>
    /// <returns>True if name exists</returns>
    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        return await _brandRepository.NameExistsAsync(name, excludeId);
    }

    /// <summary>
    /// Gets brands with their product count
    /// </summary>
    /// <returns>Collection of brands with product counts</returns>
    public async Task<IEnumerable<BrandDto>> GetBrandsWithProductCountAsync()
    {
        var brands = await _brandRepository.GetBrandsWithProductCountAsync();
        return brands.Select(b => 
        {
            var dto = MapToDto(b);
            dto.ProductCount = b.Products?.Count ?? 0;
            return dto;
        });
    }

    /// <summary>
    /// Maps Brand entity to BrandDto
    /// </summary>
    /// <param name="brand">Brand entity</param>
    /// <returns>Brand DTO</returns>
    private static BrandDto MapToDto(Brand brand)
    {
        return new BrandDto
        {
            Id = brand.Id,
            Name = brand.Name,
            NameArabic = brand.NameArabic,
            Description = brand.Description,
            LogoUrl = brand.LogoUrl,
            IsActive = brand.IsActive,
            ProductCount = brand.Products?.Count ?? 0,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };
    }
}
