using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Motorbike.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        // Thêm cho trường hợp người dùng có tài khoản
        public int? AccountId { get; set; }
    }
}