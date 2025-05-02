using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models.ViewModels;
using Bulky.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = RoleName.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> companys = _unitOfWork.companyRepository.GetAll().ToList();
            return View(companys);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companys = _unitOfWork.companyRepository.GetAll().ToList();
            return Json(new { data = companys });

        }


        //return a create/update page
        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company company = _unitOfWork.companyRepository.Get(u => u.Id == id);
                return View(company);
            }


        }

        [HttpPost]
        public IActionResult Create(Company _company)
        {

            if (ModelState.IsValid)
            {
                //create
                if (_company.Id == 0)
                {
                    _unitOfWork.companyRepository.Add(_company);
                    TempData["success"] = "Company created successfully";
                }
                else
                {
                    //update
                    _unitOfWork.companyRepository.Update(_company);
                    TempData["success"] = "Company updated successfully";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            else
            {
                return View(_company);
            }
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            Company company = _unitOfWork.companyRepository.Get(u => u.Id == id);
            if (company == null)
            {
                return Json(new { success = false, message = "Company not found" });
            }
            _unitOfWork.companyRepository.Remove(company);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Delete successfully" });
        }

    }
}
