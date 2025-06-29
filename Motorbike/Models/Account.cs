using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Motorbike.Models
{
    [Table("account")]
    public class Account
    {
        [Key]
        [Column("account_id")]
        public int AccountId { get; set; }
        
        [Column("username")]
        public string Username { get; set; }
        
        [Column("password")]
        public string Password { get; set; }
        
        [Column("email")]
        public string Email { get; set; }
        
        [Column("phone")]
        public string Phone { get; set; }
        
        [Column("address")]
        public string Address { get; set; }
        
        [Column("role_id")]
        public int? RoleId { get; set; }
        
        // Navigation property để liên kết với UserRole
        [ForeignKey("RoleId")]
        public virtual UserRole? UserRole { get; set; }
    }
}