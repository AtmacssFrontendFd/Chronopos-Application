using ChronoPos.Application.DTOs;
using ChronoPos.Application.Interfaces;
using ChronoPos.Domain.Entities;
using ChronoPos.Domain.Interfaces;

namespace ChronoPos.Application.Services;

/// <summary>
/// Service implementation for ProductImage operations
/// </summary>
public class ProductImageService : IProductImageService
{
    private readonly IProductImageRepository _productImageRepository;

    public ProductImageService(IProductImageRepository productImageRepository)
    {
        _productImageRepository = productImageRepository;
    }

    /// <summary>
    /// Gets all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of product image DTOs</returns>
    public async Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(int productId)
    {
        var images = await _productImageRepository.GetByProductIdAsync(productId);
        return images.Select(MapToDto);
    }

    /// <summary>
    /// Gets all images for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>Collection of product image DTOs</returns>
    public async Task<IEnumerable<ProductImageDto>> GetByProductUnitIdAsync(int productUnitId)
    {
        var images = await _productImageRepository.GetByProductUnitIdAsync(productUnitId);
        return images.Select(MapToDto);
    }

    /// <summary>
    /// Gets all images for a specific product group
    /// </summary>
    /// <param name="productGroupId">Product group ID</param>
    /// <returns>Collection of product image DTOs</returns>
    public async Task<IEnumerable<ProductImageDto>> GetByProductGroupIdAsync(int productGroupId)
    {
        var images = await _productImageRepository.GetByProductGroupIdAsync(productGroupId);
        return images.Select(MapToDto);
    }

    /// <summary>
    /// Gets the primary image for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Primary product image DTO if exists</returns>
    public async Task<ProductImageDto?> GetPrimaryImageAsync(int productId)
    {
        var image = await _productImageRepository.GetPrimaryImageAsync(productId);
        return image != null ? MapToDto(image) : null;
    }

    /// <summary>
    /// Gets product image by ID
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <returns>Product image DTO if found</returns>
    public async Task<ProductImageDto?> GetByIdAsync(int id)
    {
        var image = await _productImageRepository.GetByIdAsync(id);
        return image != null ? MapToDto(image) : null;
    }

    /// <summary>
    /// Gets all product images
    /// </summary>
    /// <returns>Collection of all product image DTOs</returns>
    public async Task<IEnumerable<ProductImageDto>> GetAllAsync()
    {
        var images = await _productImageRepository.GetAllAsync();
        return images.Select(MapToDto);
    }

    /// <summary>
    /// Creates a new product image
    /// </summary>
    /// <param name="createImageDto">Product image data</param>
    /// <returns>Created product image DTO</returns>
    public async Task<ProductImageDto> CreateAsync(CreateProductImageDto createImageDto)
    {
        try
        {
            // Get next sort order if not specified
            var sortOrder = createImageDto.SortOrder > 0 
                ? createImageDto.SortOrder 
                : await _productImageRepository.GetNextSortOrderAsync(createImageDto.ProductId);

            var productImage = new ProductImage
            {
                ProductId = createImageDto.ProductId,
                ProductUnitId = createImageDto.ProductUnitId,
                ProductGroupId = createImageDto.ProductGroupId,
                ImageUrl = createImageDto.ImageUrl.Trim(),
                AltText = createImageDto.AltText?.Trim() ?? string.Empty,
                SortOrder = sortOrder,
                IsPrimary = createImageDto.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // If this is set as primary, clear other primary flags
            if (createImageDto.IsPrimary)
            {
                await _productImageRepository.SetPrimaryImageAsync(createImageDto.ProductId, 0); // Clear all primaries first
            }

            var createdImage = await _productImageRepository.AddAsync(productImage);

            // Set as primary after creation if needed
            if (createImageDto.IsPrimary)
            {
                await _productImageRepository.SetPrimaryImageAsync(createImageDto.ProductId, createdImage.Id);
            }

            return MapToDto(createdImage);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create product image: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates multiple product images
    /// </summary>
    /// <param name="createImageDtos">Collection of product image data</param>
    /// <returns>Collection of created product image DTOs</returns>
    public async Task<IEnumerable<ProductImageDto>> CreateMultipleAsync(IEnumerable<CreateProductImageDto> createImageDtos)
    {
        var imageDtoList = createImageDtos.ToList();
        if (!imageDtoList.Any())
        {
            return Enumerable.Empty<ProductImageDto>();
        }

        var productId = imageDtoList.First().ProductId;
        var nextSortOrder = await _productImageRepository.GetNextSortOrderAsync(productId);
        var createdImages = new List<ProductImage>();

        foreach (var createImageDto in imageDtoList)
        {
            var sortOrder = createImageDto.SortOrder > 0 ? createImageDto.SortOrder : nextSortOrder++;

            var productImage = new ProductImage
            {
                ProductId = createImageDto.ProductId,
                ProductUnitId = createImageDto.ProductUnitId,
                ProductGroupId = createImageDto.ProductGroupId,
                ImageUrl = createImageDto.ImageUrl.Trim(),
                AltText = createImageDto.AltText?.Trim() ?? string.Empty,
                SortOrder = sortOrder,
                IsPrimary = createImageDto.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdImage = await _productImageRepository.AddAsync(productImage);
            createdImages.Add(createdImage);
        }

        // Handle primary image logic
        var primaryImage = createdImages.FirstOrDefault(i => i.IsPrimary);
        if (primaryImage != null)
        {
            await _productImageRepository.SetPrimaryImageAsync(productId, primaryImage.Id);
        }

        return createdImages.Select(MapToDto);
    }

    /// <summary>
    /// Updates an existing product image
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <param name="updateImageDto">Updated product image data</param>
    /// <returns>Updated product image DTO</returns>
    public async Task<ProductImageDto?> UpdateAsync(int id, UpdateProductImageDto updateImageDto)
    {
        var productImage = await _productImageRepository.GetByIdAsync(id);
        if (productImage == null)
        {
            return null;
        }

        productImage.ProductUnitId = updateImageDto.ProductUnitId;
        productImage.ProductGroupId = updateImageDto.ProductGroupId;
        productImage.ImageUrl = updateImageDto.ImageUrl.Trim();
        productImage.AltText = updateImageDto.AltText?.Trim() ?? string.Empty;
        productImage.SortOrder = updateImageDto.SortOrder;
        productImage.UpdatedAt = DateTime.UtcNow;

        // Handle primary image logic
        if (updateImageDto.IsPrimary && !productImage.IsPrimary)
        {
            await _productImageRepository.SetPrimaryImageAsync(productImage.ProductId, id);
        }
        else if (!updateImageDto.IsPrimary && productImage.IsPrimary)
        {
            // If removing primary flag, set first image as primary
            productImage.IsPrimary = false;
            var firstImage = (await _productImageRepository.GetByProductIdAsync(productImage.ProductId))
                .Where(i => i.Id != id)
                .OrderBy(i => i.SortOrder)
                .FirstOrDefault();

            if (firstImage != null)
            {
                await _productImageRepository.SetPrimaryImageAsync(productImage.ProductId, firstImage.Id);
            }
        }

        await _productImageRepository.UpdateAsync(productImage);
        return MapToDto(productImage);
    }

    /// <summary>
    /// Deletes a product image
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var productImage = await _productImageRepository.GetByIdAsync(id);
            if (productImage == null)
            {
                return false;
            }

            var wasPrimary = productImage.IsPrimary;
            var productId = productImage.ProductId;

            await _productImageRepository.DeleteAsync(productImage.Id);

            // If deleted image was primary, set first remaining image as primary
            if (wasPrimary)
            {
                var firstImage = (await _productImageRepository.GetByProductIdAsync(productId))
                    .OrderBy(i => i.SortOrder)
                    .FirstOrDefault();

                if (firstImage != null)
                {
                    await _productImageRepository.SetPrimaryImageAsync(productId, firstImage.Id);
                }
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteByProductIdAsync(int productId)
    {
        return await _productImageRepository.DeleteByProductIdAsync(productId);
    }

    /// <summary>
    /// Deletes all images for a specific product unit
    /// </summary>
    /// <param name="productUnitId">Product unit ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteByProductUnitIdAsync(int productUnitId)
    {
        return await _productImageRepository.DeleteByProductUnitIdAsync(productUnitId);
    }

    /// <summary>
    /// Deletes all images for a specific product group
    /// </summary>
    /// <param name="productGroupId">Product group ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteByProductGroupIdAsync(int productGroupId)
    {
        return await _productImageRepository.DeleteByProductGroupIdAsync(productGroupId);
    }

    /// <summary>
    /// Sets a specific image as primary
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID to set as primary</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> SetPrimaryImageAsync(int productId, int imageId)
    {
        return await _productImageRepository.SetPrimaryImageAsync(productId, imageId);
    }

    /// <summary>
    /// Reorders product images
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageOrders">Dictionary of image ID to new sort order</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> ReorderImagesAsync(int productId, Dictionary<int, int> imageOrders)
    {
        return await _productImageRepository.ReorderImagesAsync(productId, imageOrders);
    }

    /// <summary>
    /// Gets the next sort order for a product's images
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Next available sort order</returns>
    public async Task<int> GetNextSortOrderAsync(int productId)
    {
        return await _productImageRepository.GetNextSortOrderAsync(productId);
    }

    /// <summary>
    /// Maps ProductImage entity to ProductImageDto
    /// </summary>
    /// <param name="productImage">ProductImage entity</param>
    /// <returns>ProductImage DTO</returns>
    private static ProductImageDto MapToDto(ProductImage productImage)
    {
        return new ProductImageDto
        {
            Id = productImage.Id,
            ProductId = productImage.ProductId,
            ProductUnitId = productImage.ProductUnitId,
            ProductGroupId = productImage.ProductGroupId,
            ImageUrl = productImage.ImageUrl,
            AltText = productImage.AltText,
            SortOrder = productImage.SortOrder,
            IsPrimary = productImage.IsPrimary,
            CreatedAt = productImage.CreatedAt,
            UpdatedAt = productImage.UpdatedAt
        };
    }
}
