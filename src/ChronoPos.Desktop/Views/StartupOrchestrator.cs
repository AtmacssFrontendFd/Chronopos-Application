using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Views.Dialogs;
using ChronoPos.Infrastructure;

namespace ChronoPos.Desktop.Views
{
    /// <summary>
    /// Orchestrates the application startup flow in the correct sequence
    /// </summary>
    public class StartupOrchestrator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Action<string> _log;
        private readonly Dispatcher _dispatcher;

        public StartupOrchestrator(IServiceProvider serviceProvider, Action<string> log, Dispatcher dispatcher)
        {
            _serviceProvider = serviceProvider;
            _log = log;
            _dispatcher = dispatcher;
        }

        public async Task<bool> RunStartupSequenceAsync()
        {
            try
            {
                _log("Starting startup sequence...");

                // Step 1: Check for valid license
                _log("Checking license status...");
                var licensingService = _serviceProvider.GetRequiredService<ILicensingService>();
                
                if (!licensingService.IsLicenseValid())
                {
                    _log("No valid license found, showing onboarding...");
                    
                    // Show onboarding on UI thread
                    bool? onboardingResult = null;
                    await _dispatcher.InvokeAsync(() =>
                    {
                        var onboardingWindow = _serviceProvider.GetRequiredService<OnboardingWindow>();
                        onboardingResult = onboardingWindow.ShowDialog();
                    });
                    
                    if (onboardingResult != true)
                    {
                        _log("Onboarding cancelled, exiting...");
                        return false;
                    }
                    
                    _log("Onboarding completed successfully");
                }
                else
                {
                    _log("Valid license found, skipping onboarding");
                }

                // Step 2: Check if admin user exists
                _log("Checking for admin user...");
                if (!await AdminUserExistsAsync())
                {
                    _log("No admin user found, showing create admin window...");
                    
                    // Show CreateAdminWindow on UI thread
                    bool adminCreated = false;
                    Exception? windowException = null;
                    
                    await _dispatcher.InvokeAsync(() =>
                    {
                        try
                        {
                            _log(">>> DISPATCHER: About to get CreateAdminWindow from service provider");
                            var createAdminWindow = _serviceProvider.GetRequiredService<CreateAdminWindow>();
                            _log($">>> DISPATCHER: CreateAdminWindow instance created: {createAdminWindow != null}");
                            
                            _log(">>> DISPATCHER: About to call ShowDialog()");
                            var result = createAdminWindow.ShowDialog();
                            _log($">>> DISPATCHER: ShowDialog() returned: {result}");
                            
                            adminCreated = result == true;
                            _log($">>> DISPATCHER: adminCreated flag set to: {adminCreated}");
                        }
                        catch (Exception ex)
                        {
                            windowException = ex;
                            _log($">>> DISPATCHER EXCEPTION: {ex.Message}");
                            _log($">>> DISPATCHER STACK TRACE: {ex.StackTrace}");
                        }
                    });
                    
                    _log(">>> DISPATCHER: InvokeAsync completed");
                    
                    if (windowException != null)
                    {
                        _log($"Exception occurred while showing CreateAdminWindow: {windowException.Message}");
                        return false;
                    }
                    
                    if (!adminCreated)
                    {
                        _log("Admin creation cancelled, exiting...");
                        return false;
                    }
                    
                    _log("Admin user created successfully");
                }
                else
                {
                    _log("Admin user already exists, skipping admin creation");
                }

                // Step 3: Show login window
                _log("Showing login window...");
                int loggedInUserId = 0;
                bool loginSuccess = false;
                Exception? loginException = null;
                
                await _dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        _log(">>> DISPATCHER: About to get LoginWindow from service provider");
                        var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                        _log($">>> DISPATCHER: LoginWindow instance created: {loginWindow != null}");
                        
                        _log(">>> DISPATCHER: About to call LoginWindow.ShowDialog()");
                        var result = loginWindow.ShowDialog();
                        _log($">>> DISPATCHER: LoginWindow.ShowDialog() returned: {result}");
                        
                        if (result == true)
                        {
                            loggedInUserId = loginWindow.LoggedInUserId;
                            loginSuccess = true;
                            _log($">>> DISPATCHER: User logged in successfully (User ID: {loggedInUserId})");
                        }
                        else
                        {
                            _log(">>> DISPATCHER: Login cancelled");
                        }
                    }
                    catch (Exception ex)
                    {
                        loginException = ex;
                        _log($">>> DISPATCHER LOGIN EXCEPTION: {ex.Message}");
                        _log($">>> DISPATCHER LOGIN STACK TRACE: {ex.StackTrace}");
                    }
                });
                
                _log(">>> DISPATCHER: LoginWindow InvokeAsync completed");
                
                if (loginException != null)
                {
                    _log($"Exception occurred while showing LoginWindow: {loginException.Message}");
                    return false;
                }
                
                if (!loginSuccess)
                {
                    _log("Login failed or cancelled, exiting...");
                    return false;
                }

                // Step 4: Show main window
                _log("Login successful, showing main window...");
                Exception? mainWindowException = null;
                
                await _dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        _log(">>> DISPATCHER: About to get MainWindow from service provider");
                        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                        _log($">>> DISPATCHER: MainWindow instance created: {mainWindow != null}");
                        
                        // Set as the application main window to prevent shutdown
                        var app = System.Windows.Application.Current as App;
                        if (app != null)
                        {
                            app.MainWindow = mainWindow;
                            _log(">>> DISPATCHER: Set MainWindow as Application.MainWindow");
                        }
                        
                        _log(">>> DISPATCHER: About to call MainWindow.Show()");
                        mainWindow.Show();
                        _log(">>> DISPATCHER: MainWindow.Show() completed successfully");
                    }
                    catch (Exception ex)
                    {
                        mainWindowException = ex;
                        _log($">>> DISPATCHER MAINWINDOW EXCEPTION: {ex.Message}");
                        _log($">>> DISPATCHER MAINWINDOW STACK TRACE: {ex.StackTrace}");
                    }
                });
                
                _log(">>> DISPATCHER: MainWindow InvokeAsync completed");
                
                if (mainWindowException != null)
                {
                    _log($"Exception occurred while showing MainWindow: {mainWindowException.Message}");
                    return false;
                }

                _log("Startup sequence completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                _log($"Startup sequence failed: {ex.Message}");
                _log($"Stack trace: {ex.StackTrace}");
                
                await _dispatcher.InvokeAsync(() =>
                {
                    new MessageDialog(
                        "Startup Error",
                        $"Application startup failed: {ex.Message}\n\nSee log file for details.",
                        MessageDialog.MessageType.Error).ShowDialog();
                });
                
                return false;
            }
        }

        private async Task<bool> AdminUserExistsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();
                
                var tableExists = await dbContext.Database.CanConnectAsync();
                _log($"Database connection status: {tableExists}");
                
                var userCount = await dbContext.Users.CountAsync(u => !u.Deleted);
                _log($"Found {userCount} non-deleted users in database");
                
                if (userCount > 0)
                {
                    var users = await dbContext.Users
                        .Where(u => !u.Deleted)
                        .Select(u => new { u.Id, u.Email, u.FullName })
                        .ToListAsync();
                    
                    foreach (var user in users)
                    {
                        _log($"Existing user: ID={user.Id}, Email={user.Email}, Name={user.FullName}");
                    }
                }
                
                return userCount > 0;
            }
            catch (Exception ex)
            {
                _log($"Error checking for admin user: {ex.Message}");
                _log($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
