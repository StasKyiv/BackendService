namespace BackendService.Repository.Interfaces;

public interface IBaseRepository<T> where T : class
{
    Task<T> FindByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> CreateAsync(T item);
    Task<T> UpdateAsync(T item);
    Task SaveChangesAsync();
}