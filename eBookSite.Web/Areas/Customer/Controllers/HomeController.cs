using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace eBookSite.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //set the session again after login
            var claimsIdentity=(ClaimsIdentity)User.Identity;
            var claim=claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim!=null)
            {
                HttpContext.Session.SetInt32("SessionShoppingCart",
                   _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == claim.Value).Count());
            }
            var productList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            return View(productList);
        }
        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.Product.GetById(u => u.Id == productId),
                Count = 1,
                ProductId=productId
            };

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cartObj) 
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId=claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cartObj.ApplicationUserId = userId;

          ShoppingCart cartFromDB=_unitOfWork.ShoppingCart.
                GetById(m=>m.ApplicationUserId == userId && m.ProductId==cartObj.ProductId);

            if (cartFromDB != null)
            {
                //update database
                cartFromDB.Count += cartObj.Count;
                _unitOfWork.ShoppingCart.update(cartFromDB);
                _unitOfWork.Save();
            }
            else
            {
                //add to db
                _unitOfWork.ShoppingCart.Add(cartObj);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32("SessionShoppingCart",
                    _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == userId).Count());
            }

            

            TempData["Success"] = "Cart updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}