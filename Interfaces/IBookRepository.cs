using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Interfaces
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(int id);
        Task<Book?> GetByISBNAsync(string isbn);
        Task<IEnumerable<Book>> GetAllAsync();
        Task<IEnumerable<Book>> GetAvailableBooksAsync();
        Task<Book> AddAsync(Book book);
        Task<Book> UpdateAsync(Book book);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
