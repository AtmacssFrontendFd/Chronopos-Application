using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using System.IO;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Repositories;
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

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Get local app data path for database
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var chronoPosPath = Path.Combine(appDataPath, "ChronoPos");
                Directory.CreateDirectory(chronoPosPath); // Ensure directory exists
                var databasePath = Path.Combine(chronoPosPath, "chronopos.db");

                // Configure Entity Framework with SQLite
                services.AddDbContext<ChronoPosDbContext>(options =>
                    options.UseSqlite($"Data Source={databasePath}"));

                // Register repositories and unit of work
                services.AddScoped<IUnitOfWork, UnitOfWork>();

                // Register application services
                services.AddScoped<IProductService, ProductService>();

                // Register theme service
                services.AddSingleton<IThemeService, ThemeService>();

                // Register font service
                services.AddSingleton<IFontService, FontService>();

                // Register ViewModels
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<ProductsViewModel>();
                services.AddTransient<SalesViewModel>();
                services.AddTransient<CustomersViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Register Views
                services.AddTransient<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        // Initialize theme service
        var themeService = _host.Services.GetRequiredService<IThemeService>();
        themeService.LoadThemeFromSettings();

        // Initialize font service
        var fontService = _host.Services.GetRequiredService<IFontService>();
        fontService.LoadFontFromSettings();

        // Initialize database on startup
        await InitializeDatabaseAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        base.OnStartup(e);
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
