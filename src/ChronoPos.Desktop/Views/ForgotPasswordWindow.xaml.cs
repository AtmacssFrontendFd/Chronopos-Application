using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using ChronoPos.Infrastructure;
using ChronoPos.Infrastructure.Services;
using ChronoPos.Desktop.Services;
using ChronoPos.Desktop.Models.Licensing;

namespace ChronoPos.Desktop.Views
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDatabaseLocalizationService _localizationService;
        private readonly ILicensingService _licensingService;
        
        private bool _isNewPasswordVisible = false;
        private bool _isConfirmPasswordVisible = false;
        
        private string _selectedLicenseFilePath = string.Empty;
        private string _licenseFileContent = string.Empty;
        private bool _isLicenseVerified = false;

        public bool IsPasswordResetSuccessful { get; private set; }

        public ForgotPasswordWindow(IServiceProvider serviceProvider)
        {
            try
            {
                InitializeComponent();
                
                _serviceProvider = serviceProvider;
                _localizationService = _serviceProvider.GetRequiredService<IDatabaseLocalizationService>();
                _licensingService = _serviceProvider.GetRequiredService<ILicensingService>();
                
                // Add window closing handler
                Closing += ForgotPasswordWindow_Closing;
                
                // Initialize language ComboBox
                InitializeLanguageComboBox();
                
                // Load translations
                _ = LoadTranslationsAsync();
                
                LicenseFileTextBox.Focus();
                
                LogMessage("ForgotPasswordWindow initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage($"ForgotPasswordWindow initialization error: {ex.Message}");
                throw;
            }
        }

        private void ForgotPasswordWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            LogMessage($"ForgotPasswordWindow is closing. IsPasswordResetSuccessful: {IsPasswordResetSuccessful}");
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ResetPasswordButton_Click(sender, e);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void InitializeLanguageComboBox()
        {
            try
            {
                // Get current language from localization service
                var currentLanguage = _localizationService.GetCurrentLanguageCode();
                
                // Set the selected item based on current language
                foreach (ComboBoxItem item in LanguageComboBox.Items)
                {
                    if (item.Tag?.ToString() == currentLanguage)
                    {
                        LanguageComboBox.SelectedItem = item;
                        break;
                    }
                }
                
                // Default to English if no match found
                if (LanguageComboBox.SelectedItem == null && LanguageComboBox.Items.Count > 0)
                {
                    LanguageComboBox.SelectedIndex = 0;
                }
                
                LogMessage($"Language ComboBox initialized with: {currentLanguage}");
            }
            catch (Exception ex)
            {
                LogMessage($"Error initializing language ComboBox: {ex.Message}");
            }
        }

        private async void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string? languageCode = selectedItem.Tag?.ToString();
                if (!string.IsNullOrEmpty(languageCode))
                {
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
                    
                    LogMessage($"Language changed to: {languageCode}");
                }
            }
        }

        private void BrowseLicenseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|ChronoPOS License Files (*.chronopos-license)|*.chronopos-license|All Files (*.*)|*.*",
                    Title = "Select License File",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    _selectedLicenseFilePath = openFileDialog.FileName;
                    _licenseFileContent = File.ReadAllText(_selectedLicenseFilePath);
                    
                    LicenseFileTextBox.Text = Path.GetFileName(_selectedLicenseFilePath);
                    
                    // Enable the Verify License button
                    VerifyLicenseButton.IsEnabled = true;
                    
                    // Reset license verification status
                    _isLicenseVerified = false;
                    LicenseVerifiedBorder.Visibility = Visibility.Collapsed;
                    
                    // Disable password fields until license is verified
                    NewPasswordGrid.IsEnabled = false;
                    ConfirmPasswordGrid.IsEnabled = false;
                    ResetPasswordButton.IsEnabled = false;
                    
                    // Clear any previous error messages
                    HideMessages();
                    
                    LogMessage($"License file selected: {Path.GetFileName(_selectedLicenseFilePath)}");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load license file: {ex.Message}");
                LogMessage($"Error loading license file: {ex.Message}");
            }
        }

        private async void VerifyLicenseButton_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            if (string.IsNullOrWhiteSpace(_licenseFileContent))
            {
                ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_license_required") ?? "Please select a license file.");
                return;
            }

            // Show loading state
            VerifyLicenseButton.IsEnabled = false;
            VerifyLicenseButton.Content = "Verifying...";

            try
            {
                // Verify license
                var licenseInfo = await VerifyLicenseAsync(_licenseFileContent);
                if (licenseInfo != null)
                {
                    // License verified successfully
                    _isLicenseVerified = true;
                    LicenseVerifiedBorder.Visibility = Visibility.Visible;
                    
                    // Enable password fields
                    NewPasswordGrid.IsEnabled = true;
                    ConfirmPasswordGrid.IsEnabled = true;
                    ResetPasswordButton.IsEnabled = true;
                    
                    // Focus on the new password field
                    NewPasswordBox.Focus();
                    
                    LogMessage("License verification successful - password fields enabled");
                }
            }
            catch (Exception ex)
            {
                ShowError($"License verification failed: {ex.Message}");
                LogMessage($"License verification error: {ex.Message}");
            }
            finally
            {
                VerifyLicenseButton.Content = _isLicenseVerified ? "License Verified ✓" : "Verify License";
                VerifyLicenseButton.IsEnabled = !_isLicenseVerified;
            }
        }

        private async void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            HideMessages();

            // Check if license is verified first
            if (!_isLicenseVerified)
            {
                ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_license_not_verified") ?? "Please verify your license first.");
                return;
            }

            var newPassword = _isNewPasswordVisible ? NewPasswordTextBox.Text : NewPasswordBox.Password;
            var confirmPassword = _isConfirmPasswordVisible ? ConfirmPasswordTextBox.Text : ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_password_required") ?? "Please enter a new password.");
                return;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_confirm_password_required") ?? "Please confirm your new password.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_passwords_mismatch") ?? "Passwords do not match.");
                return;
            }

            if (newPassword.Length < 6)
            {
                ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_password_too_short") ?? "Password must be at least 6 characters long.");
                return;
            }

            // Show loading overlay
            LoadingOverlay.Visibility = Visibility.Visible;
            ResetPasswordButton.IsEnabled = false;

            try
            {
                // Reset password for the admin user
                await ResetAdminPasswordAsync(newPassword);
                
                ShowSuccess(await _localizationService.GetTranslationAsync("forgot_password.success_message") ?? "Password has been reset successfully!");
                IsPasswordResetSuccessful = true;
                
                LogMessage("Password reset completed successfully");
                
                // Auto-close after 2 seconds
                await Task.Delay(2000);
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Password reset failed: {ex.Message}");
                LogMessage($"Password reset error: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                ResetPasswordButton.IsEnabled = true;
            }
        }

        private async Task<LicenseKeyInfo?> VerifyLicenseAsync(string licenseContent)
        {
            try
            {
                // Decrypt the license
                var licenseInfo = _licensingService.DecryptLicenseKey(licenseContent.Trim());
                
                if (licenseInfo == null)
                {
                    ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_invalid_license") ?? "Invalid license file format.");
                    return null;
                }

                // Validate machine fingerprint
                var currentFingerprint = MachineFingerprint.Generate();
                if (licenseInfo.MachineFingerprint != currentFingerprint)
                {
                    ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_license_machine_mismatch") ?? "License is not valid for this machine.");
                    return null;
                }

                // Validate expiry
                if (licenseInfo.ExpiryDate < DateTime.UtcNow)
                {
                    ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_license_expired") ?? "License has expired. Please contact support.");
                    return null;
                }

                // Validate against stored sales key
                var storedSalesKey = _licensingService.GetSavedSalesKey();
                if (!string.IsNullOrEmpty(storedSalesKey) && licenseInfo.SalesKey != storedSalesKey)
                {
                    ShowError(await _localizationService.GetTranslationAsync("forgot_password.error_license_sales_key_mismatch") ?? "License does not match the sales key for this machine.");
                    return null;
                }

                LogMessage("License verification successful");
                return licenseInfo;
            }
            catch (Exception ex)
            {
                ShowError($"License verification failed: {ex.Message}");
                LogMessage($"License verification error: {ex.Message}");
                return null;
            }
        }

        private async Task ResetAdminPasswordAsync(string newPassword)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ChronoPosDbContext>();

                // Find the first admin user (assuming there's at least one)
                var adminUser = await dbContext.Users
                    .Where(u => !u.Deleted && (u.Role.ToLower().Contains("admin") || u.Role.ToLower().Contains("manager") || u.Id == 1))
                    .OrderBy(u => u.Id)
                    .FirstOrDefaultAsync();

                if (adminUser == null)
                {
                    // If no specific admin found, get the first user
                    adminUser = await dbContext.Users
                        .Where(u => !u.Deleted)
                        .OrderBy(u => u.Id)
                        .FirstOrDefaultAsync();
                }

                if (adminUser == null)
                {
                    throw new InvalidOperationException("No user accounts found in the system.");
                }

                // Hash the new password
                var hashedPassword = HashPassword(newPassword);
                adminUser.Password = hashedPassword;

                await dbContext.SaveChangesAsync();
                
                LogMessage($"Password reset successful for user: {adminUser.Email} (ID: {adminUser.Id})");
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

        // Password visibility toggle for New Password
        private void ShowNewPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isNewPasswordVisible = !_isNewPasswordVisible;
            
            if (_isNewPasswordVisible)
            {
                // Show password as text
                NewPasswordTextBox.Text = NewPasswordBox.Password;
                NewPasswordBox.Visibility = Visibility.Collapsed;
                NewPasswordTextBox.Visibility = Visibility.Visible;
                NewPasswordTextBox.Focus();
                NewPasswordTextBox.SelectionStart = NewPasswordTextBox.Text.Length;
                ShowNewPasswordButton.Content = "🙈"; // Closed eye
            }
            else
            {
                // Hide password
                NewPasswordBox.Password = NewPasswordTextBox.Text;
                NewPasswordTextBox.Visibility = Visibility.Collapsed;
                NewPasswordBox.Visibility = Visibility.Visible;
                NewPasswordBox.Focus();
                ShowNewPasswordButton.Content = "👁"; // Open eye
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isNewPasswordVisible)
            {
                // Sync password to hidden textbox
                NewPasswordTextBox.Text = NewPasswordBox.Password;
            }
        }

        private void NewPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isNewPasswordVisible)
            {
                // Sync password to password box
                NewPasswordBox.Password = NewPasswordTextBox.Text;
            }
        }

        // Password visibility toggle for Confirm Password
        private void ShowConfirmPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
            
            if (_isConfirmPasswordVisible)
            {
                // Show password as text
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
                ConfirmPasswordBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordTextBox.Visibility = Visibility.Visible;
                ConfirmPasswordTextBox.Focus();
                ConfirmPasswordTextBox.SelectionStart = ConfirmPasswordTextBox.Text.Length;
                ShowConfirmPasswordButton.Content = "🙈"; // Closed eye
            }
            else
            {
                // Hide password
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
                ConfirmPasswordTextBox.Visibility = Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = Visibility.Visible;
                ConfirmPasswordBox.Focus();
                ShowConfirmPasswordButton.Content = "👁"; // Open eye
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_isConfirmPasswordVisible)
            {
                // Sync password to hidden textbox
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
            }
        }

        private void ConfirmPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isConfirmPasswordVisible)
            {
                // Sync password to password box
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
            }
        }

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

        private void HideMessages()
        {
            ErrorBorder.Visibility = Visibility.Collapsed;
            SuccessBorder.Visibility = Visibility.Collapsed;
        }

        private void LogMessage(string message)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var logPath = System.IO.Path.Combine(appDataPath, "ChronoPos", "app.log");
            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] ForgotPassword: {message}";
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

        private async Task LoadTranslationsAsync()
        {
            try
            {
                // Load translations from database
                ResetTitleTextBlock.Text = await _localizationService.GetTranslationAsync("forgot_password.title") ?? "Reset Password";
                ResetSubtitleTextBlock.Text = await _localizationService.GetTranslationAsync("forgot_password.subtitle") ?? "Please provide your license file and set a new password";
                LicenseLabelTextBlock.Text = await _localizationService.GetTranslationAsync("forgot_password.license_file") ?? "License File";
                NewPasswordLabelTextBlock.Text = await _localizationService.GetTranslationAsync("forgot_password.new_password") ?? "New Password";
                ConfirmPasswordLabelTextBlock.Text = await _localizationService.GetTranslationAsync("forgot_password.confirm_password") ?? "Confirm Password";
                BackToLoginButton.Content = await _localizationService.GetTranslationAsync("forgot_password.back_to_login") ?? "← Back to Login";
                ResetPasswordButton.Content = await _localizationService.GetTranslationAsync("forgot_password.reset_button") ?? "Reset Password";
                BrowseLicenseButton.Content = await _localizationService.GetTranslationAsync("forgot_password.browse_button") ?? "Browse...";
                VerifyLicenseButton.Content = await _localizationService.GetTranslationAsync("forgot_password.verify_license_button") ?? "Verify License";
                LicenseVerifiedTextBlock.Text = await _localizationService.GetTranslationAsync("forgot_password.license_verified_message") ?? "License verified successfully. You can now set a new password.";
                
                LogMessage("Translations loaded for forgot password window");
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading translations: {ex.Message}");
                // Keep default English text if translation fails
            }
        }
    }
}