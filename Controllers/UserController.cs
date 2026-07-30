using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Models;

using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;

namespace LibraryManagementSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "UserOnly")]
    public class UserController : ControllerBase
    {
        private readonly LibraryService _libraryService;
        private readonly LibraryDbContext _dbContext;

        public UserController(LibraryService libraryService, LibraryDbContext dbContext)
        {
            _libraryService = libraryService;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get current user ID from JWT token
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Invalid user token");
        }

        /// <summary>
        /// View all available books (User only)
        /// </summary>
        [HttpGet("books/available")]
        public async Task<IActionResult> GetAvailableBooks()
        {
            var books = await _libraryService.GetAvailableBooksAsync();
            var bookDtos = books.Select(b => new
            {
                b.Id,
                b.Title,
                b.Author,
                b.ISBN,
                b.AvailableCopies,
                b.PublishedYear
            });

            return Ok(bookDtos);
        }

        /// <summary>
        /// Borrow a book (User only)
        /// </summary>
        [HttpPost("books/{bookId}/borrow")]
        public async Task<IActionResult> BorrowBook(int bookId)
        {
            var userId = GetCurrentUserId();

            var (success, message, transaction) = await _libraryService.BorrowBookAsync(userId, bookId);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message, transactionId = transaction?.Id });
        }

        /// <summary>
        /// Return a book (User only)
        /// </summary>
        [HttpPost("transactions/{transactionId}/return")]
        public async Task<IActionResult> ReturnBook(int transactionId)
        {
            var (success, message, transaction) = await _libraryService.ReturnBookAsync(transactionId);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        /// <summary>
        /// View personal transaction history (User only)
        /// </summary>
        [HttpGet("transactions")]
        public async Task<IActionResult> GetMyTransactions()
        {
            var userId = GetCurrentUserId();
            var transactions = await _libraryService.GetUserTransactionHistoryAsync(userId);

            var transactionDtos = transactions.Select(t => new
            {
                t.Id,
                BookTitle = t.Book?.Title,
                BookAuthor = t.Book?.Author,
                t.IssueDate,
                t.DueDate,
                t.ReturnDate,
                t.Status,
                IsOverdue = t.Status == TransactionStatus.Issued && t.DueDate < DateTime.UtcNow
            });

            return Ok(transactionDtos);
        }

        /// <summary>
        /// View all newspapers (User only)
        /// </summary>
        [HttpGet("newspapers")]
        public async Task<IActionResult> GetNewspapers()
        {
            var items = await _dbContext.Newspapers.OrderByDescending(n => n.PublicationDate).ToListAsync();
            return Ok(items);
        }

        /// <summary>
        /// View all magazines (User only)
        /// </summary>
        [HttpGet("magazines")]
        public async Task<IActionResult> GetMagazines()
        {
            var items = await _dbContext.Magazines.OrderByDescending(m => m.PublishedYear).ToListAsync();
            return Ok(items);
        }
    }
}
