using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using eBookSite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.FinancialConnections;
using Stripe;
using System.Security.Claims;
using System.Web.Helpers;
using Stripe.Checkout;
using Session = Stripe.Checkout.Session;
using SessionService = Stripe.Checkout.SessionService;
using SessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;

namespace eBookSite.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
       private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM{get;set;}
        public ShoppingCartController(IUnitOfWork unitOfWork,IConfiguration config, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _configuration = config;
            _webHostEnvironment= webHostEnvironment;
        }
     
        [Authorize]
        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach(var item in shoppingCartVM.ShoppingCartList)
            {
                item.Price = GetPriceBasedOnQuantity(item);
                shoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            return View(shoppingCartVM);
        }

        public IActionResult Plus(int Id)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.GetById(m => m.Id == Id);
            cartFromDB.Count+=1;
            _unitOfWork.ShoppingCart.update(cartFromDB);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int Id)
        {
            var cartFromDB = _unitOfWork.ShoppingCart.GetById(m => m.Id == Id,tracked:true);
            if(cartFromDB.Count==1)
            {
                HttpContext.Session.SetInt32("SessionShoppingCart",
                _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == cartFromDB.ApplicationUserId).Count() - 1);
                _unitOfWork.ShoppingCart.Remove(cartFromDB);
            }
            else
            {
                cartFromDB.Count-=1;
                _unitOfWork.ShoppingCart.update(cartFromDB);

            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int Id)
        {
            var cartFromDB=_unitOfWork.ShoppingCart.GetById(m=>m.Id==Id,tracked:true);
           
            _unitOfWork.ShoppingCart.Remove(cartFromDB);
             HttpContext.Session.SetInt32("SessionShoppingCart",
              _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == cartFromDB.ApplicationUserId).Count()-1);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            var applicationUser= _unitOfWork.ApplicationUser.GetById(m=>m.Id== userId);
            shoppingCartVM.OrderHeader.Name= applicationUser.Name;
            shoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = applicationUser.City;
            shoppingCartVM.OrderHeader.State = applicationUser.State;
            shoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;

            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                item.Price = GetPriceBasedOnQuantity(item);
                shoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == userId, includeProperties: "Product");
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId=userId;

            //ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.GetById(m => m.Id == userId);

            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                item.Price = GetPriceBasedOnQuantity(item);
                ShoppingCartVM.OrderHeader.OrderTotal += (item.Price * item.Count);
            }
            ShoppingCartVM.OrderHeader.OrderStatus = "Pending";
            ShoppingCartVM.OrderHeader.PaymentStatus = "Pending";

            _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach(var cartItems in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    Price = cartItems.Price,
                    ProductId = cartItems.ProductId,
                    Count = cartItems.Count,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id
                };
                _unitOfWork.OrderDetail.Add(orderDetail);
                _unitOfWork.Save();
            }

            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"Customer/ShoppingCart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                CancelUrl = domain + "Customer/ShoppingCart/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            

            foreach(var item in ShoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)item.Price*100,
                        Currency = "inr",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader= _unitOfWork.OrderHeader.GetById(m=>m.Id==id,includeProperties:"ApplicationUser");
            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                _unitOfWork.OrderHeader.UpdateStatus(id, "Approved", "Approved");
                _unitOfWork.Save();
            }

            //remove the cart items
            var shoppingCartList = _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCartList);
            _unitOfWork.Save();

            //send order confirmation mail to user
            SendOrderConfirmationMail(shoppingCartList, id);

            HttpContext.Session.Clear();
            return View("OrderConfirmation", id);
        }
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCartObj)
        {
            if (shoppingCartObj.Count <= 50)
                return shoppingCartObj.Product.Price;
            else
            {
                if (shoppingCartObj.Count <= 100)
                    return shoppingCartObj.Product.Price50;
                else
                    return shoppingCartObj.Product.Price100;
            }
        }

        public void SendOrderConfirmationMail(List<ShoppingCart> shoppingCartList, int id)
        {
            Utility.EmailSender emailSenderObj = new Utility.EmailSender(_configuration);
         
            string body = string.Empty;
            string pathToFile = _webHostEnvironment.WebRootPath
                          + Path.DirectorySeparatorChar.ToString()
                          + "Templates" 
                          + Path.DirectorySeparatorChar.ToString()
                          + "EmailTemplates"
                          + Path.DirectorySeparatorChar.ToString()
                          + "OderConfirmationTemplate.html";

            var quantity = HttpContext.Session.GetInt32("SessionShoppingCart");
            OrderHeader orderHeader = _unitOfWork.OrderHeader.GetById(m => m.Id == id, includeProperties: "ApplicationUser");


            using (StreamReader reader = System.IO.File.OpenText(pathToFile))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("[OrderNumber]", Convert.ToString(orderHeader.Id));
            body = body.Replace("[Total]", Convert.ToString(orderHeader.OrderTotal));
            body = body.Replace("[Quantity]", Convert.ToString(quantity));
            emailSenderObj.SendEmail(orderHeader.ApplicationUser.Email, "Order Confirmation - "+ Convert.ToString(orderHeader.Id),body);
        }
    }
}
