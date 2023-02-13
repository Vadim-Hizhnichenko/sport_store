using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Business.Contracts;
using Data.ContextDb;
using Microsoft.EntityFrameworkCore;

namespace Business.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly SportsStoreDbContext _sportsStoreDbContext;
        internal DbSet<T> _dbSet;

        public Repository(SportsStoreDbContext sportsStoreDbContext)
        {
            _sportsStoreDbContext = sportsStoreDbContext;
            _dbSet = _sportsStoreDbContext.Set<T>();
        }

        public void Add(T entity)
        {
            _dbSet.Add(entity);
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string ? includeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            
            if (filter != null)
            {
                query = query.Where(filter);
            }
            

            if (includeProperties != null)
            {
                foreach (var prop in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(prop);
                }
            }

            return query.ToList();
        }

        public T GetFirstOrDifault(Expression<Func<T, bool>> filter, string? includeProperties = null)
        {
            try
            {
                IQueryable<T> query = _dbSet;
                query = query.Where(filter);

                if (includeProperties != null)
                {
                    foreach (var prop in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        query = query.Include(prop);
                    }
                }

                return query.FirstOrDefault();

            }
            catch (Exception)
            {

                throw new ArgumentNullException(nameof(filter));
            }
            
        }
    }
}
