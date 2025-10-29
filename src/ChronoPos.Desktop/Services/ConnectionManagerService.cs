using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ChronoPos.Desktop.Models.Licensing;
using ChronoPos.Application.Logging;

namespace ChronoPos.Desktop.Services
{
    /// <summary>
    /// Manages client connections to host device
    /// </summary>
    public interface IConnectionManagerService
    {
        ConnectionToken GenerateToken(string clientFingerprint, string clientIp, string hostIp, string databaseUncPath, int planId, int maxPosDevices);
        bool ValidateToken(string token, string clientFingerprint);
        bool CanAcceptNewClient();
        List<ConnectedClient> GetConnectedClients();
        void RegisterClient(ConnectedClient client);
        void UnregisterClient(string clientFingerprint);
        int GetConnectedClientCount();
    }

    public class ConnectionManagerService : IConnectionManagerService
    {
        private readonly List<ConnectedClient> _connectedClients = new();
        private readonly ILicensingService _licensingService;

        public ConnectionManagerService(ILicensingService licensingService)
        {
            _licensingService = licensingService;
        }

        public ConnectionToken GenerateToken(string clientFingerprint, string clientIp, string hostIp, string databaseUncPath, int planId, int maxPosDevices)
        {
            var token = new ConnectionToken
            {
                Token = GenerateSecureToken(),
                HostIp = hostIp,
                HostName = Environment.MachineName,
                DatabaseUncPath = databaseUncPath,
                DatabaseShareName = "ChronoPosDB",
                IssuedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(365), // 1 year validity
                ClientFingerprint = clientFingerprint,
                PlanId = planId,
                MaxPosDevices = maxPosDevices
            };

            AppLogger.Log($"Generated connection token for client {clientFingerprint} from {clientIp}", "ConnectionManager", "licensing");
            return token;
        }

        public bool ValidateToken(string token, string clientFingerprint)
        {
            var client = _connectedClients.FirstOrDefault(c => c.Fingerprint == clientFingerprint);
            if (client == null)
            {
                AppLogger.Log($"Token validation failed: Client {clientFingerprint} not found", "ConnectionManager", "licensing");
                return false;
            }

            if (client.Token != token)
            {
                AppLogger.Log($"Token validation failed: Token mismatch for {clientFingerprint}", "ConnectionManager", "licensing");
                return false;
            }

            // Update last seen
            client.LastSeenAt = DateTime.UtcNow;
            return true;
        }

        public bool CanAcceptNewClient()
        {
            var license = _licensingService.GetCurrentLicense();
            if (license == null)
            {
                AppLogger.Log("Cannot accept new client: No license found", "ConnectionManager", "licensing");
                return false;
            }

            // No device limit check - allow unlimited clients
            var activeClients = _connectedClients.Count(c => c.IsActive);
            AppLogger.Log($"Active clients: {activeClients}, Max devices (license): {license.MaxPosDevices} (limit not enforced)", "ConnectionManager", "licensing");
            return true; // Always accept new clients if license is valid
        }

        public List<ConnectedClient> GetConnectedClients()
        {
            return _connectedClients.Where(c => c.IsActive).ToList();
        }

        public void RegisterClient(ConnectedClient client)
        {
            // Check if client already exists
            var existing = _connectedClients.FirstOrDefault(c => c.Fingerprint == client.Fingerprint);
            if (existing != null)
            {
                // Update existing client
                existing.LastSeenAt = DateTime.UtcNow;
                existing.IsActive = true;
                existing.IpAddress = client.IpAddress;
                AppLogger.Log($"Updated existing client: {client.DisplayName} ({client.Fingerprint})", "ConnectionManager", "licensing");
            }
            else
            {
                // Add new client
                client.ConnectedAt = DateTime.UtcNow;
                client.LastSeenAt = DateTime.UtcNow;
                client.IsActive = true;
                _connectedClients.Add(client);
                AppLogger.Log($"Registered new client: {client.DisplayName} ({client.Fingerprint}) from {client.IpAddress}", "ConnectionManager", "licensing");
            }
        }

        public void UnregisterClient(string clientFingerprint)
        {
            var client = _connectedClients.FirstOrDefault(c => c.Fingerprint == clientFingerprint);
            if (client != null)
            {
                client.IsActive = false;
                AppLogger.Log($"Unregistered client: {client.DisplayName} ({clientFingerprint})", "ConnectionManager", "licensing");
            }
        }

        public int GetConnectedClientCount()
        {
            return _connectedClients.Count(c => c.IsActive);
        }

        private string GenerateSecureToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var tokenBytes = new byte[32];
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
    }
}
