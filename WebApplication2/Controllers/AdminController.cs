using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    [Authorize("Admin")]
	public class AdminController : Controller
	{
        private readonly ShopContext _context;
        private readonly IDapperRepository<USER> _userRepository;

        public AdminController(ShopContext context, IDapperRepository<USER> userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }
        public IActionResult Index()
		{
            //TODO: Implement this method
            return View();
		}

		public IActionResult ListofProducts()
		{
            List<Product> products = new List<Product>();
            products = _context.Products.Where(p => !p.IsDeleted).ToList();
            return View(products);
        }

        public IActionResult ListofOrders()
        {
            List<OrderModel> orders = new List<OrderModel>();
			orders = _context.Orders.ToList();
			return View(orders);
        }

		public IActionResult OrderDetails(int id)
        {
			var orderDetails = _context.OrderDetails.Where(od => od.ID_Order == id).ToList();

			// Pass the data to the view
			return View(orderDetails);
		}

		public IActionResult ListofUsers()
		{
            List<USER> users = new List<USER>();

            users = _userRepository.GetAll().ToList();
			List<UserModel> userModels = users.Select(u => new UserModel
			{
				Id = u.Id,
				UserName = u.UserName,
				Email = u.Email
			}).ToList();

			return View(userModels);
		}

        public IActionResult UpdateProduct(int id)
        {
            var product = _context.Products.Find(id);
            return View(product);
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

        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            return View(product);
        }

        [HttpPost]
        public IActionResult SaveDelete(Product model)
        {
            var product = _context.Products.Find(model.ProductId);
            if (product == null)
            {
                return NotFound();
            }

            product.IsDeleted = true;

            _context.SaveChanges();

            return RedirectToAction("ListofProducts");
        }

        public IActionResult CreateProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveCreate(Product model)
        {
            var form = this.Request.Form;
            if (form.Files[0] != null)
            {
                var file = form.Files[0];
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data", "images");

                var imgFileName = Path.GetFileName(file.FileName);
                var filePath = Path.Combine(uploadsFolder, imgFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                model.ImageFileName = imgFileName;
            }


            _context.Products.Add(model);
            _context.SaveChanges();
            return RedirectToAction("ListofProducts");
        }

    }


}
