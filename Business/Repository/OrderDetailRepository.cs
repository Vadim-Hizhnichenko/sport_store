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
    public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly SportsStoreDbContext _sportsStoreDbContext;

        public OrderDetailRepository(SportsStoreDbContext sportsStoreDbContext) : base(sportsStoreDbContext)
        {
            _sportsStoreDbContext = sportsStoreDbContext;
        }
           
        public void Update(OrderDetail orderDetail)
        {
            _sportsStoreDbContext.OrderDetails.Update(orderDetail);
        }
    }
}
