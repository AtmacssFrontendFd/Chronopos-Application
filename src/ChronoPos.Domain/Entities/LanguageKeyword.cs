using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChronoPos.Domain.Entities
{
    [Table("language_keyword")]
    public class LanguageKeyword
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Key { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<LabelTranslation> LabelTranslations { get; set; } = new List<LabelTranslation>();
    }
}
