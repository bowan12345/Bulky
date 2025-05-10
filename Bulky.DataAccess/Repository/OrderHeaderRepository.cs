using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeaders.Update(orderHeader);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            OrderHeader? orderHeader = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderHeader != null)
            {
                orderHeader.OrderStatus = orderStatus;

                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderHeader.PaymentStatus = paymentStatus;
                }
                _db.Update(orderHeader);
            }
            
        }

        public void UpdateStripePaymentIDById(int id, string sessionId, string paymentIntentId)
        {
            OrderHeader? orderHeader = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if (orderHeader != null)
            {
                if (!string.IsNullOrEmpty(sessionId))
                {
                    orderHeader.SessionId = sessionId;
                }

                if (!string.IsNullOrEmpty(paymentIntentId))
                {
                    orderHeader.PaymentIntentId = paymentIntentId;
                    orderHeader.PaymentDate = DateTime.Now;
                }
                _db.Update(orderHeader);
            }

        }
    }
}
