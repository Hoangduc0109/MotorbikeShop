using System;
using System.ComponentModel.DataAnnotations;

namespace Motorbike.ViewModels
{
    public class ContactResponseViewModel
    {
        public int ContactId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi")]
        [Display(Name = "Nội dung phản hồi")]
        public string Response { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        [Display(Name = "Trạng thái")]
        public string Status { get; set; }
    }
    
    public class ContactFilterViewModel
    {
        public string SearchTerm { get; set; }
        public string Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string SortOrder { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}