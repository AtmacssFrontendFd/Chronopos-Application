using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Services
{
    /// <summary>
    /// Generates stable machine-bound hardware fingerprint
    /// </summary>
    public static class MachineFingerprint
    {
        public static string Generate()
        {
            var components = new List<string>();

            try
            {
                AppLogger.Log("=== MACHINE FINGERPRINT GENERATION START ===", "MachineFingerprint", "licensing");
                
                // Primary MAC Address
                var mac = GetPrimaryMacAddress();
                AppLogger.Log($"1. MAC Address: {mac}", "MachineFingerprint", "licensing");
                if (!string.IsNullOrEmpty(mac))
                    components.Add($"MAC:{mac}");

                // Motherboard Serial Number
                var motherboardSerial = GetWMIProperty("Win32_BaseBoard", "SerialNumber");
                AppLogger.Log($"2. Motherboard Serial: {motherboardSerial}", "MachineFingerprint", "licensing");
                if (!string.IsNullOrEmpty(motherboardSerial))
                    components.Add($"MB:{motherboardSerial}");

                // BIOS UUID
                var biosUuid = GetWMIProperty("Win32_ComputerSystemProduct", "UUID");
                AppLogger.Log($"3. BIOS UUID: {biosUuid}", "MachineFingerprint", "licensing");
                if (!string.IsNullOrEmpty(biosUuid))
                    components.Add($"BIOS:{biosUuid}");

                // System Drive Volume Serial
                var volumeSerial = GetVolumeSerial("C:");
                AppLogger.Log($"4. Volume Serial (C:): {volumeSerial}", "MachineFingerprint", "licensing");
                if (!string.IsNullOrEmpty(volumeSerial))
                    components.Add($"VOL:{volumeSerial}");

                // Machine Name
                var machineName = Environment.MachineName;
                AppLogger.Log($"5. Machine Name: {machineName}", "MachineFingerprint", "licensing");
                components.Add($"NAME:{machineName}");

                // OS Version
                var osVersion = Environment.OSVersion.ToString();
                AppLogger.Log($"6. OS Version: {osVersion}", "MachineFingerprint", "licensing");
                components.Add($"OS:{osVersion}");

                // Processor Count
                var processorCount = Environment.ProcessorCount;
                AppLogger.Log($"7. Processor Count: {processorCount}", "MachineFingerprint", "licensing");
                components.Add($"CPU:{processorCount}");

                // Concatenate and hash
                var fingerprint = string.Join("|", components);
                AppLogger.Log($"8. Combined String: {fingerprint}", "MachineFingerprint", "licensing");
                
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprint));
                var finalFingerprint = Convert.ToBase64String(hash);
                
                AppLogger.Log($"9. Final Fingerprint Hash: {finalFingerprint}", "MachineFingerprint", "licensing");
                AppLogger.Log("=== MACHINE FINGERPRINT GENERATION COMPLETE ===", "MachineFingerprint", "licensing");
                
                return finalFingerprint;
            }
            catch (Exception ex)
            {
                // Fallback to basic fingerprint
                AppLogger.LogError("Error generating fingerprint, using fallback", ex, filename: "licensing");
                Console.WriteLine($"Error generating fingerprint: {ex.Message}");
                var fallback = $"{Environment.MachineName}|{Environment.OSVersion}|{Environment.ProcessorCount}";
                AppLogger.Log($"Fallback String: {fallback}", "MachineFingerprint", "licensing");
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fallback));
                var finalFallback = Convert.ToBase64String(hash);
                AppLogger.Log($"Fallback Fingerprint: {finalFallback}", "MachineFingerprint", "licensing");
                return finalFallback;
            }
        }

        private static string GetPrimaryMacAddress()
        {
            try
            {
                AppLogger.Log("  → Searching for primary MAC address...", "MachineFingerprint", "licensing");
                
                var allInterfaces = NetworkInterface.GetAllNetworkInterfaces();
                AppLogger.Log($"  → Found {allInterfaces.Length} network interfaces", "MachineFingerprint", "licensing");
                
                foreach (var iface in allInterfaces)
                {
                    AppLogger.Log($"    - {iface.Name}: Type={iface.NetworkInterfaceType}, Status={iface.OperationalStatus}, Speed={iface.Speed}, MAC={iface.GetPhysicalAddress()}", 
                        "MachineFingerprint", "licensing");
                }
                
                // STABLE SELECTION: Get all physical adapters with valid MAC addresses
                // Order by: Ethernet > Wireless > Others, then by MAC address for consistency
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .Where(n => n.GetPhysicalAddress().ToString().Length >= 12) // Valid MAC address
                    .OrderByDescending(n => n.NetworkInterfaceType == NetworkInterfaceType.Ethernet ? 3 :
                                           n.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ? 2 : 1)
                    .ThenBy(n => n.GetPhysicalAddress().ToString()) // Consistent ordering by MAC
                    .FirstOrDefault();

                var macAddress = nic?.GetPhysicalAddress().ToString() ?? "UNKNOWN";
                AppLogger.Log($"  → Selected primary MAC: {macAddress} from {nic?.Name} (Type: {nic?.NetworkInterfaceType}, Status: {nic?.OperationalStatus})", "MachineFingerprint", "licensing");
                return macAddress;
            }
            catch (Exception ex)
            {
                AppLogger.LogError("  → Error getting MAC address", ex, filename: "licensing");
                return "UNKNOWN";
            }
        }

        private static string GetWMIProperty(string className, string propertyName)
        {
            try
            {
                AppLogger.Log($"  → Querying WMI: {className}.{propertyName}", "MachineFingerprint", "licensing");
                using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}");
                foreach (var obj in searcher.Get())
                {
                    var value = obj[propertyName]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        AppLogger.Log($"  → WMI Result: {value}", "MachineFingerprint", "licensing");
                        return value;
                    }
                }
                AppLogger.Log($"  → WMI returned no value", "MachineFingerprint", "licensing");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"  → WMI Error for {className}.{propertyName}", ex, filename: "licensing");
            }
            return string.Empty;
        }

        private static string GetVolumeSerial(string driveLetter)
        {
            try
            {
                AppLogger.Log($"  → Querying volume serial for {driveLetter}", "MachineFingerprint", "licensing");
                using var searcher = new ManagementObjectSearcher($"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID = '{driveLetter}'");
                foreach (var obj in searcher.Get())
                {
                    var value = obj["VolumeSerialNumber"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        AppLogger.Log($"  → Volume Serial: {value}", "MachineFingerprint", "licensing");
                        return value;
                    }
                }
                AppLogger.Log($"  → No volume serial found for {driveLetter}", "MachineFingerprint", "licensing");
            }
            catch (Exception ex)
            {
                AppLogger.LogError($"  → Error getting volume serial for {driveLetter}", ex, filename: "licensing");
            }
            return string.Empty;
        }
    }
}
