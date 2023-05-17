using eBookSite.DataAccess.Data;
using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using eBookSite.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Drawing;

namespace eBookSite.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork productRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork db,IWebHostEnvironment webHostEnvironment)
        {
            productRepo = db;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = productRepo.Product.GetAll(includeProperties:"Category").ToList();

            return View(objProductList);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        {
            //using the projection in EF core
            var CategoryList = productRepo.Category.GetAll().Select(m => new SelectListItem
            {
                Text = m.Name,
                Value = m.Id.ToString()
            });

            ProductVM productVM = new()
            {
                CategoryList = CategoryList,
                Product = new Product()
            };
            if (id == 0 || id == null)
            {
                //create page
                return View(productVM);
            }
            else
            {
                //edit page
           productVM.Product = productRepo.Product.GetById(c => c.Id == id);

                return View(productVM);
            }
            return View(productVM);
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file!=null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath,@"images\product");

                    if(!string.IsNullOrEmpty(obj.Product.ImageURL))
                    {
                        //delete old image
                        var oldPath = Path.Combine(wwwRootPath, obj.Product.ImageURL.Trim('\\'));
                        if(System.IO.File.Exists(oldPath))
                        {
                            System.IO.File.Delete(oldPath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath,fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    obj.Product.ImageURL = @"\images\product\" + fileName;
                }
                if (obj.Product.Id == 0)
                {
                    productRepo.Product.Add(obj.Product);
                }
                else
                {
                    productRepo.Product.Update(obj.Product);
                }
                productRepo.Save();
                TempData["Success"] = "Product created / updated successfully!";
                return RedirectToAction("Index");
            }
            else
            {
                obj.CategoryList = productRepo.Category.GetAll().Select(m => new SelectListItem
                {
                    Text = m.Name,
                    Value = m.Id.ToString()
                });
                
                return View(obj);
            }
            return View(obj);
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


        #region APICALL
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = productRepo.Product.GetAll(includeProperties: "Category").ToList();

            return Json(new { data = objProductList });
        }
        #endregion

    }
}
