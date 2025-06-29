using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Motorbike.Models
{
    [Table("contact")]
    public class Contact
    {
        [Key]
        [Column("contact_id")]
        public int ContactId { get; set; }
        
        [Column("account_id")]
        public int? AccountId { get; set; }
        
        [Column("name")]
        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        public string Name { get; set; }
        
        [Column("email")]
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }
        
        [Column("phone")]
        public string? Phone { get; set; }  // Thêm dấu ? để cho phép NULL
        
        [Column("subject")]
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Subject { get; set; }
        
        [Column("message")]
        [Required(ErrorMessage = "Vui lòng nhập nội dung tin nhắn")]
        public string Message { get; set; }
        
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        [Column("status")]
        public string Status { get; set; } = "Chưa xử lý";
        
        [Column("response")]
        public string? Response { get; set; }  // Thêm dấu ? để cho phép NULL
        
        [Column("response_at")]
        public DateTime? ResponseAt { get; set; }
        
        // Navigation property
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}