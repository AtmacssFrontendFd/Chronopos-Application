using System;
using System.Data.SQLite;

class Program
{
    static void Main()
    {
        string dbPath = @"C:\Users\saswa\AppData\Local\ChronoPos\chronopos.db";
        string connectionString = $"Data Source={dbPath};Version=3;";
        
        try
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine("Connected to database successfully!");
                
                // Check tables
                using (var command = new SQLiteCommand("SELECT name FROM sqlite_master WHERE type='table';", connection))
                {
                    Console.WriteLine("\nTables in database:");
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"- {reader["name"]}");
                        }
                    }
                }
                
                // Check Customer table structure
                try
                {
                    using (var command = new SQLiteCommand("PRAGMA table_info(Customer);", connection))
                    {
                        Console.WriteLine("\nCustomer table columns:");
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Console.WriteLine($"- {reader["name"]} ({reader["type"]})");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking Customer table: {ex.Message}");
                }
                
                // Check Customer data
                try
                {
                    using (var command = new SQLiteCommand("SELECT COUNT(*) FROM Customer;", connection))
                    {
                        var count = command.ExecuteScalar();
                        Console.WriteLine($"\nNumber of customers: {count}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error counting customers: {ex.Message}");
                }
                
                // Check BusinessType table
                try
                {
                    using (var command = new SQLiteCommand("SELECT COUNT(*) FROM BusinessType;", connection))
                    {
                        var count = command.ExecuteScalar();
                        Console.WriteLine($"Number of business types: {count}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"BusinessType table may not exist: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}