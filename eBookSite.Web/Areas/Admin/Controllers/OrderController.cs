using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using eBookSite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Diagnostics;

namespace eBookSite.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]

        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult GetAll(string status)
        {
            try
            {
                IEnumerable<OrderHeader> objOrderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

                switch (status)
                {
                    case "inprocess":
                        objOrderHeaderList = objOrderHeaderList.Where(m => m.OrderStatus == "Processing");
                        break;
                    case "completed":
                        objOrderHeaderList = objOrderHeaderList.Where(m => m.OrderStatus == "Shipped");
                        break;
                    case "approved":
                        objOrderHeaderList = objOrderHeaderList.Where(m => m.OrderStatus == "Approved");
                        break;
                    default:
                        break;

                }
                return Json(new { data = objOrderHeaderList });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IActionResult Details(int orderId)
        {
            try
            {
                OrderVM = new()
                {
                    OrderHeader = _unitOfWork.OrderHeader.GetById(m => m.Id == orderId, includeProperties: "ApplicationUser"),
                    OrderDetail = _unitOfWork.OrderDetail.GetAll(m => m.OrderHeaderId == orderId, includeProperties: "Product")
                };
            }
            catch (Exception ex)
            {

            }
            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult UpdateOrderDetail()
        {
            try
            {
                var orderFromDB = _unitOfWork.OrderHeader.GetById(m => m.Id == OrderVM.OrderHeader.Id);
                orderFromDB.Name = OrderVM.OrderHeader.Name;
                orderFromDB.State = OrderVM.OrderHeader.State;
                orderFromDB.StreetAddress = OrderVM.OrderHeader.StreetAddress;
                orderFromDB.PostalCode = OrderVM.OrderHeader.PostalCode;
                orderFromDB.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
                orderFromDB.City = OrderVM.OrderHeader.City;

                if (!string.IsNullOrEmpty(orderFromDB.Carrier))
                    orderFromDB.Carrier = OrderVM.OrderHeader.Carrier;

                if (!string.IsNullOrEmpty(orderFromDB.TrackingNumber))
                    orderFromDB.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;

                _unitOfWork.OrderHeader.Update(orderFromDB);
                _unitOfWork.Save();

                TempData["Success"] = "Order details updated successfully";
                return RedirectToAction("Details", new { orderId = orderFromDB.Id });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IActionResult StartProcessing()
        {
            try
            {
                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, "Processing");
                _unitOfWork.Save();
                TempData["Success"] = "Order details updated successfully";
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
        }
        public IActionResult ShipOrder()
        {
            try
            {
                var orderFromDB=_unitOfWork.OrderHeader.GetById(m=>m.Id==OrderVM.OrderHeader.Id);
                orderFromDB.TrackingNumber=OrderVM.OrderHeader.TrackingNumber;
                orderFromDB.Carrier=   OrderVM.OrderHeader.Carrier;
                orderFromDB.ShippingDate = DateTime.Now;
                orderFromDB.OrderStatus="Shipped";

                _unitOfWork.OrderHeader.Update(orderFromDB);
                _unitOfWork.Save();
                TempData["Success"] = "Order shipped successfully";
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
        }

        public IActionResult CancelOrder()
        {
            try
            {
                var orderFromDB = _unitOfWork.OrderHeader.GetById(m => m.Id == OrderVM.OrderHeader.Id);
                if(orderFromDB.PaymentStatus=="Approved")
                {
                    var options = new RefundCreateOptions
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = orderFromDB.PaymentIntentId
                    };

                    var service = new RefundService();
                    var refund = service.Create(options);
                    _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, "Cancelled", "Refunded");
                }
                else
                {
                    _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, "Cancelled", "Cancelled");
                }
                
                _unitOfWork.Save();
                TempData["Success"] = "Order cancelled successfully";
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
        }

    }
}
