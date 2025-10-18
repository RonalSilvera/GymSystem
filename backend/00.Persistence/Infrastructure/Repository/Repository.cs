using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.Repository.Base
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> All()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T?> Detail(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task Create(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public async Task Update(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task Delete(T entity)
        {
            _dbSet.Remove(entity);
            await Task.CompletedTask;
        }
    }
}