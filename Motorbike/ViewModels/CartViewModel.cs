using Motorbike.Models;

namespace Motorbike.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
    }

    public class CartItem
    {
        public int MotorbikeId { get; set; }
        public string MotorbikeName { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity; // Đảm bảo có thuộc tính này
    }
}