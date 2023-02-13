using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Models;

namespace Business.Contracts
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        void UpdateStatus (int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripeSessionIds(int id, string sessionId, string paymentIntentId);

    }
}
