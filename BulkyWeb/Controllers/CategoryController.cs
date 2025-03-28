using Bulky.DataAccess.Repository.IRepository;
using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Category> categories = _unitOfWork.categoryReposity.GetAll().ToList();
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
                _unitOfWork.categoryReposity.Add(_category);
                _unitOfWork.Save();
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
            Category category = _unitOfWork.categoryReposity.Get(u => u.Id ==id );
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
                _unitOfWork.categoryReposity.Update(_category);
                _unitOfWork.Save();
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
            Category category = _unitOfWork.categoryReposity.Get(u => u.Id == id);
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
            Category category = _unitOfWork.categoryReposity.Get(u => u.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            _unitOfWork.categoryReposity.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "Category Deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
