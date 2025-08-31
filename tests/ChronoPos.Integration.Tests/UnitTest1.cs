using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Repositories;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Services;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Integration.Tests;

/// <summary>
/// Integration tests for the ChronoPos application
/// </summary>
public class DatabaseIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly ChronoPosDbContext _context;

    public DatabaseIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Configure in-memory database for testing
        services.AddDbContext<ChronoPosDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Register services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProductService, ProductService>();
        
        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ChronoPosDbContext>();
        
        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ProductService_CreateProduct_ShouldPersistToDatabase()
    {
        // Arrange
        var productService = _serviceProvider.GetRequiredService<IProductService>();
        
        // Create a test category first
        var category = new Category
        {
            Name = "Test Category",
            Description = "Test Description"
        };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        var productDto = new ChronoPos.Application.DTOs.ProductDto
        {
            Name = "Test Product",
            Description = "Test Description",
            Sku = "TEST001",
            Price = 19.99m,
            StockQuantity = 10,
            CategoryId = category.Id
        };

        // Act
        var createdProduct = await productService.CreateProductAsync(productDto);

        // Assert
        Assert.NotNull(createdProduct);
        Assert.Equal("Test Product", createdProduct.Name);
        Assert.Equal("TEST001", createdProduct.Sku);
        Assert.Equal(19.99m, createdProduct.Price);
        
        // Verify it was actually saved to database
        var productInDb = await _context.Products.FindAsync(createdProduct.Id);
        Assert.NotNull(productInDb);
        Assert.Equal("Test Product", productInDb.Name);
    }

    [Fact]
    public async Task ProductService_GetAllProducts_ShouldReturnProducts()
    {
        // Arrange
        var productService = _serviceProvider.GetRequiredService<IProductService>();
        
        // Create test data
        var category = new Category { Name = "Electronics", Description = "Electronic items" };
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        
        var product1 = new Product
        {
            Name = "Laptop",
            SKU = "LAP001",
            Price = 999.99m,
            Stock = 5,
            CategoryId = category.Id
        };
        
        var product2 = new Product
        {
            Name = "Mouse",
            SKU = "MOU001", 
            Price = 29.99m,
            Stock = 20,
            CategoryId = category.Id
        };
        
        _context.Products.AddRange(product1, product2);
        await _context.SaveChangesAsync();

        // Act
        var products = await productService.GetAllProductsAsync();

        // Assert
        Assert.NotNull(products);
        Assert.Equal(2, products.Count());
        Assert.Contains(products, p => p.Name == "Laptop");
        Assert.Contains(products, p => p.Name == "Mouse");
    }

    [Fact]
    public async Task Database_CanCreateTables_ShouldSucceed()
    {
        // Arrange & Act
        var canConnect = await _context.Database.CanConnectAsync();
        
        // Assert
        Assert.True(canConnect);
        
        // Verify tables exist
        Assert.NotNull(_context.Products);
        Assert.NotNull(_context.Categories);
        Assert.NotNull(_context.Customers);
        Assert.NotNull(_context.Sales);
        Assert.NotNull(_context.SaleItems);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
