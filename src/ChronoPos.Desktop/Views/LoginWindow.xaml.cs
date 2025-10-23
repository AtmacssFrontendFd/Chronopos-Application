using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Infrastructure;

namespace ChronoPos.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        public int LoggedInUserId { get; private set; }

        public LoginWindow(IServiceProvider serviceProvider)
        {
            LogMessage("LoginWindow constructor called");
            try
            {
                InitializeComponent();
                LogMessage("LoginWindow InitializeComponent completed");
                
                _serviceProvider = serviceProvider;
                
                // Add window closing handler
                Closing += LoginWindow_Closing;
                
                LoadSavedUsername();
                UsernameTextBox.Focus();
                
                LogMessage("LoginWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"LoginWindow constructor error: {ex.Message}");
                LogMessage($"LoginWindow constructor stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void LoginWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            LogMessage($"LoginWindow is closing. DialogResult: {DialogResult}");
            
            // If closing without successful login, set DialogResult to false
            if (DialogResult != true)
            {
                DialogResult = false;
                LogMessage("LoginWindow DialogResult set to false (no successful login)");
            }
        }

        private void LoadSavedUsername()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var chronoPosPath = System.IO.Path.Combine(appDataPath, "ChronoPos");
                var usernamePath = System.IO.Path.Combine(chronoPosPath, "username.dat");

                if (System.IO.File.Exists(usernamePath))
                {
                    var data = System.IO.File.ReadAllText(usernamePath);
                    var parts = data.Split('|');
                    if (parts.Length > 0)
                    {
                        UsernameTextBox.Text = parts[0];
                        RememberMeCheckBox.IsChecked = true;
                        PasswordBox.Focus();
                    }
                }
            }
            catch
            {
                // Ignore errors loading saved username
            }
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
            ErrorTextBlock.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                ShowError("Please enter your username or email.");
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                ShowError("Please enter your password.");
                return;
            }

            LoadingOverlay.Visibility = Visibility.Visible;
            LoginButton.IsEnabled = false;

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();

                    // Hash the password
                    var hashedPassword = HashPassword(PasswordBox.Password);

                    // Try to find user by email first, then by any match
                    var user = await dbContext.Users
                        .Where(u => !u.Deleted && u.Email.ToLower() == UsernameTextBox.Text.ToLower())
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        ShowError("Invalid username or password.");
                        return;
                    }

                    if (user.Password != hashedPassword)
                    {
                        ShowError("Invalid username or password.");
                        return;
                    }

                    // Save username if remember me is checked
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        SaveUsername(UsernameTextBox.Text, user.Id);
                    }
                    else
                    {
                        ClearSavedUsername();
                    }

                    LogMessage($">>> Login successful for user: {user.Email} (ID: {user.Id})");
                    LoggedInUserId = user.Id;
                    LogMessage($">>> Setting DialogResult = true");
                    DialogResult = true;
                    LogMessage($">>> DialogResult set successfully");
                    LogMessage($">>> Calling Close()");
                    Close();
                    LogMessage($">>> Close() completed");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Login failed: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                LoginButton.IsEnabled = true;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Are you sure you want to exit ChronoPOS?",
                "Exit Application",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
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

        private void ClearSavedUsername()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var chronoPosPath = System.IO.Path.Combine(appDataPath, "ChronoPos");
                var usernamePath = System.IO.Path.Combine(chronoPosPath, "username.dat");
                if (System.IO.File.Exists(usernamePath))
                {
                    System.IO.File.Delete(usernamePath);
                }
            }
            catch
            {
                // Ignore errors
            }
        }
    }
}
