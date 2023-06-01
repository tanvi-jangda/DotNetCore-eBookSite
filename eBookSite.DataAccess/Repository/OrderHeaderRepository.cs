using eBookSite.DataAccess.Data;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eBookSite.DataAccess.Repository
{
    public class OrderHeaderRepository:Repository<OrderHeader>,IOrderHeaderRepository
    {
        private ApplicationDBContext _db;
        public OrderHeaderRepository(ApplicationDBContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            var orderFromDB = _db.OrderHeaders.FirstOrDefault(m => m.Id == id);
            if(orderFromDB!=null)
            {
                orderFromDB.OrderStatus = orderStatus;
                if(!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDB.PaymentStatus = paymentStatus;
                }
            }
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            var orderFromDB = _db.OrderHeaders.FirstOrDefault(m => m.Id == id);
            if(!string.IsNullOrEmpty(sessionId))
            {
                orderFromDB.SessionId= sessionId;
            }
            if (!string.IsNullOrEmpty(paymentIntentId))
            {
                orderFromDB.PaymentIntentId= paymentIntentId;
                orderFromDB.PaymentDate = DateTime.Now;
            }

        }
    }
}
