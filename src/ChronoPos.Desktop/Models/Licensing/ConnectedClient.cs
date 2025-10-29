using System;

namespace ChronoPos.Desktop.Models.Licensing
{
    /// <summary>
    /// Represents a client device connected to the host
    /// </summary>
    public class ConnectedClient
    {
        public string Fingerprint { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime LastSeenAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
