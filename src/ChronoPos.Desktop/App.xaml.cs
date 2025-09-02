using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.IO;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Repositories;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Domain.Interfaces;
using ChronoPos.Application.Interfaces;
using ChronoPos.Application.Services;
using ChronoPos.Desktop.ViewModels;
using ChronoPos.Desktop.Views;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop;

/// <summary>
/// Main application class for ChronoPos Desktop POS System
/// </summary>
public partial class App : System.Windows.Application
{
    private readonly IHost _host;
    private static string _logFilePath = string.Empty;

    public App()
    {
        // Setup logging
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var chronoPosPath = Path.Combine(appDataPath, "ChronoPos");
        Directory.CreateDirectory(chronoPosPath);
        _logFilePath = Path.Combine(chronoPosPath, "app.log");
        
        LogMessage("=== Application Starting ===");
        
        try
        {
            LogMessage("Creating host...");
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    LogMessage("Configuring services...");
                    
                    // Get local app data path for database
                    var databasePath = Path.Combine(chronoPosPath, "chronopos.db");
                    LogMessage($"Database path: {databasePath}");

                    // Configure Entity Framework with SQLite
                    services.AddDbContext<ChronoPosDbContext>(options =>
                        options.UseSqlite($"Data Source={databasePath}"));
                    LogMessage("DbContext configured");

                    // Register repositories and unit of work
                    services.AddScoped<IUnitOfWork, UnitOfWork>();
                    LogMessage("UnitOfWork registered");

                    // Register application services
                    services.AddScoped<IProductService, ProductService>();
                    LogMessage("ProductService registered");

                    // Register theme service
                    services.AddSingleton<IThemeService, ThemeService>();
                    LogMessage("ThemeService registered");

                    // Register font service
                    services.AddSingleton<IFontService, FontService>();
                    LogMessage("FontService registered");

                    // Register localization service
                    services.AddSingleton<ILocalizationService, LocalizationService>();
                    LogMessage("LocalizationService registered");

                    // Register database localization service
                    services.AddScoped<IDatabaseLocalizationService, DatabaseLocalizationService>();
                    LogMessage("DatabaseLocalizationService registered");

                    // Register color scheme service
                    services.AddSingleton<IColorSchemeService, ColorSchemeService>();
                    LogMessage("ColorSchemeService registered");

                    // Register layout direction service
                    services.AddSingleton<ILayoutDirectionService, LayoutDirectionService>();
                    LogMessage("LayoutDirectionService registered");

                    // Register ViewModels
                    services.AddTransient<MainWindowViewModel>();
                    LogMessage("MainWindowViewModel registered");
                    services.AddTransient<ProductsViewModel>();
                    LogMessage("ProductsViewModel registered");
                    services.AddTransient<SalesViewModel>();
                    LogMessage("SalesViewModel registered");
                    services.AddTransient<CustomersViewModel>();
                    LogMessage("CustomersViewModel registered");
                    services.AddTransient<SettingsViewModel>();
                    LogMessage("SettingsViewModel registered");

                    // Register Views
                    services.AddTransient<MainWindow>();
                    LogMessage("MainWindow registered");
                    
                    LogMessage("All services configured successfully");
                })
                .Build();
            LogMessage("Host created successfully");
        }
        catch (Exception ex)
        {
            LogMessage($"Error creating host: {ex.Message}");
            LogMessage($"Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static void LogMessage(string message)
    {
        var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
        Console.WriteLine(logEntry);
        try
        {
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Ignore file write errors
        }
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        LogMessage("OnStartup called");
        try
        {
            LogMessage("Starting host...");
            await _host.StartAsync();
            LogMessage("Host started successfully");

            // Initialize theme service
            try 
            {
                LogMessage("Initializing theme service...");
                var themeService = _host.Services.GetRequiredService<IThemeService>();
                LogMessage("Theme service retrieved");
                themeService.LoadThemeFromSettings();
                LogMessage("Theme service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Theme service error: {ex.Message}");
                LogMessage($"Theme service stack trace: {ex.StackTrace}");
            }

            // Initialize font service
            try 
            {
                LogMessage("Initializing font service...");
                var fontService = _host.Services.GetRequiredService<IFontService>();
                LogMessage("Font service retrieved");
                fontService.LoadFontFromSettings();
                LogMessage("Font service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Font service error: {ex.Message}");
                LogMessage($"Font service stack trace: {ex.StackTrace}");
            }

            // Initialize localization service
            try 
            {
                LogMessage("Initializing localization service...");
                var localizationService = _host.Services.GetRequiredService<ILocalizationService>();
                LogMessage("Localization service retrieved");
                localizationService.LoadLanguageFromSettings();
                LogMessage("Localization service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Localization service error: {ex.Message}");
                LogMessage($"Localization service stack trace: {ex.StackTrace}");
            }

            // Initialize color scheme service
            try 
            {
                LogMessage("Initializing color scheme service...");
                var colorSchemeService = _host.Services.GetRequiredService<IColorSchemeService>();
                LogMessage("Color scheme service retrieved");
                colorSchemeService.LoadColorsFromSettings();
                LogMessage("Color scheme service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Color scheme service error: {ex.Message}");
                LogMessage($"Color scheme service stack trace: {ex.StackTrace}");
            }

            // Initialize layout direction service
            try 
            {
                LogMessage("Initializing layout direction service...");
                var layoutDirectionService = _host.Services.GetRequiredService<ILayoutDirectionService>();
                LogMessage("Layout direction service retrieved");
                layoutDirectionService.LoadDirectionFromSettings();
                LogMessage("Layout direction service initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"Layout direction service error: {ex.Message}");
                LogMessage($"Layout direction service stack trace: {ex.StackTrace}");
            }

            // Initialize database on startup
            LogMessage("Initializing database...");
            await InitializeDatabaseAsync();
            LogMessage("Database initialized successfully");

            LogMessage("Getting MainWindow...");
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            LogMessage("MainWindow retrieved, showing...");
            mainWindow.Show();
            LogMessage("MainWindow shown successfully");

            base.OnStartup(e);
            LogMessage("OnStartup completed successfully");
        }
        catch (Exception ex)
        {
            LogMessage($"Application startup failed: {ex.Message}");
            LogMessage($"Startup stack trace: {ex.StackTrace}");
            MessageBox.Show($"Application startup failed: {ex.Message}\n\nStack trace:\n{ex.StackTrace}", 
                          "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown(1);
        }
    }

    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var scope = _host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
            
            // Create database and apply any pending migrations
            await dbContext.Database.EnsureCreatedAsync();
            
            // Alternative: Use migrations for production
            // await dbContext.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            // Log error but don't crash the application
            MessageBox.Show($"Database initialization failed: {ex.Message}\nThe application will continue without database functionality.", 
                          "Database Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync();
        }

        base.OnExit(e);
    }
}
