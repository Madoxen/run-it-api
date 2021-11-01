using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Repositories
{
    public interface IRepository<T> where T : class, IEntity
    {
        Task<List<T>> GetAll();
        Task<T> Get(int id);
        Task Add(T entity);
        Task Update(T entity);
        Task Delete(int id);
    }
}