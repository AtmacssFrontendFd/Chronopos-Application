using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using ChronoPos.Desktop.Models.Licensing;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Services
{
    /// <summary>
    /// Core licensing service implementing ChronoPOS cryptography contract
    /// </summary>
    public interface ILicensingService
    {
        ScratchCardInfo? DecryptScratchCard(string encryptedBase64);
        string EncryptSalesKey(SalesKeyInfo salesKeyInfo);
        LicenseKeyInfo? DecryptLicenseKey(string encryptedBase64);
        bool IsLicenseValid();
        LicenseKeyInfo? GetCurrentLicense();
        void SaveLicense(string encryptedLicense);
        void SaveSalesKey(string encryptedSalesKey);
        string? GetSavedSalesKey();
    }

    public class LicensingService : ILicensingService
    {
        // Cryptography constants - MUST match admin panel exactly
        private const string MASTER_KEY = "ChronoPos2025!@#$%^&*()_+SecureKey";
        private const string SCRATCH_CARD_SALT = "ScratchCard_Salt_2025";
        private const string SALES_KEY_SALT = "SalesKey_Salt_2025";
        private const string LICENSE_KEY_SALT = "LicenseKey_Salt_2025";
        private const string KEY_DERIVATION_SALT = "ChronoPosSalt2025";
        private const int PBKDF2_ITERATIONS = 10000;
        private const int AES_KEY_SIZE = 32;  // 256-bit
        private const int AES_IV_SIZE = 16;   // 128-bit

        private readonly string _appDataPath;

        public LicensingService()
        {
            _appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ChronoPos",
                "Licensing"
            );
            Directory.CreateDirectory(_appDataPath);
        }

        public ScratchCardInfo? DecryptScratchCard(string encryptedBase64)
        {
            try
            {
                string key = MASTER_KEY + SCRATCH_CARD_SALT;
                string decryptedJson = DecryptString(encryptedBase64, key);
                return JsonConvert.DeserializeObject<ScratchCardInfo>(decryptedJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decrypt scratch card: {ex.Message}");
                return null;
            }
        }

        public string EncryptSalesKey(SalesKeyInfo salesKeyInfo)
        {
            string key = MASTER_KEY + SALES_KEY_SALT;
            string jsonData = JsonConvert.SerializeObject(salesKeyInfo);
            return EncryptString(jsonData, key);
        }

        public LicenseKeyInfo? DecryptLicenseKey(string encryptedBase64)
        {
            try
            {
                string key = MASTER_KEY + LICENSE_KEY_SALT;
                string decryptedJson = DecryptString(encryptedBase64, key);
                return JsonConvert.DeserializeObject<LicenseKeyInfo>(decryptedJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to decrypt license key: {ex.Message}");
                return null;
            }
        }

        public bool IsLicenseValid()
        {
            var license = GetCurrentLicense();
            if (license == null)
            {
                AppLogger.Log("License validation failed: No license found", "LicensingService", "licensing");
                return false;
            }

            // Check expiry
            if (license.ExpiryDate < DateTime.UtcNow)
            {
                AppLogger.Log($"License validation failed: License expired on {license.ExpiryDate:yyyy-MM-dd}", "LicensingService", "licensing");
                return false;
            }

            // Check machine fingerprint
            var currentFingerprint = MachineFingerprint.Generate();
            if (license.MachineFingerprint != currentFingerprint)
            {
                AppLogger.Log($"License validation failed: Machine fingerprint mismatch", "LicensingService", "licensing");
                AppLogger.Log($"Expected fingerprint: {license.MachineFingerprint}", "DEBUG", "licensing");
                AppLogger.Log($"Current fingerprint: {currentFingerprint}", "DEBUG", "licensing");
                return false;
            }

            AppLogger.Log($"License validation successful. Expires: {license.ExpiryDate:yyyy-MM-dd}", "LicensingService", "licensing");
            return true;
        }

        public LicenseKeyInfo? GetCurrentLicense()
        {
            try
            {
                var licensePath = Path.Combine(_appDataPath, "license.dat");
                if (!File.Exists(licensePath))
                    return null;

                var encryptedLicense = File.ReadAllText(licensePath);
                return DecryptLicenseKey(encryptedLicense);
            }
            catch
            {
                return null;
            }
        }

        public void SaveLicense(string encryptedLicense)
        {
            var licensePath = Path.Combine(_appDataPath, "license.dat");
            File.WriteAllText(licensePath, encryptedLicense);

            // Also save activation flag
            var activationPath = Path.Combine(_appDataPath, "activated.flag");
            File.WriteAllText(activationPath, DateTime.UtcNow.ToString("O"));
        }

        public void SaveSalesKey(string encryptedSalesKey)
        {
            var salesKeyPath = Path.Combine(_appDataPath, "saleskey.dat");
            File.WriteAllText(salesKeyPath, encryptedSalesKey);
        }

        public string? GetSavedSalesKey()
        {
            try
            {
                var salesKeyPath = Path.Combine(_appDataPath, "saleskey.dat");
                if (!File.Exists(salesKeyPath))
                    return null;
                return File.ReadAllText(salesKeyPath);
            }
            catch
            {
                return null;
            }
        }

        private string EncryptString(string plainText, string key)
        {
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var aes = Aes.Create();
            using var derivation = new Rfc2898DeriveBytes(
                key,
                Encoding.UTF8.GetBytes(KEY_DERIVATION_SALT),
                PBKDF2_ITERATIONS,
                HashAlgorithmName.SHA256);

            aes.Key = derivation.GetBytes(AES_KEY_SIZE);
            aes.IV = derivation.GetBytes(AES_IV_SIZE);

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
                cs.FlushFinalBlock();
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private string DecryptString(string encryptedBase64, string key)
        {
            byte[] cipherBytes = Convert.FromBase64String(encryptedBase64);

            using var aes = Aes.Create();
            using var derivation = new Rfc2898DeriveBytes(
                key,
                Encoding.UTF8.GetBytes(KEY_DERIVATION_SALT),
                PBKDF2_ITERATIONS,
                HashAlgorithmName.SHA256);

            aes.Key = derivation.GetBytes(AES_KEY_SIZE);
            aes.IV = derivation.GetBytes(AES_IV_SIZE);

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(cipherBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);

            return reader.ReadToEnd();
        }
    }
}
