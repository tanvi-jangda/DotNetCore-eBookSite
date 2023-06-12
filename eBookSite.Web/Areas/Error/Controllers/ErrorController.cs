using Microsoft.AspNetCore.Mvc;

namespace eBookSite.Web.Areas.Error.Controllers
{
    [Area("Error")]
    public class ErrorController : Controller
    {
        [Route("Error/{StatusCode}")]
        public IActionResult Index(int StatusCode)
        {
            switch (StatusCode)
            {
                case 404:
                    ViewData["Error"] = "Page Not Found";
                    break;
                case 500:
                    ViewData["Error"] = "Internal Server Error";
                    break;
                default:
                    break;
            }
            return View();
        }
    }
}
