using System.ComponentModel.DataAnnotations;
namespace WebApplication2.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        [Required(ErrorMessage = "Please enter product name")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Please enter product description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please enter product price")]
        [Range(0, 100000, ErrorMessage = "Price must be between 0 and 100000")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Please enter product category")]
        public string? Category { get; set; }

        public string? ImageFileName { get; set; }

        public bool IsDeleted { get; set; }
    }
}
