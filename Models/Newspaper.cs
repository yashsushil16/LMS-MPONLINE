using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Newspaper
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Publisher { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Language { get; set; } = string.Empty;

        [Required]
        public DateTime PublicationDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int Copies { get; set; } = 1;
    }
}
