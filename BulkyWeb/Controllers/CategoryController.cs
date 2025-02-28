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
            _db.Categories.Add(_category);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
