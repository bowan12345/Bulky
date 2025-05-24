using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
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
                //update shopping cart session
                HttpContext.Session.SetInt32(SessionConstants.SessionCart, _unitOfWork.shoppingCartRepository                                        .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);            }
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
            //update shopping cart session
            HttpContext.Session.SetInt32(SessionConstants.SessionCart, _unitOfWork.shoppingCartRepository
                                    .GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
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
                ShoppingCartVM.OrderHeader.PaymentStatus = OrderStatus.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = OrderStatus.StatusPending;
            }
            else
            {
                //company
                ShoppingCartVM.OrderHeader.PaymentStatus = OrderStatus.PaymentStatusDelayedPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = OrderStatus.StatusApproved;
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

            //customer
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7047/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/Cart/Orderconfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + $"Customer/Cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                            Currency = "nzd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                // update orderheader to database
                _unitOfWork.orderHeaderRepository.UpdateStripePaymentIDById(ShoppingCartVM.OrderHeader.Id,session.Id,session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
			//return View(ShoppingCartVM);

		}


        //after payment successfully, recall to update payment status
		public IActionResult OrderConfirmation(int id) 
        {
            OrderHeader orderHeader = _unitOfWork.orderHeaderRepository.Get(x => x.Id == id, includeProperties: "ApplicationUser");

            if (orderHeader.PaymentStatus != OrderStatus.PaymentStatusDelayedPayment) 
            {
                //customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() =="paid")
                {
                    _unitOfWork.orderHeaderRepository.UpdateStripePaymentIDById(id, session.Id, session.PaymentIntentId);                    _unitOfWork.orderHeaderRepository.UpdateStatus(id, OrderStatus.StatusApproved, OrderStatus.PaymentStatusApproved);                    _unitOfWork.Save();
                }
                HttpContext.Session.Clear();            }

            //remove shopping cart
            List<ShoppingCart> shoppingCarts = _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();            _unitOfWork.shoppingCartRepository.RemoveRange(shoppingCarts);            _unitOfWork.Save();            return View(id);



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
