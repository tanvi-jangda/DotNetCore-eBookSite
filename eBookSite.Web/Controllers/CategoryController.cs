
using eBookSite.DataAccess.Data;
using eBookSite.Models;
using Microsoft.AspNetCore.Mvc;

namespace eBookSite.Web.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDBContext _dbContext;
        public CategoryController(ApplicationDBContext db)
        {
            _dbContext = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _dbContext.Categories.ToList();
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
                _dbContext.Categories.Add(objCategory);
                _dbContext.SaveChanges();
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
            var objCategory=_dbContext.Categories.FirstOrDefault(c => c.Id == id);

            if(objCategory == null)
                return NotFound();
            
            return View(objCategory);
        }

        [HttpPost]
        public IActionResult Edit(Category objCategory)
        {
            if (ModelState.IsValid)
            {
                _dbContext.Categories.Update(objCategory);
                _dbContext.SaveChanges();
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
            var objCategory = _dbContext.Categories.FirstOrDefault(c => c.Id == id);

            if (objCategory == null)
                return NotFound();

            return View(objCategory);
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeleteCategory(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var category = _dbContext.Categories.FirstOrDefault(m=>m.Id== id);

            if (category == null)
                    return NotFound();

            _dbContext.Categories.Remove(category);
            _dbContext.SaveChanges();
            TempData["Success"] = "Category deleted successfully!";
            return RedirectToAction("Index");
        }

    }
}
