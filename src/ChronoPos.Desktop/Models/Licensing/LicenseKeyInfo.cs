using System;

namespace ChronoPos.Desktop.Models.Licensing
{
    public class LicenseKeyInfo
    {
        public string SalesKey { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public string MachineFingerprint { get; set; } = string.Empty;
        public string LicenseType { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int MaxPosDevices { get; set; }
        public string[] Features { get; set; } = Array.Empty<string>();
    }
}
