using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Repositories;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("Testing database connection and products...");
            
            // Create configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
                
            // Get connection string
            var connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? "Server=localhost;Database=chronopos;Uid=root;Pwd=;";
            
            Console.WriteLine($"Using connection string: {connectionString}");
            
            // Create DbContext
            var options = new DbContextOptionsBuilder<ChronoPosDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .Options;
                
            using var context = new ChronoPosDbContext(options);
            
            // Test connection
            Console.WriteLine("Testing database connection...");
            await context.Database.EnsureCreatedAsync();
            
            // Count total products
            var totalProducts = await context.Products.CountAsync();
            Console.WriteLine($"Total products in database: {totalProducts}");
            
            // Get first 10 products
            var products = await context.Products.Take(10).ToListAsync();
            Console.WriteLine($"First {products.Count} products:");
            foreach (var product in products)
            {
                Console.WriteLine($"  - ID: {product.Id}, Name: '{product.Name}', SKU: '{product.SKU}'");
            }
            
            // Test search specifically for "Vanilla"
            Console.WriteLine("\nTesting search for 'Vanilla':");
            var repository = new ProductRepository(context);
            var searchResults = await repository.SearchProductsAsync("Vanilla");
            var resultsList = searchResults.ToList();
            
            Console.WriteLine($"Search returned {resultsList.Count} results:");
            foreach (var product in resultsList)
            {
                Console.WriteLine($"  - ID: {product.Id}, Name: '{product.Name}', SKU: '{product.SKU}'");
            }
            
            // Test search for partial matches
            Console.WriteLine("\nTesting search for 'Van':");
            var partialResults = await repository.SearchProductsAsync("Van");
            var partialList = partialResults.ToList();
            
            Console.WriteLine($"Partial search returned {partialList.Count} results:");
            foreach (var product in partialList)
            {
                Console.WriteLine($"  - ID: {product.Id}, Name: '{product.Name}', SKU: '{product.SKU}'");
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
