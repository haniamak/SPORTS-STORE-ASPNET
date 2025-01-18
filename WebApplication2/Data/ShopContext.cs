using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebApplication2.Models;

namespace WebApplication2.Data
{
    public class ShopContext : DbContext
    {
        public ShopContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderDetailsModel> OrderDetails { get; set; }

        public void SeedData()
        {
            

                //context.Database.EnsureCreated();

                // Load JSON file
                string jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "products.json");
                if (File.Exists(jsonFilePath))
                {
                    string jsonData = File.ReadAllText(jsonFilePath);
                    var products = JsonConvert.DeserializeObject<List<Product>>(jsonData);

                    if (products != null && !this.Products.Any())
                    {
                        this.Products.AddRange(products);
                        this.SaveChanges();
                        Console.WriteLine("Products added to the database.");
                    }
                    else
                    {
                        Console.WriteLine("Products already exist in the database or JSON is empty.");
                    }
                }
                else
                {
                    Console.WriteLine($"JSON file not found at {jsonFilePath}");
                }
            }
        }
    
}
