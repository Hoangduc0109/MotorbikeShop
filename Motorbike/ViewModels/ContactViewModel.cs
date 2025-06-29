using System.ComponentModel.DataAnnotations;

namespace Motorbike.ViewModels
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [Display(Name = "Họ và tên")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }
        
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^(0\d{9,10})$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        [Display(Name = "Tiêu đề")]
        public string Subject { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn")]
        [Display(Name = "Tin nhắn của bạn")]
        public string Message { get; set; }
    }
}