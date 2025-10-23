using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoPos.Domain.Entities
{
    [Table("label_translation")]
    public class LabelTranslation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Column("language_id")]
        public int LanguageId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("translation_key")]
        public string TranslationKey { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Value { get; set; } = string.Empty;

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
        [ForeignKey("LanguageId")]
        public virtual Language Language { get; set; } = null!;
    }
}
