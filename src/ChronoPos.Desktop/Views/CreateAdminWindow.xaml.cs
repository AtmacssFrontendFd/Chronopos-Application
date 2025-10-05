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
                LogMessage("CreateAdminWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"CreateAdminWindow constructor error: {ex.Message}");
                LogMessage($"CreateAdminWindow constructor stack trace: {ex.StackTrace}");
                throw;
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

                    // Create admin user
                    var adminUser = new User
                    {
                        FullName = FullNameTextBox.Text.Trim(),
                        Email = EmailTextBox.Text.Trim().ToLower(),
                        Password = HashPassword(PasswordBox.Password),
                        Role = "Administrator",
                        RolePermissionId = 1, // Admin permission
                        ShopId = 1, // Default shop
                        ChangeAccess = true,
                        CreatedAt = DateTime.UtcNow,
                        Deleted = false
                    };

                    dbContext.Users.Add(adminUser);
                    await dbContext.SaveChangesAsync();

                    // Save username for login
                    SaveUsername(UsernameTextBox.Text.Trim(), adminUser.Id);

                    ShowSuccess("Admin user created successfully!");
                    await Task.Delay(1500);

                    // Set a flag that admin was created successfully
                    AdminCreated = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
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
            ErrorTextBlock.Visibility = Visibility.Visible;
            SuccessTextBlock.Visibility = Visibility.Collapsed;
        }

        private void ShowSuccess(string message)
        {
            SuccessTextBlock.Text = message;
            SuccessTextBlock.Visibility = Visibility.Visible;
            ErrorTextBlock.Visibility = Visibility.Collapsed;
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
