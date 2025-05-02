using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{

    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        private ShoppingCartVM shoppingCartVM;

        public CartController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            //get login user info
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            //get userId
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(x=> x.ApplicationUserId == userId, includeProperties:"Product"),
            };

            foreach (var shoppingCart in shoppingCartVM.ShoppingCartList)
            {
                shoppingCart.Price = GetPriceBasedOnQuantity(shoppingCart);
                //check if price is valid
                if (shoppingCart.Price<0)
                {
                    TempData["error"] = "Shopping Cart is invalid. Please remove the item(s) and try again.";
                    return RedirectToAction(nameof(Index));
                }
                shoppingCartVM.OrderTotal += (shoppingCart.Count*shoppingCart.Price);
            }


            return View(shoppingCartVM);
        }


        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.shoppingCartRepository.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.Get(u => u.Id == cartId);
            cartFromDb.Count -= 1;
            //check if count is less than 0, if yes, remove the item from cart
            if (cartFromDb.Count ==0)
            {
                _unitOfWork.shoppingCartRepository.Remove(cartFromDb);
            }
            else
            {
                _unitOfWork.shoppingCartRepository.Update(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.Get(u => u.Id == cartId);
            _unitOfWork.shoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        //summary page
        public IActionResult Summary()
        {
            return View();
        }


        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            
            if (shoppingCart.Count > 0 && shoppingCart.Count<=50)
            {
                shoppingCart.Price = shoppingCart.Product.Price;
            }
            else if (shoppingCart.Count > 50 && shoppingCart.Count <= 100)
            {
                shoppingCart.Price = shoppingCart.Product.Price50;
            }
            else if (shoppingCart.Count > 100)
            {
                shoppingCart.Price =shoppingCart.Product.Price100;
            }
            else
            {
                shoppingCart.Price = -1;
            }
            return shoppingCart.Price;
        }
    }
}
