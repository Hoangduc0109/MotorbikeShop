using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Motorbike.Models
{
    [Table("order_details")]
    public class OrderDetail
    {
        [Key]
        [Column("detail_id")]
        public int DetailId { get; set; }
        
        [Column("order_id")]
        public int OrderId { get; set; }
        
        [Column("motorbike_id")]
        public int MotorbikeId { get; set; }
        
        [Column("quantity")]
        public int Quantity { get; set; }
        
        [Column("unit_price", TypeName = "decimal(12, 2)")]
        public decimal UnitPrice { get; set; }
        
        // Navigation properties
        [ForeignKey("OrderId")]
        public virtual Order Order { get; set; }
        
        [ForeignKey("MotorbikeId")]
        public virtual Motorbike Motorbike { get; set; }
    }
}