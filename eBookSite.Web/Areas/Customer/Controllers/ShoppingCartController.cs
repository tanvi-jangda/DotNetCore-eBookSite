using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using eBookSite.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eBookSite.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
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
                includeProperties: "Product")
            };

            foreach(var item in shoppingCartVM.ShoppingCartList)
            {
               double priceBasedOnQuantity = GetPriceBasedOnQuantity(item);
               item.Price += priceBasedOnQuantity * item.Count;
                shoppingCartVM.OrderTotal += item.Price;
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
            return View();
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
