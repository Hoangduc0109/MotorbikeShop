using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Motorbike.Models
{
    [Table("motorbike")]
    public class Motorbike
    {
        [Key]
        [Column("motorbike_id")]
        public int MotorbikeId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string MotorbikeName { get; set; } = null!;

        [Required]
        [Column("brand_id")]
        public int BrandId { get; set; }

        [Required]
        [Column("price", TypeName = "decimal(12, 2)")]
        public decimal Price { get; set; }

        [Column("image")]
        [StringLength(255)]
        public string? Image { get; set; }

        [Column("description", TypeName = "nvarchar(max)")]
        public string? Description { get; set; }

        [Column("total_sold")]
        public int? TotalSold { get; set; }

        [Required]
        [Column("stock")]
        public int Stock { get; set; }

        // Thêm các thuộc tính giả định (không có trong DB)
        [NotMapped]
        public decimal? DiscountedPrice { get; set; }

        [NotMapped]
        public bool IsBestSeller => TotalSold.HasValue && TotalSold.Value > 100;

        [ForeignKey("BrandId")]
        public virtual Brand? Brand { get; set; }
        
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}