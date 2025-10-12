using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Infrastructure;
using ChronoPos.Domain.Entities;

namespace ChronoPos.Desktop.Views
{
    public partial class CreateAdminWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public CreateAdminWindow(IServiceProvider serviceProvider)
        {
            LogMessage("CreateAdminWindow constructor called");
            try
            {
                InitializeComponent();
                LogMessage("CreateAdminWindow InitializeComponent completed");
                _serviceProvider = serviceProvider;
                
                // Add window closing handler
                Closing += CreateAdminWindow_Closing;
                
                LogMessage("CreateAdminWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"CreateAdminWindow constructor error: {ex.Message}");
                LogMessage($"CreateAdminWindow constructor stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void CreateAdminWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogMessage($"CreateAdminWindow is closing. AdminCreated: {AdminCreated}");
            
            // If window is closing without admin created, confirm with user
            if (!AdminCreated)
            {
                var result = MessageBox.Show(
                    "Admin account is required to use ChronoPos. Are you sure you want to exit?",
                    "Exit Application",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    LogMessage("Window close cancelled by user");
                }
                else
                {
                    LogMessage("User confirmed exit without creating admin");
                }
            }
        }

        private void LogMessage(string message)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logPath = System.IO.Path.Combine(appDataPath, "ChronoPos", "app.log");
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
            Console.WriteLine(logEntry);
            try
            {
                System.IO.File.AppendAllText(logPath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore file write errors
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LogMessage("CreateAdminWindow Loaded event fired");
            try
            {
                LogMessage("CreateAdminWindow rendered successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"CreateAdminWindow Loaded error: {ex.Message}");
                LogMessage($"CreateAdminWindow Loaded stack trace: {ex.StackTrace}");
            }
        }

        private async void CreateAdminButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorTextBlock.Visibility = Visibility.Collapsed;
            SuccessTextBlock.Visibility = Visibility.Collapsed;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(FullNameTextBox.Text))
            {
                ShowError("Full name is required.");
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
            {
                ShowError("Email address is required.");
                return;
            }

            if (!IsValidEmail(EmailTextBox.Text))
            {
                ShowError("Please enter a valid email address.");
                return;
            }

            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                ShowError("Username is required.");
                return;
            }

            if (UsernameTextBox.Text.Length < 3)
            {
                ShowError("Username must be at least 3 characters long.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Password is required.");
                return;
            }

            if (PasswordBox.Password.Length < 6)
            {
                ShowError("Password must be at least 6 characters long.");
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                ShowError("Passwords do not match.");
                return;
            }

            // Show loading
            LoadingOverlay.Visibility = Visibility.Visible;
            CreateAdminButton.IsEnabled = false;

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();

                    // Check if admin user already exists
                    var existingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == EmailTextBox.Text);
                    if (existingUser != null)
                    {
                        ShowError("A user with this email already exists.");
                        return;
                    }

                    // Step 1: Create "Company" Permission with All Screens and All Operations
                    LogMessage(">>> Creating Company permission...");
                    var companyPermission = new Permission
                    {
                        Name = "Company - Full Access",
                        Code = "COMPANY.FULL_ACCESS",
                        ScreenName = "-- All Screens --",  // Special value for all screens
                        TypeMatrix = "-- All Operations --",  // Special value for all operations
                        IsParent = true,  // This is a parent permission
                        ParentPermissionId = null,
                        Status = "Active",
                        CreatedBy = null,  // Will be updated after user is created
                        CreatedAt = DateTime.UtcNow
                    };
                    dbContext.Permissions.Add(companyPermission);
                    await dbContext.SaveChangesAsync();
                    LogMessage($">>> Company permission created with ID: {companyPermission.PermissionId}");

                    // Step 2: Create "Company" Role
                    LogMessage(">>> Creating Company role...");
                    var companyRole = new Role
                    {
                        RoleName = "Company",
                        Description = "Company owner with full access to all screens and operations",
                        Status = "Active",
                        CreatedBy = null,  // Will be updated after user is created
                        CreatedAt = DateTime.UtcNow
                    };
                    dbContext.Roles.Add(companyRole);
                    await dbContext.SaveChangesAsync();
                    LogMessage($">>> Company role created with ID: {companyRole.RoleId}");

                    // Step 3: Assign Company Permission to Company Role
                    LogMessage(">>> Assigning Company permission to Company role...");
                    var rolePermission = new RolePermission
                    {
                        RoleId = companyRole.RoleId,
                        PermissionId = companyPermission.PermissionId,
                        Status = "Active",
                        CreatedBy = null,  // Will be updated after user is created
                        CreatedAt = DateTime.UtcNow
                    };
                    dbContext.RolePermissions.Add(rolePermission);
                    await dbContext.SaveChangesAsync();
                    LogMessage($">>> Permission assigned to role with ID: {rolePermission.RolePermissionId}");

                    // Step 4: Create admin user with Company role
                    LogMessage(">>> Creating admin user with Company role...");
                    var adminUser = new User
                    {
                        FullName = FullNameTextBox.Text.Trim(),
                        Email = EmailTextBox.Text.Trim().ToLower(),
                        Password = HashPassword(PasswordBox.Password),
                        Role = "Company Owner",
                        RolePermissionId = companyRole.RoleId,  // Assign Company role
                        ShopId = 1, // Default shop
                        ChangeAccess = true,
                        CreatedAt = DateTime.UtcNow,
                        Deleted = false
                    };

                    dbContext.Users.Add(adminUser);
                    await dbContext.SaveChangesAsync();
                    LogMessage($">>> Admin user created with ID: {adminUser.Id}");

                    // Step 5: Update CreatedBy fields now that we have the admin user
                    companyPermission.CreatedBy = adminUser.Id;
                    companyRole.CreatedBy = adminUser.Id;
                    rolePermission.CreatedBy = adminUser.Id;
                    await dbContext.SaveChangesAsync();
                    LogMessage(">>> Updated CreatedBy fields with admin user ID");

                    // Save username for login
                    SaveUsername(UsernameTextBox.Text.Trim(), adminUser.Id);

                    ShowSuccess("Admin user created successfully!");
                    await Task.Delay(1500);

                    // Set a flag that admin was created successfully
                    LogMessage(">>> BEFORE setting AdminCreated = true");
                    AdminCreated = true;
                    LogMessage($">>> AFTER setting AdminCreated = true, value is: {AdminCreated}");
                    
                    // Set DialogResult to true to indicate success
                    try
                    {
                        LogMessage(">>> BEFORE setting DialogResult = true");
                        DialogResult = true;
                        LogMessage(">>> AFTER setting DialogResult = true");
                    }
                    catch (InvalidOperationException ex)
                    {
                        // If DialogResult can't be set, just close the window
                        LogMessage($">>> Could not set DialogResult: {ex.Message}");
                        LogMessage(">>> Closing window normally instead");
                    }
                    
                    LogMessage(">>> BEFORE calling Close()");
                    Close();
                    LogMessage(">>> AFTER calling Close()");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error creating admin user: {ex.Message}");
                LogMessage($"Stack trace: {ex.StackTrace}");
                ShowError($"Failed to create admin user: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                CreateAdminButton.IsEnabled = true;
            }
        }

        public bool AdminCreated { get; private set; } = false;

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
            SuccessBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess(string message)
        {
            SuccessTextBlock.Text = message;
            SuccessBorder.Visibility = Visibility.Visible;
            ErrorBorder.Visibility = Visibility.Collapsed;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private void SaveUsername(string username, int userId)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var chronoPosPath = System.IO.Path.Combine(appDataPath, "ChronoPos");
            System.IO.Directory.CreateDirectory(chronoPosPath);
            var usernamePath = System.IO.Path.Combine(chronoPosPath, "username.dat");
            System.IO.File.WriteAllText(usernamePath, $"{username}|{userId}");
        }
    }
}
