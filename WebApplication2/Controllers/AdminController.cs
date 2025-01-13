using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
	
	public class AdminController : Controller
	{
        private readonly ShopContext _context;

        public AdminController(ShopContext context)
        {
            _context = context;
        }
        public IActionResult Index()
		{
            //TODO: Implement this method
            return View();
		}

		public IActionResult ListofProducts()
		{
            List<Product> products = new List<Product>();
            products = _context.Products.ToList();
            return View(products);
        }


        public IActionResult ListofUsers()
		{
            //TODO: Implement this method
            return View();
        }

        public IActionResult UpdateProduct(int id)
        {
            var product = _context.Products.Find(id);
            return View(product);
        }

        public IActionResult DeleteProduct()
        {
            //TODO: Implement this method
            return View();
        }
        [HttpPost]
        public IActionResult SaveUpdate(Product model)
        {
            if (ModelState.IsValid)
            {
                var product = _context.Products.Find(model.ProductId);
                if (product == null)
                {
                    return NotFound();
                }

                product.Name = model.Name;
                product.Category = model.Category;
                product.Description = model.Description;
                product.Price = model.Price;

                _context.SaveChanges();

                return RedirectToAction("ListofProducts");
            }

            return View("ListofProducts");
        }

    }  
        
        
}
