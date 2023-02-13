using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApp.Models;

namespace Business.Contracts
{
    public interface IShopingCartRepository : IRepository<ShopingCart>
    {
        int IncrementShopCartCount(ShopingCart shopingCart, int count);
        int DecrementShopCartCount(ShopingCart shopingCart, int count);
    }

}
