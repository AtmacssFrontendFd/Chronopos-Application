using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Services
{
    /// <summary>
    /// Manages database folder sharing and network paths
    /// </summary>
    public interface IDatabaseSharingService
    {
        string GetLocalIpAddress();
        string GetDatabaseSharePath(string localDatabasePath);
        string GetUncPath(string shareName);
        bool ValidateNetworkPath(string uncPath);
        void EnsureDatabaseFolderExists(string path);
    }

    public class DatabaseSharingService : IDatabaseSharingService
    {
        private const string SHARE_NAME = "ChronoPosDB";

        public string GetLocalIpAddress()
        {
            try
            {
                // Get all network interfaces
                var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                                 ni.NetworkInterfaceType != NetworkInterfaceType.Loopback);

                foreach (var ni in interfaces)
                {
                    var properties = ni.GetIPProperties();
                    var ipv4 = properties.UnicastAddresses
                        .FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                                             !IPAddress.IsLoopback(ip.Address));

                    if (ipv4 != null)
                    {
                        var ip = ipv4.Address.ToString();
                        AppLogger.Log($"Local IP address detected: {ip}", "DatabaseSharing", "network");
                        return ip;
                    }
                }

                // Fallback method
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                var localIp = endPoint?.Address.ToString() ?? "127.0.0.1";
                AppLogger.Log($"Local IP address (fallback): {localIp}", "DatabaseSharing", "network");
                return localIp;
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to get local IP address", ex, "Using localhost as fallback", "network");
                return "127.0.0.1";
            }
        }

        public string GetDatabaseSharePath(string localDatabasePath)
        {
            try
            {
                var databaseFolder = Path.GetDirectoryName(localDatabasePath);
                if (string.IsNullOrEmpty(databaseFolder))
                {
                    throw new InvalidOperationException("Invalid database path");
                }

                EnsureDatabaseFolderExists(databaseFolder);

                // For Windows, we'll use the standard sharing approach
                // The actual share creation requires admin privileges or manual setup
                // So we'll return the UNC path format that should be manually configured
                var localIp = GetLocalIpAddress();
                var uncPath = $"\\\\{localIp}\\{SHARE_NAME}\\{Path.GetFileName(localDatabasePath)}";

                AppLogger.Log($"Database share path: {uncPath}", "DatabaseSharing", "network");
                AppLogger.Log($"Local database path: {localDatabasePath}", "DatabaseSharing", "network");
                AppLogger.Log($"IMPORTANT: Please manually share the folder: {databaseFolder} as '{SHARE_NAME}'", "DatabaseSharing", "network");

                return uncPath;
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to get database share path", ex, localDatabasePath, "network");
                throw;
            }
        }

        public string GetUncPath(string shareName)
        {
            var localIp = GetLocalIpAddress();
            var uncPath = $"\\\\{localIp}\\{shareName}";
            AppLogger.Log($"Generated UNC path: {uncPath}", "DatabaseSharing", "network");
            return uncPath;
        }

        public bool ValidateNetworkPath(string uncPath)
        {
            try
            {
                if (string.IsNullOrEmpty(uncPath))
                {
                    AppLogger.Log("Network path validation failed: Empty path", "DatabaseSharing", "network");
                    return false;
                }

                // Check if path is accessible
                var exists = File.Exists(uncPath) || Directory.Exists(uncPath);
                AppLogger.Log($"Network path validation: {uncPath} - {(exists ? "Accessible" : "Not accessible")}", "DatabaseSharing", "network");
                return exists;
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Network path validation failed", ex, uncPath, "network");
                return false;
            }
        }

        public void EnsureDatabaseFolderExists(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    AppLogger.Log($"Created database folder: {path}", "DatabaseSharing", "filesystem");
                }
            }
            catch (Exception ex)
            {
                AppLogger.LogError("Failed to create database folder", ex, path, "filesystem");
                throw;
            }
        }
    }
}
