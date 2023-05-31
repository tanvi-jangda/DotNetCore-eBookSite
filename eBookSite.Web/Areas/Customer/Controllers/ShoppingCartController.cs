using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using eBookSite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Web.Helpers;

namespace eBookSite.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
       
        [BindProperty]
        public ShoppingCartVM ShoppingCartVM{get;set;}
        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
            var cartFromDB = _unitOfWork.ShoppingCart.GetById(m => m.Id == Id);
            if(cartFromDB.Count==1)
            {
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
            var cartFromDB=_unitOfWork.ShoppingCart.GetById(m=>m.Id==Id);
            _unitOfWork.ShoppingCart.Remove(cartFromDB);
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
            ShoppingCartVM.OrderHeader.OrderDate=DateTime.Now;
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
            return RedirectToAction("OrderConfirmation", new { id = ShoppingCartVM.OrderHeader.Id });
        }

        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
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
    }
}
