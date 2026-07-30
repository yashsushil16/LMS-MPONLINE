using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Magazine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Publisher { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string IssueNumber { get; set; } = string.Empty;

        [Required]
        public int PublishedYear { get; set; }

        [Required]
        public int Copies { get; set; } = 1;
    }
}
