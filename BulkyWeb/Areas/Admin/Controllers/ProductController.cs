using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = RoleName.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.productRepository.GetAll(includeProperties: "Category").ToList();
            /*IEnumerable<SelectListItem> categoryList = _unitOfWork.categoryRepository.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });*/
            return View(products);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> products = _unitOfWork.productRepository.GetAll(includeProperties: "Category").ToList();
            return Json(new { data = products });

        }


        //return a create page
        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.categoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product()
            };
            if (id == null || id == 0)
            {
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.productRepository.Get(u => u.Id == id);
                return View(productVM);
            }


        }

        [HttpPost]
        public IActionResult Create(ProductVM _Product, IFormFile? file)
        {

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");//C:\\document\\CSharp\\Bulky\\BulkyWeb\\wwwroot\\images\\product"

                    //check imageUrl is null or not
                    if (!string.IsNullOrEmpty(_Product.Product.ImageUrl))
                    {
                        //delete old image
                        string oldImage = Path.Combine(wwwRootPath, _Product.Product.ImageUrl.Trim('\\'));
                        if (System.IO.File.Exists(oldImage))
                        {
                            System.IO.File.Delete(oldImage);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    _Product.Product.ImageUrl = @"\images\product\" + fileName;
                }

                if (_Product.Product.Id == 0)
                {
                    _unitOfWork.productRepository.Add(_Product.Product);
                    TempData["success"] = "Product created successfully";
                }
                else
                {
                    _unitOfWork.productRepository.Update(_Product.Product);
                    TempData["success"] = "Product updated successfully";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                _Product.CategoryList = _unitOfWork.categoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(_Product);
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        { 
        
            Product product = _unitOfWork.productRepository.Get(u => u.Id == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Product not found" });
            }
            //delete old image
            string oldImage = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.Trim('\\'));
            if (System.IO.File.Exists(oldImage))
            {
                System.IO.File.Delete(oldImage);
            }
            _unitOfWork.productRepository.Remove(product);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successfully" });
        }




        //edit button
        /*        public IActionResult Edit(int id)
                {
                    if (id == null || id == 0)
                    {
                        return NotFound();
                    }
                    Product product = _unitOfWork.productRepository.Get(u => u.Id == id);
                    if (product == null)
                    {
                        return NotFound();
                    }
                    return View(product);
                }*/

        /*
                [HttpPost]
                public IActionResult Edit(Product _Product)
                {

                    if (ModelState.IsValid)
                    {
                        _unitOfWork.productRepository.Update(_Product);
                        _unitOfWork.Save();
                        TempData["success"] = "Product updated successfully";
                        return RedirectToAction("Index");
                    }
                    return View();
                }*/

        //Delete button
        /* public IActionResult Delete(int id)
         {
             if (id == null || id == 0)
             {
                 return NotFound();
             }
             Product product = _unitOfWork.productRepository.Get(u => u.Id == id);
             if (product == null)
             {
                 return NotFound();
             }
             return View(product);
         }*/

        /* [HttpPost, ActionName("Delete")]
         public IActionResult DeleteGet(int? id)
         {
             if (id == null || id == 0)
             {
                 return NotFound();
             }
             Product product = _unitOfWork.productRepository.Get(u => u.Id == id);
             if (product == null)
             {
                 return NotFound();
             }
             _unitOfWork.productRepository.Remove(product);
             _unitOfWork.Save();
             TempData["success"] = "Product Deleted successfully";
             return RedirectToAction("Index");
         }*/


    }
}