using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
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

		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }


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

            ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(x => x.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };
            

            foreach (var shoppingCart in ShoppingCartVM.ShoppingCartList)
            {
                shoppingCart.Price = GetPriceBasedOnQuantity(shoppingCart);
                //check if price is valid
                if (shoppingCart.Price<0)
                {
                    TempData["error"] = "Shopping Cart is invalid. Please remove the item(s) and try again.";
                    return RedirectToAction(nameof(Index));
                }
				ShoppingCartVM.OrderHeader.OrderTotal += (shoppingCart.Count * shoppingCart.Price);
                //shoppingCartVM.OrderTotal += (shoppingCart.Count*shoppingCart.Price);
            }


            return View(ShoppingCartVM);
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

        // go to the summary page
        public IActionResult Summary()
        {

            //get login user info
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //create a shoppinglist return obj
            ShoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(x => x.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            // assemble user address info
            ApplicationUser applicationUser = _unitOfWork.applicationUserRepository.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.ApplicationUser = applicationUser;
            ShoppingCartVM.OrderHeader.Name = applicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = applicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = applicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = applicationUser.City;
            ShoppingCartVM.OrderHeader.State = applicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = applicationUser.PostalCode;

            //calculate the total price
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);

        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{

			//get login user info
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            //create a shoppinglist return obj
            ShoppingCartVM.ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId,
            includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;


            //calculate the total price
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			// assemble user address info
			ApplicationUser applicationUser = _unitOfWork.applicationUserRepository.Get(u => u.Id == userId);
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                //customer
                ShoppingCartVM.OrderHeader.PaymentStatus = OrderStauts.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = OrderStauts.StatusPending;
            }
            else
            {
                //company
                ShoppingCartVM.OrderHeader.PaymentStatus = OrderStauts.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = OrderStauts.StatusApproved;
            }

            //save orderheader to database
            _unitOfWork.orderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();


			//save order details to the database
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count
				};

				_unitOfWork.orderDetailRepository.Add(orderDetail);
				_unitOfWork.Save();
			}

			return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
			//return View(ShoppingCartVM);

		}


		public IActionResult OrderConfirmation(int id) 
        {
            return View(id);
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
