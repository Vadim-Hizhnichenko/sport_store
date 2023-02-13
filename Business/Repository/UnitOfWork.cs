using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Contracts;
using Data.ContextDb;

namespace Business.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly SportsStoreDbContext _sportsStoreDbContext;
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }

        public IShopingCartRepository ShopingCartRepository { get; private set; }

        public IUserRepository UserRepository { get; private set; }

        public IOrderDetailRepository OrderDetailRepository { get; private set; }

        public IOrderHeaderRepository OrderHeaderRepository { get; private set; }

        public UnitOfWork(SportsStoreDbContext sportsStoreDbContext)
        {
            _sportsStoreDbContext = sportsStoreDbContext;
            CategoryRepository = new CategoryRepository(_sportsStoreDbContext);
            ProductRepository = new ProductRepository(_sportsStoreDbContext);
            ShopingCartRepository = new ShopingCartRepository(_sportsStoreDbContext);
            UserRepository = new UserRepository(_sportsStoreDbContext);
            OrderDetailRepository = new OrderDetailRepository(_sportsStoreDbContext);
            OrderHeaderRepository = new OrderdHeaderRepository(_sportsStoreDbContext);
        }

        

        public void Save()
        {
            _sportsStoreDbContext.SaveChanges();
        }
    }
}
