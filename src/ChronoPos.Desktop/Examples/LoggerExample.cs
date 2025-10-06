using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Examples
{
    /// <summary>
    /// Example class demonstrating the new AppLogger usage
    /// </summary>
    public class LoggerExample
    {
        public void DemonstrateLogging()
        {
            // Basic logging to default file (application_YYYYMMDD.log)
            AppLogger.Log("Application started successfully");

            // Logging with reason/context
            AppLogger.Log("User authentication successful", "Username: admin", "authentication");

            // Logging to specific files
            AppLogger.Log("Database connection established", "Connection string: ***", "database");
            AppLogger.Log("UI component loaded", "Component: ProductGrid", "ui");

            // Specialized logging methods
            AppLogger.LogInfo("Information message", "Additional context", "application");
            AppLogger.LogWarning("Memory usage high", "Available: 100MB", "system");
            AppLogger.LogError("Failed to save data", null, "Database timeout", "errors");
            AppLogger.LogDebug("Debug information", "Variable state", "debug");

            // Performance logging
            using (var perfLogger = new PerformanceLogger("Heavy computation", "performance"))
            {
                // Simulate work
                System.Threading.Thread.Sleep(100);
                // Duration will be logged automatically when disposed
            }

            // Extension methods
            this.LogThis("Method started", "Processing data", "business");
            this.LogError("Operation failed", new System.Exception("Test exception"), "Invalid input", "errors");

            // Performance logging with object context
            using (this.StartPerformanceLog("Database query", "performance"))
            {
                // Simulate database work
                System.Threading.Thread.Sleep(50);
            }

            // SQL logging
            AppLogger.LogSql("SELECT * FROM Products WHERE Id = @id", new { id = 123 });

            // Separators for log organization
            AppLogger.LogSeparator("APPLICATION STARTUP", "application");
            AppLogger.LogSeparator(); // Just a line separator

            // Configuration example
            LoggerConfig.MinimumLogLevel = LogLevel.Info;
            LoggerConfig.EnableConsoleOutput = true;
            LoggerConfig.LogRetentionDays = 30;

            // Cleanup old logs (call this periodically)
            AppLogger.CleanupOldLogs(30);

            // Get log directory path
            var logPath = AppLogger.GetLogDirectory();
            AppLogger.LogInfo($"Logs are saved to: {logPath}", filename: "system");
        }
    }
}