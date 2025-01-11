namespace WebApplication2.Models
{
    public class CartModel
    {
        public List<CartItemModel> CartItems { get; set; } = new List<CartItemModel>();
        public decimal TotalPrice;
        public int TotalQuantity;
	}
}
