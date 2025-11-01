using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Desktop.Views.Dialogs;

namespace ChronoPos.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabaseLocalizationService _localizationService;
        private bool _isPasswordVisible = false;
        private string _currentLanguageCode = "en";
        
        public int LoggedInUserId { get; private set; }

        public LoginWindow(IServiceProvider serviceProvider)
        {
            LogMessage("LoginWindow constructor called");
            try
            {
                InitializeComponent();
                LogMessage("LoginWindow InitializeComponent completed");
                
                _serviceProvider = serviceProvider;
                _localizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
                
                // Add window closing handler
                Closing += LoginWindow_Closing;
                
                // Load translations
                _ = LoadTranslationsAsync();
                
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

        private void LoginWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
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
                ShowError(await _localizationService.GetTranslationAsync("login.error_username_required"));
                return;
            }

            // Get password from whichever control is currently visible
            var password = _isPasswordVisible ? PasswordTextBox.Text : PasswordBox.Password;
            
            if (string.IsNullOrWhiteSpace(password))
            {
                ShowError(await _localizationService.GetTranslationAsync("login.error_password_required"));
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
                    var hashedPassword = HashPassword(password);

                    // Find user by username
                    var user = await dbContext.Users
                        .Where(u => !u.Deleted && u.Username.ToLower() == UsernameTextBox.Text.ToLower())
                        .FirstOrDefaultAsync();

                    if (user == null)
                    {
                        ShowError(await _localizationService.GetTranslationAsync("login.error_invalid_credentials"));
                        return;
                    }

                    if (user.Password != hashedPassword)
                    {
                        ShowError(await _localizationService.GetTranslationAsync("login.error_invalid_credentials"));
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

                    LogMessage($">>> Login successful for user: {user.Username} (ID: {user.Id})");
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
            var result = new ConfirmationDialog(
                "Exit Application",
                "Are you sure you want to exit ChronoPOS?",
                ConfirmationDialog.DialogType.Warning).ShowDialog();

            if (result == true)
            {
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void ShowError(string message)
        {
            // Ensure message is not null or empty
            if (string.IsNullOrWhiteSpace(message))
            {
                message = "An error occurred. Please try again.";
            }
            
            ErrorTextBlock.Text = message;
            ErrorBorder.Visibility = Visibility.Visible;
            
            LogMessage($"Error displayed to user: {message}");
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

        // Password visibility toggle
        private void ShowPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;
            
            if (_isPasswordVisible)
            {
                // Show password as text
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Focus();
                PasswordTextBox.SelectionStart = PasswordTextBox.Text.Length;
                ShowPasswordButton.Content = "🙈"; // Closed eye
            }
            else
            {
                // Hide password
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordBox.Focus();
                ShowPasswordButton.Content = "👁"; // Open eye
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isPasswordVisible)
            {
                // Sync password to hidden textbox
                PasswordTextBox.Text = PasswordBox.Password;
            }
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                // Sync password to password box
                PasswordBox.Password = PasswordTextBox.Text;
            }
        }

        // Language toggle
        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var languageCode = selectedItem.Tag?.ToString() ?? "en";
                _currentLanguageCode = languageCode;
                
                await _localizationService.SetCurrentLanguageAsync(languageCode);
                await LoadTranslationsAsync();
                
                // Update FlowDirection for RTL languages
                if (languageCode == "ar")
                {
                    this.FlowDirection = FlowDirection.RightToLeft;
                }
                else
                {
                    this.FlowDirection = FlowDirection.LeftToRight;
                }
            }
        }

        private async void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var forgotPasswordWindow = new ForgotPasswordWindow(_serviceProvider);
                forgotPasswordWindow.Owner = this;
                forgotPasswordWindow.ShowDialog();
                
                // If password was reset successfully, show a message
                if (forgotPasswordWindow.IsPasswordResetSuccessful)
                {
                    ShowError(await _localizationService.GetTranslationAsync("login.password_reset_success") ?? "Password has been reset successfully! Please login with your new password.");
                    // Clear the password fields
                    PasswordBox.Password = string.Empty;
                    PasswordTextBox.Text = string.Empty;
                    UsernameTextBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Unable to open password reset window: {ex.Message}");
                LogMessage($"Error opening ForgotPasswordWindow: {ex.Message}");
            }
        }

        private async Task LoadTranslationsAsync()
        {
            try
            {
                // Load translations from database
                LoginTitleTextBlock.Text = await _localizationService.GetTranslationAsync("login.title");
                LoginSubtitleTextBlock.Text = await _localizationService.GetTranslationAsync("login.subtitle");
                UsernameLabelTextBlock.Text = await _localizationService.GetTranslationAsync("login.username");
                PasswordLabelTextBlock.Text = await _localizationService.GetTranslationAsync("login.password");
                UsernameTextBox.Tag = await _localizationService.GetTranslationAsync("login.username_placeholder");
                PasswordTextBox.Tag = await _localizationService.GetTranslationAsync("login.password_placeholder");
                RememberMeCheckBox.Content = await _localizationService.GetTranslationAsync("login.remember_me");
                ForgotPasswordButton.Content = await _localizationService.GetTranslationAsync("login.forgot_password");
                LoginButton.Content = await _localizationService.GetTranslationAsync("login.button");
                
                LogMessage($"Translations loaded for language: {_currentLanguageCode}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading translations: {ex.Message}");
                // Keep default English text if translation fails
            }
        }
    }
}
