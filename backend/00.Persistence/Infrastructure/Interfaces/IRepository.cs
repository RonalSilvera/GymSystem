using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Persistence.Repository.Base
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> All();
        Task<T?> Detail(Guid id);
        Task Create(T entity);
        Task Update(T entity);
        Task Delete(T entity);
    }
}