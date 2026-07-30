using LibraryManagementSystem.Interfaces;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Services
{
    public class LibraryService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ITransactionRepository _transactionRepository;

        public LibraryService(IBookRepository bookRepository, ITransactionRepository transactionRepository)
        {
            _bookRepository = bookRepository;
            _transactionRepository = transactionRepository;
        }

        /// <summary>
        /// Borrow a book: Check availability, decrement copies, create transaction with 7-day due date
        /// </summary>
        public async Task<(bool success, string message, Transaction? transaction)> BorrowBookAsync(int userId, int bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return (false, "Book not found.", null);
            }

            if (book.AvailableCopies <= 0)
            {
                return (false, "No copies available for borrowing.", null);
            }

            // Decrement available copies
            book.AvailableCopies--;
            await _bookRepository.UpdateAsync(book);

            // Create transaction with 7-day due date
            var transaction = new Transaction
            {
                UserId = userId,
                BookId = bookId,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                Status = TransactionStatus.Issued
            };

            var createdTransaction = await _transactionRepository.AddAsync(transaction);

            return (true, "Book borrowed successfully.", createdTransaction);
        }

        /// <summary>
        /// Return a book: Update transaction status, set return date, increment available copies
        /// </summary>
        public async Task<(bool success, string message, Transaction? transaction)> ReturnBookAsync(int transactionId)
        {
            var transaction = await _transactionRepository.GetByIdAsync(transactionId);
            if (transaction == null)
            {
                return (false, "Transaction not found.", null);
            }

            if (transaction.Status == TransactionStatus.Returned)
            {
                return (false, "Book has already been returned.", null);
            }

            // Update transaction
            transaction.Status = TransactionStatus.Returned;
            transaction.ReturnDate = DateTime.UtcNow;
            await _transactionRepository.UpdateAsync(transaction);

            // Increment available copies
            var book = await _bookRepository.GetByIdAsync(transaction.BookId);
            if (book != null)
            {
                book.AvailableCopies++;
                await _bookRepository.UpdateAsync(book);
            }

            return (true, "Book returned successfully.", transaction);
        }

        /// <summary>
        /// Get user's transaction history
        /// </summary>
        public async Task<IEnumerable<Transaction>> GetUserTransactionHistoryAsync(int userId)
        {
            return await _transactionRepository.GetByUserIdAsync(userId);
        }

        /// <summary>
        /// Get all available books
        /// </summary>
        public async Task<IEnumerable<Book>> GetAvailableBooksAsync()
        {
            return await _bookRepository.GetAvailableBooksAsync();
        }
    }
}
