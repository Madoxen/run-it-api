using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Repositories
{
    public abstract class EfCoreRepository<TEntity, TContext> : IRepository<TEntity>
        where TEntity : class, IEntity
        where TContext : DbContext
    {
        private readonly TContext context;
        public EfCoreRepository(TContext context)
        {
            this.context = context;
        }
        public async Task Add(TEntity entity)
        {
            context.Set<TEntity>().Add(entity);
            await context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var entity = await context.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                return;
            }

            context.Set<TEntity>().Remove(entity);
            await context.SaveChangesAsync();

            return;
        }

        public async Task<TEntity> Get(int id)
        {
            return await context.Set<TEntity>().FindAsync(id);
        }

        public async Task<List<TEntity>> GetAll()
        {
            return await context.Set<TEntity>().ToListAsync();
        }

        public async Task Update(TEntity entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
        }

    }
}