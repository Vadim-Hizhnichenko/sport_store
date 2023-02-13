using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Contracts;
using Data.ContextDb;
using Data.Models;

namespace Business.Repository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly SportsStoreDbContext _sportsStoreDbContext;

        public CategoryRepository(SportsStoreDbContext sportsStoreDbContext) : base(sportsStoreDbContext)
        {
            _sportsStoreDbContext = sportsStoreDbContext;
        }

        public void Update(Category category)
        {
            _sportsStoreDbContext.Categories.Update(category);
        }
    }
}
