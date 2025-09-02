using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoPos.Domain.Entities
{
    [Table("language")]
    public class Language
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        [Column("language_name")]
        public string LanguageName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("language_code")]
        public string LanguageCode { get; set; } = string.Empty;

        [Column("is_rtl")]
        public bool IsRtl { get; set; }

        [StringLength(255)]
        public string Status { get; set; } = "Active";

        [StringLength(255)]
        [Column("created_by")]
        public string? CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(255)]
        [Column("updated_by")]
        public string? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [StringLength(255)]
        [Column("deleted_by")]
        public string? DeletedBy { get; set; }

        // Navigation properties
        public virtual ICollection<LabelTranslation> LabelTranslations { get; set; } = new List<LabelTranslation>();
    }
}
