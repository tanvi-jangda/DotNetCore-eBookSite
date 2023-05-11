
using eBookSite.DataAccess.Data;
using eBookSite.DataAccess.Repository;
using eBookSite.DataAccess.Repository.IRepository;
using eBookSite.Models;
using Microsoft.AspNetCore.Mvc;

namespace eBookSite.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork categoryRepo;
         public CategoryController(IUnitOfWork db)
        {
            categoryRepo = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = categoryRepo.Category.GetAll().ToList();
            return View(objCategoryList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category objCategory)
        {
            if (ModelState.IsValid)
            {
                categoryRepo.Category.Add(objCategory);
                categoryRepo.Save();
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            //fetch the category details from db using EF Core
            var objCategory= categoryRepo.Category.GetById(c => c.Id == id);

            if(objCategory == null)
                return NotFound();
            
            return View(objCategory);
        }

        [HttpPost]
        public IActionResult Edit(Category objCategory)
        {
            if (ModelState.IsValid)
            {
                categoryRepo.Category.Update(objCategory);
                categoryRepo.Save();
                TempData["Success"] = "Category edited successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            //fetch the category details from db using EF Core
            var objCategory = categoryRepo.Category.GetById(c => c.Id == id);

            if (objCategory == null)
                return NotFound();

            return View(objCategory);
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeleteCategory(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var category = categoryRepo.Category.GetById(m=>m.Id== id);

            if (category == null)
                    return NotFound();

            categoryRepo.Category.Remove(category);
            categoryRepo.Save();
            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }

    }
}
