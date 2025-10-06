using System;

namespace ChronoPos.Desktop.Models.Licensing
{
    public class ScratchCardInfo
    {
        public string CardCode { get; set; } = string.Empty;
        public string DisplayCode { get; set; } = string.Empty;
        public int PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public int DurationInDays { get; set; }
        public int SalespersonId { get; set; }
        public string SalespersonName { get; set; } = string.Empty;
        public string SalespersonEmail { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
    }
}
