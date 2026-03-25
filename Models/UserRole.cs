using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendingMachineApp.Models
{
    public class UserRole : BaseEntity
    {
        [Key]
        public int IdUserRole { get; set; }

        [Required]
        public int IdUser { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [ForeignKey("IdUser")]
        public UserLogin? UserLogin { get; set; }
    }
}
