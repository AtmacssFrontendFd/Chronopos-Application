# ChronoPOS Encryption & Decryption Reference

## Purpose

This document is the single source of truth for every cryptographic operation used across the ChronoPOS ecosystem (Admin Panel, POS client, host-connected devices, and supporting services). Implementations **must** follow these specifications exactly to guarantee interoperability.

---

## Cryptographic Parameters

| Setting | Value | Notes |
| --- | --- | --- |
| Master symmetric secret | `ChronoPos2025!@#$%^&*()_+SecureKey` | Shared across all apps; keep confidential |
| Scratch card salt | `ScratchCard_Salt_2025` | Concatenate with master secret before key derivation |
| Sales key salt | `SalesKey_Salt_2025` | Concatenate with master secret before key derivation |
| License key salt | `LicenseKey_Salt_2025` | Concatenate with master secret before key derivation |
| PBKDF2 salt | `ChronoPosSalt2025` (UTF-8 bytes) | Static salt for PBKDF2 | 
| PBKDF2 iterations | 10,000 | Increase requires coordination across all apps |
| Derived key length | 32 bytes | AES-256 |
| Derived IV length | 16 bytes | AES block size |
| Cipher | AES (CBC) | `System.Security.Cryptography.Aes` defaults |
| Padding | PKCS7 | Provided by .NET AES implementation |
| Encoding | UTF-8 → Base64 | JSON payloads serialized to UTF-8, encrypted bytes encoded to Base64 |

> 🔐 **Never** alter these values in production. Changing any constant breaks compatibility with existing data.

---

## Process-to-Algorithm Map

| Process | Encryption/Hash Algorithm | Key / Salt Combination | Notes |
| --- | --- | --- | --- |
| Scratch card payload encryption (Admin → POS) | AES-256-CBC + PBKDF2 (SHA-256) | `MASTER_KEY + SCRATCH_CARD_SALT` with PBKDF2 salt `ChronoPosSalt2025` | Produces Base64 ciphertext stored in DB + QR code |
| Scratch card payload decryption (POS) | AES-256-CBC + PBKDF2 (SHA-256) | `MASTER_KEY + SCRATCH_CARD_SALT` with PBKDF2 salt `ChronoPosSalt2025` | Reverse of admin encryption; validates expiry/checksum |
| Sales key generation (POS → Admin) | AES-256-CBC + PBKDF2 (SHA-256) | `MASTER_KEY + SALES_KEY_SALT` with PBKDF2 salt `ChronoPosSalt2025` | Encrypts onboarded customer + system fingerprint |
| Sales key decryption (Admin) | AES-256-CBC + PBKDF2 (SHA-256) | `MASTER_KEY + SALES_KEY_SALT` with PBKDF2 salt `ChronoPosSalt2025` | Admin inspects payload prior to license issuance |
| License key issuance (Admin → POS) | AES-256-CBC + PBKDF2 (SHA-256) | `MASTER_KEY + LICENSE_KEY_SALT` with PBKDF2 salt `ChronoPosSalt2025` | Embeds encrypted sales key, plan info, device limits |
| License key activation (POS) | AES-256-CBC + PBKDF2 (SHA-256) | `MASTER_KEY + LICENSE_KEY_SALT` with PBKDF2 salt `ChronoPosSalt2025` | Validates fingerprint, expiry, stored sales key |
| Display code checksum | SHA-256 → Base36 | No secret key; deterministic hash | Used for offline format validation |
| Machine fingerprinting | SHA-256 hash of hardware identifiers | No secret key; base64 output | Must remain stable across reboots |

Use this table as the authoritative reference when wiring each onboarding stage.

---

## Shared Data Contracts

```csharp
public class ScratchCardInfo
{
	public string CardCode { get; set; }
	public string DisplayCode { get; set; }
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

public class SalesKeyInfo
{
	public string ScratchCardCode { get; set; }
	public CustomerInfo Customer { get; set; }
	public SystemInfo System { get; set; }
	public DateTime CreatedAt { get; set; }
	public int SalespersonId { get; set; }
}

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

All applications must serialize payloads into JSON using the property names above before encryption.

---

## Core Helper Functions

```csharp
private static string EncryptString(string plainText, string key)
{
	byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

	using var aes = Aes.Create();
	using var derivation = new Rfc2898DeriveBytes(
		key,
		Encoding.UTF8.GetBytes("ChronoPosSalt2025"),
		10000,
		HashAlgorithmName.SHA256);

	aes.Key = derivation.GetBytes(32);   // 256-bit key
	aes.IV = derivation.GetBytes(16);    // 128-bit IV

	using var encryptor = aes.CreateEncryptor();
	using var ms = new MemoryStream();
	using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
	{
		cs.Write(plainBytes, 0, plainBytes.Length);
		cs.FlushFinalBlock();
	}

	return Convert.ToBase64String(ms.ToArray());
}

private static string DecryptString(string encryptedBase64, string key)
{
	byte[] cipherBytes = Convert.FromBase64String(encryptedBase64);

	using var aes = Aes.Create();
	using var derivation = new Rfc2898DeriveBytes(
		key,
		Encoding.UTF8.GetBytes("ChronoPosSalt2025"),
		10000,
		HashAlgorithmName.SHA256);

	aes.Key = derivation.GetBytes(32);
	aes.IV = derivation.GetBytes(16);

	using var decryptor = aes.CreateDecryptor();
	using var ms = new MemoryStream(cipherBytes);
	using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
	using var reader = new StreamReader(cs);

	return reader.ReadToEnd();
}
```

> ✅ The admin panel already exposes these methods through `ChronoPosAdminPanel.Services.CryptographyService`.

---

## Scratch Card Encryption Workflow

1. Create a populated `ScratchCardInfo` object.
2. Serialize to JSON.
3. Concatenate `MASTER_KEY + SCRATCH_CARD_SALT` to form the password for PBKDF2.
4. Call `EncryptString(json, password)` → Base64 ciphertext.
5. Store the ciphertext in `ScratchCard.EncryptedData` (database) and encode it in the printed QR code.

### Decryption (POS Client)

1. Read QR payload or fetch encrypted data via API.
2. Call `DecryptScratchCard<ScratchCardInfo>(cipherText)` where implementation uses the shared helper above.
3. Validate:
   - `ExpiryDate >= DateTime.UtcNow`.
   - Display code format + checksum matches `DisplayCode`.
   - Optionally check card code against backend record.

---

## Sales Key Encryption Workflow

1. Gather onboarding data on the POS device:
   - `ScratchCardCode` (internal card code).
   - Customer profile (Dubai-compliant fields).
   - Machine fingerprint (MAC, motherboard serial, BIOS UUID, drive serial, etc. hashed via SHA-256 → Base64).
2. Populate `SalesKeyInfo` and serialize to JSON.
3. Password = `MASTER_KEY + SALES_KEY_SALT`.
4. Encrypt via `EncryptString` → Base64 sales key.
5. Salesperson sends encrypted sales key to admin.

### Decryption (Admin Panel)

1. Call `DecryptSalesKey<SalesKeyInfo>(encryptedSalesKey)`.
2. Inspect decoded payload to confirm:
   - Scratch card references a valid record.
   - Machine fingerprint is unique.
   - Customer data is complete.

---

## License Key Encryption Workflow

1. Admin validates sales key and composes `LicenseKeyInfo`:
   - Copy the encrypted sales key into `LicenseKeyInfo.SalesKey`.
   - Populate plan, expiry, device limits, enabled features, machine fingerprint (copied from sales key).
2. Serialize to JSON.
3. Password = `MASTER_KEY + LICENSE_KEY_SALT`.
4. Encrypt via `EncryptString`.
5. Deliver Base64 license string (or `.chronopos-license` file) back to salesperson.

### Decryption (POS Client)

1. User pastes or loads license.
2. Call `DecryptLicenseKey<LicenseKeyInfo>(encryptedLicense)`.
3. Validate before activation:
   - Current machine fingerprint matches `MachineFingerprint` from license (compare Base64 strings).
   - License has not expired.
   - Stored sales key equals `LicenseKeyInfo.SalesKey`.
   - Plan/device constraints align with feature toggles.
4. Persist encrypted license and decoded metadata securely.

---

## Machine Fingerprint Guidance

Construct a stable fingerprint using multiple hardware identifiers to mitigate spoofing:

```csharp
string fingerprint = string.Join("|", new[]
{
	NetworkUtilities.GetPrimaryMacAddress(),
	HardwareInfo.GetMotherboardSerial(),
	HardwareInfo.GetBiosUuid(),
	HardwareInfo.GetSystemDriveSerial(),
	Environment.MachineName,
	Environment.OSVersion.ToString(),
	Environment.ProcessorCount.ToString()
});

using var sha256 = SHA256.Create();
string machineFingerprint = Convert.ToBase64String(
	sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprint)));
```

Cache and reuse the generated value to ensure the sales key and subsequent license validation stay in sync.

---

## Sample Test Vector

```json
// ScratchCardInfo (plaintext)
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

Encrypt the JSON above with the scratch card parameters to verify cross-implementation compatibility. Use the admin panel to produce the canonical ciphertext for QA comparisons.

---

## Error Handling & Validation Checklist

- Base64 decoding failures → treat as tampered input.
- AES decryption exceptions → return generic "Invalid key" message without revealing internals.
- JSON deserialization errors → log internally, show user-friendly error.
- Date checks must use UTC to avoid timezone issues.
- Ensure secure storage (DPAPI / Keychain / ProtectedData) for encrypted sales key, license, and fingerprint snapshots.
- Audit logs should record activation attempts but never store plaintext secrets.

---

## Reference Implementation

- Admin panel source: `Services/CryptographyService.cs`
- POS developer guide: `POS_DEVELOPER_IMPLEMENTATION_GUIDE.md`
- Scratch card code architecture: `SCRATCH_CARD_CODE_ARCHITECTURE.md`

Keep this document in sync with the service code whenever parameters change. Any deviation requires reissuing all keys and immediate coordination with every deployed POS instance.

