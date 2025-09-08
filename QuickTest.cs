using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.IO;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Repositories;

class QuickTest
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== Quick Database Search Test ===");
            
            // Get database path (same as in App.xaml.cs)
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var chronoPosPath = Path.Combine(appDataPath, "ChronoPos");
            var databasePath = Path.Combine(chronoPosPath, "chronopos.db");
            
            Console.WriteLine($"Database path: {databasePath}");
            Console.WriteLine($"Database exists: {File.Exists(databasePath)}");
            
            // Configure DbContext same as application
            var options = new DbContextOptionsBuilder<ChronoPosDbContext>()
                .UseSqlite($"Data Source={databasePath}")
                .Options;
                
            using var context = new ChronoPosDbContext(options);
            
            // Test direct search
            Console.WriteLine("\n=== Testing Direct Search ===");
            
            var repository = new ProductRepository(context);
            
            // Test search for "Vae" (should find Vaeella Pizza)
            Console.WriteLine("\n1. Searching for 'Vae':");
            var results1 = await repository.SearchProductsAsync("Vae");
            Console.WriteLine($"Found {results1.Count()} results");
            foreach (var product in results1)
            {
                Console.WriteLine($"  - {product.Name} (ID: {product.Id})");
            }
            
            // Test search for "vae" (lowercase)
            Console.WriteLine("\n2. Searching for 'vae' (lowercase):");
            var results2 = await repository.SearchProductsAsync("vae");
            Console.WriteLine($"Found {results2.Count()} results");
            foreach (var product in results2)
            {
                Console.WriteLine($"  - {product.Name} (ID: {product.Id})");
            }
            
            // Test search for "Pizza"
            Console.WriteLine("\n3. Searching for 'Pizza':");
            var results3 = await repository.SearchProductsAsync("Pizza");
            Console.WriteLine($"Found {results3.Count()} results");
            foreach (var product in results3)
            {
                Console.WriteLine($"  - {product.Name} (ID: {product.Id})");
            }
            
            // Test search for "pizza" (lowercase)
            Console.WriteLine("\n4. Searching for 'pizza' (lowercase):");
            var results4 = await repository.SearchProductsAsync("pizza");
            Console.WriteLine($"Found {results4.Count()} results");
            foreach (var product in results4)
            {
                Console.WriteLine($"  - {product.Name} (ID: {product.Id})");
            }
            
            // Test search for "Vanilla"
            Console.WriteLine("\n5. Searching for 'Vanilla':");
            var results5 = await repository.SearchProductsAsync("Vanilla");
            Console.WriteLine($"Found {results5.Count()} results");
            foreach (var product in results5)
            {
                Console.WriteLine($"  - {product.Name} (ID: {product.Id})");
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
        
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }
}
