using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Motorbike.Models
{
    [Table("orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("order_date", TypeName = "date")]
        public DateTime OrderDate { get; set; }

        [Column("delivery_date", TypeName = "date")]
        public DateTime? DeliveryDate { get; set; }

        [Required]
        [Column("account_id")]
        public int? AccountId { get; set; }  // Thêm ? để làm kiểu nullable

        [Column("status")]
        [StringLength(50)]
        public string? Status { get; set; }

        [Column("address")]
        [StringLength(255)]
        public string? Address { get; set; }

        [Column("total_price", TypeName = "decimal(12, 2)")]
        public decimal? TotalPrice { get; set; }

        [Column("payment_method")]
        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("phone")]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Column("customer_name")]
        [StringLength(100)]
        public string? CustomerName { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account? Account { get; set; }
        
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}