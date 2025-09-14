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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _logger;

    public ProductImageService(IUnitOfWork unitOfWork, ILoggingService logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Collection of product image DTOs</returns>
    public async Task<IEnumerable<ProductImageDto>> GetByProductIdAsync(int productId)
    {
        _logger.Log($"ProductImageService.GetByProductIdAsync called for product ID: {productId}");
        
    var images = await _unitOfWork.ProductImages.GetByProductIdAsync(productId);
        var imagesList = images.ToList();
        
        _logger.Log($"ProductImageRepository returned {imagesList.Count} images for product ID: {productId}");
        foreach (var img in imagesList)
        {
            _logger.Log($"  Image ID {img.Id}: URL='{img.ImageUrl}', IsPrimary={img.IsPrimary}, SortOrder={img.SortOrder}");
        }
        
        var dtos = imagesList.Select(MapToDto).ToList();
        _logger.Log($"Converted to {dtos.Count} DTOs");
        
        return dtos;
    }

    /// <summary>
    /// Gets the primary image for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Primary product image DTO if exists</returns>
    public async Task<ProductImageDto?> GetPrimaryImageAsync(int productId)
    {
    var image = await _unitOfWork.ProductImages.GetPrimaryImageAsync(productId);
        return image != null ? MapToDto(image) : null;
    }

    /// <summary>
    /// Gets product image by ID
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <returns>Product image DTO if found</returns>
    public async Task<ProductImageDto?> GetByIdAsync(int id)
    {
    var image = await _unitOfWork.ProductImages.GetByIdAsync(id);
        return image != null ? MapToDto(image) : null;
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
            _logger.Log($"ProductImageService.CreateAsync: Starting for ProductId={createImageDto.ProductId}, IsPrimary={createImageDto.IsPrimary}");
            
            // Get next sort order if not specified
            var sortOrder = createImageDto.SortOrder > 0 
                ? createImageDto.SortOrder 
                : await _unitOfWork.ProductImages.GetNextSortOrderAsync(createImageDto.ProductId);

            _logger.Log($"ProductImageService.CreateAsync: Sort order = {sortOrder}");

            var productImage = new ProductImage
            {
                ProductId = createImageDto.ProductId,
                ImageUrl = createImageDto.ImageUrl.Trim(),
                AltText = createImageDto.AltText.Trim(),
                SortOrder = sortOrder,
                IsPrimary = createImageDto.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // If this is set as primary, clear other primary flags
            if (createImageDto.IsPrimary)
            {
                _logger.Log($"ProductImageService.CreateAsync: Clearing other primary flags for ProductId={createImageDto.ProductId}");
                await _unitOfWork.ProductImages.SetPrimaryImageAsync(createImageDto.ProductId, 0); // Clear all primaries first
            }

            _logger.Log($"ProductImageService.CreateAsync: Adding image to repository");
            await _unitOfWork.ProductImages.AddAsync(productImage);
            
            _logger.Log($"ProductImageService.CreateAsync: Calling SaveChangesAsync");
            
            try
            {
                _logger.Log($"ProductImageService.CreateAsync: Entity state before save - Id: {productImage.Id}, ProductId: {productImage.ProductId}, ImageUrl: {productImage.ImageUrl}");
                var saveResult = await _unitOfWork.SaveChangesAsync();
                _logger.Log($"ProductImageService.CreateAsync: SaveChanges returned {saveResult} affected rows");
                _logger.Log($"ProductImageService.CreateAsync: Entity state after save - Id: {productImage.Id}, ProductId: {productImage.ProductId}");
            }
            catch (Exception saveEx)
            {
                _logger.LogError($"ProductImageService.CreateAsync: SaveChanges failed", saveEx);
                throw;
            }
            
            _logger.Log($"ProductImageService.CreateAsync: Image saved with ID={productImage.Id}");
            
            // Verify the entity was saved properly
            if (productImage.Id <= 0)
            {
                _logger.LogError($"ProductImageService.CreateAsync: Failed to save image - ID is still {productImage.Id}", new InvalidOperationException("Image not saved to database"));
                throw new InvalidOperationException("Failed to save product image to database");
            }

            // Set as primary after creation if needed - this will trigger another SaveChanges internally
            if (createImageDto.IsPrimary)
            {
                _logger.Log($"ProductImageService.CreateAsync: Setting image ID={productImage.Id} as primary (will call SaveChanges again)");
                await _unitOfWork.ProductImages.SetPrimaryImageAsync(createImageDto.ProductId, productImage.Id);
                _logger.Log($"ProductImageService.CreateAsync: Primary flag set successfully");
            }

            var result = MapToDto(productImage);
            _logger.Log($"ProductImageService.CreateAsync: Completed successfully, returning DTO with ID={result.Id}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"ProductImageService.CreateAsync failed for ProductId={createImageDto.ProductId}", ex);
            throw; // Re-throw the exception to be handled by caller
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
    var nextSortOrder = await _unitOfWork.ProductImages.GetNextSortOrderAsync(productId);
        var createdImages = new List<ProductImage>();

        foreach (var createImageDto in imageDtoList)
        {
            var sortOrder = createImageDto.SortOrder > 0 ? createImageDto.SortOrder : nextSortOrder++;

            var productImage = new ProductImage
            {
                ProductId = createImageDto.ProductId,
                ImageUrl = createImageDto.ImageUrl.Trim(),
                AltText = createImageDto.AltText.Trim(),
                SortOrder = sortOrder,
                IsPrimary = createImageDto.IsPrimary,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductImages.AddAsync(productImage);
            createdImages.Add(productImage);
        }

        await _unitOfWork.SaveChangesAsync();

        // Handle primary image logic
        var primaryImage = createdImages.FirstOrDefault(i => i.IsPrimary);
        if (primaryImage != null)
        {
            await _unitOfWork.ProductImages.SetPrimaryImageAsync(productId, primaryImage.Id);
        }

        return createdImages.Select(MapToDto);
    }

    /// <summary>
    /// Updates an existing product image
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <param name="updateImageDto">Updated product image data</param>
    /// <returns>Updated product image DTO</returns>
    public async Task<ProductImageDto> UpdateAsync(int id, UpdateProductImageDto updateImageDto)
    {
    var productImage = await _unitOfWork.ProductImages.GetByIdAsync(id);
        if (productImage == null)
        {
            throw new ArgumentException($"Product image with ID {id} not found");
        }

        productImage.ImageUrl = updateImageDto.ImageUrl.Trim();
        productImage.AltText = updateImageDto.AltText.Trim();
        productImage.SortOrder = updateImageDto.SortOrder;
        productImage.UpdatedAt = DateTime.UtcNow;

        // Handle primary image logic
        if (updateImageDto.IsPrimary && !productImage.IsPrimary)
        {
            await _unitOfWork.ProductImages.SetPrimaryImageAsync(productImage.ProductId, id);
        }
        else if (!updateImageDto.IsPrimary && productImage.IsPrimary)
        {
            // If removing primary flag, set first image as primary
            productImage.IsPrimary = false;
            var firstImage = (await _unitOfWork.ProductImages.GetByProductIdAsync(productImage.ProductId))
                .Where(i => i.Id != id)
                .OrderBy(i => i.SortOrder)
                .FirstOrDefault();

            if (firstImage != null)
            {
                await _unitOfWork.ProductImages.SetPrimaryImageAsync(productImage.ProductId, firstImage.Id);
            }
        }

    await _unitOfWork.ProductImages.UpdateAsync(productImage);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(productImage);
    }

    /// <summary>
    /// Deletes a product image
    /// </summary>
    /// <param name="id">Product image ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteAsync(int id)
    {
    var productImage = await _unitOfWork.ProductImages.GetByIdAsync(id);
        if (productImage == null)
        {
            return false;
        }

        var wasPrimary = productImage.IsPrimary;
        var productId = productImage.ProductId;

    await _unitOfWork.ProductImages.DeleteAsync(productImage.Id);
        await _unitOfWork.SaveChangesAsync();

        // If deleted image was primary, set first remaining image as primary
        if (wasPrimary)
        {
            var firstImage = (await _unitOfWork.ProductImages.GetByProductIdAsync(productId))
                .OrderBy(i => i.SortOrder)
                .FirstOrDefault();

            if (firstImage != null)
            {
                await _unitOfWork.ProductImages.SetPrimaryImageAsync(productId, firstImage.Id);
            }
        }

        return true;
    }

    /// <summary>
    /// Deletes all images for a specific product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteByProductIdAsync(int productId)
    {
    return await _unitOfWork.ProductImages.DeleteByProductIdAsync(productId);
    }

    /// <summary>
    /// Sets a specific image as primary
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageId">Image ID to set as primary</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> SetPrimaryImageAsync(int productId, int imageId)
    {
    return await _unitOfWork.ProductImages.SetPrimaryImageAsync(productId, imageId);
    }

    /// <summary>
    /// Reorders product images
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="imageOrders">Dictionary of image ID to new sort order</param>
    /// <returns>True if operation was successful</returns>
    public async Task<bool> ReorderImagesAsync(int productId, Dictionary<int, int> imageOrders)
    {
    return await _unitOfWork.ProductImages.ReorderImagesAsync(productId, imageOrders);
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
            ImageUrl = productImage.ImageUrl,
            AltText = productImage.AltText,
            SortOrder = productImage.SortOrder,
            IsPrimary = productImage.IsPrimary,
            CreatedAt = productImage.CreatedAt,
            UpdatedAt = productImage.UpdatedAt
        };
    }
}
