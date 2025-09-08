using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Repositories;
using ChronoPos.Domain.Interfaces;

class DatabaseChecker
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== ChronoPos Database Search Test ===");
            
            // Use the same connection string as the app
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var chronoPosPath = System.IO.Path.Combine(appDataPath, "ChronoPos");
            var databasePath = System.IO.Path.Combine(chronoPosPath, "chronopos.db");
            
            Console.WriteLine($"Database path: {databasePath}");
            Console.WriteLine($"Database exists: {System.IO.File.Exists(databasePath)}");
            
            // Create DbContext with the same configuration as the app
            var options = new DbContextOptionsBuilder<ChronoPosDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;
                
            using var context = new ChronoPosDbContext(options);
            
            // Test connection
            Console.WriteLine("\n1. Testing database connection...");
            var canConnect = await context.Database.CanConnectAsync();
            Console.WriteLine($"   Can connect: {canConnect}");
            
            // Count products
            Console.WriteLine("\n2. Counting products...");
            var totalProducts = await context.Products.CountAsync();
            Console.WriteLine($"   Total products: {totalProducts}");
            
            // List all products
            Console.WriteLine("\n3. All products in database:");
            var allProducts = await context.Products.Include(p => p.Category).ToListAsync();
            foreach (var product in allProducts)
            {
                Console.WriteLine($"   - ID: {product.Id}, Name: '{product.Name}', Code: '{product.Code}', Category: '{product.Category?.Name}'");
            }
            
            // Test search for exact match
            Console.WriteLine("\n4. Testing search functionality...");
            var repository = new ProductRepository(context);
            
            // Test 1: Search for "Vaeella" (exact match)
            Console.WriteLine("\n   Test 1: Searching for 'Vaeella'...");
            var results1 = await repository.SearchProductsAsync("Vaeella");
            var list1 = results1.ToList();
            Console.WriteLine($"   Results: {list1.Count}");
            foreach (var product in list1)
            {
                Console.WriteLine($"     - {product.Name} (ID: {product.Id})");
            }
            
            // Test 2: Search for "Vae" (partial match)
            Console.WriteLine("\n   Test 2: Searching for 'Vae'...");
            var results2 = await repository.SearchProductsAsync("Vae");
            var list2 = results2.ToList();
            Console.WriteLine($"   Results: {list2.Count}");
            foreach (var product in list2)
            {
                Console.WriteLine($"     - {product.Name} (ID: {product.Id})");
            }
            
            // Test 3: Search for "Pizza" (partial match)
            Console.WriteLine("\n   Test 3: Searching for 'Pizza'...");
            var results3 = await repository.SearchProductsAsync("Pizza");
            var list3 = results3.ToList();
            Console.WriteLine($"   Results: {list3.Count}");
            foreach (var product in list3)
            {
                Console.WriteLine($"     - {product.Name} (ID: {product.Id})");
            }
            
            // Test 4: Search for "Vanilla" (what user was trying)
            Console.WriteLine("\n   Test 4: Searching for 'Vanilla'...");
            var results4 = await repository.SearchProductsAsync("Vanilla");
            var list4 = results4.ToList();
            Console.WriteLine($"   Results: {list4.Count}");
            foreach (var product in list4)
            {
                Console.WriteLine($"     - {product.Name} (ID: {product.Id})");
            }
            
            // Test 5: Case-insensitive search
            Console.WriteLine("\n   Test 5: Searching for 'vaeella' (lowercase)...");
            var results5 = await repository.SearchProductsAsync("vaeella");
            var list5 = results5.ToList();
            Console.WriteLine($"   Results: {list5.Count}");
            foreach (var product in list5)
            {
                Console.WriteLine($"     - {product.Name} (ID: {product.Id})");
            }
            
            Console.WriteLine("\n=== Search Test Complete ===");
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
