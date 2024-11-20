using BackendService.Repository.Interfaces;
using DataBase;
using Microsoft.EntityFrameworkCore;

namespace BackendService.Repository.Implementation
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly ApplicationDbContext _db;
        private readonly DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext db)
        {
            _db = db;
            _dbSet = db.Set<T>();
        }
        
        public async Task<T> FindByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }
        
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }
        public async Task<T?> CreateAsync(T item)
        {
            var result = await _dbSet.AddAsync(item);
            await SaveChangesAsync();
            return result.Entity;
        }

        public async Task<T> UpdateAsync(T item)
        {
            _dbSet.Update(item);
            await SaveChangesAsync();
            return item;
        }
        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}