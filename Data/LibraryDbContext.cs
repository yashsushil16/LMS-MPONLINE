using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Newspaper> Newspapers { get; set; }
        public DbSet<Magazine> Magazines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure Book entity
            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ISBN).IsUnique();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ISBN).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TotalCopies).IsRequired();
                entity.Property(e => e.AvailableCopies).IsRequired();
                entity.Property(e => e.PublishedYear).IsRequired();
            });

            // Configure Transaction entity
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.BookId).IsRequired();
                entity.Property(e => e.IssueDate).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.DueDate).IsRequired();
                entity.Property(e => e.ReturnDate).IsRequired(false);
                entity.Property(e => e.Status).IsRequired();

                // Configure relationship with User
                entity.HasOne(t => t.User)
                    .WithMany(u => u.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Configure relationship with Book
                entity.HasOne(t => t.Book)
                    .WithMany(b => b.Transactions)
                    .HasForeignKey(t => t.BookId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
