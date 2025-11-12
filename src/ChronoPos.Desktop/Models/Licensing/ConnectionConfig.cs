using System;

namespace ChronoPos.Desktop.Models.Licensing
{
    /// <summary>
    /// Stores the connection configuration for host/client mode
    /// </summary>
    public class ConnectionConfig
    {
        public bool IsClient { get; set; }
        public bool IsHost { get; set; }
        public string? HostIp { get; set; }
        public string? DatabasePath { get; set; }
        public ConnectionToken? Token { get; set; }
        public DateTime? ConfiguredAt { get; set; }
    }
}
