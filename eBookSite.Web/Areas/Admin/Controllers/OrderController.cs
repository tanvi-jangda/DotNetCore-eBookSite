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
        private readonly ILogger<OrderController> _logger;

        [BindProperty]

        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork, ILogger<OrderController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;

        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaderList=null;
            try
            {
                objOrderHeaderList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

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
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return Json(new { data = objOrderHeaderList });
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
                _logger.LogError(ex.ToString());
            }
            return View(OrderVM);
        }

        [HttpPost]
        public IActionResult UpdateOrderDetail()
        {
            var orderFromDB = _unitOfWork.OrderHeader.GetById(m => m.Id == OrderVM.OrderHeader.Id);
            try
            {
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
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
            return RedirectToAction("Details", new { orderId = orderFromDB.Id });
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
                _logger.LogError(ex.ToString());
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
                _logger.LogError(ex.ToString());
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
                _logger.LogError(ex.ToString());
            }
            return RedirectToAction("Details", new { orderId = OrderVM.OrderHeader.Id });
        }

    }
}
