using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using FoodPicker.Infrastructure.Models;
using FoodPicker.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace FoodPicker.Infrastructure.Data
{
    public class EfRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext DbContext;

        public EfRepository(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
        }
        
        public virtual async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var keyValues = new object[] { id };
            return await DbContext.Set<T>().FindAsync(keyValues, cancellationToken);
        }

        public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T,bool>> query, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().Where(query).ToListAsync(cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<T,bool>> query, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().CountAsync(query, cancellationToken);
        }
        
        public async Task<bool> AnyAsync(Expression<Func<T,bool>> query, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().AnyAsync(query, cancellationToken);
        }

        public async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await DbContext.Set<T>().AddAsync(entity, cancellationToken);
            await DbContext.SaveChangesAsync(cancellationToken);

            return entity;
        }

        public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            DbContext.Entry(entity).State = EntityState.Modified;
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            DbContext.Set<T>().Remove(entity);
            await DbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<T> FirstAsync(Expression<Func<T,bool>> query, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().FirstAsync(query, cancellationToken);
        }
        
        public async Task<T> FirstOrDefaultAsync(Expression<Func<T,bool>> query, CancellationToken cancellationToken = default)
        {
            return await DbContext.Set<T>().FirstOrDefaultAsync(query, cancellationToken);
        }
    }
}