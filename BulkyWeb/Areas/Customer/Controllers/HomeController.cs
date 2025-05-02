using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.productRepository.GetAll(includeProperties:"Category");
            return View(productList);
        }


        public IActionResult Details(int productId)
        {
            //Product product = _unitOfWork.productRepository.Get(obj => obj.Id == productId, includeProperties: "Category");
            ShoppingCart cart = new()
            {

                Product = _unitOfWork.productRepository.Get(obj => obj.Id == productId, includeProperties: "Category"),
                Count = 1,
                ProductId = productId
            };

            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            //get login user info
            var claimsIdentity = (ClaimsIdentity)User.Identity;            //get userId            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;            cart.ApplicationUserId = userId;

            ShoppingCart shoppingCartFromDb = _unitOfWork.shoppingCartRepository.Get(x => x.ApplicationUserId == userId && x.ProductId == cart.ProductId);            if (shoppingCartFromDb == null)            {
                //add new one
                _unitOfWork.shoppingCartRepository.Add(cart);
                _unitOfWork.Save();
            }            else
            {
                //update only increase the count
                shoppingCartFromDb.Count += cart.Count;
                _unitOfWork.shoppingCartRepository.Update(shoppingCartFromDb);
                _unitOfWork.Save();
            }
            TempData["success"] = "Shopping Cart Updated Successfully!!!";
            return RedirectToAction("Index");

        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
