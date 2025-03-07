using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {   
            this._db = db;
        }

        public IActionResult Index()
        {
            List<Category> categories = _db.Categories.ToList();
            return View(categories);
        }

        //return a create page
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category _category)
        {
            if (_category.Name.Equals(_category.DisplayOrder.ToString())) 
            {
                ModelState.AddModelError("Name","The Display Order cannot exactly match the name ");
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Add(_category);
                _db.SaveChanges();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        //edit button
        public IActionResult Edit(int id)
        {
            if (id==null || id==0) 
            {
                return NotFound();
            }
            Category category = _db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }


        [HttpPost]
        public IActionResult Edit(Category _category)
        {
            if (_category.Name.Equals(_category.DisplayOrder.ToString()))
            {
                ModelState.AddModelError("Name", "The Display Order cannot exactly match the name ");
            }

            if (ModelState.IsValid)
            {
                _db.Categories.Update(_category);
                _db.SaveChanges();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        //Delete button
        public IActionResult Delete(int id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category category = _db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost,ActionName("Delete")]
        public IActionResult DeleteGet(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category category = _db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(category);
            _db.SaveChanges();
            TempData["success"] = "Category Deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
