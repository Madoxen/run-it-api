using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Repositories
{
    public interface IRepository<T> where T : class, IEntity
    {
        Task<IEnumerable<T>> GetAll();
        Task<T> Get(int id);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int id);
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
     
    }
}