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
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly SportsStoreDbContext _sportsStoreDbContext;

        public ProductRepository(SportsStoreDbContext sportsStoreDbContext) : base(sportsStoreDbContext)
        {
            _sportsStoreDbContext = sportsStoreDbContext;
        }

        public void Update(Product product)
        {
            var productFromDb = _sportsStoreDbContext.Products.FirstOrDefault(p => p.Id == product.Id);

            if (productFromDb != null)
            {
                productFromDb.Id = product.Id;
                productFromDb.Name = product.Name;
                productFromDb.Description = product.Description;
                productFromDb.Price = product.Price;
                productFromDb.Brend = product.Brend;
                productFromDb.CategoryId = product.CategoryId;

                if (product.ImageUrl != null)
                {
                    productFromDb.ImageUrl = product.ImageUrl;
                }
            }
        }
    }
}
