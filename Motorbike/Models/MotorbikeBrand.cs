using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Motorbike.Models
{
    [Table("motorbike_brand")]
    public class Brand
    {
        [Key]
        [Column("brand_id")]
        public int BrandId { get; set; }

        [Required]
        [Column("brand_name")]
        [StringLength(100)]
        public string BrandName { get; set; } = null!;

        [Column("logo")]
        [StringLength(255)]
        public string? Logo { get; set; }
        
        public virtual ICollection<Motorbike>? Motorbikes { get; set; }
    }
}