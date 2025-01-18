namespace WebApplication2.Models
{
    public class OrderDetailsModel
    {
        public int ID { get; set; }
        public int ID_Order { get; set; }
        public int ID_Product { get; set; }
        public int QUANTITY { get; set; }
        public decimal PRICE { get; set; }
    }
}
