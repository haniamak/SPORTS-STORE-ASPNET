using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class ShopController : Controller
    {
        private readonly ShopContext _context;
        public ShopController(ShopContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult List(string category = "all")
        {
            List<Product> products = new List<Product>();
            if(category.Equals("all"))
            {
                products = _context.Products.ToList();
            }
            else
            {
                products = _context.Products.Where(p => p.Category == category).ToList();
            }
            return View(products);
        }

        public IActionResult Details(int id)
        {
            var product = _context.Products.Find(id);
            return View(product);
        }

        [HttpPost]
        public IActionResult Search(SearchModel searchModel)
        {
            var productQuery = searchModel.ProductQuery?.ToLower();

            var products = _context.Products
                .Where(p =>
                    (p.Name != null && p.Name.ToLower().Contains(productQuery)) ||
                    (p.Description != null && p.Description.ToLower().Contains(productQuery)))
                .ToList();

            return View("List", products);
        }
    }
}
