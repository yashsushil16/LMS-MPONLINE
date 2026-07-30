using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces
{
    public interface ITransactionRepository
    {
        Task<Transaction?> GetByIdAsync(int id);
        Task<IEnumerable<Transaction>> GetAllAsync();
        Task<IEnumerable<Transaction>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Transaction>> GetByBookIdAsync(int bookId);
        Task<Transaction> AddAsync(Transaction transaction);
        Task<Transaction> UpdateAsync(Transaction transaction);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
