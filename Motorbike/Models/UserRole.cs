using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Motorbike.Models
{
    [Table("user_role")]
    public class UserRole
    {
        [Key]
        [Column("role_id")]
        public int RoleId { get; set; }
        
        [Column("role_name")]
        public string RoleName { get; set; }

        // Navigation property - một role có thể gắn với nhiều tài khoản
        public virtual ICollection<Account> Accounts { get; set; }
    }
}