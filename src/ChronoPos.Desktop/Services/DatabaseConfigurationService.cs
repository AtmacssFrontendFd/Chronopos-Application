using System;
using Microsoft.Data.Sqlite;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Services
{
    /// <summary>
    /// Configures SQLite database for optimal network sharing
    /// </summary>
    public static class DatabaseConfigurationService
    {
        /// <summary>
        /// Enable Write-Ahead Logging mode for better concurrency
        /// </summary>
        public static void EnableWalMode(string databasePath)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={databasePath}");
                connection.Open();

                using var cmd = connection.CreateCommand();
                
                // Enable WAL mode for better concurrent access
                cmd.CommandText = "PRAGMA journal_mode=WAL;";
                var result = cmd.ExecuteScalar()?.ToString();
                AppLogger.Log($"WAL mode enabled. Result: {result}", "DatabaseConfig", "database");

                // Set synchronous mode to NORMAL for better performance
                cmd.CommandText = "PRAGMA synchronous=NORMAL;";
                cmd.ExecuteNonQuery();
                AppLogger.Log("Synchronous mode set to NORMAL", "DatabaseConfig", "database");

                // Set busy timeout to 5 seconds for network delays
                cmd.CommandText = "PRAGMA busy_timeout=5000;";
                cmd.ExecuteNonQuery();
                AppLogger.Log("Busy timeout set to 5000ms", "DatabaseConfig", "database");

                // Set cache size (negative value = KB, -64000 = 64MB)
                cmd.CommandText = "PRAGMA cache_size=-64000;";
                cmd.ExecuteNonQuery();
                AppLogger.Log("Cache size set to 64MB", "DatabaseConfig", "database");

                // Enable shared cache mode
                cmd.CommandText = "PRAGMA cache=shared;";
                cmd.ExecuteNonQuery();
                AppLogger.Log("Shared cache mode enabled", "DatabaseConfig", "database");

                AppLogger.Log($"Database configuration completed for: {databasePath}", "DatabaseConfig", "database");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to configure database", ex, databasePath, "database");
                throw;
            }
        }

        /// <summary>
        /// Verify database connectivity
        /// </summary>
        public static bool TestConnection(string databasePath)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={databasePath}");
                connection.Open();
                
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT 1;";
                var result = cmd.ExecuteScalar();
                
                var isConnected = result?.ToString() == "1";
                AppLogger.Log($"Database connection test: {(isConnected ? "SUCCESS" : "FAILED")}", "DatabaseConfig", "database");
                return isConnected;
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Database connection test failed", ex, databasePath, "database");
                return false;
            }
        }

        /// <summary>
        /// Get database statistics
        /// </summary>
        public static string GetDatabaseInfo(string databasePath)
        {
            try
            {
                using var connection = new SqliteConnection($"Data Source={databasePath}");
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT 
                        (SELECT COUNT(*) FROM sqlite_master WHERE type='table') as TableCount,
                        (SELECT page_count * page_size / 1024.0 / 1024.0 FROM pragma_page_count(), pragma_page_size()) as SizeMB;
                ";
                
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var tableCount = reader.GetInt32(0);
                    var sizeMB = reader.GetDouble(1);
                    return $"Tables: {tableCount}, Size: {sizeMB:F2} MB";
                }

                return "Unable to read database info";
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to get database info", ex, databasePath, "database");
                return $"Error: {ex.Message}";
            }
        }
    }
}
