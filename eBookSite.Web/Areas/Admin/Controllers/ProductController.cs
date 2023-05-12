using eBookSite.DataAccess.Data;
using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using Microsoft.AspNetCore.Mvc;

namespace eBookSite.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork productRepo;
        public ProductController(IUnitOfWork db)
        {
            productRepo = db;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = productRepo.Product.GetAll().ToList();
            return View(objProductList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product objCategory)
        {
            if (ModelState.IsValid)
            {
                productRepo.Product.Add(objCategory);
                productRepo.Save();
                TempData["Success"] = "Product created successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            //fetch the category details from db using EF Core
            var objProduct = productRepo.Product.GetById(c => c.Id == id);

            if (objProduct == null)
                return NotFound();

            return View(objProduct);
        }

        [HttpPost]
        public IActionResult Edit(Product objProduct)
        {
            if (ModelState.IsValid)
            {
                productRepo.Product.Update(objProduct);
                productRepo.Save();
                TempData["Success"] = "Product edited successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            //fetch the category details from db using EF Core
            var objProduct = productRepo.Product.GetById(c => c.Id == id);

            if (objProduct == null)
                return NotFound();

            return View(objProduct);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteProduct(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var product = productRepo.Product.GetById(m => m.Id == id);

            if (product == null)
                return NotFound();

            productRepo.Product.Remove(product);
            productRepo.Save();
            TempData["Success"] = "Product deleted successfully!";
            return RedirectToAction("Index");
        }

    }
}
