using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace eBookSite.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public HomeController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //set the session again after login
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (claim != null)
            {
                HttpContext.Session.SetInt32("SessionShoppingCart",
                   _unitOfWork.ShoppingCart.GetAll(m => m.ApplicationUserId == claim.Value).Count());

                //log the client ip and mac address
                var applicationUserFromDB = _unitOfWork.ApplicationUser.GetById(m => m.Id == claim.Value);
                var clientIPAddress = GetIPAddress();
                applicationUserFromDB.ClientIPAddress = clientIPAddress;

                var macAddress = GetClientMAC(clientIPAddress);
                applicationUserFromDB.ClientMACAddress = macAddress;
                _unitOfWork.ApplicationUser.Update(applicationUserFromDB);
                _unitOfWork.Save();

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
                ProductId = productId
            };

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cartObj)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cartObj.ApplicationUserId = userId;

            ShoppingCart cartFromDB = _unitOfWork.ShoppingCart.
                  GetById(m => m.ApplicationUserId == userId && m.ProductId == cartObj.ProductId);

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

        public string GetIPAddress()
        {
            //   var context =  System.Web.httpcontext.HttpContext;
            //    string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            //    if (!string.IsNullOrEmpty(ipAddress))
            //    {
            //        string[] addresses = ipAddress.Split(',');
            //        if (addresses.Length != 0)
            //        {
            //            return addresses[0];
            //        }
            //    }

            //    return context.Request.ServerVariables["REMOTE_ADDR"];

            var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();

            return ipAddress;
        }
        [DllImport("Iphlpapi.dll")]
        private static extern int SendARP(Int32 dest, Int32 host, ref Int64 mac, ref Int32 length);
        [DllImport("Ws2_32.dll")]
        private static extern Int32 inet_addr(string ip);

        private static string GetClientMAC(string strClientIP)
        {
            string mac_dest = "";
            try
            {
                Int32 ldest = inet_addr(strClientIP);
                Int32 lhost = inet_addr("");
                Int64 macinfo = new Int64();
                Int32 len = 6;
                int res = SendARP(ldest, 0, ref macinfo, ref len);
                string mac_src = macinfo.ToString("X");

                while (mac_src.Length < 12)
                {
                    mac_src = mac_src.Insert(0, "0");
                }

                for (int i = 0; i < 11; i++)
                {
                    if (0 == (i % 2))
                    {
                        if (i == 10)
                        {
                            mac_dest = mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }
                        else
                        {
                            mac_dest = "-" + mac_dest.Insert(0, mac_src.Substring(i, 2));
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw new Exception("L?i " + err.Message);
            }
            return mac_dest;
        }
    }
}