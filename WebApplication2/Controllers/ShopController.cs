using Microsoft.AspNetCore.Mvc;
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

        [Route("shop/{category?}")]
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
    }
}
