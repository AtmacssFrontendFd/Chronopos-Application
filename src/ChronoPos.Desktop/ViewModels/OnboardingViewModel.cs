using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using ChronoPos.Desktop.Models.Licensing;
using ChronoPos.Desktop.Services;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.ViewModels
{
    public partial class OnboardingViewModel : ObservableObject
    {
        private readonly ILicensingService _licensingService;
        private readonly IHostDiscoveryService _hostDiscoveryService;
        private readonly ICameraService _cameraService;
        private readonly IActiveCurrencyService _activeCurrencyService;

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
        private string _supportPerson = string.Empty; // Renamed from ContactPerson

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phone = string.Empty;

        [ObservableProperty]
        private string _address = string.Empty;

        [ObservableProperty]
        private string _country = string.Empty; // New field

        [ObservableProperty]
        private string _invoiceNumber = string.Empty; // New field

        [ObservableProperty]
        private DateTime _invoiceDate = DateTime.Now; // New field

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

        /// <summary>
        /// Gets the active currency symbol for display in UI
        /// </summary>
        public string CurrencySymbol => _activeCurrencyService?.CurrencySymbol ?? "$";

        public System.Collections.ObjectModel.ObservableCollection<string> Countries { get; } = new()
        {
            "United Arab Emirates", "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda",
            "Argentina", "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain", "Bangladesh",
            "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bhutan", "Bolivia", "Bosnia and Herzegovina",
            "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso", "Burundi", "Cabo Verde", "Cambodia",
            "Cameroon", "Canada", "Central African Republic", "Chad", "Chile", "China", "Colombia", "Comoros",
            "Congo", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czech Republic", "Denmark", "Djibouti",
            "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador", "Equatorial Guinea", "Eritrea",
            "Estonia", "Eswatini", "Ethiopia", "Fiji", "Finland", "France", "Gabon", "Gambia", "Georgia",
            "Germany", "Ghana", "Greece", "Grenada", "Guatemala", "Guinea", "Guinea-Bissau", "Guyana", "Haiti",
            "Honduras", "Hungary", "Iceland", "India", "Indonesia", "Iran", "Iraq", "Ireland", "Israel", "Italy",
            "Jamaica", "Japan", "Jordan", "Kazakhstan", "Kenya", "Kiribati", "Kosovo", "Kuwait", "Kyrgyzstan",
            "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania", "Luxembourg",
            "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands", "Mauritania",
            "Mauritius", "Mexico", "Micronesia", "Moldova", "Monaco", "Mongolia", "Montenegro", "Morocco",
            "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal", "Netherlands", "New Zealand", "Nicaragua",
            "Niger", "Nigeria", "North Korea", "North Macedonia", "Norway", "Oman", "Pakistan", "Palau",
            "Palestine", "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal",
            "Qatar", "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines",
            "Samoa", "San Marino", "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia", "Seychelles",
            "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa",
            "South Korea", "South Sudan", "Spain", "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland",
            "Syria", "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tonga",
            "Trinidad and Tobago", "Tunisia", "Turkey", "Turkmenistan", "Tuvalu", "Uganda", "Ukraine",
            "United Kingdom", "United States", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City", "Venezuela",
            "Vietnam", "Yemen", "Zambia", "Zimbabwe"
        };

        public OnboardingViewModel(
            ILicensingService licensingService,
            IHostDiscoveryService hostDiscoveryService,
            ICameraService cameraService,
            IActiveCurrencyService activeCurrencyService)
        {
            _licensingService = licensingService;
            _hostDiscoveryService = hostDiscoveryService;
            _cameraService = cameraService;
            _activeCurrencyService = activeCurrencyService;

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
            AppLogger.LogInfo("[ONBOARDING] Starting host discovery from UI", filename: "host_discovery");
            IsDiscoveringHosts = true;
            ErrorMessage = string.Empty;
            DiscoveredHosts.Clear();

            try
            {
                StatusMessage = "Searching for ChronoPOS hosts on your network...";
                AppLogger.LogInfo("[ONBOARDING] Calling DiscoverHostsAsync(10)...", filename: "host_discovery");
                
                var hosts = await _hostDiscoveryService.DiscoverHostsAsync(10);
                
                AppLogger.LogInfo($"[ONBOARDING] Discovery returned {hosts.Count} host(s)", filename: "host_discovery");
                
                foreach (var host in hosts)
                {
                    DiscoveredHosts.Add(host);
                    AppLogger.LogInfo($"[ONBOARDING] Added to UI collection: {host.HostName} ({host.HostIp})", filename: "host_discovery");
                }

                if (DiscoveredHosts.Count == 0)
                {
                    StatusMessage = "No hosts found. Make sure the host device is running and on the same network.";
                    AppLogger.LogWarning("[ONBOARDING] ⚠️ No hosts found - UI showing empty state", filename: "host_discovery");
                }
                else
                {
                    StatusMessage = $"Found {DiscoveredHosts.Count} host(s)";
                    AppLogger.LogInfo($"[ONBOARDING] ✅ UI updated with {DiscoveredHosts.Count} host(s)", filename: "host_discovery");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Discovery error: {ex.Message}";
                AppLogger.LogError("[ONBOARDING] Discovery error in ViewModel", ex, filename: "host_discovery");
            }
            finally
            {
                IsDiscoveringHosts = false;
                AppLogger.LogInfo("[ONBOARDING] Discovery UI state reset", filename: "host_discovery");
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
                StatusMessage = $"Connecting to {SelectedHost.HostName}...";
                
                // Generate client fingerprint
                var clientFingerprint = MachineFingerprint.Generate();
                var clientIp = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName())
                    .AddressList
                    .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    ?.ToString() ?? "Unknown";
                
                // In real implementation, this would be an HTTP call to host's API
                // For now, we'll simulate the connection by creating a token locally
                var connectionToken = new ConnectionToken
                {
                    Token = Guid.NewGuid().ToString(),
                    HostIp = SelectedHost.HostIp,
                    HostName = SelectedHost.HostName,
                    DatabaseUncPath = $"\\\\{SelectedHost.HostIp}\\ChronoPosDB\\chronopos.db",
                    DatabaseShareName = "ChronoPosDB",
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(365),
                    ClientFingerprint = clientFingerprint,
                    PlanId = SelectedHost.PlanId,
                    MaxPosDevices = SelectedHost.MaxPosDevices
                };
                
                StatusMessage = "Validating network path...";
                await Task.Delay(500);
                
                // Test if database path is accessible
                var dbSharingService = new DatabaseSharingService();
                AppLogger.LogInfo($"[ONBOARDING] Validating network path: {connectionToken.DatabaseUncPath}", filename: "host_discovery");
                var isAccessible = dbSharingService.ValidateNetworkPath(connectionToken.DatabaseUncPath);
                
                if (!isAccessible)
                {
                    ErrorMessage = $"Cannot access database at {connectionToken.DatabaseUncPath}\n\n" +
                                 $"Please ensure:\n" +
                                 $"1. The folder is shared as 'ChronoPosDB' on the host\n" +
                                 $"2. You have read/write permissions\n" +
                                 $"3. Windows file sharing is enabled";
                    return;
                }
                
                StatusMessage = "Saving connection configuration...";
                
                // Save connection configuration
                SaveConnectionConfig(connectionToken);
                
                StatusMessage = "Connected successfully!";
                await Task.Delay(1000);
                
                // Complete onboarding
                CurrentStep = 6; // Success screen
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Connection failed: {ex.Message}";
                AppLogger.LogError("[ONBOARDING] Connection to host failed", ex, filename: "host_discovery");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SaveConnectionConfig(ConnectionToken token)
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var chronoPosPath = Path.Combine(appDataPath, "ChronoPos");
            Directory.CreateDirectory(chronoPosPath);
            
            var connectionConfig = new ConnectionConfig
            {
                IsClient = true,
                IsHost = false,
                HostIp = token.HostIp,
                DatabasePath = token.DatabaseUncPath,
                Token = token,
                ConfiguredAt = DateTime.UtcNow
            };
            
            var configPath = Path.Combine(chronoPosPath, "connection.json");
            try
            {
                var configJson = Newtonsoft.Json.JsonConvert.SerializeObject(connectionConfig, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(configPath, configJson);
                AppLogger.LogInfo($"[ONBOARDING] Connection config saved: {configPath}", filename: "host_discovery");
            }
            catch (Exception ex)
            {
                AppLogger.LogError("[ONBOARDING] Failed to save connection configuration", ex, configPath, "host_discovery");
                throw;
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
                        SupportPerson = SupportPerson,
                        Email = Email,
                        Phone = Phone,
                        Address = Address,
                        Country = Country,
                        InvoiceNumber = InvoiceNumber,
                        InvoiceDate = InvoiceDate,
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

            if (string.IsNullOrWhiteSpace(SupportPerson))
            {
                ErrorMessage = "Support person is required.";
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

            if (string.IsNullOrWhiteSpace(Country))
            {
                ErrorMessage = "Country is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(InvoiceNumber))
            {
                ErrorMessage = "Invoice number is required.";
                return false;
            }

            if (InvoiceDate == default(DateTime) || InvoiceDate > DateTime.Now)
            {
                ErrorMessage = "Valid invoice date is required (cannot be in the future).";
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
