using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string ISBN { get; set; } = string.Empty;

        [Required]
        [Range(0, int.MaxValue)]
        public int TotalCopies { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int AvailableCopies { get; set; }

        [Required]
        [Range(1000, int.MaxValue)]
        public int PublishedYear { get; set; }

        // Navigation property
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
