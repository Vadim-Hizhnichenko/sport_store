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
    public class OrderdHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private readonly SportsStoreDbContext _sportsStoreDbContext;

        public OrderdHeaderRepository(SportsStoreDbContext sportsStoreDbContext) : base(sportsStoreDbContext)
        {
            _sportsStoreDbContext = sportsStoreDbContext;
        }

        public void Update(OrderHeader orderHeader)
        {
            _sportsStoreDbContext.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderHeaderDb = _sportsStoreDbContext.OrderHeaders.FirstOrDefault(x => x.Id == id);

            if (orderHeaderDb == null)
            {
                orderHeaderDb.OrderStatus = orderStatus;

                if (paymentStatus != null)
                {
                    orderHeaderDb.PaymentStatus = paymentStatus;
                }
            }

        }

        public void UpdateStripeSessionIds(int id, string sessionId, string paymentIntentId)
        {
            var orderHeaderDb = _sportsStoreDbContext.OrderHeaders.FirstOrDefault(x => x.Id == id);

            orderHeaderDb.SessionId = sessionId;
            orderHeaderDb.PayementIntentId = paymentIntentId;
        }
    }
}
