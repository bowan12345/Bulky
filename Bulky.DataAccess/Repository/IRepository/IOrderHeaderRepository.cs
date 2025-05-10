using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository: IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);

        //update order status by id
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);

        //update psyment intent id and seesion id by id
        void UpdateStripePaymentIDById(int id, string sessionId, string paymentIntentId);

    }
}
