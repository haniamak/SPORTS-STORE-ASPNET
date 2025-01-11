using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Models;
using Microsoft.AspNetCore.Http;

namespace WebApplication2.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ShopContext _context;
        private List<CartItemModel> _cartItems;
        public CartController(ShopContext context)
        {
            _context = context;
            _cartItems = new List<CartItemModel>();
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
	}
}
