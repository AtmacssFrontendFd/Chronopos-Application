using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== Direct SQLite Database Test ===");
            
            // Use the same connection string as the app
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var chronoPosPath = System.IO.Path.Combine(appDataPath, "ChronoPos");
            var databasePath = System.IO.Path.Combine(chronoPosPath, "chronopos.db");
            
            Console.WriteLine($"Database path: {databasePath}");
            Console.WriteLine($"Database exists: {System.IO.File.Exists(databasePath)}");
            
            var connectionString = $"Data Source={databasePath}";
            
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            Console.WriteLine("\n1. Testing database connection...");
            Console.WriteLine($"   Connection state: {connection.State}");
            
            // Count products
            Console.WriteLine("\n2. Counting products...");
            using var countCommand = connection.CreateCommand();
            countCommand.CommandText = "SELECT COUNT(*) FROM Products";
            var totalProducts = await countCommand.ExecuteScalarAsync();
            Console.WriteLine($"   Total products: {totalProducts}");
            
            // List all products
            Console.WriteLine("\n3. All products in database:");
            using var listCommand = connection.CreateCommand();
            listCommand.CommandText = @"
                SELECT p.Id, p.Name, p.Code, p.SKU, c.Name as CategoryName 
                FROM Products p 
                LEFT JOIN Categories c ON p.CategoryId = c.Id 
                ORDER BY p.Id";
            
            using var reader = await listCommand.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var id = reader.GetInt32("Id");
                var name = reader.GetString("Name");
                var code = reader.IsDBNull("Code") ? "" : reader.GetString("Code");
                var sku = reader.IsDBNull("SKU") ? "" : reader.GetString("SKU");
                var categoryName = reader.IsDBNull("CategoryName") ? "" : reader.GetString("CategoryName");
                Console.WriteLine($"   - ID: {id}, Name: '{name}', Code: '{code}', SKU: '{sku}', Category: '{categoryName}'");
            }
            
            // Test search functionality
            Console.WriteLine("\n4. Testing search functionality...");
            
            // Test 1: Search for "Vaeella" (exact match)
            Console.WriteLine("\n   Test 1: Searching for 'Vaeella'...");
            await TestSearch(connection, "Vaeella");
            
            // Test 2: Search for "Vae" (partial match)
            Console.WriteLine("\n   Test 2: Searching for 'Vae'...");
            await TestSearch(connection, "Vae");
            
            // Test 3: Search for "Pizza" (partial match)
            Console.WriteLine("\n   Test 3: Searching for 'Pizza'...");
            await TestSearch(connection, "Pizza");
            
            // Test 4: Search for "Vanilla" (what user was trying)
            Console.WriteLine("\n   Test 4: Searching for 'Vanilla'...");
            await TestSearch(connection, "Vanilla");
            
            // Test 5: Case-insensitive search
            Console.WriteLine("\n   Test 5: Searching for 'vaeella' (lowercase)...");
            await TestSearch(connection, "vaeella");
            
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
    
    static async Task TestSearch(SqliteConnection connection, string searchTerm)
    {
        using var searchCommand = connection.CreateCommand();
        searchCommand.CommandText = @"
            SELECT p.Id, p.Name, p.Code, p.SKU 
            FROM Products p 
            LEFT JOIN ProductBarcodes pb ON p.Id = pb.ProductId
            WHERE p.Name LIKE @searchTerm 
               OR (p.SKU IS NOT NULL AND p.SKU LIKE @searchTerm)
               OR (pb.Value IS NOT NULL AND pb.Value LIKE @searchTerm)
            GROUP BY p.Id, p.Name, p.Code, p.SKU";
        
        searchCommand.Parameters.AddWithValue("@searchTerm", $"%{searchTerm}%");
        
        using var reader = await searchCommand.ExecuteReaderAsync();
        var count = 0;
        while (await reader.ReadAsync())
        {
            count++;
            var id = reader.GetInt32("Id");
            var name = reader.GetString("Name");
            var code = reader.IsDBNull("Code") ? "" : reader.GetString("Code");
            var sku = reader.IsDBNull("SKU") ? "" : reader.GetString("SKU");
            Console.WriteLine($"     - {name} (ID: {id}, Code: '{code}', SKU: '{sku}')");
        }
        Console.WriteLine($"   Results: {count}");
    }
}
