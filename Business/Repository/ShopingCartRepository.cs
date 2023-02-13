using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.Contracts;
using Data.ContextDb;
using Data.Models;
using WebApp.Models;

namespace Business.Repository
{
    public class ShopingCartRepository : Repository<ShopingCart>, IShopingCartRepository
    {
        private readonly SportsStoreDbContext _sportsStoreDbContext;

        public ShopingCartRepository(SportsStoreDbContext sportsStoreDbContext) : base(sportsStoreDbContext)
        {
            _sportsStoreDbContext = sportsStoreDbContext;
        }

        public int DecrementShopCartCount(ShopingCart shopingCart, int count)
        {
            shopingCart.Count -= count;
            return shopingCart.Count;
        }

        public int IncrementShopCartCount(ShopingCart shopingCart, int count)
        {
            shopingCart.Count += count;
            return shopingCart.Count;
        }
    }
}
