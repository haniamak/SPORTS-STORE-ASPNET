using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ShopContext _context;
        private List<CartItemModel> _cartItems;
        private readonly IDapperRepository<USER> _userRepository;
        public CartController(ShopContext context, IDapperRepository<USER> userRepository)
        {
            _context = context;
            _cartItems = new List<CartItemModel>();
            _userRepository = userRepository;
        }
        public IActionResult Index()
        {
            return View();
        }

        
        public IActionResult AddToCart(int productId)
        {
            if (User.Identity.IsAuthenticated == false)
			{
				return RedirectToAction("Login", "Index");
			}
            else
            {
				var product = _context.Products.Find(productId);

				var cartItems = HttpContext.Session.Get<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

				var existingItem = cartItems.FirstOrDefault(i => i.Product.ProductId == productId);
				if (existingItem != null)
				{
					existingItem.Quantity++;
				}
				else
				{
					cartItems.Add(new CartItemModel
					{
						Product = product,
						Quantity = 1
					});
				}

				HttpContext.Session.Set("Cart", cartItems);

				return RedirectToAction("ViewCart");
			}
			
        }

        
        public IActionResult ViewCart()
        {
			if (User.Identity.IsAuthenticated == false)
			{
				return RedirectToAction("Login", "Index");
			}
			var cartItems = HttpContext.Session.Get<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

            var cart = new CartModel { 
                CartItems = cartItems,
				TotalPrice = cartItems.Sum(i => i.Product.Price * i.Quantity),
			};
            return View(cart);
        }

        public IActionResult RemoveFromCart(int id) {
			var cartItems = HttpContext.Session.Get<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			var itemToRemove = cartItems.FirstOrDefault(i => i.Product.ProductId == id);
			if (itemToRemove != null)
			{
				if (itemToRemove.Quantity > 1)
				{
					itemToRemove.Quantity--;
				}
				else
				{
					cartItems.Remove(itemToRemove);
				}
			}

			HttpContext.Session.Set("Cart", cartItems);

			return RedirectToAction("ViewCart");
		}

        public IActionResult AddExtra(int id)
		{
			var cartItems = HttpContext.Session.Get<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			var itemToAdd = cartItems.FirstOrDefault(i => i.Product.ProductId == id);
			if (itemToAdd != null)
			{
				itemToAdd.Quantity++;
			}

			HttpContext.Session.Set("Cart", cartItems);

			return RedirectToAction("ViewCart");
		}

        public IActionResult SaveOrder(OrderModel model)
        {
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(userEmail))
            {
                ModelState.AddModelError("", "Email not found for the current user.");
                return RedirectToAction("ViewCart");
            }

            var user = _userRepository.GetUserByEmail(userEmail);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found.");
                return RedirectToAction("ViewCart");
            }

            var cartItems = HttpContext.Session.Get<List<CartItemModel>>("Cart");
            if (cartItems == null || !cartItems.Any())
            {
                ModelState.AddModelError("", "Your cart is empty.");
                return RedirectToAction("ViewCart");
            }

            var order = new OrderModel
            {
                ID_User = user.Id, // Assuming the username is the User ID
                Date = DateTime.Now,
                ORDERVALUE = cartItems.Sum(i => i.Product.Price * i.Quantity)
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetailsModel
                {
                    ID_Order = order.ID,
                    ID_Product = item.Product.ProductId,
                    QUANTITY = item.Quantity,
                    PRICE = item.Product.Price
                };
                _context.OrderDetails.Add(orderDetail);
            }

            _context.SaveChanges();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("Index", "Home");
        }


    }
}
