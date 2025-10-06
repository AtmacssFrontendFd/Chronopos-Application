using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

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
                // Primary MAC Address
                var mac = GetPrimaryMacAddress();
                if (!string.IsNullOrEmpty(mac))
                    components.Add($"MAC:{mac}");

                // Motherboard Serial Number
                var motherboardSerial = GetWMIProperty("Win32_BaseBoard", "SerialNumber");
                if (!string.IsNullOrEmpty(motherboardSerial))
                    components.Add($"MB:{motherboardSerial}");

                // BIOS UUID
                var biosUuid = GetWMIProperty("Win32_ComputerSystemProduct", "UUID");
                if (!string.IsNullOrEmpty(biosUuid))
                    components.Add($"BIOS:{biosUuid}");

                // System Drive Volume Serial
                var volumeSerial = GetVolumeSerial("C:");
                if (!string.IsNullOrEmpty(volumeSerial))
                    components.Add($"VOL:{volumeSerial}");

                // Machine Name
                components.Add($"NAME:{Environment.MachineName}");

                // OS Version
                components.Add($"OS:{Environment.OSVersion}");

                // Processor Count
                components.Add($"CPU:{Environment.ProcessorCount}");

                // Concatenate and hash
                var fingerprint = string.Join("|", components);
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fingerprint));
                return Convert.ToBase64String(hash);
            }
            catch (Exception ex)
            {
                // Fallback to basic fingerprint
                Console.WriteLine($"Error generating fingerprint: {ex.Message}");
                var fallback = $"{Environment.MachineName}|{Environment.OSVersion}|{Environment.ProcessorCount}";
                using var sha256 = SHA256.Create();
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(fallback));
                return Convert.ToBase64String(hash);
            }
        }

        private static string GetPrimaryMacAddress()
        {
            try
            {
                var nic = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up)
                    .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                    .OrderByDescending(n => n.Speed)
                    .FirstOrDefault();

                return nic?.GetPhysicalAddress().ToString() ?? "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        private static string GetWMIProperty(string className, string propertyName)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {className}");
                foreach (var obj in searcher.Get())
                {
                    var value = obj[propertyName]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }
            catch
            {
                // Ignore WMI errors
            }
            return string.Empty;
        }

        private static string GetVolumeSerial(string driveLetter)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher($"SELECT VolumeSerialNumber FROM Win32_LogicalDisk WHERE DeviceID = '{driveLetter}'");
                foreach (var obj in searcher.Get())
                {
                    var value = obj["VolumeSerialNumber"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }
            catch
            {
                // Ignore errors
            }
            return string.Empty;
        }
    }
}
