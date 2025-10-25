using System;

namespace ChronoPos.Desktop.Models.Licensing
{
    public class SalesKeyInfo
    {
        public string ScratchCardCode { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = string.Empty;
        public CustomerInfo Customer { get; set; } = new CustomerInfo();
        public SystemInfo System { get; set; } = new SystemInfo();
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerInfo
    {
        public string BusinessName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        
        // Dubai-specific compliance fields
        public string TradeLicenseNumber { get; set; } = string.Empty;
        public string VATNumber { get; set; } = string.Empty;
        public string EmiratesID { get; set; } = string.Empty;
        public string IndustryCategory { get; set; } = string.Empty;
        public int NumberOfOutlets { get; set; } = 1;
    }

    public class SystemInfo
    {
        public string MachineName { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string MachineFingerprint { get; set; } = string.Empty;
        public int ProcessorCount { get; set; }
        public string SystemVersion { get; set; } = string.Empty;
    }
}
