using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : ControllerBase
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly LibraryService _libraryService;
        private readonly LibraryDbContext _dbContext;

        public AdminController(
            IBookRepository bookRepository,
            IUserRepository userRepository,
            ITransactionRepository transactionRepository,
            LibraryService libraryService,
            LibraryDbContext dbContext)
        {
            _bookRepository = bookRepository;
            _userRepository = userRepository;
            _transactionRepository = transactionRepository;
            _libraryService = libraryService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all books (Admin only)
        /// </summary>
        [HttpGet("books")]
        public async Task<IActionResult> GetAllBooks()
        {
            var books = await _bookRepository.GetAllAsync();
            return Ok(books);
        }

        /// <summary>
        /// Add a new book (Admin only)
        /// </summary>
        [HttpPost("books")]
        public async Task<IActionResult> AddBook([FromBody] CreateBookRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if ISBN already exists
            var existingBook = await _bookRepository.GetByISBNAsync(request.ISBN);
            if (existingBook != null)
                return BadRequest(new { message = "Book with this ISBN already exists" });

            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                ISBN = request.ISBN,
                TotalCopies = request.TotalCopies,
                AvailableCopies = request.TotalCopies,
                PublishedYear = request.PublishedYear
            };

            await _bookRepository.AddAsync(book);

            return Ok(new { message = "Book added successfully", bookId = book.Id });
        }

        /// <summary>
        /// Update an existing book (Admin only)
        /// </summary>
        [HttpPut("books/{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
                return NotFound(new { message = "Book not found" });

            // Update book properties
            book.Title = request.Title ?? book.Title;
            book.Author = request.Author ?? book.Author;
            book.ISBN = request.ISBN ?? book.ISBN;
            book.PublishedYear = request.PublishedYear ?? book.PublishedYear;

            // Update copies if provided
            if (request.TotalCopies.HasValue)
            {
                var difference = request.TotalCopies.Value - book.TotalCopies;
                book.TotalCopies = request.TotalCopies.Value;
                book.AvailableCopies = Math.Max(0, book.AvailableCopies + difference);
            }

            await _bookRepository.UpdateAsync(book);

            return Ok(new { message = "Book updated successfully" });
        }

        /// <summary>
        /// Delete a book (Admin only)
        /// </summary>
        [HttpDelete("books/{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
                return NotFound(new { message = "Book not found" });

            await _bookRepository.DeleteAsync(id);

            return Ok(new { message = "Book deleted successfully" });
        }

        /// <summary>
        /// View all users (Admin only)
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepository.GetAllAsync();
            var userDtos = users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.Phone,
                u.Role,
                u.CreatedAt
            });

            return Ok(userDtos);
        }

        /// <summary>
        /// Create a new user / student / librarian (Admin only)
        /// </summary>
        [HttpPost("users")]
        public async Task<IActionResult> AddUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                return BadRequest(new { message = "Email already registered" });

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone ?? string.Empty,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            return Ok(new { message = "User created successfully", userId = user.Id });
        }

        /// <summary>
        /// Update user / student / librarian (Admin only)
        /// </summary>
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            user.FullName = request.FullName ?? user.FullName;
            user.Email = request.Email ?? user.Email;
            user.Phone = request.Phone ?? user.Phone;

            if (request.Role.HasValue)
                user.Role = request.Role.Value;

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _userRepository.UpdateAsync(user);
            return Ok(new { message = "User updated successfully" });
        }

        /// <summary>
        /// Delete a user (Admin only)
        /// </summary>
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            await _userRepository.DeleteAsync(id);

            return Ok(new { message = "User deleted successfully" });
        }

        /// <summary>
        /// View all transactions (Admin only)
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetAllTransactions()
        {
            var transactions = await _transactionRepository.GetAllAsync();
            return Ok(transactions);
        }

        /// <summary>
        /// Process book return (Admin only)
        /// </summary>
        [HttpPost("transactions/{transactionId}/return")]
        public async Task<IActionResult> ReturnBook(int transactionId)
        {
            var (success, message, transaction) = await _libraryService.ReturnBookAsync(transactionId);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        // --- NEWSPAPERS ---

        [HttpGet("newspapers")]
        public async Task<IActionResult> GetNewspapers()
        {
            var items = await _dbContext.Newspapers.OrderByDescending(n => n.PublicationDate).ToListAsync();
            return Ok(items);
        }

        [HttpPost("newspapers")]
        public async Task<IActionResult> AddNewspaper([FromBody] Newspaper item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _dbContext.Newspapers.Add(item);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Newspaper added successfully", id = item.Id });
        }

        [HttpPut("newspapers/{id}")]
        public async Task<IActionResult> UpdateNewspaper(int id, [FromBody] Newspaper item)
        {
            var existing = await _dbContext.Newspapers.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Newspaper not found" });

            existing.Title = item.Title;
            existing.Publisher = item.Publisher;
            existing.Language = item.Language;
            existing.PublicationDate = item.PublicationDate;
            existing.Copies = item.Copies;

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Newspaper updated successfully" });
        }

        [HttpDelete("newspapers/{id}")]
        public async Task<IActionResult> DeleteNewspaper(int id)
        {
            var existing = await _dbContext.Newspapers.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Newspaper not found" });

            _dbContext.Newspapers.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Newspaper deleted successfully" });
        }

        // --- MAGAZINES ---

        [HttpGet("magazines")]
        public async Task<IActionResult> GetMagazines()
        {
            var items = await _dbContext.Magazines.OrderByDescending(m => m.PublishedYear).ToListAsync();
            return Ok(items);
        }

        [HttpPost("magazines")]
        public async Task<IActionResult> AddMagazine([FromBody] Magazine item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            _dbContext.Magazines.Add(item);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Magazine added successfully", id = item.Id });
        }

        [HttpPut("magazines/{id}")]
        public async Task<IActionResult> UpdateMagazine(int id, [FromBody] Magazine item)
        {
            var existing = await _dbContext.Magazines.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Magazine not found" });

            existing.Title = item.Title;
            existing.Category = item.Category;
            existing.Publisher = item.Publisher;
            existing.IssueNumber = item.IssueNumber;
            existing.PublishedYear = item.PublishedYear;
            existing.Copies = item.Copies;

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Magazine updated successfully" });
        }

        [HttpDelete("magazines/{id}")]
        public async Task<IActionResult> DeleteMagazine(int id)
        {
            var existing = await _dbContext.Magazines.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Magazine not found" });

            _dbContext.Magazines.Remove(existing);
            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Magazine deleted successfully" });
        }
    }

    // DTOs
    public class CreateBookRequest
    {
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
        [Range(1, int.MaxValue)]
        public int TotalCopies { get; set; }

        [Required]
        [Range(1000, int.MaxValue)]
        public int PublishedYear { get; set; }
    }

    public class UpdateBookRequest
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(100)]
        public string? Author { get; set; }

        [MaxLength(20)]
        public string? ISBN { get; set; }

        [Range(1, int.MaxValue)]
        public int? TotalCopies { get; set; }

        [Range(1000, int.MaxValue)]
        public int? PublishedYear { get; set; }
    }

    public class CreateUserRequest
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Phone { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.User;
    }

    public class UpdateUserRequest
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        public UserRole? Role { get; set; }

        [MinLength(6)]
        public string? Password { get; set; }
    }
}
