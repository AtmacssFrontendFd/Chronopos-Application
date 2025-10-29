using System;

namespace ChronoPos.Desktop.Models.Licensing
{
    /// <summary>
    /// Token issued by host to client for database access
    /// </summary>
    public class ConnectionToken
    {
        public string Token { get; set; } = string.Empty;
        public string HostIp { get; set; } = string.Empty;
        public string HostName { get; set; } = string.Empty;
        public string DatabaseUncPath { get; set; } = string.Empty;
        public string DatabaseShareName { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string ClientFingerprint { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public int MaxPosDevices { get; set; }
    }
}
