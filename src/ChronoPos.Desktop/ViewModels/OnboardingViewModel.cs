using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using ChronoPos.Desktop.Models.Licensing;
using ChronoPos.Desktop.Services;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly ILicensingService _licensingService;
        private readonly IHostDiscoveryService _hostDiscoveryService;
        private readonly ICameraService _cameraService;

        [ObservableProperty]
        private int _currentStep = 0; // 0 = Welcome, 1-6 = Steps

        [ObservableProperty]
        private bool _isStandaloneMode = true;

        [ObservableProperty]
        private ScratchCardInfo? _scratchCardInfo;

        [ObservableProperty]
        private string _qrCodeData = string.Empty;

        [ObservableProperty]
        private string _businessName = string.Empty;

        [ObservableProperty]
        private string _tradeLicenseNumber = string.Empty;

        [ObservableProperty]
        private string _vatNumber = string.Empty;

        [ObservableProperty]
        private string _contactPerson = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phone = string.Empty;

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private string _emiratesID = string.Empty;

        [ObservableProperty]
        private string _industryCategory = string.Empty;

        [ObservableProperty]
        private int _numberOfOutlets = 1;

        [ObservableProperty]
        private string _generatedSalesKey = string.Empty;

        [ObservableProperty]
        private string _licenseKeyInput = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _isCameraAvailable = true;

        [ObservableProperty]
        private System.Collections.ObjectModel.ObservableCollection<HostBroadcastMessage> _discoveredHosts = new();

        [ObservableProperty]
        private HostBroadcastMessage? _selectedHost;

        [ObservableProperty]
        private bool _isDiscoveringHosts = false;

        public OnboardingViewModel(
            ILicensingService licensingService,
            IHostDiscoveryService hostDiscoveryService,
            ICameraService cameraService)
        {
            _licensingService = licensingService;
            _hostDiscoveryService = hostDiscoveryService;
            _cameraService = cameraService;

            IsCameraAvailable = _cameraService.IsCameraAvailable();
        }

        [RelayCommand]
        private async Task StartCamera()
        {
            try
            {
                _cameraService.StartCamera();
                var qrData = await _cameraService.ScanQRCodeAsync();
                
                if (!string.IsNullOrWhiteSpace(qrData))
                {
                    QrCodeData = qrData;
                    await ProcessScratchCard();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Camera error: {ex.Message}";
            }
            finally
            {
                _cameraService.StopCamera();
            }
        }

        [RelayCommand]
        private void StartStandaloneSetup()
        {
            IsStandaloneMode = true;
            CurrentStep = 1; // Scratch card entry
        }

        [RelayCommand]
        private void StartHostConnection()
        {
            IsStandaloneMode = false;
            CurrentStep = 10; // Host discovery screen
            _ = DiscoverHosts();
        }

        [RelayCommand]
        private async Task DiscoverHosts()
        {
            IsDiscoveringHosts = true;
            ErrorMessage = string.Empty;
            DiscoveredHosts.Clear();

            try
            {
                StatusMessage = "Searching for ChronoPOS hosts on your network...";
                var hosts = await _hostDiscoveryService.DiscoverHostsAsync(10);
                
                foreach (var host in hosts)
                {
                    DiscoveredHosts.Add(host);
                }

                if (DiscoveredHosts.Count == 0)
                {
                    StatusMessage = "No hosts found. Make sure the host device is running and on the same network.";
                }
                else
                {
                    StatusMessage = $"Found {DiscoveredHosts.Count} host(s)";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Discovery error: {ex.Message}";
            }
            finally
            {
                IsDiscoveringHosts = false;
            }
        }

        [RelayCommand]
        private async Task ConnectToSelectedHost()
        {
            if (SelectedHost == null)
            {
                ErrorMessage = "Please select a host to connect to.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                // TODO: Implement actual connection handshake
                StatusMessage = $"Connecting to {SelectedHost.HostName}...";
                await Task.Delay(2000); // Placeholder

                // For now, just complete the onboarding
                CurrentStep = 6; // Success screen
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Connection failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SelectHost(HostBroadcastMessage host)
        {
            SelectedHost = host;
            ConnectToSelectedHostCommand.Execute(host);
        }

        [RelayCommand]
        private async Task ProcessScratchCard()
        {
            if (string.IsNullOrWhiteSpace(QrCodeData))
            {
                ErrorMessage = "Please scan QR code or paste encrypted scratch card data.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var cardInfo = _licensingService.DecryptScratchCard(QrCodeData);
                
                if (cardInfo == null)
                {
                    ErrorMessage = "Invalid scratch card data. Please check and try again.";
                    return;
                }

                if (cardInfo.ExpiryDate < DateTime.UtcNow)
                {
                    ErrorMessage = "This scratch card has expired. Please contact your salesperson.";
                    return;
                }

                ScratchCardInfo = cardInfo;
                CurrentStep = 2; // Salesperson confirmation
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error processing scratch card: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void ConfirmSalesperson()
        {
            CurrentStep = 3; // Business information
        }

        [RelayCommand]
        private async Task GenerateSalesKey()
        {
            // Validate business information
            if (!ValidateBusinessInfo())
                return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var fingerprint = MachineFingerprint.Generate();

                var salesKeyInfo = new SalesKeyInfo
                {
                    ScratchCardCode = ScratchCardInfo!.CardCode,
                    ApplicationName = ScratchCardInfo.ApplicationName,
                    Customer = new CustomerInfo
                    {
                        BusinessName = BusinessName,
                        ContactPerson = ContactPerson,
                        Email = Email,
                        Phone = Phone,
                        Address = Address,
                        TradeLicenseNumber = TradeLicenseNumber,
                        VATNumber = VatNumber,
                        EmiratesID = EmiratesID,
                        IndustryCategory = IndustryCategory,
                        NumberOfOutlets = NumberOfOutlets
                    },
                    System = new SystemInfo
                    {
                        MachineName = Environment.MachineName,
                        OperatingSystem = Environment.OSVersion.ToString(),
                        MachineFingerprint = fingerprint,
                        ProcessorCount = Environment.ProcessorCount,
                        SystemVersion = Environment.OSVersion.Version.ToString()
                    },
                    CreatedAt = DateTime.UtcNow
                };

                GeneratedSalesKey = _licensingService.EncryptSalesKey(salesKeyInfo);
                _licensingService.SaveSalesKey(GeneratedSalesKey);

                CurrentStep = 4; // Sales key display
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error generating sales key: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CopySalesKey()
        {
            try
            {
                Clipboard.SetText(GeneratedSalesKey);
                StatusMessage = "Sales key copied to clipboard!";
                Task.Delay(3000).ContinueWith(_ => StatusMessage = string.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to copy: {ex.Message}";
            }
        }

        [RelayCommand]
        private void SaveSalesKeyToFile()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|ChronoPOS Sales Key (*.chronopos-saleskey)|*.chronopos-saleskey|All Files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"ChronoPOS-SalesKey-{DateTime.Now:yyyyMMdd-HHmmss}.txt",
                    Title = "Save Sales Key"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, GeneratedSalesKey);
                    StatusMessage = $"Sales key saved to: {Path.GetFileName(saveFileDialog.FileName)}";
                    Task.Delay(5000).ContinueWith(_ => StatusMessage = string.Empty);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to save sales key: {ex.Message}";
            }
        }

        [RelayCommand]
        private void ProceedToLicenseActivation()
        {
            CurrentStep = 5; // License activation
        }

        [RelayCommand]
        private void LoadLicenseFromFile()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|ChronoPOS License Files (*.chronopos-license)|*.chronopos-license|All Files (*.*)|*.*",
                    Title = "Select License File"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    var licenseContent = File.ReadAllText(openFileDialog.FileName);
                    LicenseKeyInput = licenseContent.Trim();
                    StatusMessage = $"License loaded from: {Path.GetFileName(openFileDialog.FileName)}";
                    Task.Delay(3000).ContinueWith(_ => StatusMessage = string.Empty);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed to load license file: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task ActivateLicense()
        {
            if (string.IsNullOrWhiteSpace(LicenseKeyInput))
            {
                ErrorMessage = "Please paste or load your license key.";
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var licenseInfo = _licensingService.DecryptLicenseKey(LicenseKeyInput);
                
                if (licenseInfo == null)
                {
                    ErrorMessage = "Invalid license key format.";
                    return;
                }

                // Validate machine fingerprint
                var currentFingerprint = MachineFingerprint.Generate();
                if (licenseInfo.MachineFingerprint != currentFingerprint)
                {
                    ErrorMessage = "License is not valid for this machine. Please ensure you generated the license from the exact sales key produced on this machine.";
                    return;
                }

                // Validate expiry
                if (licenseInfo.ExpiryDate < DateTime.UtcNow)
                {
                    ErrorMessage = "License has expired. Please contact support.";
                    return;
                }

                // Validate sales key match
                var storedSalesKey = _licensingService.GetSavedSalesKey();
                if (licenseInfo.SalesKey != storedSalesKey)
                {
                    ErrorMessage = "License does not match the sales key generated on this machine.";
                    return;
                }

                // Save license
                _licensingService.SaveLicense(LicenseKeyInput);

                CurrentStep = 6; // Success
            }
            catch (Exception ex)
            {
                ErrorMessage = $"License activation failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void CompleteOnboarding()
        {
            // Signal completion
            OnboardingCompleted?.Invoke();
        }

        [RelayCommand]
        private void GoBack()
        {
            if (CurrentStep > 0)
            {
                if (CurrentStep == 10) // Host discovery
                    CurrentStep = 0; // Back to welcome
                else if (CurrentStep > 10)
                    CurrentStep = 0; // Host flow back to welcome
                else
                    CurrentStep--;
            }
        }

        public event Action? OnboardingCompleted;

        private bool ValidateBusinessInfo()
        {
            if (string.IsNullOrWhiteSpace(BusinessName))
            {
                ErrorMessage = "Business name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TradeLicenseNumber))
            {
                ErrorMessage = "Trade license number is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(VatNumber))
            {
                ErrorMessage = "VAT number is required.";
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(VatNumber, @"^100\d{9}$"))
            {
                ErrorMessage = "VAT number must start with 100 followed by 9 digits.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ContactPerson))
            {
                ErrorMessage = "Contact person is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Phone))
            {
                ErrorMessage = "Phone number is required.";
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(Phone, @"^\+971[0-9]{8,9}$"))
            {
                ErrorMessage = "Phone number must be in UAE format (+971 followed by 8-9 digits).";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Address))
            {
                ErrorMessage = "Address is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmiratesID))
            {
                ErrorMessage = "Emirates ID or Passport is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(IndustryCategory))
            {
                ErrorMessage = "Industry category is required.";
                return false;
            }

            return true;
        }
    }
}
