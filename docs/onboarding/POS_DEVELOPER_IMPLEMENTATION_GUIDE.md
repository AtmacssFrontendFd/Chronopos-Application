# ChronoPOS - POS System Onboarding Implementation Guide

## 📋 Document Purpose
This comprehensive guide provides all technical specifications for implementing the ChronoPOS point-of-sale (POS) client onboarding flow with advanced "Connect to Host Device" capabilities.

---

## 🎯 Executive Summary

### What You're Building
A robust POS onboarding system that:
1. Activates new POS installations using scratch cards
2. Collects Dubai-compliant business information
3. Generates machine-bound license keys
4. Supports multi-device LAN synchronization via host-client architecture
5. Enforces cryptographic security throughout the activation lifecycle

### Integration Points
- **Admin Panel**: WPF application (SQLite + EF Core) that manages plans, salespersons, scratch cards, sales keys, and licenses
- **POS System**: Your application that consumes scratch cards and validates licenses
- **Security**: AES-256 encryption with shared cryptographic contracts

---

## 🔐 Cryptography Contract

### Master Configuration
All encryption/decryption MUST use identical parameters to the admin panel:

```csharp
// Constants (NEVER change these)
const string MASTER_KEY = "ChronoPos2025!@#$%^&*()_+SecureKey";
const string SCRATCH_CARD_SALT = "ScratchCard_Salt_2025";
const string SALES_KEY_SALT = "SalesKey_Salt_2025";
const string LICENSE_KEY_SALT = "LicenseKey_Salt_2025";
const string KEY_DERIVATION_SALT = "ChronoPosSalt2025"; // UTF-8 bytes

// PBKDF2 Parameters
const int PBKDF2_ITERATIONS = 10000;
const int AES_KEY_SIZE = 32;  // 256-bit
const int AES_IV_SIZE = 16;   // 128-bit

// Algorithm
Algorithm: AES (System.Security.Cryptography.Aes)
Mode: CBC (default)
Padding: PKCS7 (default)
```

### Encryption Functions

#### Scratch Card Decryption
```csharp
public static T DecryptScratchCard<T>(string encryptedBase64)
{
    string key = MASTER_KEY + SCRATCH_CARD_SALT;
    string decryptedJson = DecryptString(encryptedBase64, key);
    return JsonConvert.DeserializeObject<T>(decryptedJson);
}
```

#### Sales Key Encryption
```csharp
public static string EncryptSalesKey(object salesKeyData)
{
    string key = MASTER_KEY + SALES_KEY_SALT;
    string jsonData = JsonConvert.SerializeObject(salesKeyData);
    return EncryptString(jsonData, key);
}
```

#### License Key Decryption
```csharp
public static T DecryptLicenseKey<T>(string encryptedBase64)
{
    string key = MASTER_KEY + LICENSE_KEY_SALT;
    string decryptedJson = DecryptString(encryptedBase64, key);
    return JsonConvert.DeserializeObject<T>(decryptedJson);
}
```

#### Core AES Implementation
```csharp
private static string DecryptString(string encryptedText, string key)
{
    byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
    using (Aes aes = Aes.Create())
    {
        using (var keyDerivation = new Rfc2898DeriveBytes(
            key, 
            Encoding.UTF8.GetBytes(KEY_DERIVATION_SALT), 
            PBKDF2_ITERATIONS, 
            HashAlgorithmName.SHA256))
        {
            aes.Key = keyDerivation.GetBytes(AES_KEY_SIZE);
            aes.IV = keyDerivation.GetBytes(AES_IV_SIZE);
        }

        using (var decryptor = aes.CreateDecryptor())
        using (var ms = new MemoryStream(encryptedBytes))
        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (var reader = new StreamReader(cs))
        {
            return reader.ReadToEnd();
        }
    }
}
```

---

## 📦 Data Contracts

### ⚠️ CRITICAL: Understanding Scratch Card Codes

The scratch card system uses **TWO DIFFERENT CODES** for different purposes:

#### 1. Display Code (USER-FACING) ✅
**Format**: `ABCD-1234-EFGH-5678` (16 characters + dashes)

**Purpose**:
- **This is what users see and manually enter**
- Human-friendly format with checksum validation
- Printed prominently on scratch cards
- Used when QR scanning fails

**Validation** (Client-Side):
```csharp
public class DisplayCodeValidator
{
    public static ValidationResult ValidateFormat(string displayCode)
    {
        // Remove dashes and normalize
        string normalized = displayCode.Replace("-", "").ToUpper();
        
        if (normalized.Length != 16)
            return new ValidationResult { IsValid = false, Message = "Invalid length" };
        
        // Extract checksum (last 4 chars)
        string datapart = normalized.Substring(0, 12);
        string checksum = normalized.Substring(12, 4);
        
        // Calculate expected checksum
        string expectedChecksum = GenerateChecksum(datapart);
        
        if (checksum != expectedChecksum)
            return new ValidationResult { IsValid = false, Message = "Invalid checksum" };
        
        return new ValidationResult { IsValid = true };
    }
    
    private static string GenerateChecksum(string data)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            int checksumValue = BitConverter.ToInt32(hashBytes, 0) & 0x7FFFFFFF;
            return EncodeBase36(checksumValue % (36 * 36 * 36 * 36), 4);
        }
    }
}
```

#### 2. Card Code (INTERNAL) 🔒
**Format**: `41FOUMQQH2VT` (12 alphanumeric characters)

**Purpose**:
- Internal database unique identifier
- Used in cryptographic validation
- Embedded in encrypted QR payload
- **NOT for manual entry - too error-prone**

**Usage**: Backend validation only

---

### ScratchCardInfo (Decrypted from QR Code)

```json
{
  "CardCode": "41FOUMQQH2VT",              // Internal ID - for validation
  "DisplayCode": "QWER-1234-ASDF-5678",     // User-facing ID - for manual entry
  "PlanId": 2,
  "PlanName": "Professional Plan",
  "PlanPrice": 59.99,
  "DurationInDays": 30,
  "SalespersonId": 5,
  "SalespersonName": "John Doe",
  "SalespersonEmail": "john.doe@chronopos.com",
  "CreatedAt": "2025-10-01T12:00:00Z",
  "ExpiryDate": "2026-10-01T12:00:00Z",
  "BatchNumber": "BATCH_20251001_120000"
}
```

**Source**: QR code on scratch card (encrypted with SCRATCH_CARD_SALT)

**C# Model**:
```csharp
public class ScratchCardInfo
{
    public string CardCode { get; set; }          // Internal
    public string DisplayCode { get; set; }       // User-facing
    public int PlanId { get; set; }
    public string PlanName { get; set; }
    public decimal PlanPrice { get; set; }
    public int DurationInDays { get; set; }
    public int SalespersonId { get; set; }
    public string SalespersonName { get; set; }
    public string SalespersonEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string BatchNumber { get; set; }
}
```

---

### SalesKeyInfo (POS → Admin)

```json
{
  "ScratchCardCode": "41FOUMQQH2VT",         // Internal CardCode
  "SalespersonId": 5,
  "Customer": {
    "BusinessName": "Dubai Tech Trading LLC",
    "ContactPerson": "Ahmed Al Rashid",
    "Email": "ahmed@dubaitech.ae",
    "Phone": "+971501234567",
    "Address": "Sheikh Zayed Road, Dubai, UAE"
  },
  "System": {
    "MachineName": "POS-REGISTER-01",
    "OperatingSystem": "Windows 11 Pro",
    "MachineFingerprint": "A8F3D2E1B9C4... (SHA-256 hash)",
    "ProcessorCount": 8,
    "SystemVersion": "10.0.22621"
  },
  "CreatedAt": "2025-10-04T14:30:00Z"
}
```

**Purpose**: POS encrypts this and sends to salesperson → salesperson → admin

**C# Model**:
```csharp
public class SalesKeyInfo
{
    public string ScratchCardCode { get; set; }   // Internal CardCode
    public int SalespersonId { get; set; }
    public CustomerInfo Customer { get; set; }
    public SystemInfo System { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerInfo
{
    public string BusinessName { get; set; }
    public string ContactPerson { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Address { get; set; }
}

public class SystemInfo
{
    public string MachineName { get; set; }
    public string OperatingSystem { get; set; }
    public string MachineFingerprint { get; set; }
    public int ProcessorCount { get; set; }
    public string SystemVersion { get; set; }
}
```

---

### LicenseKeyInfo (Admin → POS)

```json
{
  "SalesKey": "encrypted_sales_key_here...",
  "PlanId": 2,
  "PlanName": "Professional Plan",
  "ExpiryDate": "2025-11-04T14:30:00Z",
  "MachineFingerprint": "A8F3D2E1B9C4...",
  "LicenseType": "Standard",
  "CreatedAt": "2025-10-04T15:00:00Z",
  "MaxPosDevices": 3,
  "Features": [
    "Inventory Management",
    "Sales Tracking",
    "Advanced Reports",
    "Multi-User Support",
    "Customer Management"
  ]
}
```

**Purpose**: Admin issues this encrypted license; POS decrypts and validates

**C# Model**:
```csharp
public class LicenseKeyInfo
{
    public string SalesKey { get; set; }
    public int PlanId { get; set; }
    public string PlanName { get; set; }
    public DateTime ExpiryDate { get; set; }
    public string MachineFingerprint { get; set; }
    public string LicenseType { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MaxPosDevices { get; set; }
    public string[] Features { get; set; }
}
```

---

## 🚀 Onboarding Flow: Start New Setup

### Step 1: Scratch Card Entry

**UI Screen**:
```
┌──────────────────────────────────────────┐
│  ChronoPOS Activation                    │
│                                          │
│  Step 1 of 6: Enter Activation Card     │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │  [Scan QR Code]  [Enter Manually]  │ │
│  └────────────────────────────────────┘ │
│                                          │
│  OR Enter Activation Code:              │
│  ┌────┬────┬────┬────┐                 │
│  │    │    │    │    │                 │
│  └────┴────┴────┴────┘                 │
│                                          │
│  [ Next ]                                │
└──────────────────────────────────────────┘
```

**Implementation Flow**:

#### Option A: QR Code Scan
```csharp
public async Task<ScratchCardInfo> ScanQRCode()
{
    // 1. Use camera library to scan QR
    string qrContent = await QRScanner.ScanAsync();
    
    // 2. Decrypt QR content
    var cardInfo = CryptographyService.DecryptScratchCard<ScratchCardInfo>(qrContent);
    
    // 3. Validate
    if (cardInfo.ExpiryDate < DateTime.UtcNow)
        throw new Exception("Scratch card has expired");
    
    // 4. Display card info for confirmation
    return cardInfo;
}
```

#### Option B: Manual Display Code Entry
```csharp
public async Task<ScratchCardInfo> ManualEntry(string displayCode)
{
    // 1. Validate format + checksum
    var validation = DisplayCodeValidator.ValidateFormat(displayCode);
    if (!validation.IsValid)
        throw new Exception(validation.Message);
    
    // 2. Look up card in database OR call admin API
    var cardInfo = await LookupCardByDisplayCode(displayCode);
    
    if (cardInfo == null)
        throw new Exception("Card not found. Please verify the code.");
    
    // 3. Validate expiry
    if (cardInfo.ExpiryDate < DateTime.UtcNow)
        throw new Exception("Scratch card has expired");
    
    return cardInfo;
}

private async Task<ScratchCardInfo> LookupCardByDisplayCode(string displayCode)
{
    // Option 1: Call admin panel API (if available)
    // var response = await httpClient.GetAsync($"/api/scratch-cards/by-display-code/{displayCode}");
    
    // Option 2: Use local database sync
    // return await db.ScratchCards.FirstOrDefaultAsync(c => c.DisplayCode == displayCode);
    
    // For MVP: Require QR scan for full info
    throw new NotImplementedException("Display code lookup requires admin API or local sync");
}
```

**⚠️ Important**: Display code validates format but doesn't contain plan data. Always encourage QR scan for complete activation.

---

### Step 2: Salesperson Confirmation

**UI Screen**:
```
┌──────────────────────────────────────────┐
│  ChronoPOS Activation                    │
│                                          │
│  Step 2 of 6: Confirm Salesperson        │
│                                          │
│  Salesperson Information:                │
│  Name: John Doe                          │
│  Email: john.doe@chronopos.com          │
│  Employee ID: EMP001                     │
│                                          │
│  Plan Details:                           │
│  Professional Plan - $59.99/month        │
│  Max Devices: 3                          │
│                                          │
│  [ Back ]  [ Confirm ]                   │
└──────────────────────────────────────────┘
```

---

### Step 3: Business Information (Dubai Compliance)

**Required Fields**:
```csharp
public class DubaiBusinessInfo
{
    [Required]
    public string TradeLicenseNumber { get; set; }
    
    [Required]
    public string LegalBusinessName { get; set; }
    
    [Required]
    [RegularExpression(@"^100\d{9}$")]
    public string VATNumber { get; set; }  // Format: 100 + 9 digits
    
    [Required]
    public string OwnerName { get; set; }
    
    [Required]
    public string EmiratesID { get; set; }  // or Passport Number
    
    [Required]
    public string RegisteredAddress { get; set; }
    
    [Required]
    [RegularExpression(@"^\+971[0-9]{8,9}$")]
    public string PhoneNumber { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string IndustryCategory { get; set; }
    
    public int NumberOfOutlets { get; set; } = 1;
    
    public string TaxConfiguration { get; set; } = "5% VAT";
}
```

**Validation Rules**:
- VAT Number: `100` prefix + 9 digits
- Phone: UAE format `+971` + 8-9 digits
- Email: Standard RFC 5322
- Trade License: Alphanumeric, 4-15 characters

---

### Step 4: System Fingerprinting

**Critical**: Must generate stable, unique machine identifier

```csharp
public class MachineFingerprint
{
    public static string Generate()
    {
        var components = new List<string>();
        
        // 1. Primary MAC Address
        var macAddress = GetPrimaryMacAddress();
        components.Add(macAddress);
        
        // 2. Motherboard Serial (Windows)
        var boardSerial = GetWMIProperty("Win32_BaseBoard", "SerialNumber");
        components.Add(boardSerial);
        
        // 3. BIOS UUID
        var biosUuid = GetWMIProperty("Win32_ComputerSystemProduct", "UUID");
        components.Add(biosUuid);
        
        // 4. System Drive Volume Serial
        var volumeSerial = GetVolumeSerial("C:\\");
        components.Add(volumeSerial);
        
        // 5. Machine Name
        components.Add(Environment.MachineName);
        
        // 6. OS Version
        components.Add(Environment.OSVersion.ToString());
        
        // 7. Processor Count
        components.Add(Environment.ProcessorCount.ToString());
        
        // Create stable fingerprint
        string combined = string.Join("|", components.Where(c => !string.IsNullOrEmpty(c)));
        
        // Hash to SHA-256
        using (var sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
            return Convert.ToBase64String(hashBytes);
        }
    }
    
    private static string GetPrimaryMacAddress()
    {
        var nic = NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .OrderByDescending(n => n.Speed)
            .FirstOrDefault();
            
        return nic?.GetPhysicalAddress().ToString() ?? "UNKNOWN";
    }
    
    private static string GetWMIProperty(string className, string propertyName)
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}"))
            {
                foreach (var obj in searcher.Get())
                {
                    return obj[propertyName]?.ToString() ?? "";
                }
            }
        }
        catch { }
        
        return "";
    }
    
    private static string GetVolumeSerial(string driveLetter)
    {
        try
        {
            var driveInfo = new DriveInfo(driveLetter);
            // Note: Requires WMI query for actual volume serial
            return driveInfo.VolumeLabel;
        }
        catch
        {
            return "";
        }
    }
}
```

---

### Step 5: Sales Key Generation

```csharp
public string GenerateSalesKey(ScratchCardInfo card, DubaiBusinessInfo business)
{
    var salesKeyInfo = new SalesKeyInfo
    {
        ScratchCardCode = card.CardCode,  // Use internal CardCode
        SalespersonId = card.SalespersonId,
        Customer = new CustomerInfo
        {
            BusinessName = business.LegalBusinessName,
            ContactPerson = business.OwnerName,
            Email = business.Email,
            Phone = business.PhoneNumber,
            Address = business.RegisteredAddress
        },
        System = new SystemInfo
        {
            MachineName = Environment.MachineName,
            OperatingSystem = Environment.OSVersion.ToString(),
            MachineFingerprint = MachineFingerprint.Generate(),
            ProcessorCount = Environment.ProcessorCount,
            SystemVersion = Environment.OSVersion.Version.ToString()
        },
        CreatedAt = DateTime.UtcNow
    };
    
    // Encrypt with SALES_KEY_SALT
    string encryptedSalesKey = CryptographyService.EncryptSalesKey(salesKeyInfo);
    
    // Store locally for verification
    SecureStorage.Save("SalesKey", encryptedSalesKey);
    
    // Display to user with copy options
    return encryptedSalesKey;
}
```

**UI Screen**:
```
┌────────────────────────────────────────────┐
│  ChronoPOS Activation                      │
│                                            │
│  Step 5 of 6: Sales Key Generated          │
│                                            │
│  ✅ System fingerprint created             │
│  ✅ Business information validated         │
│  ✅ Sales key generated                    │
│                                            │
│  Sales Key:                                │
│  ┌────────────────────────────────────┐   │
│  │ AbCd1234... (encrypted, long)      │   │
│  └────────────────────────────────────┘   │
│                                            │
│  [📋 Copy]  [💾 Save File]  [📧 Email]    │
│                                            │
│  ⚠️ Send this key to your admin to         │
│     receive your license activation file   │
│                                            │
│  [ Back ]  [ Next: Await License ]         │
└────────────────────────────────────────────┘
```

---

### Step 6: License Activation

```csharp
public async Task<bool> ActivateLicense(string encryptedLicense)
{
    // 1. Decrypt license
    var licenseInfo = CryptographyService.DecryptLicenseKey<LicenseKeyInfo>(encryptedLicense);
    
    if (licenseInfo == null)
        throw new Exception("Invalid license format");
    
    // 2. Validate machine fingerprint
    string currentFingerprint = MachineFingerprint.Generate();
    if (licenseInfo.MachineFingerprint != currentFingerprint)
        throw new Exception("License is not valid for this machine");
    
    // 3. Validate expiry
    if (licenseInfo.ExpiryDate < DateTime.UtcNow)
        throw new Exception("License has expired");
    
    // 4. Validate sales key match
    string storedSalesKey = SecureStorage.Load("SalesKey");
    if (licenseInfo.SalesKey != storedSalesKey)
        throw new Exception("License does not match sales key");
    
    // 5. Save license
    SecureStorage.Save("License", encryptedLicense);
    SecureStorage.Save("LicenseInfo", JsonConvert.SerializeObject(licenseInfo));
    
    // 6. Mark as activated
    AppSettings.IsActivated = true;
    AppSettings.PlanId = licenseInfo.PlanId;
    AppSettings.PlanName = licenseInfo.PlanName;
    AppSettings.MaxPosDevices = licenseInfo.MaxPosDevices;
    AppSettings.Features = licenseInfo.Features;
    
    return true;
}
```

**UI Screen**:
```
┌────────────────────────────────────────────┐
│  ChronoPOS Activation                      │
│                                            │
│  Step 6 of 6: License Activation           │
│                                            │
│  Paste or load your license file:         │
│  ┌────────────────────────────────────┐   │
│  │                                    │   │
│  │  [Paste License] [Load File]      │   │
│  │                                    │   │
│  └────────────────────────────────────┘   │
│                                            │
│  [ Validate & Activate ]                   │
│                                            │
│  Status: ⏳ Waiting for license...         │
└────────────────────────────────────────────┘
```

After activation success:
```
┌────────────────────────────────────────────┐
│  🎉 ChronoPOS Activated Successfully!      │
│                                            │
│  Plan: Professional Plan                   │
│  Expires: November 4, 2025                 │
│  Max Devices: 3                            │
│                                            │
│  Features:                                 │
│  ✅ Inventory Management                   │
│  ✅ Sales Tracking                         │
│  ✅ Advanced Reports                       │
│  ✅ Multi-User Support                     │
│  ✅ Customer Management                    │
│                                            │
│  Next Step: Create Admin User              │
│  [ Continue Setup ]                        │
└────────────────────────────────────────────┘
```

---

## 🌐 Connect to Host Device (LAN Mode)

### Host Device Requirements
- Valid activated license
- `MaxPosDevices > 1`
- LAN network connectivity
- Host service running

### Discovery Protocol

**Host Broadcast** (UDP Multicast):
```csharp
public class HostBroadcaster
{
    private const string MULTICAST_ADDRESS = "239.255.42.99";
    private const int MULTICAST_PORT = 42099;
    
    public void StartBroadcast()
    {
        var udpClient = new UdpClient();
        udpClient.JoinMulticastGroup(IPAddress.Parse(MULTICAST_ADDRESS));
        
        var endpoint = new IPEndPoint(IPAddress.Parse(MULTICAST_ADDRESS), MULTICAST_PORT);
        
        while (true)
        {
            var broadcastData = new HostBroadcastMessage
            {
                Type = "ChronoPOS_HOST_BROADCAST",
                HostName = Environment.MachineName,
                HostIp = GetLocalIPAddress(),
                LicenseFingerprint = GetLicenseFingerprint(),
                LicenseExpiry = AppSettings.LicenseExpiry,
                PlanId = AppSettings.PlanId,
                MaxPosDevices = AppSettings.MaxPosDevices,
                CurrentClientCount = ConnectedClients.Count
            };
            
            string json = JsonConvert.SerializeObject(broadcastData);
            byte[] data = Encoding.UTF8.GetBytes(json);
            
            udpClient.Send(data, data.Length, endpoint);
            
            Thread.Sleep(3000); // Broadcast every 3 seconds
        }
    }
}
```

**Client Discovery**:
```csharp
public class HostDiscovery
{
    public async Task<List<HostBroadcastMessage>> DiscoverHosts(int timeoutSeconds = 10)
    {
        var hosts = new List<HostBroadcastMessage>();
        var udpClient = new UdpClient(MULTICAST_PORT);
        udpClient.JoinMulticastGroup(IPAddress.Parse(MULTICAST_ADDRESS));
        
        var endpoint = new IPEndPoint(IPAddress.Any, 0);
        var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        
        while (!cancellation.Token.IsCancellationRequested)
        {
            try
            {
                var result = await udpClient.ReceiveAsync();
                string json = Encoding.UTF8.GetString(result.Buffer);
                var host = JsonConvert.DeserializeObject<HostBroadcastMessage>(json);
                
                if (host != null && !hosts.Any(h => h.HostIp == host.HostIp))
                {
                    hosts.Add(host);
                }
            }
            catch { }
        }
        
        return hosts;
    }
}
```

---

### Connection Handshake

**Client → Host**:
```csharp
public async Task<ConnectionToken> ConnectToHost(string hostIp)
{
    var httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri($"https://{hostIp}:8443");
    
    var request = new ConnectionRequest
    {
        ClientMachineFingerprint = MachineFingerprint.Generate(),
        ClientDisplayName = Environment.MachineName,
        ClientVersion = AppSettings.Version
    };
    
    var response = await httpClient.PostAsJsonAsync("/api/host/connect", request);
    response.EnsureSuccessStatusCode();
    
    var token = await response.Content.ReadAsAsync<ConnectionToken>();
    
    // Store token for API calls
    SecureStorage.Save("HostConnectionToken", token.Token);
    
    return token;
}
```

**Host Validation**:
```csharp
public ConnectionToken ValidateConnection(ConnectionRequest request)
{
    // Check device limit
    if (ConnectedClients.Count >= AppSettings.MaxPosDevices)
        throw new Exception("Maximum device limit reached");
    
    // Check if fingerprint already connected
    if (ConnectedClients.Any(c => c.Fingerprint == request.ClientMachineFingerprint))
        throw new Exception("Device already connected");
    
    // Generate session token
    var token = new ConnectionToken
    {
        Token = GenerateSecureToken(),
        HostDatabaseUrl = "Server=localhost;Database=ChronoPOS;...",
        ExpiresAt = DateTime.UtcNow.AddHours(24)
    };
    
    // Register client
    ConnectedClients.Add(new ConnectedClient
    {
        Fingerprint = request.ClientMachineFingerprint,
        DisplayName = request.ClientDisplayName,
        Token = token.Token,
        ConnectedAt = DateTime.UtcNow
    });
    
    return token;
}
```

---

### Real-Time Sync

**Use SignalR for push updates**:

```csharp
// Host Hub
public class POSHub : Hub
{
    public async Task NotifyInventoryChange(InventoryItem item)
    {
        await Clients.Others.SendAsync("InventoryUpdated", item);
    }
    
    public async Task NotifySale(Sale sale)
    {
        await Clients.All.SendAsync("SaleCompleted", sale);
    }
}

// Client
public class POSClient
{
    private HubConnection _connection;
    
    public async Task Initialize(string hostIp, string token)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"https://{hostIp}:8443/pos-hub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token);
            })
            .Build();
        
        _connection.On<InventoryItem>("InventoryUpdated", item =>
        {
            // Update local UI
            UpdateInventoryDisplay(item);
        });
        
        _connection.On<Sale>("SaleCompleted", sale =>
        {
            // Refresh sales dashboard
            RefreshSalesDashboard();
        });
        
        await _connection.StartAsync();
    }
}
```

---

## ✅ Testing Checklist

### Encryption Tests
- [ ] Scratch card decryption matches admin panel encryption
- [ ] Sales key encryption readable by admin panel
- [ ] License key decryption works correctly
- [ ] Machine fingerprint is stable across reboots

### Onboarding Tests
- [ ] QR code scan successfully decrypts scratch card
- [ ] Manual display code validation works
- [ ] Dubai business info validation enforces rules
- [ ] Sales key generation includes all required data
- [ ] License activation validates machine fingerprint
- [ ] License activation validates expiry date
- [ ] Mismatched license shows clear error

### Host-Connect Tests
- [ ] Host broadcast detected by clients
- [ ] Client can connect to host
- [ ] Max device limit enforced
- [ ] Real-time sync updates all clients
- [ ] Disconnected client auto-reconnects
- [ ] Host offline shows error message

---

## 📚 Sample Test Vector

### Test Scratch Card
```json
{
  "CardCode": "ABC123XYZ789",
  "DisplayCode": "QWER-1234-ASDF-5678",
  "PlanId": 2,
  "PlanName": "Professional Plan",
  "PlanPrice": 59.99,
  "DurationInDays": 30,
  "SalespersonId": 5,
  "SalespersonName": "Jane Smith",
  "SalespersonEmail": "jane@chronopos.com",
  "CreatedAt": "2025-10-01T12:00:00Z",
  "ExpiryDate": "2026-10-01T12:00:00Z",
  "BatchNumber": "BATCH_20251001"
}
```

**Encrypted (for QR)**:
Use admin panel to generate actual encrypted value with matching parameters.

---

## 🎓 Key Takeaways

1. **Always use Display Code for user entry** - CardCode is internal only
2. **QR scanning is primary** - Manual entry is fallback
3. **Machine fingerprint must be stable** - Test across reboots
4. **Encryption parameters must match exactly** - Use constants
5. **Host-connect requires license with MaxPosDevices > 1**
6. **Real-time sync prevents write conflicts** - Use SignalR + optimistic concurrency

---

## 📞 Support & Questions

For implementation questions, refer to:
- Admin panel source code: `ChronoPosAdminPanel/Services/CryptographyService.cs`
- This document
- SCRATCH_CARD_CODE_ARCHITECTURE.md

**End of Implementation Guide**
