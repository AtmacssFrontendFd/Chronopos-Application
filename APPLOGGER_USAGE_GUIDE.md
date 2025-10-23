# AppLogger Usage Guide

The `AppLogger` class provides a comprehensive logging solution for the entire ChronoPos application. It can be used from any project within the solution and creates log files in a `logs` folder outside the `src` directory.

## Features

- **Cross-project compatibility**: Can be used from Desktop, Infrastructure, Domain, and Application projects
- **Automatic file organization**: Logs are saved outside `src` in a `logs` folder
- **Multiple log files**: Support for different log files based on purpose
- **Automatic caller information**: Captures file, method, and line number automatically
- **Thread-safe**: Safe to use from multiple threads
- **Performance logging**: Built-in performance measurement capabilities
- **Log rotation**: Automatic cleanup of old log files

## Basic Usage

### Simple Logging
```csharp
using ChronoPos.Application.Logging;

// Basic log message
AppLogger.Log("Application started successfully");

// Log with reason/context
AppLogger.Log("User login attempt", "Username: admin", "authentication");

// Log to specific file
AppLogger.Log("Database connection established", filename: "database");
```

### Specialized Logging Methods
```csharp
// Error logging
AppLogger.LogError("Failed to connect to database", exception, "Connection timeout", "database");

// Information logging
AppLogger.LogInfo("Processing order", "OrderId: 12345", "orders");

// Debug logging (only shows in debug builds)
AppLogger.LogDebug("Variable value", $"count = {count}", "debug");

// Warning logging
AppLogger.LogWarning("Low memory detected", "Available: 100MB", "system");

// Performance logging
AppLogger.LogPerformance("Database query", TimeSpan.FromMilliseconds(150), "Query: SELECT * FROM Products");
```

### Extension Methods
```csharp
// Log from any object
this.LogThis("Method started", "Processing user data", "user_operations");

// Error logging with object context
this.LogError("Operation failed", exception, "Invalid input data", "errors");

// Performance logging with automatic disposal
using (this.StartPerformanceLog("Heavy computation", "performance"))
{
    // Your code here
    // Duration will be logged automatically when disposed
}
```

## File Organization

The logger creates files in the following structure:
```
pos-software/
├── src/
│   ├── ChronoPos.Desktop/
│   ├── ChronoPos.Application/
│   └── ...
└── logs/
    ├── application_20250927.log       # General application logs
    ├── database_20250927.log          # Database-related logs
    ├── authentication_20250927.log    # Authentication logs
    ├── errors_20250927.log           # Error logs
    ├── performance_20250927.log      # Performance logs
    └── debug_20250927.log            # Debug logs
```

## Log Entry Format

Each log entry includes:
- Timestamp with milliseconds
- Source location (file.method:line)
- Message
- Optional reason/context

Example:
```
[2025-09-27 14:30:15.123] [StockManagementViewModel.LoadProducts:45] Loading products from database | Reason: User opened product list
```

## Usage Examples by Project

### In Desktop (ViewModels, Views)
```csharp
using ChronoPos.Application.Logging;

public class ProductViewModel
{
    public async Task LoadProducts()
    {
        AppLogger.LogInfo("Starting product load", "User requested product list", "ui");
        
        try
        {
            using (this.StartPerformanceLog("Product loading", "performance"))
            {
                var products = await _productService.GetAllAsync();
                AppLogger.LogInfo($"Loaded {products.Count} products", filename: "ui");
            }
        }
        catch (Exception ex)
        {
            this.LogError("Failed to load products", ex, "Service call failed", "errors");
        }
    }
}
```

### In Infrastructure (Services, Repositories)
```csharp
using ChronoPos.Application.Logging;

public class ProductRepository
{
    public async Task<Product> GetByIdAsync(int id)
    {
        AppLogger.LogDebug($"Fetching product with ID: {id}", filename: "database");
        
        try
        {
            using (this.StartPerformanceLog("Database query", "performance"))
            {
                var product = await _dbContext.Products.FindAsync(id);
                AppLogger.LogInfo($"Product found: {product?.Name}", $"ID: {id}", "database");
                return product;
            }
        }
        catch (Exception ex)
        {
            AppLogger.LogError("Database query failed", ex, $"Product ID: {id}", "database");
            throw;
        }
    }
}
```

### In Domain (Entities, Business Logic)
```csharp
using ChronoPos.Application.Logging;

public class Product
{
    public void UpdateStock(decimal quantity, string reason)
    {
        AppLogger.LogInfo($"Updating stock for {Name}", $"New quantity: {quantity}, Reason: {reason}", "inventory");
        
        if (quantity < 0)
        {
            AppLogger.LogWarning("Negative stock quantity", $"Product: {Name}, Quantity: {quantity}", "inventory");
        }
        
        Stock = quantity;
        this.LogThis("Stock updated successfully", $"Final quantity: {Stock}", "inventory");
    }
}
```

## Configuration

Configure the logger behavior:
```csharp
using ChronoPos.Application.Logging;

// Set minimum log level
LoggerConfig.MinimumLogLevel = LogLevel.Info;

// Configure output options
LoggerConfig.EnableConsoleOutput = true;
LoggerConfig.EnableFileOutput = true;

// Set log retention
LoggerConfig.LogRetentionDays = 30;

// Clean up old logs
AppLogger.CleanupOldLogs(30);
```

## Best Practices

1. **Use appropriate log levels**:
   - Debug: Detailed diagnostic information
   - Info: General application flow
   - Warning: Potentially harmful situations
   - Error: Error events that allow application to continue
   - Critical: Fatal errors that cause application termination

2. **Use meaningful file names**:
   - "database" for database operations
   - "authentication" for login/security
   - "ui" for user interface events
   - "business" for business logic
   - "performance" for timing information

3. **Include context in reasons**:
   ```csharp
   AppLogger.Log("Order processed", $"OrderId: {orderId}, Amount: {amount:C}", "orders");
   ```

4. **Use performance logging for slow operations**:
   ```csharp
   using (this.StartPerformanceLog("Complex calculation", "performance"))
   {
       // Long-running operation
   }
   ```

5. **Don't log sensitive information**:
   ```csharp
   // Good
   AppLogger.Log("User authenticated", $"UserId: {userId}", "auth");
   
   // Bad - don't log passwords
   AppLogger.Log("Login attempt", $"Password: {password}", "auth");
   ```

## Integration with Existing Code

The new logger can work alongside existing loggers. To migrate:

1. Replace `DesktopFileLogger.Log()` with `AppLogger.Log()`
2. Add appropriate filename parameters
3. Use specialized methods like `LogError()`, `LogInfo()` for better categorization
4. Add performance logging where needed

Example migration:
```csharp
// Old
DesktopFileLogger.Log("[StockManagement] Loading products");

// New
AppLogger.LogInfo("Loading products", "User opened stock management", "stock");
```